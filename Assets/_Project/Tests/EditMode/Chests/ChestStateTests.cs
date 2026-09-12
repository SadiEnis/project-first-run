using NUnit.Framework;
using ProjectFirstRun.Chests;

namespace ProjectFirstRun.Tests.EditMode.Chests
{
    public sealed class ChestStateTests
    {
        [Test]
        public void Constructor_StartsAvailable()
        {
            ChestState state =
                new ChestState();

            Assert.That(
                state.Status,
                Is.EqualTo(ChestStatus.Available));

            Assert.That(
                state.IsAvailable,
                Is.True);

            Assert.That(
                state.IsSelecting,
                Is.False);

            Assert.That(
                state.IsOpened,
                Is.False);
        }

        [Test]
        public void BeginSelection_FromAvailable_SetsSelecting()
        {
            ChestState state =
                new ChestState();

            state.BeginSelection();

            Assert.That(
                state.Status,
                Is.EqualTo(ChestStatus.Selecting));

            Assert.That(
                state.IsSelecting,
                Is.True);
        }

        [Test]
        public void BeginSelection_WhileSelecting_Throws()
        {
            ChestState state =
                CreateSelectingState();

            Assert.That(
                state.BeginSelection,
                Throws.InvalidOperationException);

            Assert.That(
                state.IsSelecting,
                Is.True);
        }

        [Test]
        public void CancelSelection_WhileSelecting_ReturnsToAvailable()
        {
            ChestState state =
                CreateSelectingState();

            state.CancelSelection();

            Assert.That(
                state.IsAvailable,
                Is.True);
        }

        [Test]
        public void CancelSelection_WhileAvailable_Throws()
        {
            ChestState state =
                new ChestState();

            Assert.That(
                state.CancelSelection,
                Throws.InvalidOperationException);
        }

        [Test]
        public void CompleteSelection_WhileSelecting_SetsOpened()
        {
            ChestState state =
                CreateSelectingState();

            state.CompleteSelection();

            Assert.That(
                state.Status,
                Is.EqualTo(ChestStatus.Opened));

            Assert.That(
                state.IsOpened,
                Is.True);
        }

        [Test]
        public void CompleteSelection_WhileAvailable_Throws()
        {
            ChestState state =
                new ChestState();

            Assert.That(
                state.CompleteSelection,
                Throws.InvalidOperationException);
        }

        [Test]
        public void OpenedState_RejectsEveryTransition()
        {
            ChestState state =
                CreateSelectingState();

            state.CompleteSelection();

            Assert.That(
                state.BeginSelection,
                Throws.InvalidOperationException);

            Assert.That(
                state.CancelSelection,
                Throws.InvalidOperationException);

            Assert.That(
                state.CompleteSelection,
                Throws.InvalidOperationException);
        }

        private static ChestState CreateSelectingState()
        {
            ChestState state =
                new ChestState();

            state.BeginSelection();

            return state;
        }
    }
}
