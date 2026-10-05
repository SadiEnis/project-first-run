using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Abilities.Fireball;
using ProjectFirstRun.Abilities.Shuriken;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace ProjectFirstRun.Tests.PlayMode.Enemies
{
    public sealed class EnemyPerceptionTests
    {
        private readonly List<Object> _objects = new List<Object>();
        private NavMeshDataInstance _mesh;
        private EnemyDefinition _definition;
        private EnemyPerceptionProfile _profile;
        private EnemyController _enemy;
        private EnemyAttackController _attack;
        private EnemyMotor _motor;
        private HealthComponent _target;
        private EnemyRegistry _registry;
        private EnemyPerceptionController Perception => _enemy.Perception;

        private GameObject Make(string name)
        {
            var go = new GameObject(name); _objects.Add(go); return go;
        }

        [SetUp]
        public void SetUp()
        {
            var data = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByID(0), new List<NavMeshBuildSource>
            { new NavMeshBuildSource { shape = NavMeshBuildSourceShape.Box, size = new Vector3(80, .2f, 80),
                transform = Matrix4x4.TRS(new Vector3(0, -.1f, 0), Quaternion.identity, Vector3.one), area = 0 } },
                new Bounds(Vector3.zero, new Vector3(80, 5, 80)), Vector3.zero, Quaternion.identity);
            _objects.Add(data); _mesh = NavMesh.AddNavMeshData(data);
            var target = Make("Player"); target.transform.position = Vector3.forward * 7;
            _target = target.AddComponent<HealthComponent>(); _target.Initialize(100);
            var body = target.AddComponent<BoxCollider>(); body.center = Vector3.up;
            _registry = Make("Registry").AddComponent<EnemyRegistry>();
            var enemy = Make("Perception enemy"); enemy.AddComponent<NavMeshAgent>();
            _enemy = enemy.AddComponent<EnemyController>();
            _motor = enemy.GetComponent<EnemyMotor>();
            _attack = enemy.AddComponent<EnemyAttackController>();
            _definition = ScriptableObject.CreateInstance<EnemyDefinition>(); _objects.Add(_definition);
            _profile = ScriptableObject.CreateInstance<EnemyPerceptionProfile>(); _objects.Add(_profile);
            _enemy.ConfigurePerception(_profile);
            Initialize(EnemyBehavior.Chaser);
            Physics.SyncTransforms();
        }

        private void Initialize(EnemyBehavior behavior)
        {
            typeof(EnemyDefinition).GetField("_behavior", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_definition, behavior);
            _enemy.Initialize(_definition, _target.transform, _registry);
            _attack.Initialize(_definition, _target.transform, _target);
        }

        private void MoveTarget(Vector3 position)
        {
            _target.transform.position = position; Physics.SyncTransforms();
        }

        private GameObject Wall()
        {
            var wall = Make("Sight blocker"); wall.transform.position = new Vector3(0, 1, 3);
            wall.AddComponent<BoxCollider>().size = new Vector3(20, 4, .4f);
            Physics.SyncTransforms(); return wall;
        }

        private void Damage(GameObject source, float amount = 1) =>
            _enemy.Health.ApplyDamage(new DamageInfo(amount, source, _enemy.transform.position, Vector3.forward));

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1;
            foreach (var projectile in Object.FindObjectsByType<EnemyRangedProjectile>(FindObjectsSortMode.None))
                Object.DestroyImmediate(projectile.gameObject);
            for (int i = _objects.Count - 1; i >= 0; i--) if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
            _objects.Clear(); if (_mesh.valid) _mesh.Remove();
        }

        [Test]
        public void InitialMotorIsStoppedUntilDetectionAndRegistryMembershipRemains()
        {
            Assert.That(_motor.IsMovementEnabled, Is.False);
            MoveTarget(Vector3.back * 7);
            _attack.Tick(.1f);
            Assert.That(Perception.State.Awareness, Is.EqualTo(EnemyAwareness.Idle));
            Assert.That(_motor.IsMovementEnabled, Is.False);
            Assert.That(_registry.ActiveCount, Is.EqualTo(1));
            MoveTarget(Vector3.forward * 7); _attack.Tick(.1f);
            Assert.That(Perception.HasSight, Is.True);
            Assert.That(_motor.IsMovementEnabled, Is.True);
        }

        [Test]
        public void AlertTrackingIgnoresConeButNotWiderRange()
        {
            _attack.Tick(.1f);
            MoveTarget(Vector3.back * 22); _attack.Tick(.1f);
            Assert.That(Perception.HasSight, Is.True);
            MoveTarget(Vector3.back * 25); _attack.Tick(.1f);
            Assert.That(Perception.HasSight, Is.False);
            Assert.That(Perception.State.LastKnownPosition, Is.EqualTo(Vector3.back * 22));
        }

        [Test]
        public void WallBlocksButTriggersAndOwnOrTargetCollidersDoNot()
        {
            var self = Make("Self collider"); self.transform.SetParent(_enemy.transform);
            self.transform.localPosition = new Vector3(0, 1, .5f); self.AddComponent<BoxCollider>();
            var wall = Wall();
            _attack.Tick(.1f); Assert.That(Perception.HasSight, Is.False);
            wall.GetComponent<BoxCollider>().isTrigger = true; Physics.SyncTransforms();
            _attack.Tick(.1f); Assert.That(Perception.HasSight, Is.True);
        }

        [UnityTest]
        public IEnumerator HiddenPursuitUsesFrozenSnapshotAndReacquisitionResumesLiveDestination()
        {
            // Advance AI explicitly while letting Unity complete asynchronous navigation jobs.
            _attack.enabled = false;
            _attack.Resume();
            var agent = _enemy.GetComponent<NavMeshAgent>();
            // This test measures destination ownership, not locomotion. Keep the sensing
            // origin fixed while navigation jobs span frames on slower editor machines.
            agent.speed = 0f;
            _attack.Tick(.1f);
            yield return WaitForPath(agent);
            var wall = Wall();
            MoveTarget(new Vector3(5, 0, 8)); _attack.Tick(.1f);
            Assert.That(Perception.State.Awareness, Is.EqualTo(EnemyAwareness.Investigating));
            Assert.That(Perception.State.LastKnownPosition, Is.EqualTo(Vector3.forward * 7));
            yield return WaitForPath(agent);
            Assert.That(agent.destination.x, Is.EqualTo(0).Within(.1f));
            Assert.That(agent.destination.z, Is.EqualTo(7).Within(.1f));
            Damage(_target.gameObject);
            MoveTarget(new Vector3(6, 0, 9)); Damage(_target.gameObject); _attack.Tick(.1f);
            Assert.That(Perception.State.LastKnownPosition, Is.EqualTo(Vector3.forward * 7));
            Assert.That(agent.destination.x, Is.EqualTo(0).Within(.1f));
            Assert.That(agent.destination.z, Is.EqualTo(7).Within(.1f));
            wall.SetActive(false); _attack.Tick(.1f);
            Assert.That(Perception.HasSight, Is.True);
            yield return WaitForPath(agent);
            Assert.That(agent.destination.x, Is.EqualTo(6).Within(.1f));
            Assert.That(agent.destination.z, Is.EqualTo(9).Within(.1f));
        }

        private static IEnumerator WaitForPath(NavMeshAgent agent)
        {
            yield return null;
            for (int frame = 0; frame < 120 && agent.pathPending; frame++) yield return null;
            Assert.That(agent.pathPending, Is.False, "Navigation request did not finish within 120 frames.");
            Assert.That(agent.hasPath, Is.True, "The unobstructed test NavMesh must produce a path.");
            Assert.That(agent.pathStatus, Is.EqualTo(NavMeshPathStatus.PathComplete));
        }

        [Test]
        public void OwnedDamageOutsideConeAndRangeAlarmsOnceButDoesNotAllowAttack()
        {
            MoveTarget(Vector3.back * 30);
            Damage(null); Assert.That(Perception.State.IsAlerted, Is.False);
            Damage(Make("Environment")); Assert.That(Perception.State.IsAlerted, Is.False);
            var child = Make("Owned source"); child.transform.SetParent(_target.transform);
            Damage(child);
            Assert.That(Perception.State.LastKnownPosition, Is.EqualTo(Vector3.back * 30));
            Assert.That(Perception.HasSight, Is.False);
            MoveTarget(Vector3.right * 30); Damage(child); _attack.Tick(.1f);
            Assert.That(Perception.State.LastKnownPosition, Is.EqualTo(Vector3.back * 30));
            Assert.That(_target.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void MemoryExpiresWithoutResettingHealthAndPauseDoesNotAdvanceIt()
        {
            MoveTarget(Vector3.back * 30); Damage(_target.gameObject);
            float health = _enemy.Health.CurrentHealth;
            _attack.Tick(0); Assert.That(Perception.State.MemoryRemaining, Is.EqualTo(3));
            _attack.Tick(3.1f);
            Assert.That(Perception.State.IsAlerted, Is.False);
            Assert.That(_motor.IsMovementEnabled, Is.False);
            Assert.That(_enemy.Health.CurrentHealth, Is.EqualTo(health));
        }

        [Test]
        public void RealBurnAndBleedTicksRefreshAlarmWithoutUpdatingHiddenPosition()
        {
            MoveTarget(Vector3.back * 30);
            FireballBurn.Apply(_target.gameObject, _enemy.Health, null, 1, 3);
            var burn = _enemy.GetComponent<FireballBurn>();
            burn.Advance(1);
            Assert.That(Perception.State.LastKnownPosition, Is.EqualTo(Vector3.back * 30));
            MoveTarget(Vector3.right * 30);
            burn.Advance(1);
            ShurikenBleed.Apply(_target.gameObject, _enemy, 1);
            _enemy.GetComponent<ShurikenBleed>().Advance(1);
            Assert.That(Perception.State.LastKnownPosition, Is.EqualTo(Vector3.back * 30));
            Assert.That(Perception.State.MemoryRemaining, Is.EqualTo(3));
        }

        [Test]
        public void ArrivingAtLastPointWaitsWithoutReissuingPathOrForgettingEarly()
        {
            MoveTarget(Vector3.back); Damage(_target.gameObject);
            // An alerted enemy tracks without the acquisition cone. Keep the real player
            // outside tracking range so this measures investigation, not visual reacquisition.
            MoveTarget(Vector3.back * 30);
            _attack.Tick(.1f);
            Assert.That(Perception.State.Awareness, Is.EqualTo(EnemyAwareness.Investigating));
            Assert.That(Perception.State.LastKnownPosition, Is.EqualTo(Vector3.back));
            Assert.That(_motor.IsMovementEnabled, Is.False);
            _attack.Tick(.1f);
            Assert.That(_motor.IsMovementEnabled, Is.False);
            Assert.That(Perception.State.MemoryRemaining, Is.GreaterThan(2));
        }

        [Test]
        public void AttackBoundaryRechecksWallEvenBetweenSightSamples()
        {
            _attack.Tick(.1f);
            MoveTarget(Vector3.forward);
            var wall = Wall(); wall.transform.position = new Vector3(0, 1, .5f);
            Physics.SyncTransforms(); _attack.Tick(.001f);
            Assert.That(_target.CurrentHealth, Is.EqualTo(100));
            Assert.That(Perception.HasSight, Is.False);
        }

        [Test]
        public void DisableReenableAndReinitializeClearAwarenessAndFailClosed()
        {
            _attack.Tick(.1f);
            Perception.enabled = false; _attack.Tick(.1f);
            Assert.That(_motor.IsMovementEnabled, Is.False);
            MoveTarget(Vector3.back * 7);
            Perception.enabled = true; _attack.Tick(.1f);
            Assert.That(Perception.State.IsAlerted, Is.False);
            Damage(_target.gameObject); _enemy.enabled = false;
            Damage(_target.gameObject);
            Assert.That(Perception.State.IsAlerted, Is.False);
            _enemy.enabled = true;
            Assert.That(_motor.IsMovementEnabled, Is.False);
            Damage(_target.gameObject); Initialize(EnemyBehavior.Chaser);
            Assert.That(Perception.State.IsAlerted, Is.False);
        }

        [Test]
        public void PlayerDeathAndLethalEnemyDamageClearPermissions()
        {
            _attack.Tick(.1f);
            _target.ApplyDamage(new DamageInfo(1000, null, Vector3.zero, Vector3.zero));
            _attack.Tick(.1f);
            Assert.That(_motor.IsMovementEnabled, Is.False);
            Assert.That(Perception.State.IsAlerted, Is.False);
            _target.Initialize(100); _attack.Tick(.1f);
            Damage(_target.gameObject, 1000);
            Assert.That(Perception.State.IsAlerted, Is.False);
            Assert.That(_motor.IsMovementEnabled, Is.False);
        }

        [Test]
        public void ChaserCannotStrikeThroughWallOrAcrossFloors()
        {
            MoveTarget(Vector3.up * 4); _attack.Tick(.1f);
            Assert.That(Perception.HasSight, Is.True);
            Assert.That(_target.CurrentHealth, Is.EqualTo(100));
            var floor = Make("Floor between enemies"); floor.transform.position = Vector3.up * 3;
            floor.AddComponent<BoxCollider>().size = new Vector3(6, .2f, 6); Physics.SyncTransforms();
            _attack.Tick(.1f); Assert.That(Perception.HasSight, Is.False);
            floor.SetActive(false);
            MoveTarget(Vector3.forward); var wall = Wall(); wall.transform.position = new Vector3(0, 1, .5f);
            Physics.SyncTransforms(); _attack.Tick(.1f);
            Assert.That(_target.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void RangerCancelsUnreleasedShotAndPreservesExistingCooldown()
        {
            Initialize(EnemyBehavior.Ranger); _attack.Tick(.1f);
            Assert.That(_attack.RangedState.Phase, Is.EqualTo(EnemyRangedPhase.Windup));
            var wall = Wall(); _attack.Tick(.1f);
            Assert.That(_attack.RangedState.Phase, Is.Not.EqualTo(EnemyRangedPhase.Windup));
            Assert.That(Object.FindFirstObjectByType<EnemyRangedProjectile>(), Is.Null);
            wall.SetActive(false); _attack.Tick(.1f); _attack.Tick(.7f);
            Assert.That(_attack.RangedState.Phase, Is.EqualTo(EnemyRangedPhase.Cooldown));
            float cooldown = _attack.RangedState.CooldownRemaining;
            wall.SetActive(true); Physics.SyncTransforms(); _attack.Tick(.1f);
            Assert.That(_attack.RangedState.CooldownRemaining, Is.EqualTo(cooldown - .1f).Within(.001f));
            Assert.That(Object.FindFirstObjectByType<EnemyRangedProjectile>(), Is.Not.Null);
        }

        [Test]
        public void ChargerCancelsWindupButFinishesCommittedDirectionAndRecovery()
        {
            Initialize(EnemyBehavior.Charger); _attack.Tick(.1f);
            Assert.That(_attack.ChargeState.Phase, Is.EqualTo(EnemyChargePhase.Windup));
            var wall = Wall(); _attack.Tick(.1f);
            Assert.That(_attack.ChargeState.Phase, Is.EqualTo(EnemyChargePhase.Pursuing));
            wall.SetActive(false); _attack.Tick(.1f); _attack.Tick(.9f);
            Assert.That(_attack.ChargeState.Phase, Is.EqualTo(EnemyChargePhase.Charging));
            Vector3 direction = _attack.ChargeState.Direction;
            MoveTarget(new Vector3(30, 0, 30)); _attack.Tick(.9f);
            Assert.That(_attack.ChargeState.Direction, Is.EqualTo(direction));
            Assert.That(_attack.ChargeState.Phase, Is.EqualTo(EnemyChargePhase.Recovery));
            _attack.Tick(1.3f);
            Assert.That(_attack.ChargeState.Phase, Is.EqualTo(EnemyChargePhase.Pursuing));
            Assert.That(Perception.HasSight, Is.False);
        }

        [Test]
        public void StunDoesNotGrantAFreeAttackOrResetCooldown()
        {
            MoveTarget(Vector3.forward); _attack.Tick(.1f);
            Assert.That(_target.CurrentHealth, Is.LessThan(100));
            float cooldown = _attack.CooldownRemaining;
            _motor.ApplyStun(1); _attack.Tick(.5f);
            Assert.That(_attack.CooldownRemaining, Is.EqualTo(cooldown));
            _motor.ClearStun(); _attack.Tick(.1f);
            Assert.That(_attack.CooldownRemaining, Is.EqualTo(cooldown - .1f).Within(.001f));
        }

        [Test]
        public void EnemyWithoutProfileStillPursuesAutomatically()
        {
            var go = Make("Legacy"); go.transform.position = Vector3.right * 10;
            go.AddComponent<NavMeshAgent>(); var enemy = go.AddComponent<EnemyController>();
            enemy.Initialize(_definition, _target.transform, _registry);
            Assert.That(enemy.RequiresPerception, Is.False);
            Assert.That(go.GetComponent<EnemyMotor>().IsMovementEnabled, Is.True);
        }
    }
}
