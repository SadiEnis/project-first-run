using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Combat;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class RegionPassageTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private RegionPassageController _passage;
        private Transform _player;
        private Collider _body, _blocker;
        private HealthComponent _health;
        private BoxCollider _volume;
        private int _completed;
        private GameObject New(string name)
        {
            var value = new GameObject(name);
            _objects.Add(value);
            return value;
        }

        [SetUp]
        public void Setup()
        {
            Time.timeScale = 1;
            _player = New("Player").transform;
            _player.position = new Vector3(0, 0, -4);
            _body = _player.gameObject.AddComponent<BoxCollider>();
            ((BoxCollider)_body).size = Vector3.one * .4f;
            _health = _player.gameObject.AddComponent<HealthComponent>();
            var volumeObject = New("Generic passage");
            _volume = volumeObject.AddComponent<BoxCollider>();
            _volume.size = new Vector3(4, 4, 4);
            _volume.isTrigger = true;
            var rigidbody = volumeObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            _blocker = New("Solid barrier").AddComponent<BoxCollider>();
            ((BoxCollider)_blocker).size = new Vector3(3, 3, .2f);
            _passage = volumeObject.AddComponent<RegionPassageController>();
            _passage.TransitionCompleted += _ => _completed++;
        }

        [TearDown]
        public void Teardown()
        {
            Time.timeScale = 1;
            for (int i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
            _objects.Clear();
            _completed = 0;
        }

        private void Configure(RegionTransitionDirection direction = RegionTransitionDirection.Returnable,
            RegionEncounterSession encounter = null)
        {
            Physics.SyncTransforms();
            _passage.Configure(_volume, _blocker, _player, _health,
                new RegionTransition("a", "b", encounter == null ? RegionTransitionRequirement.Free :
                    RegionTransitionRequirement.EncounterCompleted, RegionTransitionTraversal.Walk, direction),
                encounter);
        }

        private IEnumerator Move(float z)
        {
            _player.position = new Vector3(0, 0, z);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Walk_CompoundPlayerCompletesOnlyWhenAllCollidersClear_ClosesBehind()
        {
            var trailing = New("Trailing player collider");
            trailing.transform.SetParent(_player, false);
            trailing.transform.localPosition = new Vector3(0, 0, -2);
            trailing.AddComponent<BoxCollider>().size = Vector3.one * .4f;
            Configure(RegionTransitionDirection.OneWay);
            yield return Move(-1);
            Assert.That(_passage.IsTransitioning, Is.True);
            yield return Move(1);
            Assert.That(_passage.Barrier.OccupantCount, Is.EqualTo(2));
            yield return Move(2.5f);
            Assert.That(_passage.Barrier.OccupantCount, Is.EqualTo(1));
            Assert.That(_completed, Is.Zero);
            Assert.That(_blocker.enabled, Is.False);
            yield return Move(4.5f);
            Assert.That(_completed, Is.EqualTo(1));
            Assert.That(_passage.CurrentRegionId, Is.EqualTo("b"));
            Assert.That(_blocker.enabled, Is.True);
            yield return Move(4.6f);
            Assert.That(_completed, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Returnable_WalksBothDirections_PreservesHealth()
        {
            Configure();
            _health.ApplyDamage(new DamageInfo(10, null, Vector3.zero, Vector3.forward));
            yield return Move(-1);
            yield return Move(3);
            Assert.That(_passage.CurrentRegionId, Is.EqualTo("b"));
            yield return Move(1);
            yield return Move(-3);
            Assert.That(_passage.CurrentRegionId, Is.EqualTo("a"));
            Assert.That(_completed, Is.EqualTo(2));
            Assert.That(_health.CurrentHealth, Is.EqualTo(90));
        }

        [UnityTest]
        public IEnumerator Retreat_CancelsAndAllowsAnotherAttempt()
        {
            Configure(RegionTransitionDirection.OneWay);
            yield return Move(-1);
            yield return Move(-3);
            Assert.That(_completed, Is.Zero);
            Assert.That(_passage.CurrentRegionId, Is.EqualTo("a"));
            Assert.That(_passage.IsTransitioning, Is.False);
            yield return Move(-1);
            yield return Move(3);
            Assert.That(_completed, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator SidewaysExitDoesNotCountAsReachingTheDestination()
        {
            Configure(RegionTransitionDirection.OneWay);
            yield return Move(-1);
            _player.position = new Vector3(5, 0, .1f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(_completed, Is.Zero);
            Assert.That(_passage.CurrentRegionId, Is.EqualTo("a"));
            Assert.That(_passage.IsTransitioning, Is.False);
        }

        [UnityTest]
        public IEnumerator EncounterGate_StaysClosedUntilLinkedEncounterCompletes()
        {
            using var encounter = new RegionEncounterSession("a");
            var session = new FakeEncounter();
            encounter.Prepare(session);
            encounter.Enter();
            Configure(encounter: encounter);
            Assert.That(_blocker.enabled, Is.True);
            session.Win();
            yield return Move(-4);
            Assert.That(_blocker.enabled, Is.False);
            yield return Move(-1);
            yield return Move(3);
            Assert.That(_completed, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ForeignCollider_DoesNotBeginOrHoldBarrierOpen()
        {
            Configure();
            New("Enemy").AddComponent<BoxCollider>();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(_passage.Barrier.OccupantCount, Is.Zero);
            Assert.That(_passage.IsTransitioning, Is.False);
            _passage.DestinationAvailable = false;
            yield return new WaitForFixedUpdate();
            Assert.That(_blocker.enabled, Is.True);
        }

        [UnityTest]
        public IEnumerator ReadinessLossDefersClose_UntilDisabledPlayerColliderIsRemoved()
        {
            Configure();
            yield return Move(-1);
            _passage.DestinationAvailable = false;
            yield return new WaitForFixedUpdate();
            Assert.That(_passage.IsTransitioning, Is.False);
            Assert.That(_passage.Barrier.IsCloseRequested, Is.True);
            Assert.That(_blocker.enabled, Is.False);
            _body.enabled = false;
            yield return new WaitForFixedUpdate();
            Assert.That(_passage.Barrier.OccupantCount, Is.Zero);
            Assert.That(_blocker.enabled, Is.True);
            Assert.That(_completed, Is.Zero);
        }

        [UnityTest]
        public IEnumerator DisabledComponentCancelsThenRescansOccupancyOnEnable()
        {
            Configure();
            yield return Move(-1);
            _passage.enabled = false;
            Assert.That(_passage.IsTransitioning, Is.False);
            Assert.That(_blocker.enabled, Is.False);
            _passage.DestinationAvailable = false;
            _passage.enabled = true;
            yield return new WaitForFixedUpdate();
            Assert.That(_blocker.enabled, Is.False);
            yield return Move(-3);
            Assert.That(_blocker.enabled, Is.True);
            Assert.That(_completed, Is.Zero);
        }

        [UnityTest]
        public IEnumerator PauseAndDeathCancelWithoutTrappingPlayer()
        {
            Configure();
            yield return Move(-1);
            Time.timeScale = 0;
            yield return null;
            yield return null;
            Assert.That(_passage.IsTransitioning, Is.False);
            Assert.That(_blocker.enabled, Is.False);
            Time.timeScale = 1;
            yield return Move(-1);
            _health.ApplyDamage(new DamageInfo(100, null, Vector3.zero, Vector3.forward));
            yield return Move(3);
            Assert.That(_completed, Is.Zero);
            Assert.That(_passage.CurrentRegionId, Is.EqualTo("a"));
        }

        [Test]
        public void BlockerOutsideSafetyVolume_IsRejected()
        {
            _blocker.transform.position = new Vector3(10, 0, 0);
            Assert.Throws<ArgumentException>(() => Configure());
        }

        private sealed class FakeEncounter : IArenaSession
        {
            public bool IsInitialized => true;
            public ArenaSessionStatus Status { get; private set; }
            public event Action Victory;
            public event Action Defeat { add { } remove { } }
            public void Begin() => Status = ArenaSessionStatus.Running;
            public void Win() { Status = ArenaSessionStatus.Victory; Victory?.Invoke(); }
        }
    }
}
