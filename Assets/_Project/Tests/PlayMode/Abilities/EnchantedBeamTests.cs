#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.EnchantedStaff;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
namespace ProjectFirstRun.Tests.PlayMode.Abilities
{
    public sealed class EnchantedBeamTests
    {
        private readonly List<Object> _objects = new List<Object>();
        private NavMeshDataInstance _mesh;
        private EnemyRegistry _registry;
        private HealthComponent _player;
        private EnchantedDefinition _definition;
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
            _definition = AssetDatabase.LoadAssetAtPath<EnchantedDefinition>("Assets/_Project/Data/Abilities/AD_EnchantedStaff.asset");
        }
        private EnemyController Enemy(float x)
        {
            var go = Make("Target"); go.transform.position = Vector3.right * x; go.AddComponent<NavMeshAgent>();
            var enemy = go.AddComponent<EnemyController>();
            var def = ScriptableObject.CreateInstance<EnemyDefinition>(); _objects.Add(def);
            enemy.Initialize(def, _player.transform, _registry); enemy.Health.Initialize(1000); go.GetComponent<EnemyMotor>().Stop();
            var collider = go.AddComponent<BoxCollider>(); collider.center = Vector3.up; collider.size = new Vector3(.2f, 2, .2f);
            return enemy;
        }
        private void Wall(float x)
        {
            var go = Make("Wall"); go.transform.position = new Vector3(x, 1, 0);
            go.AddComponent<BoxCollider>().size = new Vector3(.2f, 4, 10);
        }
        private EnchantedBeam Beam()
        {
            Physics.SyncTransforms();
            return EnchantedBeam.Launch(_player.gameObject, _registry, _definition,
                _definition.CreateLevelConfigs()[0], 20, Vector3.up, Vector3.right);
        }
        [TearDown] public void Cleanup()
        {
            Time.timeScale = 1;
            foreach (var beam in Object.FindObjectsByType<EnchantedBeam>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Object.DestroyImmediate(beam.gameObject);
            for (int i = _objects.Count - 1; i >= 0; i--) if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
            _objects.Clear(); if (_mesh.valid) _mesh.Remove();
        }
        [Test] public void PiercesMultipleEnemiesAndDeduplicatesColliders()
        {
            var a = Enemy(1); var b = Enemy(2); a.gameObject.AddComponent<SphereCollider>();
            Beam().Advance(.4f);
            Assert.That(a.Health.CurrentHealth, Is.EqualTo(980)); Assert.That(b.Health.CurrentHealth, Is.EqualTo(980));
        }
        [Test] public void WallReflectsWithoutDamagingEnemyBehindIt()
        {
            var behind = Enemy(3); Wall(2); var beam = Beam(); beam.Advance(.4f);
            Assert.That(beam.BounceCount, Is.GreaterThan(0));
            Assert.That(beam.Direction.x, Is.LessThan(0));
            Assert.That(behind.Health.CurrentHealth, Is.EqualTo(1000));
        }
        [Test] public void RicochetDoesNotResetRepeatHitProtection()
        {
            var enemy = Enemy(1); Wall(2); var beam = Beam(); beam.Advance(.4f);
            Assert.That(beam.BounceCount, Is.EqualTo(1));
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(980));
        }
        [Test] public void LaterReturnCanHitAgain()
        {
            var enemy = Enemy(1); Wall(4); var beam = Beam(); beam.Advance(.9f);
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(960));
        }
        [Test] public void BeamsHaveIndependentHitProtection()
        {
            var enemy = Enemy(1); Beam().Advance(.2f); Beam().Advance(.2f);
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(960));
        }
        [Test] public void InitialSolidOverlapCancelsBeam()
        {
            Wall(0); var beam = Beam(); Assert.That(beam.HasEnded, Is.True);
        }
        [Test] public void TrailRecordsRicochetAndDisappearsWithBeam()
        {
            Wall(2); var beam = Beam(); beam.Advance(.4f);
            var trail = beam.transform.Find("Beam path").GetComponent<LineRenderer>();
            Assert.That(trail.positionCount, Is.GreaterThan(2));
            Assert.That(trail.GetPosition(0), Is.EqualTo(Vector3.up));
            float furthestX = 0;
            for (int i = 0; i < trail.positionCount; i++)
                furthestX = Mathf.Max(furthestX, trail.GetPosition(i).x);
            Assert.That(furthestX, Is.GreaterThan(beam.transform.position.x + .5f));
            Assert.That(trail.GetPosition(trail.positionCount - 1), Is.EqualTo(beam.transform.position));
            Time.timeScale = 0;
            int count = trail.positionCount; beam.Advance(1);
            Assert.That(trail.positionCount, Is.EqualTo(count));
            beam.End();
            Assert.That(trail.gameObject.activeInHierarchy, Is.False);
        }
        [Test] public void PauseFreezesAndLifetimeExpires()
        {
            var beam = Beam(); var position = beam.transform.position;
            Time.timeScale = 0; beam.Advance(5);
            Assert.That(beam.transform.position, Is.EqualTo(position)); Assert.That(beam.HasEnded, Is.False);
            Time.timeScale = 1; beam.Advance(2); Assert.That(beam.HasEnded, Is.True);
        }
        [Test] public void SourceDeathClearsBeam()
        {
            var beam = Beam(); var damage = new DamageInfo(1000, null, Vector3.zero, Vector3.up);
            _player.ApplyDamage(in damage); beam.Advance(0); Assert.That(beam.HasEnded, Is.True);
        }
        [Test] public void RebindClearsOwnedBeamsAndUpgradePreservesCooldown()
        {
            var entry = new EnchantedRuntimeFactory(_registry, _player.gameObject, new PlayerStatCollection()).Create(_definition);
            entry.TryAutoCast(Vector3.up); entry.Tick(1);
            float remaining = entry.State.CooldownRemaining;
            var advance = typeof(AbilityRuntimeEntry).GetMethod("AdvanceLevel", BindingFlags.Instance | BindingFlags.NonPublic);
            while (entry.Level < 8) advance.Invoke(entry, null);
            Assert.That(entry.State.CooldownRemaining, Is.EqualTo(remaining));
            var beams = Object.FindObjectsByType<EnchantedBeam>(FindObjectsSortMode.None);
            Assert.That(beams.Length, Is.EqualTo(1));
            entry.BindEnemyRegistry(Make("Next registry").AddComponent<EnemyRegistry>());
            Assert.That(beams[0].HasEnded, Is.True);
        }
    }
}
#endif
