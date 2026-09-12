using NUnit.Framework;
using ProjectFirstRun.Arenas;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class ArenaSessionStateTests
    {
        [Test]
        public void Constructor_CreatesReadyState()
        {
            ArenaSessionState state =
                new ArenaSessionState();

            Assert.That(
                state.Status,
                Is.EqualTo(ArenaSessionStatus.Ready));

            Assert.That(
                state.IsReady,
                Is.True);

            Assert.That(
                state.IsRunning,
                Is.False);

            Assert.That(
                state.IsFinished,
                Is.False);
        }

        [Test]
        public void Begin_FromReady_ChangesStateToRunning()
        {
            ArenaSessionState state =
                new ArenaSessionState();

            state.Begin();

            Assert.That(
                state.Status,
                Is.EqualTo(ArenaSessionStatus.Running));

            Assert.That(
                state.IsRunning,
                Is.True);

            Assert.That(
                state.IsFinished,
                Is.False);
        }

        [Test]
        public void Begin_WhenAlreadyRunning_Throws()
        {
            ArenaSessionState state =
                new ArenaSessionState();

            state.Begin();

            Assert.That(
                state.Begin,
                Throws.InvalidOperationException);
        }

        [Test]
        public void MarkVictory_FromRunning_ChangesStateToVictory()
        {
            ArenaSessionState state =
                CreateRunningState();

            state.MarkVictory();

            Assert.That(
                state.Status,
                Is.EqualTo(ArenaSessionStatus.Victory));

            Assert.That(
                state.IsVictory,
                Is.True);

            Assert.That(
                state.IsFinished,
                Is.True);
        }

        [Test]
        public void MarkDefeat_FromRunning_ChangesStateToDefeat()
        {
            ArenaSessionState state =
                CreateRunningState();

            state.MarkDefeat();

            Assert.That(
                state.Status,
                Is.EqualTo(ArenaSessionStatus.Defeat));

            Assert.That(
                state.IsDefeat,
                Is.True);

            Assert.That(
                state.IsFinished,
                Is.True);
        }

        [Test]
        public void MarkVictory_FromReady_Throws()
        {
            ArenaSessionState state =
                new ArenaSessionState();

            Assert.That(
                state.MarkVictory,
                Throws.InvalidOperationException);
        }

        [Test]
        public void MarkDefeat_FromReady_Throws()
        {
            ArenaSessionState state =
                new ArenaSessionState();

            Assert.That(
                state.MarkDefeat,
                Throws.InvalidOperationException);
        }

        [Test]
        public void MarkDefeat_AfterVictory_Throws()
        {
            ArenaSessionState state =
                CreateRunningState();

            state.MarkVictory();

            Assert.That(
                state.MarkDefeat,
                Throws.InvalidOperationException);

            Assert.That(
                state.Status,
                Is.EqualTo(ArenaSessionStatus.Victory));
        }

        [Test]
        public void MarkVictory_AfterDefeat_Throws()
        {
            ArenaSessionState state =
                CreateRunningState();

            state.MarkDefeat();

            Assert.That(
                state.MarkVictory,
                Throws.InvalidOperationException);

            Assert.That(
                state.Status,
                Is.EqualTo(ArenaSessionStatus.Defeat));
        }

        private static ArenaSessionState CreateRunningState()
        {
            ArenaSessionState state =
                new ArenaSessionState();

            state.Begin();

            return state;
        }
    }
}