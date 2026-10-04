#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Abilities.Fireball;
using ProjectFirstRun.Abilities.Shuriken;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
namespace ProjectFirstRun.Tests.PlayMode.Abilities
{
    public sealed class ShurikenRuntimeTests
    {
        private readonly List<Object> _objects = new List<Object>();
        private NavMeshDataInstance _mesh;
        private EnemyRegistry _registry;
        private HealthComponent _player;
        private ShurikenRuntime _runtime;
        private EnemyDefinition _enemyDefinition;
        private ShurikenDefinition _definition;
        private PlayerStatCollection _stats;
        private GameObject Make(string name) { var go = new GameObject(name); _objects.Add(go); return go; }
        [SetUp] public void Setup()
        {
            Time.timeScale = 1;
            var data = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByID(0),
                new List<NavMeshBuildSource> { new NavMeshBuildSource {
                    shape = NavMeshBuildSourceShape.Box, size = new Vector3(40, .2f, 40),
                    transform = Matrix4x4.TRS(new Vector3(0, -.1f, 0), Quaternion.identity, Vector3.one), area = 0
                } }, new Bounds(Vector3.zero, new Vector3(40, 5, 40)), Vector3.zero, Quaternion.identity);
            _objects.Add(data); _mesh = NavMesh.AddNavMeshData(data);
            _registry = Make("Registry").AddComponent<EnemyRegistry>();
            _player = Make("Source").AddComponent<HealthComponent>(); _player.Initialize(100);
            _definition = AssetDatabase.LoadAssetAtPath<ShurikenDefinition>("Assets/_Project/Data/Abilities/AD_Shuriken.asset");
            _stats = new PlayerStatCollection();
            _runtime = new ShurikenRuntime(_definition, _registry, _player.gameObject, _stats);
            _enemyDefinition = ScriptableObject.CreateInstance<EnemyDefinition>(); _objects.Add(_enemyDefinition);
        }
        private EnemyController Enemy(float x = 2.5f)
        {
            var go = Make("Target"); go.transform.position = Vector3.right * x; go.AddComponent<NavMeshAgent>();
            var enemy = go.AddComponent<EnemyController>(); enemy.Initialize(_enemyDefinition, _player.transform, _registry);
            enemy.Health.Initialize(1000); go.GetComponent<EnemyMotor>().Stop();
            var collider = go.AddComponent<BoxCollider>(); collider.center = Vector3.up; collider.size = new Vector3(.2f, 2, .2f);
            return enemy;
        }
        private void Level(int level) => typeof(ShurikenRuntime).GetMethod("ApplyConfiguration",
            BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_runtime, new object[] { _definition.CreateLevelConfigs()[level - 1] });
        private void Tick(float delta, bool enabled = true)
        {
            Physics.SyncTransforms(); _runtime.TickContinuous(delta, _player.transform.position + Vector3.up, enabled);
        }
        [TearDown] public void Cleanup()
        {
            Time.timeScale = 1; _runtime.CancelOrbit();
            foreach (var view in Object.FindObjectsByType<ShurikenView>(FindObjectsSortMode.None))
                Object.DestroyImmediate(view.gameObject);
            for (int i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
            _objects.Clear(); if (_mesh.valid) _mesh.Remove();
        }
        [Test] public void TwoTurnsHitTwiceAndCooldownStartsAfterOrbit()
        {
            var enemy = Enemy(); enemy.gameObject.AddComponent<SphereCollider>();
            Tick(3);
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(970));
            Assert.That(_runtime.IsOrbiting, Is.False);
            Assert.That(_runtime.CooldownRemaining, Is.EqualTo(3).Within(.001f));
            Tick(2.9f); Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(970));
            Tick(.2f); Assert.That(_runtime.IsOrbiting, Is.True);
        }
        [Test] public void HourglassChangesPostOrbitWaitNotOrbitOrExistingWait()
        {
            Tick(.1f);
            _stats.Add(new StatModifier(PlayerStatType.AbilityCooldown,
                StatModifierOperation.AdditivePercent, -.25f, "hourglass"));
            Tick(2.8f);
            Assert.That(_runtime.IsOrbiting, Is.True);
            Tick(.1f);
            Assert.That(_runtime.IsOrbiting, Is.False);
            Assert.That(_runtime.CooldownRemaining, Is.EqualTo(2.25f).Within(.001f));
            _stats.RemoveBySource("hourglass");
            _runtime.BindEnemyRegistry(_registry);
            Assert.That(_runtime.CooldownRemaining, Is.EqualTo(2.25f).Within(.001f));
            Tick(2.2f);
            Assert.That(_runtime.IsOrbiting, Is.False);
            Tick(.1f);
            Assert.That(_runtime.IsOrbiting, Is.True);
            _runtime.CancelOrbit();
            Assert.That(_runtime.CooldownRemaining, Is.EqualTo(3f).Within(.001f));
        }
        [Test] public void TwoShurikensHaveIndependentHitLedgers()
        {
            var enemy = Enemy(); Level(6); Tick(2);
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(860));
        }
        [Test] public void UpgradeDoesNotChangeCurrentOrbit()
        {
            var enemy = Enemy(); Tick(.1f); Level(6); Tick(2.9f);
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(970));
            Assert.That(_runtime.CooldownRemaining, Is.EqualTo(3).Within(.001f));
            Tick(3); Tick(2);
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(830));
        }
        [Test] public void WallPreventsDamage()
        {
            var enemy = Enemy();
            var wall = Make("Wall"); wall.transform.position = new Vector3(1.2f, 1, 0);
            wall.AddComponent<BoxCollider>().size = new Vector3(.2f, 4, 10);
            Tick(3); Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(1000));
        }
        [Test] public void PauseFreezesOrbitAndControlLossCancelsWithCooldown()
        {
            Tick(.1f); Time.timeScale = 0; Tick(20);
            Assert.That(_runtime.IsOrbiting, Is.True);
            Time.timeScale = 1; Tick(.1f, false);
            Assert.That(_runtime.IsOrbiting, Is.False);
            Assert.That(_runtime.CooldownRemaining, Is.EqualTo(3));
        }
        [Test] public void TeleportAndMapRebindCancelOrbit()
        {
            Tick(.1f); _player.transform.position = Vector3.forward * 10; Tick(.1f);
            Assert.That(_runtime.IsOrbiting, Is.False);
            Tick(3.1f); Assert.That(_runtime.IsOrbiting, Is.True);
            _runtime.BindEnemyRegistry(Make("Next registry").AddComponent<EnemyRegistry>());
            Assert.That(_runtime.IsOrbiting, Is.False);
            Assert.That(_runtime.CooldownRemaining, Is.EqualTo(3));
        }
        [Test] public void BleedRefreshPreservesTickAndCoexistsWithBurnAfterSourceDeath()
        {
            var enemy = Enemy();
            ShurikenBleed.Apply(_player.gameObject, enemy, 5);
            var bleed = enemy.GetComponent<ShurikenBleed>(); bleed.Advance(.4f);
            ShurikenBleed.Apply(_player.gameObject, enemy, 5);
            Assert.That(enemy.GetComponents<ShurikenBleed>().Length, Is.EqualTo(1));
            FireballBurn.Apply(_player.gameObject, enemy.Health, enemy.GetComponent<Collider>(), 3, 2);
            bleed.Advance(.1f);
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(995));
            enemy.GetComponent<FireballBurn>().Advance(.5f);
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(992));
            var damage = new DamageInfo(1000, null, Vector3.zero, Vector3.up); _player.ApplyDamage(in damage);
            bleed.Advance(.5f); Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(987));
        }
        [Test] public void ReinitializedEnemyDoesNotInheritBleed()
        {
            var enemy = Enemy(); ShurikenBleed.Apply(_player.gameObject, enemy, 5);
            var bleed = enemy.GetComponent<ShurikenBleed>();
            enemy.Initialize(_enemyDefinition, _player.transform, _registry);
            bleed.Advance(.5f);
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(_enemyDefinition.MaximumHealth));
        }
        [Test] public void LevelEightContactAppliesBleed()
        {
            var enemy = Enemy(3.5f); Level(8); Tick(.1f);
            Assert.That(enemy.GetComponent<ShurikenBleed>(), Is.Not.Null);
        }
    }
}
#endif
