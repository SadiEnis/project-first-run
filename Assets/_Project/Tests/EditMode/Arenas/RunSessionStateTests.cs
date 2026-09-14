using NUnit.Framework;
using ProjectFirstRun.Arenas;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class RunSessionStateTests
    {
        [Test]
        public void Constructor_RequiresAtLeastOneArena()
        {
            Assert.That(
                () => new RunSessionState(0),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Begin_StartsFirstArena()
        {
            RunSessionState state = new RunSessionState(2);

            state.Begin();

            Assert.That(state.Status, Is.EqualTo(RunSessionStatus.Running));
            Assert.That(state.CurrentArenaIndex, Is.EqualTo(0));
            Assert.That(state.CompletedArenas, Is.EqualTo(0));
        }

        [Test]
        public void CompleteBeforeLast_EntersTransition()
        {
            RunSessionState state = CreateRunningState(2);

            state.CompleteCurrentArena();

            Assert.That(state.Status, Is.EqualTo(RunSessionStatus.Transition));
            Assert.That(state.CompletedArenas, Is.EqualTo(1));
            Assert.That(state.CurrentArenaIndex, Is.EqualTo(0));
        }

        [Test]
        public void BeginNextArena_AdvancesToCompletedCount()
        {
            RunSessionState state = CreateRunningState(2);
            state.CompleteCurrentArena();

            state.BeginNextArena();

            Assert.That(state.Status, Is.EqualTo(RunSessionStatus.Running));
            Assert.That(state.CurrentArenaIndex, Is.EqualTo(1));
        }

        [Test]
        public void CompleteLastArena_EntersVictory()
        {
            RunSessionState state = CreateRunningState(1);

            state.CompleteCurrentArena();

            Assert.That(state.Status, Is.EqualTo(RunSessionStatus.Victory));
            Assert.That(state.IsFinished, Is.True);
        }

        [Test]
        public void Defeat_FromTransition_IsTerminal()
        {
            RunSessionState state = CreateRunningState(2);
            state.CompleteCurrentArena();

            state.MarkDefeat();

            Assert.That(state.Status, Is.EqualTo(RunSessionStatus.Defeat));
            Assert.That(state.IsFinished, Is.True);
        }

        private static RunSessionState CreateRunningState(int arenaCount)
        {
            RunSessionState state = new RunSessionState(arenaCount);
            state.Begin();
            return state;
        }
    }
}
