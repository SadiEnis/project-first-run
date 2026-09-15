using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Player;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class RunSessionControllerTests
    {
        private GameObject _controllerObject;
        private RunSessionController _controller;
        private FakeArenaSession _firstArena;
        private FakeArenaSession _secondArena;

        [SetUp]
        public void SetUp()
        {
            _controllerObject = new GameObject("RunSessionController_Test");
            _controller = _controllerObject.AddComponent<RunSessionController>();
            _firstArena = new FakeArenaSession();
            _secondArena = new FakeArenaSession();
        }

        [TearDown]
        public void TearDown()
        {
            if (_controllerObject != null)
            {
                Object.DestroyImmediate(_controllerObject);
            }
        }

        [Test]
        public void Initialize_RequiresInitializedArenaSessions()
        {
            Assert.That(
                () => _controller.Initialize(new List<IArenaSession> { _firstArena }),
                Throws.InvalidOperationException);
        }

        [Test]
        public void Begin_StartsOnlyFirstArena()
        {
            _firstArena.Initialize();
            _secondArena.Initialize();
            _controller.Initialize(new List<IArenaSession> { _firstArena, _secondArena });

            _controller.Begin();

            Assert.That(_controller.Status, Is.EqualTo(RunSessionStatus.Running));
            Assert.That(_controller.State.CurrentArenaIndex, Is.EqualTo(0));
            Assert.That(_firstArena.BeginCount, Is.EqualTo(1));
            Assert.That(_secondArena.BeginCount, Is.EqualTo(0));
        }

        [Test]
        public void ArenaVictory_EntersTransitionUntilExplicitAdvance()
        {
            InitializeTwoArenas();
            int transitionIndex = -1;
            _controller.ArenaTransitionReady += index => transitionIndex = index;
            _controller.Begin();

            _firstArena.PublishVictory();

            Assert.That(_controller.Status, Is.EqualTo(RunSessionStatus.Transition));
            Assert.That(transitionIndex, Is.EqualTo(0));
            Assert.That(_secondArena.BeginCount, Is.EqualTo(0));

            _controller.AdvanceToNextArena();

            Assert.That(_controller.Status, Is.EqualTo(RunSessionStatus.Running));
            Assert.That(_controller.State.CurrentArenaIndex, Is.EqualTo(1));
            Assert.That(_secondArena.BeginCount, Is.EqualTo(1));
        }

        [Test]
        public void FinalArenaVictory_CompletesRunOnce()
        {
            InitializeTwoArenas();
            int victoryCount = 0;
            _controller.Victory += () => victoryCount++;
            _controller.Begin();
            _firstArena.PublishVictory();
            _controller.AdvanceToNextArena();

            _secondArena.PublishVictory();
            _secondArena.PublishVictory();

            Assert.That(_controller.Status, Is.EqualTo(RunSessionStatus.Victory));
            Assert.That(_controller.State.CompletedArenas, Is.EqualTo(2));
            Assert.That(victoryCount, Is.EqualTo(1));
        }

        [Test]
        public void ActiveArenaDefeat_CompletesRunAsDefeat()
        {
            InitializeTwoArenas();
            int defeatCount = 0;
            _controller.Defeat += () => defeatCount++;
            _controller.Begin();

            _firstArena.PublishDefeat();

            Assert.That(_controller.Status, Is.EqualTo(RunSessionStatus.Defeat));
            Assert.That(defeatCount, Is.EqualTo(1));

            _firstArena.PublishVictory();
            Assert.That(_controller.Status, Is.EqualTo(RunSessionStatus.Defeat));
        }

        [Test]
        public void PlayerDeathDuringTransition_EndsRunOnceAndBlocksLateEvents()
        {
            InitializeTwoArenas();
            var death = new FakeDeathSource();
            Object.DestroyImmediate(_controllerObject);
            _controllerObject = new GameObject("RunSessionController_Test");
            _controller = _controllerObject.AddComponent<RunSessionController>();
            _controller.Initialize(new List<IArenaSession> { _firstArena, _secondArena }, death);
            int defeats = 0;
            _controller.Defeat += () => defeats++;
            _controller.Begin();
            _firstArena.PublishVictory();
            Assert.That(_controller.Status, Is.EqualTo(RunSessionStatus.Transition));
            death.Die();
            Assert.That(_controller.Status, Is.EqualTo(RunSessionStatus.Defeat));
            Assert.That(defeats, Is.EqualTo(1));
            death.Die();
            _firstArena.PublishVictory();
            Assert.That(defeats, Is.EqualTo(1));
            Assert.That(_controller.Status, Is.EqualTo(RunSessionStatus.Defeat));
        }

        [Test]
        public void FinalObjective_CompletesRunningRunOnce_WithoutArenaVictory()
        {
            InitializeTwoArenas();
            int victories = 0;
            _controller.Victory += () => victories++;
            _controller.Begin();
            _controller.CompleteFinalObjective();
            _controller.CompleteFinalObjective();
            Assert.That(_controller.Status, Is.EqualTo(RunSessionStatus.Victory));
            Assert.That(victories, Is.EqualTo(1));
        }

        [Test]
        public void RestartRequiresAtomicResetCallbackAndStartsFreshFirstArena()
        {
            InitializeTwoArenas();
            var death = new FakeDeathSource();
            Object.DestroyImmediate(_controllerObject);
            _controllerObject = new GameObject("RunSessionController_Test");
            _controller = _controllerObject.AddComponent<RunSessionController>();
            _controller.Initialize(new List<IArenaSession> { _firstArena, _secondArena }, death);
            _controller.Begin();
            death.Die();
            Assert.That(_controller.CanRestart, Is.True);
            Assert.Throws<ArgumentNullException>(() => _controller.Restart(null));
            Assert.That(_controller.Status, Is.EqualTo(RunSessionStatus.Defeat));
            Assert.Throws<InvalidOperationException>(() => _controller.Restart(() => throw new InvalidOperationException("reset failed")));
            Assert.That(_controller.Status, Is.EqualTo(RunSessionStatus.Defeat));
            death.Revive();
            _firstArena.ResetForRun();
            _secondArena.ResetForRun();
            int restarted = 0;
            _controller.SessionRestarted += () => restarted++;
            _controller.Restart(() => { _firstArena.ResetForRun(); _secondArena.ResetForRun(); });
            Assert.That(_controller.Status, Is.EqualTo(RunSessionStatus.Running));
            Assert.That(_controller.State.CurrentArenaIndex, Is.Zero);
            Assert.That(_controller.State.CompletedArenas, Is.Zero);
            Assert.That(_firstArena.BeginCount, Is.EqualTo(2));
            Assert.That(_secondArena.BeginCount, Is.Zero);
            Assert.That(restarted, Is.EqualTo(1));
        }

        [Test]
        public void RestartRejectsCallbackThatLeavesDeadPlayerOrTerminalArena()
        {
            InitializeTwoArenas();
            var death = new FakeDeathSource();
            Object.DestroyImmediate(_controllerObject);
            _controllerObject = new GameObject("RunSessionController_Test");
            _controller = _controllerObject.AddComponent<RunSessionController>();
            _controller.Initialize(new List<IArenaSession> { _firstArena, _secondArena }, death);
            _controller.Begin();
            death.Die();
            Assert.Throws<InvalidOperationException>(() => _controller.Restart(() => { _firstArena.ResetForRun(); _secondArena.ResetForRun(); }));
            Assert.That(_controller.Status, Is.EqualTo(RunSessionStatus.Defeat));
        }

        private void InitializeTwoArenas()
        {
            _firstArena.Initialize();
            _secondArena.Initialize();
            _controller.Initialize(new List<IArenaSession> { _firstArena, _secondArena });
        }

        private sealed class FakeArenaSession : IArenaSession
        {
            public bool IsInitialized { get; private set; }
            public ArenaSessionStatus Status { get; private set; } = ArenaSessionStatus.Ready;
            public int BeginCount { get; private set; }

            public event Action Victory;
            public event Action Defeat;

            public void Initialize() => IsInitialized = true;

            public void Begin()
            {
                if (!IsInitialized || Status != ArenaSessionStatus.Ready)
                {
                    throw new InvalidOperationException();
                }

                BeginCount++;
                Status = ArenaSessionStatus.Running;
            }

            public void PublishVictory()
            {
                if (Status != ArenaSessionStatus.Running) return;
                Status = ArenaSessionStatus.Victory;
                Victory?.Invoke();
            }

            public void PublishDefeat()
            {
                if (Status != ArenaSessionStatus.Running) return;
                Status = ArenaSessionStatus.Defeat;
                Defeat?.Invoke();
            }

            public void ResetForRun() { Status = ArenaSessionStatus.Ready; }
        }

        private sealed class FakeDeathSource : IPlayerDeathSource
        {
            public bool IsDead { get; private set; }
            public event Action<DamageInfo, DamageResult> PlayerDied;
            public void Die() { IsDead = true; PlayerDied?.Invoke(default, default); }
            public void Revive() => IsDead = false;
        }
    }
}
