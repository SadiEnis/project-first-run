using System;
using System.Collections.Generic;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;

namespace ProjectFirstRun.Waves
{
    public sealed class WaveEnemyTracker
    {
        private readonly HashSet<EnemyController> _trackedEnemies =
            new HashSet<EnemyController>();

        public event Action<EnemyController> EnemyDefeated;
        public event Action AllEnemiesDefeated;

        public int TrackedEnemyCount =>
            _trackedEnemies.Count;

        public bool HasTrackedEnemies =>
            _trackedEnemies.Count > 0;

        public void TrackWave(
            IReadOnlyList<EnemyController> enemies)
        {
            ValidateWave(enemies);

            foreach (EnemyController enemy in enemies)
            {
                _trackedEnemies.Add(enemy);
                enemy.Died += HandleEnemyDied;
            }
        }

        public bool Contains(
            EnemyController enemy)
        {
            return enemy != null &&
                   _trackedEnemies.Contains(enemy);
        }

        public void Clear()
        {
            foreach (EnemyController enemy in _trackedEnemies)
            {
                if (enemy != null)
                {
                    enemy.Died -= HandleEnemyDied;
                }
            }

            _trackedEnemies.Clear();
        }

        private void HandleEnemyDied(
            EnemyController enemy,
            DamageInfo damageInfo,
            DamageResult damageResult)
        {
            if (enemy == null ||
                !_trackedEnemies.Remove(enemy))
            {
                return;
            }

            enemy.Died -= HandleEnemyDied;

            EnemyDefeated?.Invoke(enemy);

            if (_trackedEnemies.Count == 0)
            {
                AllEnemiesDefeated?.Invoke();
            }
        }

        private void ValidateWave(
            IReadOnlyList<EnemyController> enemies)
        {
            if (enemies == null)
            {
                throw new ArgumentNullException(
                    nameof(enemies));
            }

            if (enemies.Count == 0)
            {
                throw new ArgumentException(
                    "At least one enemy must be tracked.",
                    nameof(enemies));
            }

            if (_trackedEnemies.Count > 0)
            {
                throw new InvalidOperationException(
                    "A new wave cannot be tracked while enemies " +
                    "from the previous wave are still tracked.");
            }

            HashSet<EnemyController> uniqueEnemies =
                new HashSet<EnemyController>();

            for (int index = 0;
                 index < enemies.Count;
                 index++)
            {
                EnemyController enemy =
                    enemies[index];

                if (enemy == null)
                {
                    throw new ArgumentException(
                        $"The enemy collection contains a null " +
                        $"entry at index {index}.",
                        nameof(enemies));
                }

                if (!uniqueEnemies.Add(enemy))
                {
                    throw new ArgumentException(
                        $"The enemy collection contains a duplicate " +
                        $"entry at index {index}.",
                        nameof(enemies));
                }
            }
        }
    }
}