using System;
using NUnit.Framework;
using ProjectFirstRun.Arenas;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class RegionTransitionTests
    {
        private static RegionTransition New(RegionTransitionDirection direction =
            RegionTransitionDirection.Returnable, bool singleUse = false,
            RegionTransitionRequirement requirement = RegionTransitionRequirement.Free)
            => new RegionTransition("a", "b", requirement, RegionTransitionTraversal.Walk, direction, singleUse);

        [Test]
        public void DirectionIsNotUsageCount()
        {
            var route = New(RegionTransitionDirection.OneWay);
            Assert.That(route.CanBegin("b", true, true), Is.False);
            Assert.That(route.CanBegin("unrelated", true, true), Is.False);
            Assert.That(route.TryBegin("a", false, true, out var attempt), Is.True);
            Assert.That(route.Complete(attempt), Is.True);
            Assert.That(route.IsConsumed, Is.False);
            Assert.That(route.CanBegin("a", false, true), Is.True);
            Assert.That(route.CanBegin("b", true, true), Is.False);
        }

        [Test]
        public void ReturnableConnectionResolvesBothDestinations()
        {
            var route = New();
            Assert.That(route.TryBegin("a", false, true, out var forward), Is.True);
            Assert.That(forward.ToId, Is.EqualTo("b"));
            Assert.That(route.Complete(forward), Is.True);
            Assert.That(route.TryBegin("b", false, true, out var backward), Is.True);
            Assert.That(backward.ToId, Is.EqualTo("a"));
        }

        [Test]
        public void PendingAttemptRejectsDuplicatesFromEitherEnd()
        {
            var route = New();
            route.TryBegin("a", true, true, out var attempt);
            Assert.That(route.TryBegin("a", true, true, out _), Is.False);
            Assert.That(route.TryBegin("b", true, true, out _), Is.False);
            Assert.That(route.Cancel(attempt), Is.True);
            Assert.That(route.CanBegin("a", true, true), Is.True);
        }

        [Test]
        public void FailedExecutionDoesNotConsumeSingleUseAndStaleCompletionCannotCommitRetry()
        {
            var route = New(singleUse: true);
            route.TryBegin("a", true, true, out var failed);
            Assert.That(route.IsConsumed, Is.False);
            route.Cancel(failed);
            Assert.That(route.TryBegin("a", true, true, out var retry), Is.True);
            Assert.That(route.Complete(failed), Is.False);
            Assert.That(route.Cancel(failed), Is.False);
            Assert.That(route.IsInProgress, Is.True);
            Assert.That(route.Complete(retry), Is.True);
            Assert.That(route.Complete(retry), Is.False);
            Assert.That(route.IsConsumed, Is.True);
            Assert.That(route.CanBegin("b", true, true), Is.False);
        }

        [Test]
        public void ForeignAndNullAttemptsAreRejected()
        {
            var first = New();
            var second = New();
            first.TryBegin("a", true, true, out var attempt);
            second.TryBegin("a", true, true, out _);
            Assert.That(second.Complete(attempt), Is.False);
            Assert.That(second.Cancel(attempt), Is.False);
            Assert.That(second.Complete(null), Is.False);
            Assert.That(second.IsInProgress, Is.True);
        }

        [Test]
        public void EncounterAndReadinessAreCheckedBeforeStarting()
        {
            var route = New(requirement: RegionTransitionRequirement.EncounterCompleted);
            Assert.That(route.TryBegin("a", false, true, out _), Is.False);
            Assert.That(route.TryBegin("a", true, false, out _), Is.False);
            Assert.That(route.IsInProgress, Is.False);
            Assert.That(route.TryBegin("a", true, true, out _), Is.True);
        }

        [Test]
        public void InvalidIdentitiesAndEnumsAreRejected()
        {
            Assert.Throws<ArgumentException>(() => new RegionTransition("", "b", 0, 0, 0));
            Assert.Throws<ArgumentException>(() => new RegionTransition("a", "a", 0, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RegionTransition("a", "b", (RegionTransitionRequirement)99, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RegionTransition("a", "b", 0, (RegionTransitionTraversal)99, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RegionTransition("a", "b", 0, 0, (RegionTransitionDirection)99));
        }

        [TestCase(RegionTransitionTraversal.Relocate)]
        [TestCase(RegionTransitionTraversal.SceneLoad)]
        public void ExecutorFailureCanCancelForAnyTraversal(RegionTransitionTraversal traversal)
        {
            var route = new RegionTransition("a", "b", 0, traversal, RegionTransitionDirection.OneWay, true);
            route.TryBegin("a", true, true, out var attempt);
            Assert.That(route.Traversal, Is.EqualTo(traversal));
            route.Cancel(attempt);
            Assert.That(route.TryBegin("a", true, true, out _), Is.True);
        }
    }
}
