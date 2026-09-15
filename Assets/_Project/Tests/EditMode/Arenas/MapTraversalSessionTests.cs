using System;
using NUnit.Framework;
using ProjectFirstRun.Arenas;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class MapTraversalSessionTests
    {
        private static MapTraversalSession Map() => new MapTraversalSession("main", new[] { "main", "side", "next" });
        private static RegionTransition Route(string from, string to,
            RegionTransitionDirection direction = RegionTransitionDirection.Returnable,
            bool singleUse = false, RegionTransitionRequirement requirement = RegionTransitionRequirement.Free) =>
            new RegionTransition(from, to, requirement, RegionTransitionTraversal.Walk, direction, singleUse);

        [Test]
        public void SideTripThenOneWay_DoesNotRequireCombatCompletion()
        {
            var map = Map();
            var side = Route("main", "side");
            var next = Route("main", "next", RegionTransitionDirection.OneWay);
            Cross(map, side);
            Assert.That(map.CurrentRegionId, Is.EqualTo("side"));
            Assert.That(map.CanBegin(next, false, true), Is.False);
            Cross(map, side);
            Cross(map, next);
            Assert.That(map.CurrentRegionId, Is.EqualTo("next"));
            Assert.That(map.CanBegin(next, true, true), Is.False);
            Assert.That(map.ContainsRoute(side), Is.True, "One-way traversal does not delete regions.");
        }

        [Test]
        public void PendingRoute_BlocksEveryOtherRouteUntilCancellation()
        {
            var map = Map();
            var side = Route("main", "side", singleUse: true);
            var next = Route("main", "next");
            Assert.That(map.TryBegin(side, false, true, out var attempt), Is.True);
            Assert.That(map.CurrentRegionId, Is.EqualTo("main"));
            Assert.That(map.TryBegin(next, false, true, out _), Is.False);
            Assert.That(map.Complete(next, attempt), Is.False);
            Assert.That(map.Cancel(next, attempt), Is.False);
            Assert.That(map.IsTransitioning, Is.True);
            Assert.That(map.Cancel(side, attempt), Is.True);
            Assert.That(side.IsConsumed, Is.False);
            Assert.That(map.Complete(side, attempt), Is.False);
            Cross(map, next);
        }

        [Test]
        public void StaleAttempt_CannotCompleteOrCancelLaterTraversal()
        {
            var map = Map();
            var side = Route("main", "side", singleUse: true);
            map.TryBegin(side, false, true, out var stale);
            map.Cancel(side, stale);
            map.TryBegin(side, false, true, out var current);
            Assert.That(map.Cancel(side, stale), Is.False);
            Assert.That(map.Complete(side, stale), Is.False);
            Assert.That(map.Complete(side, current), Is.True);
            Assert.That(side.IsConsumed, Is.True);
            Assert.That(map.CanBegin(side, true, true), Is.False);
        }

        [Test]
        public void ExternallyCancelledRoute_DoesNotMoveMapOrLeakLock()
        {
            var map = Map();
            var side = Route("main", "side");
            map.TryBegin(side, false, true, out var attempt);
            side.Cancel(attempt);
            Assert.That(map.Complete(side, attempt), Is.False);
            Assert.That(map.CurrentRegionId, Is.EqualTo("main"));
            Assert.That(map.IsTransitioning, Is.False);
        }

        [Test]
        public void ExplicitGateAndReadinessAreStillRequired()
        {
            var map = Map();
            var gate = Route("main", "side", requirement: RegionTransitionRequirement.EncounterCompleted);
            Assert.That(map.CanBegin(gate, false, true), Is.False);
            Assert.That(map.CanBegin(gate, true, false), Is.False);
            Assert.That(map.CanBegin(gate, true, true), Is.True);
            Assert.That(map.CanBegin(Route("main", "unknown"), true, true), Is.False);
            Assert.That(map.CanBegin(null, true, true), Is.False);
        }

        [Test]
        public void InvalidMapConfigurationFailsEarly()
        {
            Assert.Throws<ArgumentNullException>(() => new MapTraversalSession("a", null));
            Assert.Throws<ArgumentException>(() => new MapTraversalSession("a", new[] { "a", "a" }));
            Assert.Throws<ArgumentException>(() => new MapTraversalSession("a", new[] { "a", " " }));
            Assert.Throws<ArgumentException>(() => new MapTraversalSession("a", new[] { "a", null }));
            Assert.Throws<ArgumentException>(() => new MapTraversalSession("b", new[] { "a" }));
            Assert.Throws<ArgumentException>(() => new MapTraversalSession("a", Array.Empty<string>()));
            Assert.That(new MapTraversalSession("a", new[] { "a", "A" }).CurrentRegionId, Is.EqualTo("a"));
        }

        private static void Cross(MapTraversalSession map, RegionTransition route)
        {
            Assert.That(map.TryBegin(route, false, true, out var attempt), Is.True);
            Assert.That(map.Complete(route, attempt), Is.True);
        }
    }
}
