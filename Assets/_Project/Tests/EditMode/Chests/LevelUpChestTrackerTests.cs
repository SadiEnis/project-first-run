using System;
using NUnit.Framework;
using ProjectFirstRun.Chests.Spawning;

namespace ProjectFirstRun.Tests.EditMode.Chests
{
    public sealed class LevelUpChestTrackerTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        public void InvalidInitialLevel_IsRejected(int level) =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new LevelUpChestTracker(level));

        [Test]
        public void LevelOne_DoesNotAwardAChest()
        {
            var tracker = new LevelUpChestTracker(1);
            tracker.ObserveLevel(1);
            Assert.That(tracker.PendingCount, Is.Zero);
            Assert.That(tracker.SpawnedCount, Is.Zero);
        }

        [Test]
        public void MultiLevelGain_EarnsOneChestPerLevel()
        {
            var tracker = new LevelUpChestTracker(1);
            tracker.ObserveLevel(4);
            Assert.That(tracker.PendingCount, Is.EqualTo(3));
            tracker.RecordSpawned();
            Assert.That(tracker.PendingCount, Is.EqualTo(2));
            Assert.That(tracker.SpawnedCount, Is.EqualTo(1));
        }

        [Test]
        public void RepeatedAndStaleLevels_DoNotDuplicateEntitlements()
        {
            var tracker = new LevelUpChestTracker(1);
            tracker.ObserveLevel(4);
            tracker.RecordSpawned();
            tracker.ObserveLevel(4);
            tracker.ObserveLevel(2);
            tracker.ObserveLevel(5);
            Assert.That(tracker.PendingCount, Is.EqualTo(3));
            Assert.That(tracker.HighestObservedLevel, Is.EqualTo(5));
        }

        [Test]
        public void BindingExistingLevel_DoesNotPayHistoricalLevels()
        {
            var tracker = new LevelUpChestTracker(8);
            tracker.ObserveLevel(8);
            Assert.That(tracker.PendingCount, Is.Zero);
            tracker.ObserveLevel(9);
            Assert.That(tracker.PendingCount, Is.EqualTo(1));
        }

        [Test]
        public void InvalidObservationAndEmptyConsume_PreserveState()
        {
            var tracker = new LevelUpChestTracker(1);
            Assert.Throws<ArgumentOutOfRangeException>(() => tracker.ObserveLevel(0));
            Assert.Throws<InvalidOperationException>(() => tracker.RecordSpawned());
            Assert.That(tracker.PendingCount, Is.Zero);
            Assert.That(tracker.HighestObservedLevel, Is.EqualTo(1));
        }

        [Test]
        public void MaximumRepresentableLevel_DoesNotOverflowCounters()
        {
            var tracker = new LevelUpChestTracker(1);
            tracker.ObserveLevel(int.MaxValue);
            tracker.RecordSpawned();
            Assert.That(tracker.PendingCount, Is.EqualTo(int.MaxValue - 2));
            tracker.ObserveLevel(int.MaxValue);
            Assert.That(tracker.PendingCount + tracker.SpawnedCount, Is.EqualTo(int.MaxValue - 1));
        }
    }
}
