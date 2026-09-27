using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.ForceWave;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectFirstRun.Tests.EditMode.Abilities
{
    public sealed class ForceWaveTests
    {
        readonly List<Object> owned = new List<Object>();
        GameObject source;
        EnemyRegistry registry;
        EnemyDefinition enemyDefinition;
        ForceWaveDefinition definition;
        PlayerStatCollection stats;
        GameObject Make(string name) { var go = new GameObject(name); owned.Add(go); return go; }
        [SetUp] public void Setup()
        {
            Time.timeScale = 1; source = Make("wave source");
            registry = Make("wave registry").AddComponent<EnemyRegistry>();
            enemyDefinition = ScriptableObject.CreateInstance<EnemyDefinition>(); owned.Add(enemyDefinition);
            definition = ScriptableObject.CreateInstance<ForceWaveDefinition>(); owned.Add(definition);
            stats = new PlayerStatCollection();
        }
        [TearDown] public void Cleanup()
        {
            Time.timeScale = 1;
            for (int i = owned.Count - 1; i >= 0; i--) if (owned[i] != null) Object.DestroyImmediate(owned[i]);
            owned.Clear();
        }
        EnemyController Enemy(Vector3 position)
        {
            var go = Make("wave target"); go.SetActive(false); go.transform.position = position;
            go.AddComponent<NavMeshAgent>().enabled = false;
            go.AddComponent<HealthComponent>(); go.AddComponent<EnemyMotor>();
            var enemy = go.AddComponent<EnemyController>();
            enemy.Initialize(enemyDefinition, source.transform, registry);
            go.AddComponent<BoxCollider>(); go.SetActive(true); registry.Register(enemy); Physics.SyncTransforms();
            return enemy;
        }
        AbilityRuntimeEntry Entry() => new ForceWaveRuntimeFactory(registry, source, stats).Create(definition);
        [TestCase(0, 4, 0, true)] [TestCase(0, 4.01f, 0, false)]
        [TestCase(50, 4, 0, true)] [TestCase(51, 4, 0, false)]
        [TestCase(0, 3, 2, true)] [TestCase(0, 3, 2.01f, false)]
        [TestCase(180, 2, 0, false)]
        public void SectorBoundaries(float angle, float range, float height, bool expected)
        {
            Vector3 point = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * range + Vector3.up * height;
            Assert.That(ForceWaveRuntime.InSector(Vector3.zero, Vector3.forward, point, 4, 100, 2), Is.EqualTo(expected));
        }
        [Test] public void BehindOnlyDoesNotConsumeCooldown()
        {
            Enemy(Vector3.back); var entry = Entry();
            Assert.That(entry.TryAutoCast(Vector3.zero), Is.EqualTo(AbilityAutoCastResult.NoTarget)); Assert.That(entry.IsReady, Is.True);
        }
        [Test] public void OneCastHitsEachReceiverOnceAndUsesAbilityStats()
        {
            var a = Enemy(Vector3.forward * 2); var b = Enemy(new Vector3(1, 0, 3));
            a.gameObject.AddComponent<SphereCollider>();
            float aBefore = a.Health.CurrentHealth, bBefore = b.Health.CurrentHealth;
            stats.Add(new StatModifier(PlayerStatType.AbilityDamage, StatModifierOperation.AdditivePercent, .5f, "wave"));
            stats.Add(new StatModifier(PlayerStatType.WeaponDamage, StatModifierOperation.AdditivePercent, 10, "weapon"));
            var entry = Entry(); entry.TryAutoCast(Vector3.zero);
            Assert.That(a.Health.CurrentHealth, Is.EqualTo(aBefore - 30)); Assert.That(b.Health.CurrentHealth, Is.EqualTo(bBefore - 30));
            Assert.That(entry.TryAutoCast(Vector3.zero), Is.EqualTo(AbilityAutoCastResult.OnCooldown));
            entry.Tick(5); Assert.That(entry.IsReady, Is.True);
        }
        [Test] public void WallBlocksButTriggerDoesNot()
        {
            Enemy(Vector3.forward * 3);
            var wall = Make("wall").AddComponent<BoxCollider>(); wall.transform.position = Vector3.forward;
            Physics.SyncTransforms(); var entry = Entry();
            Assert.That(entry.TryAutoCast(Vector3.zero), Is.EqualTo(AbilityAutoCastResult.NoTarget));
            wall.isTrigger = true; Physics.SyncTransforms(); entry.TryAutoCast(Vector3.zero);
            Assert.That(entry.IsReady, Is.False);
        }
        [Test] public void FacingChangesSelectionWithoutAutoAim()
        {
            Enemy(Vector3.right * 2); var entry = Entry();
            Assert.That(entry.TryAutoCast(Vector3.zero), Is.EqualTo(AbilityAutoCastResult.NoTarget));
            source.transform.rotation = Quaternion.Euler(0, 90, 0); entry.TryAutoCast(Vector3.zero);
            Assert.That(entry.IsReady, Is.False);
        }
        [Test] public void RebindingDropsOldMapTargets()
        {
            Enemy(Vector3.forward * 2); var entry = Entry();
            entry.BindEnemyRegistry(Make("new map registry").AddComponent<EnemyRegistry>());
            Assert.That(entry.TryAutoCast(Vector3.zero), Is.EqualTo(AbilityAutoCastResult.NoTarget));
        }
        [Test] public void PauseAndSourceDeathCannotDealDamage()
        {
            var target = Enemy(Vector3.forward * 2); float before = target.Health.CurrentHealth; var entry = Entry();
            Time.timeScale = 0; entry.TryAutoCast(Vector3.zero); Assert.That(entry.IsReady, Is.True);
            Time.timeScale = 1; var health = source.AddComponent<HealthComponent>(); health.Initialize(1);
            health.ApplyDamage(new DamageInfo(1, target.gameObject, Vector3.zero, Vector3.up));
            entry.TryAutoCast(Vector3.zero); Assert.That(target.Health.CurrentHealth, Is.EqualTo(before));
        }
        [Test] public void SavedAssetMatchesAllEightLevels()
        {
            var asset = AssetDatabase.LoadAssetAtPath<ForceWaveDefinition>("Assets/_Project/Data/Abilities/AD_ForceWave.asset");
            Assert.That(asset.StableId, Is.EqualTo("ability.force-wave"));
            var levels = asset.CreateLevelConfigs(); Assert.That(levels.Length, Is.EqualTo(8));
            float[] damage = { 20,30,30,30,30,40,40,40 }, reach = { 4,4,5,5,5,5,6,6 }, cooldown = { 5,5,5,3,3,3,3,1.5f };
            for (int i = 0; i < 8; i++)
            {
                Assert.That(levels[i].Damage, Is.EqualTo(damage[i]));
                Assert.That(levels[i].Reach, Is.EqualTo(reach[i]));
                Assert.That(levels[i].Ability.Cooldown, Is.EqualTo(cooldown[i]));
                Assert.That(levels[i].Push, Is.EqualTo(i < 4 ? 2 : 3));
            }
            Assert.That(asset.VisualMaterial, Is.Not.Null);
        }
    }
}
