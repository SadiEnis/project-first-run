using NUnit.Framework;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Waves;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class ArenaSessionControllerTests :
        ArenaSessionControllerTestFixture
    {
        [Test]
        public void Begin_BeforeInitialize_Throws()
        {
            Assert.That(
                ArenaSession.Begin,
                Throws.InvalidOperationException);

            Assert.That(
                ArenaSession.IsInitialized,
                Is.False);
        }

        [Test]
        public void Initialize_WithNullWaveController_Throws()
        {
            Assert.That(
                () =>
                    ArenaSession.Initialize(
                        null,
                        PlayerDeathSource),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Begin_AfterInitialize_StartsArenaAndWave()
        {
            InitializeArenaSession();

            ArenaSession.Begin();

            Assert.That(
                ArenaSession.Status,
                Is.EqualTo(ArenaSessionStatus.Running));

            Assert.That(
                ArenaSession.State.IsRunning,
                Is.True);

            Assert.That(
                WaveController.HasBegun,
                Is.True);

            Assert.That(
                WaveController.SequenceState.Status,
                Is.EqualTo(WaveSequenceStatus.Running));

            Assert.That(
                Registry.ActiveCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Begin_PublishesSessionStartedOnce()
        {
            int startedCount = 0;

            ArenaSession.SessionStarted +=
                () => startedCount++;

            InitializeArenaSession();

            ArenaSession.Begin();

            Assert.That(
                startedCount,
                Is.EqualTo(1));

            Assert.That(
                ArenaSession.Begin,
                Throws.InvalidOperationException);

            Assert.That(
                startedCount,
                Is.EqualTo(1));
        }

        [Test]
        public void FinalWaveCompletion_MarksArenaAsVictory()
        {
            int victoryCount = 0;

            ArenaSession.Victory +=
                () => victoryCount++;

            InitializeArenaSession();

            ArenaSession.Begin();

            KillSpawnedEnemy();

            Assert.That(
                WaveController.SequenceState.IsCompleted,
                Is.True);

            Assert.That(
                ArenaSession.Status,
                Is.EqualTo(ArenaSessionStatus.Victory));

            Assert.That(
                ArenaSession.State.IsVictory,
                Is.True);

            Assert.That(
                ArenaSession.State.IsFinished,
                Is.True);

            Assert.That(
                victoryCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Restart_AfterVictory_StartsFreshWaveAndPublishesSessionStartedAgain()
        {
            int startedCount = 0;
            ArenaSession.SessionStarted += () => startedCount++;

            InitializeArenaSession();
            ArenaSession.Begin();
            KillSpawnedEnemy();

            ArenaSession.Restart();

            Assert.That(ArenaSession.Status, Is.EqualTo(ArenaSessionStatus.Running));
            Assert.That(WaveController.HasBegun, Is.True);
            Assert.That(WaveController.SequenceState.Status, Is.EqualTo(WaveSequenceStatus.Running));
            Assert.That(WaveController.SequenceState.CompletedWaves, Is.EqualTo(0));
            Assert.That(Registry.ActiveCount, Is.EqualTo(1));
            Assert.That(startedCount, Is.EqualTo(2));
        }

        [Test]
        public void Restart_WhileRunning_Throws()
        {
            InitializeArenaSession();
            ArenaSession.Begin();

            Assert.That(ArenaSession.Restart, Throws.InvalidOperationException);
            Assert.That(ArenaSession.Status, Is.EqualTo(ArenaSessionStatus.Running));
        }

        [Test]
        public void Restart_WhenPlayerIsDead_Throws()
        {
            InitializeArenaSession();
            ArenaSession.Begin();
            PlayerDeathSource.Die();

            Assert.That(ArenaSession.Restart, Throws.InvalidOperationException);
            Assert.That(ArenaSession.Status, Is.EqualTo(ArenaSessionStatus.Defeat));
        }
    }
}
