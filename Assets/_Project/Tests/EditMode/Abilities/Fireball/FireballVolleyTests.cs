using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.Fireball;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

namespace ProjectFirstRun.Tests.EditMode.Abilities.Fireball
{
    public sealed class FireballVolleyTests
    {
        readonly List<Object> owned = new List<Object>();
        EnemyRegistry registry;
        GameObject source;
        EnemyDefinition enemyDefinition;
        GameObject Make(string name) { var go = new GameObject(name); owned.Add(go); return go; }
        [SetUp] public void Setup()
        {
            source = Make("source");
            registry = Make("registry").AddComponent<EnemyRegistry>();
            enemyDefinition = ScriptableObject.CreateInstance<EnemyDefinition>(); owned.Add(enemyDefinition);
        }
        [TearDown] public void Cleanup()
        {
            foreach (var p in Object.FindObjectsByType<FireballProjectile>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Object.DestroyImmediate(p.gameObject);
            for (int i = owned.Count - 1; i >= 0; i--) if (owned[i] != null) Object.DestroyImmediate(owned[i]);
            owned.Clear();
        }
        EnemyController Enemy(float z)
        {
            var go = Make("enemy"); go.SetActive(false); go.transform.position = Vector3.forward * z;
            go.AddComponent<NavMeshAgent>().enabled = false;
            go.AddComponent<HealthComponent>(); go.AddComponent<EnemyMotor>();
            var enemy = go.AddComponent<EnemyController>();
            enemy.Initialize(enemyDefinition, source.transform, registry); go.SetActive(true); registry.Register(enemy);
            return enemy;
        }
        FireballTargetSelector Selector() => new FireballTargetSelector(registry, source, 20, 129, n => n - 1);
        [Test] public void Volley_UsesInjectedRandomWithoutRepeatingBeforeCycleEnds()
        {
            var a = Enemy(3); var b = Enemy(6); var selector = Selector();
            var volley = selector.SelectVolley(Vector3.zero, 4);
            Assert.That(volley, Is.EqualTo(new[] { b.transform, a.transform, b.transform, a.transform }));
        }
        [Test] public void OneTarget_CanReceiveWholeVolley()
        {
            var enemy = Enemy(3);
            Assert.That(Selector().SelectVolley(Vector3.zero, 4), Is.EqualTo(new[] { enemy.transform, enemy.transform, enemy.transform, enemy.transform }));
        }
        [Test] public void Range_IsInclusiveAndInactiveAndUnregisteredTargetsAreExcluded()
        {
            var edge = Enemy(20); Enemy(20.01f); var inactive = Enemy(2); inactive.gameObject.SetActive(false);
            var removed = Enemy(3); registry.Unregister(removed);
            Assert.That(Selector().SelectVolley(Vector3.zero, 2), Is.EqualTo(new[] { edge.transform, edge.transform }));
        }
        [Test] public void WorldCover_BlocksButTriggersDoNot()
        {
            Enemy(8); var wall = Make("wall").AddComponent<BoxCollider>(); wall.transform.position = Vector3.forward * 4;
            Physics.SyncTransforms(); var selector = Selector();
            Assert.That(selector.TrySelectTarget(Vector3.zero, out _), Is.False);
            wall.isTrigger = true; Physics.SyncTransforms();
            Assert.That(selector.TrySelectTarget(Vector3.zero, out _), Is.True);
        }
        [Test] public void RegistryRebind_DoesNotRetainPreviousMapTargets()
        {
            Enemy(3); var selector = Selector();
            selector.BindEnemyRegistry(Make("next map").AddComponent<EnemyRegistry>());
            Assert.That(selector.SelectVolley(Vector3.zero, 2), Is.Empty);
        }
        [TestCase(2)] [TestCase(3)] [TestCase(4)]
        public void Runtime_WholeVolleyConsumesOnlyOneCooldown(int count)
        {
            Enemy(8);
            var definition = ScriptableObject.CreateInstance<FireballDefinition>(); owned.Add(definition);
            var prefab = Make("prefab"); prefab.AddComponent<SphereCollider>(); prefab.AddComponent<Rigidbody>();
            var projectile = prefab.AddComponent<FireballProjectile>();
            Set(definition, "_projectilePrefab", projectile); Set(definition, "_projectileCount", count);
            var entry = new FireballAbilityRuntimeFactory(registry, source, new PlayerStatCollection()).Create(definition);
            entry.TryAutoCast(Vector3.zero);
            Assert.That(entry.IsReady, Is.False);
            Assert.That(Object.FindObjectsByType<FireballProjectile>(FindObjectsSortMode.None).Length, Is.EqualTo(count + 1));
            Assert.That(entry.TryAutoCast(Vector3.zero), Is.EqualTo(AbilityAutoCastResult.OnCooldown));
            entry.Tick(definition.CreateRuntimeConfig().Cooldown);
            Assert.That(entry.IsReady, Is.True);
        }
        [Test] public void NoTarget_PreservesReadyState()
        {
            var definition = ScriptableObject.CreateInstance<FireballDefinition>(); owned.Add(definition);
            var entry = new FireballAbilityRuntimeFactory(registry, source, new PlayerStatCollection()).Create(definition);
            Assert.That(entry.TryAutoCast(Vector3.zero), Is.EqualTo(AbilityAutoCastResult.NoTarget));
            Assert.That(entry.IsReady, Is.True);
        }
        [Test] public void AuthoredAsset_PreservesIdentityAndDefinesTwoThreeFourProgression()
        {
            var definition = AssetDatabase.LoadAssetAtPath<FireballDefinition>("Assets/_Project/Data/Abilities/Fireball/AD_Fireball.asset");
            Assert.That(definition, Is.Not.Null);
            Assert.That(definition.ProjectileCount, Is.EqualTo(2));
            Assert.That(definition.TargetRange, Is.EqualTo(20));
            var serialized = new SerializedObject(definition);
            var levels = serialized.FindProperty("_additionalLevels");
            Assert.That(levels.arraySize, Is.EqualTo(7));
            int[] counts = { 2, 2, 3, 3, 3, 4, 4 };
            for (int i = 0; i < counts.Length; i++)
                Assert.That(levels.GetArrayElementAtIndex(i).FindPropertyRelative("_projectileCount").intValue, Is.EqualTo(counts[i]));
        }
        static void Set(object target, string field, object value) => typeof(FireballDefinition)
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
