using System;

namespace ProjectFirstRun.Chests.Spawning
{
    public sealed class LevelUpChestTracker
    {
        public int HighestObservedLevel { get; private set; }
        public int PendingCount { get; private set; }
        public int SpawnedCount { get; private set; }

        public LevelUpChestTracker(int initialLevel)
        {
            if (initialLevel < 1) throw new ArgumentOutOfRangeException(nameof(initialLevel));
            HighestObservedLevel = initialLevel;
        }

        public void ObserveLevel(int level)
        {
            if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
            if (level <= HighestObservedLevel) return;
            PendingCount += level - HighestObservedLevel;
            HighestObservedLevel = level;
        }

        public void RecordSpawned()
        {
            if (PendingCount == 0) throw new InvalidOperationException("No pending level-up chest.");
            PendingCount--;
            SpawnedCount++;
        }
    }
}
