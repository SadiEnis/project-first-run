using NUnit.Framework;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Waves;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class ArenaSessionDefeatTests :
        ArenaSessionControllerTestFixture
    {
        [Test]
        public void PlayerDeath_WhileRunning_MarksArenaAsDefeat()
        {
            InitializeArenaSession();
            ArenaSession.Begin();

            PlayerDeathSource.Die();

            Assert.That(
                ArenaSession.Status,
                Is.EqualTo(ArenaSessionStatus.Defeat));

            Assert.That(
                ArenaSession.State.IsDefeat,
                Is.True);

            Assert.That(
                ArenaSession.State.IsFinished,
                Is.True);
        }

        [Test]
        public void PlayerDeath_PublishesDefeatOnce()
        {
            InitializeArenaSession();

            int defeatCount = 0;

            ArenaSession.Defeat +=
                () => defeatCount++;

            ArenaSession.Begin();

            PlayerDeathSource.Die();
            PlayerDeathSource.Die();

            Assert.That(
                defeatCount,
                Is.EqualTo(1));

            Assert.That(
                ArenaSession.Status,
                Is.EqualTo(ArenaSessionStatus.Defeat));
        }

        [Test]
        public void Begin_WhenPlayerIsAlreadyDead_Throws()
        {
            InitializeArenaSession();

            PlayerDeathSource.SetDeadBeforeSession();

            Assert.That(
                ArenaSession.Begin,
                Throws.InvalidOperationException);

            Assert.That(
                ArenaSession.Status,
                Is.EqualTo(ArenaSessionStatus.Ready));

            Assert.That(
                WaveController.HasBegun,
                Is.False);
        }

        [Test]
        public void PlayerDeath_BeforeSessionBegins_DoesNotMarkDefeat()
        {
            InitializeArenaSession();

            PlayerDeathSource.Die();

            Assert.That(
                ArenaSession.Status,
                Is.EqualTo(ArenaSessionStatus.Ready));

            Assert.That(
                ArenaSession.State.IsFinished,
                Is.False);
        }

        [Test]
        public void PlayerDeath_AfterVictory_DoesNotOverwriteVictory()
        {
            InitializeArenaSession();
            ArenaSession.Begin();

            KillSpawnedEnemy();

            Assert.That(
                ArenaSession.Status,
                Is.EqualTo(ArenaSessionStatus.Victory));

            PlayerDeathSource.Die();

            Assert.That(
                ArenaSession.Status,
                Is.EqualTo(ArenaSessionStatus.Victory));

            Assert.That(
                ArenaSession.State.IsDefeat,
                Is.False);
        }

        [Test]
        public void Defeat_PreventsLaterWaveCompletionFromBecomingVictory()
        {
            InitializeArenaSession();
            ArenaSession.Begin();

            PlayerDeathSource.Die();

            Assert.That(
                ArenaSession.Status,
                Is.EqualTo(ArenaSessionStatus.Defeat));

            KillSpawnedEnemy();

            Assert.That(
                WaveController.SequenceState.Status,
                Is.EqualTo(WaveSequenceStatus.Completed));

            Assert.That(
                ArenaSession.Status,
                Is.EqualTo(ArenaSessionStatus.Defeat));

            Assert.That(
                ArenaSession.State.IsVictory,
                Is.False);
        }
    }
}