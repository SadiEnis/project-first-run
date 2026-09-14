using System;
using NUnit.Framework;
using ProjectFirstRun.Arenas;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class RegionTransitionTests
    {
        [Test]
        public void Constructor_RequiresDistinctNonEmptyIdentities()
        {
            Assert.Throws<ArgumentException>(() => New(null, "b"));
            Assert.Throws<ArgumentException>(() => New("a", ""));
            Assert.Throws<ArgumentException>(() => New("a", "a"));
        }

        [Test]
        public void FreeTransition_DoesNotRequireEncounterCompletion()
        {
            var transition = New("start", "side", RegionTransitionRequirement.Free);
            Assert.That(transition.CanUse(false, true), Is.True);
            Assert.That(transition.TryUse(false, true), Is.True);
            Assert.That(transition.IsConsumed, Is.False);
            Assert.That(transition.TryUse(false, true), Is.True);
        }

        [Test]
        public void EncounterGate_RequiresCompletionButNotGlobalEnemyClear()
        {
            var transition = New("room", "locked", RegionTransitionRequirement.EncounterCompleted);
            Assert.That(transition.CanUse(false, true), Is.False);
            Assert.That(transition.TryUse(false, true), Is.False);
            Assert.That(transition.IsConsumed, Is.False);
            Assert.That(transition.TryUse(true, true), Is.True);
        }

        [Test]
        public void UnavailableDestination_DoesNotConsumeOrAllow()
        {
            var transition = New("room", "next", RegionTransitionRequirement.Free);
            Assert.That(transition.CanUse(false, false), Is.False);
            Assert.That(transition.TryUse(false, false), Is.False);
            Assert.That(transition.IsConsumed, Is.False);
        }

        [Test]
        public void ReturnableTransition_CanBeUsedRepeatedly()
        {
            var transition = New("a", "b", RegionTransitionRequirement.Free,
                RegionTransitionDirection.Returnable);
            Assert.That(transition.TryUse(false, true), Is.True);
            Assert.That(transition.TryUse(false, true), Is.True);
            Assert.That(transition.IsConsumed, Is.False);
        }

        [Test]
        public void OneWayTransition_IsConsumedOnlyAfterSuccessfulUse()
        {
            var transition = New("a", "b", RegionTransitionRequirement.Free,
                RegionTransitionDirection.OneWay);
            Assert.That(transition.TryUse(false, false), Is.False);
            Assert.That(transition.IsConsumed, Is.False);
            Assert.That(transition.TryUse(false, true), Is.True);
            Assert.That(transition.IsConsumed, Is.True);
            Assert.That(transition.TryUse(false, true), Is.False);
        }

        [Test]
        public void TraversalMetadata_IsRetainedForExecutionLayer()
        {
            var transition = New("map_a", "map_b", RegionTransitionRequirement.Free,
                RegionTransitionDirection.Returnable, RegionTransitionTraversal.SceneLoad);
            Assert.That(transition.SourceId, Is.EqualTo("map_a"));
            Assert.That(transition.DestinationId, Is.EqualTo("map_b"));
            Assert.That(transition.Traversal, Is.EqualTo(RegionTransitionTraversal.SceneLoad));
        }

        private static RegionTransition New(
            string source,
            string destination,
            RegionTransitionRequirement requirement = RegionTransitionRequirement.Free,
            RegionTransitionDirection direction = RegionTransitionDirection.Returnable,
            RegionTransitionTraversal traversal = RegionTransitionTraversal.Walk)
        {
            return new RegionTransition(source, destination, requirement, traversal, direction);
        }
    }
}
