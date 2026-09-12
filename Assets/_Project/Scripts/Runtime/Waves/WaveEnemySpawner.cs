using System;
using System.Collections.Generic;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using UnityEngine;

namespace ProjectFirstRun.Waves
{
    public sealed class WaveEnemySpawner
    {
        private readonly EnemySpawner _enemySpawner;
        private readonly EnemyRegistry _enemyRegistry;
        private readonly Transform _target;
        private readonly IDamageable _targetDamageable;
        private readonly UnityEngine.Object _targetDamageableObject;
        private readonly RoundRobinSpawnPointSelector _spawnPointSelector;

        public WaveEnemySpawner(
            EnemySpawner enemySpawner,
            EnemyRegistry enemyRegistry,
            Transform target,
            IDamageable targetDamageable,
            RoundRobinSpawnPointSelector spawnPointSelector)
        {
            if (enemySpawner == null)
            {
                throw new ArgumentNullException(
                    nameof(enemySpawner));
            }

            if (enemyRegistry == null)
            {
                throw new ArgumentNullException(
                    nameof(enemyRegistry));
            }

            if (target == null)
            {
                throw new ArgumentNullException(
                    nameof(target));
            }

            if (targetDamageable == null)
            {
                throw new ArgumentNullException(
                    nameof(targetDamageable));
            }

            if (targetDamageable is not UnityEngine.Object
                damageableObject)
            {
                throw new ArgumentException(
                    "The target damageable must be a Unity object.",
                    nameof(targetDamageable));
            }

            if (damageableObject == null)
            {
                throw new ArgumentException(
                    "The target damageable Unity object has been destroyed.",
                    nameof(targetDamageable));
            }

            _enemySpawner = enemySpawner;
            _enemyRegistry = enemyRegistry;
            _target = target;
            _targetDamageable = targetDamageable;
            _targetDamageableObject = damageableObject;
            _spawnPointSelector =
                spawnPointSelector ??
                throw new ArgumentNullException(
                    nameof(spawnPointSelector));
        }

        public IReadOnlyList<EnemySpawnResult> SpawnWave(
            EnemyWaveDefinition waveDefinition)
        {
            if (waveDefinition == null)
            {
                throw new ArgumentNullException(
                    nameof(waveDefinition));
            }

            ValidateRuntimeDependencies();
            waveDefinition.Validate();

            List<EnemySpawnResult> spawnResults =
                new List<EnemySpawnResult>(
                    waveDefinition.TotalEnemyCount);

            try
            {
                SpawnEntries(
                    waveDefinition.Entries,
                    spawnResults);

                return spawnResults.ToArray();
            }
            catch
            {
                CleanupSpawnedEnemies(
                    spawnResults);

                throw;
            }
        }

        private void SpawnEntries(
            IReadOnlyList<EnemyWaveEntry> entries,
            ICollection<EnemySpawnResult> spawnResults)
        {
            foreach (EnemyWaveEntry entry in entries)
            {
                SpawnEntry(
                    entry,
                    spawnResults);
            }
        }

        private void SpawnEntry(
            EnemyWaveEntry entry,
            ICollection<EnemySpawnResult> spawnResults)
        {
            for (int enemyIndex = 0;
                 enemyIndex < entry.Count;
                 enemyIndex++)
            {
                Transform spawnPoint =
                    _spawnPointSelector.GetNext();

                EnemySpawnRequest request =
                    new EnemySpawnRequest(
                        entry.EnemyPrefab,
                        entry.EnemyDefinition,
                        _target,
                        _targetDamageable,
                        _enemyRegistry,
                        spawnPoint.position,
                        spawnPoint.rotation);

                EnemySpawnResult result =
                    _enemySpawner.Spawn(
                        in request);

                spawnResults.Add(result);
            }
        }

        private void ValidateRuntimeDependencies()
        {
            if (_enemySpawner == null)
            {
                throw new InvalidOperationException(
                    "The enemy spawner is no longer available.");
            }

            if (_enemyRegistry == null)
            {
                throw new InvalidOperationException(
                    "The enemy registry is no longer available.");
            }

            if (_target == null)
            {
                throw new InvalidOperationException(
                    "The wave target is no longer available.");
            }

            if (_targetDamageableObject == null)
            {
                throw new InvalidOperationException(
                    "The target damageable is no longer available.");
            }
        }

        private static void CleanupSpawnedEnemies(
            IReadOnlyList<EnemySpawnResult> spawnResults)
        {
            foreach (EnemySpawnResult result in spawnResults)
            {
                CleanupEnemyInstance(
                    result.Instance);
            }
        }

        private static void CleanupEnemyInstance(
            GameObject enemyInstance)
        {
            if (enemyInstance == null)
            {
                return;
            }

            enemyInstance.SetActive(false);

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(
                    enemyInstance);

                return;
            }

            UnityEngine.Object.DestroyImmediate(
                enemyInstance);
        }
    }
}