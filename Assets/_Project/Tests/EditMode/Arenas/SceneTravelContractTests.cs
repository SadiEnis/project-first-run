using System;
using NUnit.Framework;
using ProjectFirstRun.Arenas;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class SceneTravelContractTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("Map")]
        [TestCase("Assets/../Map.unity")]
        [TestCase("Assets\\Map.unity")]
        [TestCase("Assets/Map.prefab")]
        public void InvalidScenePathsAreRejected(string path) =>
            Assert.Throws<ArgumentException>(() => new SceneTravelDestination(path, "entry"));

        [Test]
        public void FullPathAndEntryArePreserved()
        {
            var target = new SceneTravelDestination("Assets/Scenes/Map.unity", "north");
            Assert.That(target.ScenePath, Is.EqualTo("Assets/Scenes/Map.unity"));
            Assert.That(target.EntryId, Is.EqualTo("north"));
            Assert.Throws<ArgumentException>(() => new SceneTravelDestination(target.ScenePath, " "));
        }

        [Test]
        public void SceneDepartureAndWalkingMutuallyExcludeEachOther()
        {
            var map = new MapTraversalSession("a", new[] { "a", "b" });
            var route = new RegionTransition("a", "b", 0, 0, RegionTransitionDirection.Returnable);
            Assert.That(map.TryReserveDeparture(out var ticket), Is.True);
            Assert.That(map.TryReserveDeparture(out _), Is.False);
            Assert.That(map.TryBegin(route, true, true, out _), Is.False);
            Assert.That(map.ReleaseDeparture(new object()), Is.False);
            Assert.That(map.ReleaseDeparture(ticket), Is.True);
            Assert.That(map.ReleaseDeparture(ticket), Is.False);
            Assert.That(map.CurrentRegionId, Is.EqualTo("a"));
            Assert.That(map.TryBegin(route, true, true, out var walk), Is.True);
            Assert.That(map.TryReserveDeparture(out _), Is.False);
            map.Cancel(route, walk);
            Assert.That(map.TryReserveDeparture(out var next), Is.True);
            Assert.That(map.ReleaseDeparture(ticket), Is.False);
            Assert.That(map.ReleaseDeparture(next), Is.True);
        }
    }
}
