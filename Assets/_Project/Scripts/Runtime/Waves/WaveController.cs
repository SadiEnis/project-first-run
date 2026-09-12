using System;
using System.Collections.Generic;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using UnityEngine;

namespace ProjectFirstRun.Waves
{
    [DisallowMultipleComponent]
    public sealed class WaveController : MonoBehaviour
    {
        private ArenaWaveDefinition _arenaDefinition;
        private WaveEnemySpawner _waveEnemySpawner;
        private WaveEnemyTracker _enemyTracker;

        private WaveSequenceState _sequenceState;
        private WaveProgressState _currentWaveProgress;
        private EnemyWaveDefinition _currentWaveDefinition;

        private bool _isInitialized;
        private bool _hasBegun;

        public event Action<int, EnemyWaveDefinition> WaveStarted;
        public event Action<int, EnemySpawnResult> EnemySpawned;
        public event Action<int, EnemyController> EnemyDefeated;
        public event Action<int, EnemyWaveDefinition> WaveCompleted;
        public event Action SequenceCompleted;

        public bool IsInitialized =>
            _isInitialized;

        public bool HasBegun =>
            _hasBegun;

        public WaveSequenceState SequenceState =>
            _sequenceState;

        public WaveProgressState CurrentWaveProgress =>
            _currentWaveProgress;

        public EnemyWaveDefinition CurrentWaveDefinition =>
            _currentWaveDefinition;

        public int CurrentWaveIndex =>
            _sequenceState != null
                ? _sequenceState.CurrentWaveIndex
                : -1;

        public int TrackedEnemyCount =>
            _enemyTracker != null
                ? _enemyTracker.TrackedEnemyCount
                : 0;

        public void Initialize(
            ArenaWaveDefinition arenaDefinition,
            WaveEnemySpawner waveEnemySpawner)
        {
            if (_enemyTracker != null &&
                _enemyTracker.HasTrackedEnemies)
            {
                throw new InvalidOperationException(
                    $"{nameof(WaveController)} cannot be reinitialized " +
                    "while enemies are still being tracked.");
            }

            if (arenaDefinition == null)
            {
                throw new ArgumentNullException(
                    nameof(arenaDefinition));
            }

            if (waveEnemySpawner == null)
            {
                throw new ArgumentNullException(
                    nameof(waveEnemySpawner));
            }

            arenaDefinition.Validate();

            EnsureEnemyTracker();
            _enemyTracker.Clear();

            _arenaDefinition =
                arenaDefinition;

            _waveEnemySpawner =
                waveEnemySpawner;

            _sequenceState =
                new WaveSequenceState(
                    arenaDefinition.WaveCount);

            _currentWaveProgress = null;
            _currentWaveDefinition = null;

            _hasBegun = false;
            _isInitialized = true;
        }

        public void Begin()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(WaveController)} must be initialized " +
                    "before beginning.");
            }

            if (_hasBegun)
            {
                throw new InvalidOperationException(
                    "The wave sequence has already begun.");
            }

            ValidateRuntimeState();

            _hasBegun = true;

            StartNextWave();
        }

        private void StartNextWave()
        {
            _sequenceState.BeginNextWave();

            int waveIndex =
                _sequenceState.CurrentWaveIndex;

            _currentWaveDefinition =
                _arenaDefinition.Waves[waveIndex];

            _currentWaveDefinition.Validate();

            _currentWaveProgress =
                new WaveProgressState(
                    _currentWaveDefinition.TotalEnemyCount);

            WaveStarted?.Invoke(
                waveIndex,
                _currentWaveDefinition);

            IReadOnlyList<EnemySpawnResult> spawnResults;

            try
            {
                spawnResults =
                    _waveEnemySpawner.SpawnWave(
                        _currentWaveDefinition);
            }
            catch
            {
                MarkSequenceFailed();
                throw;
            }

            RegisterSpawnedWave(
                waveIndex,
                spawnResults);
        }

        private void RegisterSpawnedWave(
            int waveIndex,
            IReadOnlyList<EnemySpawnResult> spawnResults)
        {
            List<EnemyController> enemies =
                new List<EnemyController>(
                    spawnResults.Count);

            foreach (EnemySpawnResult result in spawnResults)
            {
                _currentWaveProgress
                    .RegisterSpawnedEnemy();

                enemies.Add(
                    result.EnemyController);
            }

            _enemyTracker.TrackWave(
                enemies);

            foreach (EnemySpawnResult result in spawnResults)
            {
                EnemySpawned?.Invoke(
                    waveIndex,
                    result);
            }

            _currentWaveProgress
                .MarkSpawningCompleted();

            TryCompleteCurrentWave();
        }

        private void HandleEnemyDefeated(
            EnemyController enemy)
        {
            if (_currentWaveProgress == null)
            {
                return;
            }

            _currentWaveProgress
                .RegisterDefeatedEnemy();

            EnemyDefeated?.Invoke(
                _sequenceState.CurrentWaveIndex,
                enemy);
        }

        private void HandleAllEnemiesDefeated()
        {
            TryCompleteCurrentWave();
        }

        private void TryCompleteCurrentWave()
        {
            if (_currentWaveProgress == null ||
                !_currentWaveProgress.IsCompleted)
            {
                return;
            }

            int completedWaveIndex =
                _sequenceState.CurrentWaveIndex;

            EnemyWaveDefinition completedWave =
                _currentWaveDefinition;

            _enemyTracker.Clear();

            _sequenceState
                .CompleteCurrentWave();

            _currentWaveProgress = null;
            _currentWaveDefinition = null;

            WaveCompleted?.Invoke(
                completedWaveIndex,
                completedWave);

            if (_sequenceState.IsCompleted)
            {
                SequenceCompleted?.Invoke();
                return;
            }

            StartNextWave();
        }

        private void MarkSequenceFailed()
        {
            if (_sequenceState != null &&
                !_sequenceState.IsCompleted &&
                !_sequenceState.IsFailed)
            {
                _sequenceState.MarkFailed();
            }

            _enemyTracker?.Clear();

            _currentWaveProgress = null;
            _currentWaveDefinition = null;
        }

        private void ValidateRuntimeState()
        {
            if (_arenaDefinition == null)
            {
                throw new InvalidOperationException(
                    "The arena wave definition is no longer available.");
            }

            if (_waveEnemySpawner == null)
            {
                throw new InvalidOperationException(
                    "The wave enemy spawner is no longer available.");
            }
        }

        private void EnsureEnemyTracker()
        {
            if (_enemyTracker != null)
            {
                return;
            }

            _enemyTracker =
                new WaveEnemyTracker();

            _enemyTracker.EnemyDefeated +=
                HandleEnemyDefeated;

            _enemyTracker.AllEnemiesDefeated +=
                HandleAllEnemiesDefeated;
        }

        private void OnDestroy()
        {
            if (_enemyTracker == null)
            {
                return;
            }

            _enemyTracker.EnemyDefeated -=
                HandleEnemyDefeated;

            _enemyTracker.AllEnemiesDefeated -=
                HandleAllEnemiesDefeated;

            _enemyTracker.Clear();
        }
    }
}