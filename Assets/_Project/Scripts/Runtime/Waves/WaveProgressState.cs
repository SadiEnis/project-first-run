using System;

namespace ProjectFirstRun.Waves
{
    public sealed class WaveProgressState
    {
        public int TotalPlannedEnemies
        {
            get;
        }

        public int SpawnedEnemies
        {
            get;
            private set;
        }

        public int LivingEnemies
        {
            get;
            private set;
        }

        public int DefeatedEnemies
        {
            get;
            private set;
        }

        public int RemainingToSpawn =>
            TotalPlannedEnemies - SpawnedEnemies;

        public bool IsSpawningCompleted
        {
            get;
            private set;
        }

        public bool IsCompleted =>
            IsSpawningCompleted &&
            SpawnedEnemies == TotalPlannedEnemies &&
            LivingEnemies == 0;

        public WaveProgressState(
            int totalPlannedEnemies)
        {
            if (totalPlannedEnemies <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(totalPlannedEnemies),
                    totalPlannedEnemies,
                    "The total planned enemy count must be greater than zero.");
            }

            TotalPlannedEnemies =
                totalPlannedEnemies;
        }

        public void RegisterSpawnedEnemy()
        {
            if (IsSpawningCompleted)
            {
                throw new InvalidOperationException(
                    "An enemy cannot be registered after spawning has completed.");
            }

            if (SpawnedEnemies >= TotalPlannedEnemies)
            {
                throw new InvalidOperationException(
                    "The number of spawned enemies cannot exceed " +
                    "the total planned enemy count.");
            }

            SpawnedEnemies++;
            LivingEnemies++;
        }

        public void RegisterDefeatedEnemy()
        {
            if (LivingEnemies <= 0)
            {
                throw new InvalidOperationException(
                    "A defeated enemy cannot be registered when " +
                    "there are no living enemies.");
            }

            LivingEnemies--;
            DefeatedEnemies++;
        }

        public void MarkSpawningCompleted()
        {
            if (IsSpawningCompleted)
            {
                throw new InvalidOperationException(
                    "Wave spawning has already been marked as completed.");
            }

            if (SpawnedEnemies != TotalPlannedEnemies)
            {
                throw new InvalidOperationException(
                    $"Wave spawning cannot be completed before all planned " +
                    $"enemies are spawned. Expected {TotalPlannedEnemies}, " +
                    $"but registered {SpawnedEnemies}.");
            }

            IsSpawningCompleted = true;
        }
    }
}