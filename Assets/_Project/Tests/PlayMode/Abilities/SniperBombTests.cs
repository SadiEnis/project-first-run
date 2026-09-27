#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.SniperBomb;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
namespace ProjectFirstRun.Tests.PlayMode.Abilities
{
    public sealed class SniperBombTests
    {
        private readonly List<Object> _objects = new List<Object>();
        private NavMeshDataInstance _mesh;
        private EnemyRegistry _registry;
        private HealthComponent _player;
        private SniperDefinition _definition;
        private GameObject Make(string name) { var go = new GameObject(name); _objects.Add(go); return go; }
        [SetUp] public void Setup()
        {
            Time.timeScale = 1;
            var data = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByID(0),
                new List<NavMeshBuildSource> { new NavMeshBuildSource {
                    shape = NavMeshBuildSourceShape.Box, size = new Vector3(60, .2f, 60),
                    transform = Matrix4x4.TRS(new Vector3(0, -.1f, 0), Quaternion.identity, Vector3.one), area = 0
                } }, new Bounds(Vector3.zero, new Vector3(60, 5, 60)), Vector3.zero, Quaternion.identity);
            _objects.Add(data); _mesh = NavMesh.AddNavMeshData(data);
            _registry = Make("Registry").AddComponent<EnemyRegistry>();
            _player = Make("Source").AddComponent<HealthComponent>(); _player.Initialize(100);
            _definition = AssetDatabase.LoadAssetAtPath<SniperDefinition>("Assets/_Project/Data/Abilities/AD_SniperBomb.asset");
        }
        private EnemyController Enemy(float x, EnemyRank rank = EnemyRank.Normal)
        {
            var go = Make("Target"); go.transform.position = Vector3.right * x; go.AddComponent<NavMeshAgent>();
            var enemy = go.AddComponent<EnemyController>();
            var def = ScriptableObject.CreateInstance<EnemyDefinition>(); _objects.Add(def);
            typeof(EnemyDefinition).GetField("_rank", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(def, rank);
            enemy.Initialize(def, _player.transform, _registry); enemy.Health.Initialize(1000); go.GetComponent<EnemyMotor>().Stop();
            var collider = go.AddComponent<BoxCollider>(); collider.center = Vector3.up; collider.size = new Vector3(.2f, 2, .2f);
            return enemy;
        }
        private void Wall(float x)
        {
            var go = Make("Wall"); go.transform.position = new Vector3(x, 1, 0);
            go.AddComponent<BoxCollider>().size = new Vector3(.2f, 4, 10);
        }
        private SniperProjectile Bomb(EnemyController target, int level = 1)
        {
            Physics.SyncTransforms(); var config = _definition.CreateLevelConfigs()[level - 1];
            return SniperProjectile.Launch(_player.gameObject, _registry, _definition, config, config.Damage, Vector3.up, target);
        }
        private void Kill(EnemyController target)
        {
            var damage = new DamageInfo(10000, null, target.transform.position, Vector3.up); target.Health.ApplyDamage(in damage);
        }
        [TearDown] public void Cleanup()
        {
            Time.timeScale = 1;
            foreach (var bomb in Object.FindObjectsByType<SniperProjectile>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Object.DestroyImmediate(bomb.gameObject);
            foreach (var blast in Object.FindObjectsByType<SniperBlast>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Object.DestroyImmediate(blast.gameObject);
            for (int i = _objects.Count - 1; i >= 0; i--) if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
            _objects.Clear(); if (_mesh.valid) _mesh.Remove();
        }
        [Test] public void PriorityIsRankThenNearestAndRequiresVisibility()
        {
            Enemy(2); Enemy(4, EnemyRank.Elite); var far = Enemy(10, EnemyRank.Boss); var near = Enemy(7, EnemyRank.Boss);
            var runtime = new SniperRuntime(_definition, _registry, _player.gameObject, new PlayerStatCollection());
            Physics.SyncTransforms(); Assert.That(runtime.TrySelectTarget(Vector3.up, out var target), Is.True);
            Assert.That(target, Is.EqualTo(near.transform));
            Wall(6); Physics.SyncTransforms(); runtime.TrySelectTarget(Vector3.up, out target);
            Assert.That(target.GetComponent<EnemyController>().Definition.Rank, Is.EqualTo(EnemyRank.Elite));
        }
        [Test] public void ExplosionDamagesOnceWithoutSeparateImpactDamage()
        {
            var enemy = Enemy(3); enemy.gameObject.AddComponent<SphereCollider>();
            var bomb = Bomb(enemy); bomb.Advance(.5f);
            Assert.That(bomb.DidExplode, Is.True); Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(950));
        }
        [Test] public void WallDetonationCannotDamageEnemyBehindCover()
        {
            var enemy = Enemy(3); Wall(2); var bomb = Bomb(enemy); bomb.Advance(.5f);
            Assert.That(bomb.DidExplode, Is.True); Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(1000));
            Assert.That(bomb.transform.position.x, Is.LessThan(2));
        }
        [Test] public void BeforeLevelSevenDeadTargetFallsBackToLastPosition()
        {
            var target = Enemy(4); var bomb = Bomb(target);
            Kill(target); bomb.Advance(.6f);
            Assert.That(bomb.DidExplode, Is.True);
            Assert.That(bomb.transform.position.x, Is.EqualTo(4).Within(.01f));
        }
        [Test] public void LevelSevenRetargetsDestroyedTarget()
        {
            var target = Enemy(4); var replacement = Enemy(10, EnemyRank.Elite);
            var bomb = Bomb(target, 7); Object.DestroyImmediate(target.gameObject);
            bomb.Advance(.1f); Assert.That(bomb.Target, Is.EqualTo(replacement));
        }
        [Test] public void NoReplacementCommitsToFallback()
        {
            var target = Enemy(4); var bomb = Bomb(target, 7); Kill(target); bomb.Advance(.1f);
            Enemy(10, EnemyRank.Boss); bomb.Advance(.5f);
            Assert.That(bomb.DidExplode, Is.True); Assert.That(bomb.transform.position.x, Is.EqualTo(4).Within(.01f));
        }
        [Test] public void PauseAndSourceDeathDoNotDetonate()
        {
            var bomb = Bomb(Enemy(4)); Time.timeScale = 0; bomb.Advance(10);
            Assert.That(bomb.HasEnded, Is.False); Assert.That(bomb.transform.position, Is.EqualTo(Vector3.up));
            var damage = new DamageInfo(1000, null, Vector3.zero, Vector3.up); _player.ApplyDamage(in damage);
            bomb.Advance(0); Assert.That(bomb.HasEnded, Is.True); Assert.That(bomb.DidExplode, Is.False);
        }
        [Test] public void EmptyTargetDoesNotConsumeCooldownAndTwoBombsShareTarget()
        {
            var entry = new SniperRuntimeFactory(_registry, _player.gameObject, new PlayerStatCollection()).Create(_definition);
            Assert.That(entry.TryAutoCast(Vector3.up), Is.EqualTo(AbilityAutoCastResult.NoTarget));
            Assert.That(entry.State.CooldownRemaining, Is.Zero);
            var enemy = Enemy(5);
            var advance = typeof(AbilityRuntimeEntry).GetMethod("AdvanceLevel", BindingFlags.Instance | BindingFlags.NonPublic);
            while (entry.Level < 5) advance.Invoke(entry, null);
            Physics.SyncTransforms(); entry.TryAutoCast(Vector3.up);
            var bombs = Object.FindObjectsByType<SniperProjectile>(FindObjectsSortMode.None);
            Assert.That(bombs.Length, Is.EqualTo(2));
            foreach (var bomb in bombs) { Assert.That(bomb.Target, Is.EqualTo(enemy)); bomb.Advance(1); }
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(860));
            entry.BindEnemyRegistry(Make("Next registry").AddComponent<EnemyRegistry>());
        }
    }
}
#endif
