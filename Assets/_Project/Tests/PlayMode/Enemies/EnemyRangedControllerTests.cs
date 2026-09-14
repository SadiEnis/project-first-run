using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace ProjectFirstRun.Tests.PlayMode.Enemies
{
    public sealed class EnemyRangedControllerTests
    {
        private readonly List<Object> _objects = new List<Object>();
        private NavMeshDataInstance _mesh;
        private EnemyDefinition _definition;
        private EnemyController _enemy;
        private EnemyAttackController _attack;
        private EnemyMotor _motor;
        private HealthComponent _target;

        private GameObject Make(string name)
        {
            var go = new GameObject(name); _objects.Add(go); return go;
        }

        [SetUp]
        public void SetUp()
        {
            var data = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByID(0), new List<NavMeshBuildSource>
            { new NavMeshBuildSource { shape = NavMeshBuildSourceShape.Box, size = new Vector3(30, .2f, 30),
                transform = Matrix4x4.TRS(new Vector3(0, -.1f, 0), Quaternion.identity, Vector3.one), area = 0 } },
                new Bounds(Vector3.zero, new Vector3(30, 5, 30)), Vector3.zero, Quaternion.identity);
            _objects.Add(data); _mesh = NavMesh.AddNavMeshData(data);
            var targetObject = Make("Target"); targetObject.transform.position = Vector3.forward * 7f;
            _target = targetObject.AddComponent<HealthComponent>(); _target.Initialize(100);
            var collider = targetObject.AddComponent<BoxCollider>(); collider.center = Vector3.up; collider.size = Vector3.one;
            var enemyObject = Make("Ranger"); enemyObject.AddComponent<NavMeshAgent>();
            _enemy = enemyObject.AddComponent<EnemyController>(); _motor = enemyObject.GetComponent<EnemyMotor>();
            _attack = enemyObject.AddComponent<EnemyAttackController>();
            _definition = ScriptableObject.CreateInstance<EnemyDefinition>(); _objects.Add(_definition);
            SetField("_behavior", EnemyBehavior.Ranger); SetField("_attackDamage", 10f);
            _enemy.Initialize(_definition, targetObject.transform, Make("Registry").AddComponent<EnemyRegistry>());
            _attack.Initialize(_definition, targetObject.transform, _target);
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            foreach (var projectile in Object.FindObjectsByType<EnemyRangedProjectile>(FindObjectsSortMode.None))
                Object.DestroyImmediate(projectile.gameObject);
            foreach (var item in _objects) if (item != null) Object.DestroyImmediate(item);
            _objects.Clear(); if (_mesh.valid) _mesh.Remove();
        }

        private void SetField(string name, object value) => _definition.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_definition, value);
        private void StartWindup() { _attack.Tick(.01f); Assert.That(_attack.RangedState.Phase, Is.EqualTo(EnemyRangedPhase.Windup)); }

        [Test]
        public void RangerHoldsPreferredBandAndFiresProjectileAfterWarning()
        {
            StartWindup(); _attack.Tick(.6f);
            Assert.That(_attack.RangedState.IsReadyToRelease, Is.False);
            var projectile = Object.FindFirstObjectByType<EnemyRangedProjectile>();
            Assert.That(projectile, Is.Not.Null);
            Assert.That(_target.CurrentHealth, Is.EqualTo(100));
            Assert.That(_motor.IsMovementEnabled, Is.False);
        }

        [UnityTest]
        public IEnumerator StraightProjectileDamagesTargetOnceAndExpires()
        {
            StartWindup(); _attack.Tick(.6f); yield return new WaitForSeconds(1.1f);
            Assert.That(_target.CurrentHealth, Is.EqualTo(90));
            Assert.That(Object.FindFirstObjectByType<EnemyRangedProjectile>(), Is.Null);
        }

        [Test]
        public void WallBlocksWindupAndNoProjectileIsReleased()
        {
            var wall = Make("Wall"); wall.transform.position = new Vector3(0, 1, 3.5f); var box = wall.AddComponent<BoxCollider>(); box.size = new Vector3(4, 2, .3f); Physics.SyncTransforms();
            _attack.Tick(.01f); Assert.That(_attack.RangedState.Phase, Is.EqualTo(EnemyRangedPhase.Holding)); _attack.Tick(.6f);
            Assert.That(Object.FindFirstObjectByType<EnemyRangedProjectile>(), Is.Null);
            Assert.That(_target.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void TargetMovesDuringWindup_RangerKeepsLockedDirection()
        {
            StartWindup(); _target.transform.position += Vector3.right * 4f; Physics.SyncTransforms(); _attack.Tick(.6f);
            Assert.That(Object.FindFirstObjectByType<EnemyRangedProjectile>(), Is.Not.Null);
            Assert.That(_target.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void CloseTarget_RangerRetreatsWithinNavigation()
        {
            _target.transform.position = Vector3.forward * 2f; Physics.SyncTransforms(); _attack.Tick(.1f);
            Assert.That(_attack.RangedState.Phase, Is.EqualTo(EnemyRangedPhase.Approaching));
            Assert.That(_motor.IsMovementEnabled, Is.True);
            Assert.That(_motor.Target, Is.EqualTo(_target.transform));
        }

        [UnityTest]
        public IEnumerator PauseFreezesWindupAndProjectileDamage()
        {
            StartWindup(); Time.timeScale = 0f; yield return null; yield return null;
            Assert.That(_attack.RangedState.TimeRemaining, Is.EqualTo(.6f).Within(.03f));
            Assert.That(_target.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void DeathCancelsRangedAttack()
        {
            StartWindup(); var damage = new DamageInfo(1000, null, Vector3.zero, Vector3.forward); _enemy.Health.ApplyDamage(in damage); _attack.Tick(1);
            Assert.That(Object.FindFirstObjectByType<EnemyRangedProjectile>(), Is.Null);
            Assert.That(_target.CurrentHealth, Is.EqualTo(100));
        }
    }
}
