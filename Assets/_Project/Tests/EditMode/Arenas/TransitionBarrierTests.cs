using NUnit.Framework;
using ProjectFirstRun.Arenas;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class TransitionBarrierTests
    {
        [Test]
        public void Open_IsIdempotentAndPublishesOnce()
        {
            var barrier = new TransitionBarrier();
            int opened = 0;
            barrier.Opened += () => opened++;

            Assert.That(barrier.Open(), Is.True);
            Assert.That(barrier.Open(), Is.False);
            Assert.That(barrier.Status, Is.EqualTo(TransitionBarrierStatus.Open));
            Assert.That(opened, Is.EqualTo(1));
        }

        [Test]
        public void Entry_IsRejectedWhileClosedAndDuplicateEntryIsIgnored()
        {
            var barrier = new TransitionBarrier();
            Assert.That(barrier.NotifyPlayerEntered(), Is.False);
            barrier.Open();
            Assert.That(barrier.NotifyPlayerEntered(), Is.True);
            Assert.That(barrier.NotifyPlayerEntered(), Is.False);
            Assert.That(barrier.IsPlayerInside, Is.True);
        }

        [Test]
        public void CloseRequest_WhilePlayerInsideWaitsUntilCleared()
        {
            var barrier = new TransitionBarrier();
            int closed = 0;
            barrier.Closed += () => closed++;
            barrier.Open();
            barrier.NotifyPlayerEntered();

            Assert.That(barrier.RequestClose(), Is.False);
            Assert.That(barrier.Status, Is.EqualTo(TransitionBarrierStatus.Open));
            Assert.That(barrier.IsCloseRequested, Is.True);
            Assert.That(closed, Is.Zero);

            Assert.That(barrier.NotifyPlayerCleared(), Is.True);
            Assert.That(barrier.Status, Is.EqualTo(TransitionBarrierStatus.Closed));
            Assert.That(barrier.IsPlayerInside, Is.False);
            Assert.That(barrier.IsCloseRequested, Is.False);
            Assert.That(closed, Is.EqualTo(1));
        }

        [Test]
        public void CloseRequest_WithoutPlayerClosesImmediatelyAndPublishesOnce()
        {
            var barrier = new TransitionBarrier();
            int closed = 0;
            barrier.Closed += () => closed++;
            barrier.Open();

            Assert.That(barrier.RequestClose(), Is.True);
            Assert.That(barrier.RequestClose(), Is.False);
            Assert.That(closed, Is.EqualTo(1));
            Assert.That(barrier.Status, Is.EqualTo(TransitionBarrierStatus.Closed));
        }

        [Test]
        public void RepeatedCloseRequests_DoNotCloseBeforePlayerClears()
        {
            var barrier = new TransitionBarrier();
            barrier.Open();
            barrier.NotifyPlayerEntered();
            Assert.That(barrier.RequestClose(), Is.False);
            Assert.That(barrier.RequestClose(), Is.False);
            Assert.That(barrier.NotifyPlayerCleared(), Is.True);
            Assert.That(barrier.NotifyPlayerCleared(), Is.False);
        }

        [Test]
        public void Reopen_CancelsPendingCloseWithoutDuplicateCloseEvent()
        {
            var barrier = new TransitionBarrier();
            int closed = 0;
            barrier.Closed += () => closed++;
            barrier.Open();
            barrier.NotifyPlayerEntered();
            Assert.That(barrier.RequestClose(), Is.False);
            Assert.That(barrier.Open(), Is.False);
            Assert.That(barrier.IsCloseRequested, Is.False);
            Assert.That(barrier.NotifyPlayerCleared(), Is.True);
            Assert.That(barrier.Status, Is.EqualTo(TransitionBarrierStatus.Open));
            Assert.That(closed, Is.Zero);
        }
    }
}
