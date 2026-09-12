using System;
using ProjectFirstRun.Enemies;
using UnityEngine;

namespace ProjectFirstRun.Waves
{
    [Serializable]
    public sealed class EnemyWaveEntry
    {
        [SerializeField]
        private EnemyController _enemyPrefab;

        [SerializeField]
        private EnemyDefinition _enemyDefinition;

        [SerializeField, Min(1)]
        private int _count = 1;

        public EnemyController EnemyPrefab =>
            _enemyPrefab;

        public EnemyDefinition EnemyDefinition =>
            _enemyDefinition;

        public int Count =>
            _count;

        public EnemyWaveEntry(
            EnemyController enemyPrefab,
            EnemyDefinition enemyDefinition,
            int count)
        {
            if (enemyPrefab == null)
            {
                throw new ArgumentNullException(
                    nameof(enemyPrefab));
            }

            if (enemyDefinition == null)
            {
                throw new ArgumentNullException(
                    nameof(enemyDefinition));
            }

            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(count),
                    count,
                    "Enemy count must be greater than zero.");
            }

            _enemyPrefab = enemyPrefab;
            _enemyDefinition = enemyDefinition;
            _count = count;
        }

        public void Validate()
        {
            if (_enemyPrefab == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(EnemyWaveEntry)} requires an enemy prefab.");
            }

            if (_enemyDefinition == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(EnemyWaveEntry)} requires an enemy definition.");
            }

            if (_count <= 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(EnemyWaveEntry)} count must be greater than zero.");
            }
        }
    }
}