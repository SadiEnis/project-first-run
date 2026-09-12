#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using ProjectFirstRun.Waves;
using UnityEngine;

namespace ProjectFirstRun.Development.Waves
{
    [DisallowMultipleComponent]
    public sealed class WaveDevelopmentBootstrap : MonoBehaviour
    {
        [Header("Wave")]
        [SerializeField]
        private ArenaWaveDefinition _arenaDefinition;

        [SerializeField]
        private WaveController _waveController;

        [Header("Enemy Services")]
        [SerializeField]
        private EnemySpawner _enemySpawner;

        [SerializeField]
        private EnemyRegistry _enemyRegistry;

        [Header("Target")]
        [SerializeField]
        private Transform _target;

        [SerializeField]
        private HealthComponent _targetHealth;

        [Header("Spawn Points")]
        [SerializeField]
        private List<Transform> _spawnPoints =
            new List<Transform>();

        public bool IsInitialized =>
            _waveController != null &&
            _waveController.IsInitialized;
        
        public void InitializeWaveSystem()
        {
            if (_waveController.IsInitialized)
            {
                return;
            }

            if (!ValidateReferences())
            {
                throw new InvalidOperationException(
                    "Wave development bootstrap is not configured correctly.");
            }

            RoundRobinSpawnPointSelector spawnPointSelector =
                new RoundRobinSpawnPointSelector(
                    _spawnPoints);

            WaveEnemySpawner waveEnemySpawner =
                new WaveEnemySpawner(
                    _enemySpawner,
                    _enemyRegistry,
                    _target,
                    _targetHealth,
                    spawnPointSelector);

            _waveController.Initialize(
                _arenaDefinition,
                waveEnemySpawner);
        }

        private bool ValidateReferences()
        {
            bool isValid = true;

            if (_arenaDefinition == null)
            {
                LogMissing(
                    nameof(_arenaDefinition));

                isValid = false;
            }

            if (_waveController == null)
            {
                LogMissing(
                    nameof(_waveController));

                isValid = false;
            }

            if (_enemySpawner == null)
            {
                LogMissing(
                    nameof(_enemySpawner));

                isValid = false;
            }

            if (_enemyRegistry == null)
            {
                LogMissing(
                    nameof(_enemyRegistry));

                isValid = false;
            }

            if (_target == null)
            {
                LogMissing(
                    nameof(_target));

                isValid = false;
            }

            if (_targetHealth == null)
            {
                LogMissing(
                    nameof(_targetHealth));

                isValid = false;
            }

            if (_spawnPoints == null ||
                _spawnPoints.Count == 0)
            {
                Debug.LogError(
                    $"{nameof(WaveDevelopmentBootstrap)} " +
                    "requires at least one spawn point.",
                    this);

                isValid = false;
            }
            else
            {
                for (int index = 0;
                     index < _spawnPoints.Count;
                     index++)
                {
                    if (_spawnPoints[index] != null)
                    {
                        continue;
                    }

                    Debug.LogError(
                        $"{nameof(WaveDevelopmentBootstrap)} " +
                        $"contains a null spawn point at index {index}.",
                        this);

                    isValid = false;
                }
            }

            return isValid;
        }

        private void LogMissing(
            string fieldName)
        {
            Debug.LogError(
                $"{nameof(WaveDevelopmentBootstrap)} " +
                $"requires {fieldName}.",
                this);
        }
    }
}

#endif