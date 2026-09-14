using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Player;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class ArenaTransitionTests
    {
        private readonly List<GameObject> _objects = new();
        private RunSessionController _run;
        private ArenaTransitionController _transitions;
        private Transform _player, _entry;
        private HealthComponent _health;
        private Collider _collider;
        private FakeArena _first, _second;
        private DeathSource _death;

        private GameObject New(string name)
        {
            var go = new GameObject(name);
            _objects.Add(go);
            return go;
        }

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 1f;
            _player = New("Assigned player").transform;
            _player.position = new Vector3(500, 0, 500);
            _collider = _player.gameObject.AddComponent<BoxCollider>();
            _health = _player.gameObject.AddComponent<HealthComponent>();
            _entry = New("Destination").transform;
            _entry.SetPositionAndRotation(new Vector3(520, 0, 500), Quaternion.Euler(0, 90, 0));
            _first = new FakeArena(); _second = new FakeArena(); _death = new DeathSource();
            _run = New("Run").AddComponent<RunSessionController>();
            _run.Initialize(new IArenaSession[] { _first, _second }, _death);
            _transitions = New("Transitions").AddComponent<ArenaTransitionController>();
            _transitions.Configure(_run, _player, _health, new[] { _entry, _entry });
            _run.Begin();
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            for (int i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
            _objects.Clear();
        }

        [Test]
        public void LockedWrongArenaAndUnrelatedCollider_DoNotMovePlayer()
        {
            Vector3 before = _player.position;
            Assert.That(_transitions.TryTransition(0, _collider), Is.False);
            _first.Win();
            Assert.That(_transitions.TryTransition(1, _collider), Is.False);
            Assert.That(_transitions.TryTransition(0, New("Enemy").AddComponent<BoxCollider>()), Is.False);
            Assert.That(_player.position, Is.EqualTo(before));
            Assert.That(_second.Starts, Is.Zero);
        }

        [Test]
        public void Transition_PlacesSamePlayerBeforeBegin_PreservesHealth_OnlyOnce()
        {
            _health.ApplyDamage(new DamageInfo(20, null, Vector3.zero, Vector3.forward));
            _first.Win();
            _second.OnBegin = () => Assert.That(_player.position, Is.EqualTo(_entry.position));
            Assert.That(_transitions.TryTransition(0, _collider), Is.True);
            Assert.That(Quaternion.Angle(_player.rotation, _entry.rotation), Is.LessThan(.01f));
            Assert.That(_health.CurrentHealth, Is.EqualTo(80));
            Assert.That(_transitions.TryTransition(0, _collider), Is.False);
            Assert.That(_second.Starts, Is.EqualTo(1));
            _second.Win();
            Assert.That(_run.Status, Is.EqualTo(RunSessionStatus.Victory));
            Assert.That(_transitions.CanTransition(1), Is.False);
        }

        [Test]
        public void PauseDisabledManagerAndMissingDestination_BlockWithoutConsuming()
        {
            _first.Win();
            Time.timeScale = 0;
            Assert.That(_transitions.TryTransition(0, _collider), Is.False);
            Time.timeScale = 1;
            _transitions.enabled = false;
            Assert.That(_transitions.TryTransition(0, _collider), Is.False);
            _transitions.enabled = true;
            Object.DestroyImmediate(_entry.gameObject);
            Assert.That(_transitions.TryTransition(0, _collider), Is.False);
            Assert.That(_run.Status, Is.EqualTo(RunSessionStatus.Transition));
        }

        [Test]
        public void DeathWhileWaiting_EndsRunAndBlocksExit()
        {
            _first.Win();
            _death.Die();
            Assert.That(_run.Status, Is.EqualTo(RunSessionStatus.Defeat));
            Assert.That(_transitions.TryTransition(0, _collider), Is.False);
        }

        [Test]
        public void FailedDestinationBegin_EndsRun_AndCannotRetry()
        {
            _first.Win();
            _second.OnBegin = () => throw new InvalidOperationException("spawn failure");
            Assert.Throws<InvalidOperationException>(() => _transitions.TryTransition(0, _collider));
            Assert.That(_run.Status, Is.EqualTo(RunSessionStatus.Defeat));
            Assert.That(_transitions.TryTransition(0, _collider), Is.False);
        }

        [Test]
        public void ChildColliderAndCharacterMotor_TeleportWithoutCarryingFallSpeed()
        {
            var motor = _player.gameObject.AddComponent<PlayerMotor>();
            var character = _player.GetComponent<CharacterController>();
            motor.Tick(Vector2.zero, false, .5f);
            var child = New("Player child collider");
            child.transform.SetParent(_player, false);
            var childCollider = child.AddComponent<SphereCollider>();
            _first.Win();
            Assert.That(_transitions.TryTransition(0, childCollider), Is.True);
            Assert.That(character.enabled, Is.True);
            Assert.That(_player.position, Is.EqualTo(_entry.position));
            motor.Tick(Vector2.zero, false, .02f);
            Assert.That(_player.position.y, Is.GreaterThan(_entry.position.y - .03f));
        }

        [UnityTest]
        public IEnumerator RealSphereTrigger_AdvancesOnPhysicsEntry()
        {
            var exit = New("Generic exit");
            exit.transform.position = new Vector3(510, 0, 500);
            exit.AddComponent<SphereCollider>().isTrigger = true;
            var body = exit.AddComponent<Rigidbody>();
            body.isKinematic = true; body.useGravity = false;
            exit.AddComponent<ArenaTransitionTrigger>().Configure(_transitions, 0);
            yield return new WaitForFixedUpdate();
            _first.Win();
            _player.position = exit.transform.position;
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(_run.State.CurrentArenaIndex, Is.EqualTo(1));
            Assert.That(_second.Starts, Is.EqualTo(1));
            Assert.That(_player.position, Is.EqualTo(_entry.position));
        }

        [Test]
        public void InvalidRunList_CanRetryWithoutLeakedSubscriptions()
        {
            var run = New("Retry run").AddComponent<RunSessionController>();
            var first = new FakeArena(); var second = new FakeArena();
            Assert.Throws<ArgumentException>(() => run.Initialize(new IArenaSession[] { first, null }));
            run.Initialize(new IArenaSession[] { first, second });
            run.Begin(); first.Win(); run.AdvanceToNextArena(); second.Win();
            Assert.That(run.State.CompletedArenas, Is.EqualTo(2));
            Assert.That(run.Status, Is.EqualTo(RunSessionStatus.Victory));
        }

        private sealed class FakeArena : IArenaSession
        {
            public bool IsInitialized => true;
            public ArenaSessionStatus Status { get; private set; }
            public event Action Victory;
            public event Action Defeat { add { } remove { } }
            public int Starts;
            public Action OnBegin;
            public void Begin() { OnBegin?.Invoke(); Starts++; Status = ArenaSessionStatus.Running; }
            public void Win() { Status = ArenaSessionStatus.Victory; Victory?.Invoke(); }
        }

        private sealed class DeathSource : IPlayerDeathSource
        {
            public bool IsDead { get; private set; }
            public event Action<DamageInfo, DamageResult> PlayerDied;
            public void Die() { IsDead = true; PlayerDied?.Invoke(default, default); }
        }
    }
}
