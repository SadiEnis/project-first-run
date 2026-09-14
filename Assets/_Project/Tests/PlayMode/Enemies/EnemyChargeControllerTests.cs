using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Presentation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace ProjectFirstRun.Tests.PlayMode.Enemies
{
    public sealed class EnemyChargeControllerTests
    {
        private readonly List<Object> _objects = new List<Object>();
        private NavMeshDataInstance _mesh;
        private EnemyDefinition _definition;
        private EnemyController _enemy;
        private EnemyAttackController _attack;
        private EnemyMotor _motor;
        private EnemyRegistry _registry;
        private HealthComponent _target;

        private GameObject Make(string name)
        {
            var go = new GameObject(name); _objects.Add(go); return go;
        }

        [SetUp]
        public void SetUp()
        {
            var data = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByID(0),
                new List<NavMeshBuildSource> { new NavMeshBuildSource {
                    shape = NavMeshBuildSourceShape.Box, size = new Vector3(20, .2f, 20),
                    transform = Matrix4x4.TRS(new Vector3(0, -.1f, 0), Quaternion.identity, Vector3.one), area = 0
                } }, new Bounds(Vector3.zero, new Vector3(20, 5, 20)), Vector3.zero, Quaternion.identity);
            _objects.Add(data); _mesh = NavMesh.AddNavMeshData(data);
            _registry = Make("Registry").AddComponent<EnemyRegistry>();
            _target = Make("Target").AddComponent<HealthComponent>();
            _target.Initialize(100); _target.transform.position = Vector3.forward * 3;
            var go = Make("Charger"); go.AddComponent<NavMeshAgent>();
            _enemy = go.AddComponent<EnemyController>(); _motor = go.GetComponent<EnemyMotor>();
            _attack = go.AddComponent<EnemyAttackController>();
            _definition = ScriptableObject.CreateInstance<EnemyDefinition>(); _objects.Add(_definition);
            typeof(EnemyDefinition).GetField("_behavior", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(_definition, EnemyBehavior.Charger);
            _enemy.Initialize(_definition, _target.transform, _registry);
            _attack.Initialize(_definition, _target.transform, _target);
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1;
            for (int i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
            _objects.Clear();
            if (_mesh.valid) _mesh.Remove();
        }

        private void BeginCharge()
        {
            _attack.Tick(.01f);
            Assert.That(_attack.ChargeState.Phase, Is.EqualTo(EnemyChargePhase.Windup));
            _attack.Tick(.8f);
            Assert.That(_attack.ChargeState.Phase, Is.EqualTo(EnemyChargePhase.Charging));
        }

        [Test]
        public void SweptCharge_HitsOnceEvenWhenOneStepPassesBeyondTarget()
        {
            int hits = 0; _attack.AttackPerformed += (_, _) => hits++;
            BeginCharge(); _attack.Tick(.5f);
            Assert.That(_target.CurrentHealth, Is.EqualTo(90));
            _target.transform.position = Vector3.forward * 6;
            _attack.Tick(.2f);
            Assert.That(hits, Is.EqualTo(1));
            _attack.Tick(.5f);
            Assert.That(_attack.ChargeState.Phase, Is.EqualTo(EnemyChargePhase.Recovery));
            Assert.That(_enemy.transform.position.z, Is.InRange(7.9f, 8.1f));
        }

        [Test]
        public void DodgeDuringWarning_DoesNotRetargetCharge()
        {
            _attack.Tick(.01f);
            _target.transform.position += Vector3.right * 4;
            _attack.Tick(.8f); _attack.Tick(.8f);
            Assert.That(_target.CurrentHealth, Is.EqualTo(100));
            Assert.That(_enemy.transform.position.x, Is.EqualTo(0).Within(.05f));
            Assert.That(_attack.ChargeState.Direction, Is.EqualTo(Vector3.forward));
        }

        [Test]
        public void SolidWall_StopsChargeBeforeTarget()
        {
            var wall = Make("Wall"); wall.transform.position = new Vector3(0, 1, 1.5f);
            wall.AddComponent<BoxCollider>().size = new Vector3(5, 2, .3f);
            Physics.SyncTransforms();
            BeginCharge(); _attack.Tick(1);
            Assert.That(_enemy.transform.position.z, Is.LessThan(1.3f));
            Assert.That(_attack.ChargeState.Phase, Is.EqualTo(EnemyChargePhase.Recovery));
            Assert.That(_target.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void NavMeshEdge_StopsChargeWithoutLeavingWalkableGround()
        {
            _enemy.GetComponent<NavMeshAgent>().Warp(Vector3.forward * 7);
            _target.transform.position = Vector3.forward * 9;
            BeginCharge(); _attack.Tick(1);
            Assert.That(_enemy.transform.position.z, Is.LessThan(10));
            Assert.That(_motor.CanNavigate, Is.True);
            Assert.That(_attack.ChargeState.Phase, Is.EqualTo(EnemyChargePhase.Recovery));
        }

        [Test]
        public void DeathDuringCharge_CancelsDamageAndUnregistersOnce()
        {
            int deaths = 0; _enemy.Died += (_, _, _) => deaths++;
            BeginCharge();
            var damage = new DamageInfo(1000, null, Vector3.zero, Vector3.forward);
            _enemy.Health.ApplyDamage(in damage); _enemy.Health.ApplyDamage(in damage);
            _attack.Tick(1);
            Assert.That(deaths, Is.EqualTo(1));
            Assert.That(_registry.ActiveCount, Is.Zero);
            Assert.That(_target.CurrentHealth, Is.EqualTo(100));
            Assert.That(_motor.IsMovementEnabled, Is.False);
        }

        [Test]
        public void DisableAndEnable_RequiresFreshWindup()
        {
            BeginCharge(); _enemy.gameObject.SetActive(false); _enemy.gameObject.SetActive(true);
            _attack.Tick(.01f);
            Assert.That(_attack.ChargeState.Phase, Is.EqualTo(EnemyChargePhase.Windup));
            Assert.That(_target.CurrentHealth, Is.EqualTo(100));
            Assert.That(_attack.ChargeState.TimeRemaining, Is.EqualTo(.8f));
        }

        [Test]
        public void StopAndResume_RequiresFreshWindup()
        {
            BeginCharge(); _attack.Stop(); _attack.Tick(1); _attack.Resume(); _attack.Tick(.01f);
            Assert.That(_attack.ChargeState.Phase, Is.EqualTo(EnemyChargePhase.Windup));
            Assert.That(_target.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void InactiveTarget_CancelsCommittedAttack()
        {
            BeginCharge(); _target.gameObject.SetActive(false); _attack.Tick(1);
            Assert.That(_attack.ChargeState.Phase, Is.EqualTo(EnemyChargePhase.Pursuing));
            Assert.That(_motor.IsMovementEnabled, Is.False);
            _target.gameObject.SetActive(true); _attack.Tick(.01f);
            Assert.That(_attack.ChargeState.Phase, Is.EqualTo(EnemyChargePhase.Windup));
        }

        [Test]
        public void DestroyedTarget_CancelsWithoutException()
        {
            BeginCharge(); Object.DestroyImmediate(_target.gameObject);
            Assert.DoesNotThrow(() => _attack.Tick(1));
            Assert.That(_motor.IsMovementEnabled, Is.False);
        }

        [Test]
        public void DeadTarget_CancelsWithoutAnotherAttack()
        {
            BeginCharge(); var damage = new DamageInfo(1000, null, Vector3.zero, Vector3.forward);
            _target.ApplyDamage(in damage); int hits = 0; _attack.AttackPerformed += (_, _) => hits++;
            _attack.Tick(1);
            Assert.That(hits, Is.Zero);
            Assert.That(_motor.IsMovementEnabled, Is.False);
        }

        [UnityTest]
        public IEnumerator TimeScaleZero_SuspendsChargeAcrossFrames()
        {
            BeginCharge(); Time.timeScale = 0;
            var position = _enemy.transform.position;
            yield return null; yield return null;
            Assert.That(_target.CurrentHealth, Is.EqualTo(100));
            Assert.That(_enemy.transform.position, Is.EqualTo(position));
            Assert.That(_attack.ChargeState.Phase, Is.EqualTo(EnemyChargePhase.Charging));
        }

        [UnityTest]
        public IEnumerator Warning_IsVisibleDuringWindup_AndLabelFacesCamera()
        {
            var camera = Make("Viewing camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.rotation = Quaternion.Euler(10, 45, 0);
            _enemy.gameObject.AddComponent<EnemyChargeView>();
            _attack.Tick(.01f);
            Time.timeScale = 0;
            yield return null; yield return null;
            var line = _enemy.GetComponentInChildren<LineRenderer>();
            var label = _enemy.GetComponentInChildren<TextMesh>();
            Assert.That(line.enabled, Is.True);
            Assert.That(Vector3.Distance(line.GetPosition(1) - line.GetPosition(0), Vector3.forward * 8), Is.LessThan(.01f));
            Assert.That(Quaternion.Angle(label.transform.rotation, camera.transform.rotation), Is.LessThan(.1f));
            _attack.Stop();
            yield return null;
            Assert.That(line.enabled, Is.False);
        }
    }
}
