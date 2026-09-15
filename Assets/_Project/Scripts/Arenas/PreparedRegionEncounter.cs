using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using ProjectFirstRun.Waves;
using Unity.Profiling;
using UnityEngine;

namespace ProjectFirstRun.Arenas
{
    /// <summary>Prepares one group over several frames; entry starts combat on existing instances.</summary>
    [DisallowMultipleComponent]
    public sealed class PreparedRegionEncounter : MonoBehaviour, IArenaSession
    {
        [SerializeField] private string _regionId;
        [SerializeField] private EnemyWaveDefinition _group;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private EnemySpawner _spawner;
        [SerializeField] private EnemyRegistry _registry;
        [SerializeField] private Transform _player;
        [SerializeField] private HealthComponent _playerHealth;
        [SerializeField, Min(1)] private int _maxEnemiesPerFrame = 2;
        [SerializeField, Min(.1f)] private float _millisecondsPerFrame = 2;

        private static readonly ProfilerMarker PrepareMarker = new ProfilerMarker("Region.PrepareStep");
        private static readonly ProfilerMarker ActivateMarker = new ProfilerMarker("Region.Activate");
        private readonly List<DormantEnemy> _prepared = new List<DormantEnemy>();
        private readonly List<EnemyController> _enemies = new List<EnemyController>();
        private readonly WaveEnemyTracker _tracker = new WaveEnemyTracker();
        private EnemySpawnRequest[] _requests;
        private GameObject _ownedRoot, _inactiveRoot;
        private bool _playerInside;

        public bool IsInitialized { get; private set; }
        public ArenaSessionStatus Status { get; private set; }
        public RegionPreparationStatus PreparationStatus { get; private set; }
        public RegionEncounterSession Region { get; private set; }
        public IReadOnlyList<EnemyController> Enemies => _enemies;
        public bool IsReadyForPassage => IsInitialized &&
            PreparationStatus == RegionPreparationStatus.Ready && Status != ArenaSessionStatus.Defeat;
        public Exception LastError { get; private set; }
        public int LastStepEnemyCount { get; private set; }
        public double LastPreparationStepMilliseconds { get; private set; }
        public double MaxPreparationStepMilliseconds { get; private set; }
        public double LastActivationMilliseconds { get; private set; }
        public event Action Victory;
        public event Action Defeat;

        public void Initialize(string regionId, EnemySpawner spawner,
            IReadOnlyList<EnemySpawnRequest> requests, HealthComponent playerHealth,
            int maxEnemiesPerFrame = 2, float millisecondsPerFrame = 2)
        {
            if (IsInitialized) throw new InvalidOperationException("Encounter already initialized.");
            if (spawner == null || playerHealth == null || requests == null || requests.Count == 0)
                throw new ArgumentException("Spawner, health and at least one spawn request are required.");
            if (maxEnemiesPerFrame < 1 || !float.IsFinite(millisecondsPerFrame) || millisecondsPerFrame <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxEnemiesPerFrame));
            var copy = new EnemySpawnRequest[requests.Count];
            for (int i = 0; i < copy.Length; i++)
            {
                requests[i].Validate();
                copy[i] = requests[i];
            }
            Region = new RegionEncounterSession(regionId);
            _requests = copy;
            _spawner = spawner;
            _playerHealth = playerHealth;
            _maxEnemiesPerFrame = maxEnemiesPerFrame;
            _millisecondsPerFrame = millisecondsPerFrame;
            _tracker.AllEnemiesDefeated += HandleVictory;
            _playerHealth.Died += HandlePlayerDied;
            IsInitialized = true;
        }

        private void Start()
        {
            InitializeFromDefinition();
        }

        public void BindScenePlayer(Transform player, HealthComponent health)
        {
            if (IsInitialized) throw new InvalidOperationException("Encounter already initialized.");
            if (_group == null) throw new InvalidOperationException("Scene encounter requires a group definition.");
            _player = player;
            _playerHealth = health;
            InitializeFromDefinition();
        }

        private void InitializeFromDefinition()
        {
            if (IsInitialized || _group == null) return;
            _group.Validate();
            if (_spawnPoints == null || _spawnPoints.Length == 0)
                throw new InvalidOperationException("Assign region spawn points.");
            var requests = new List<EnemySpawnRequest>(_group.TotalEnemyCount);
            foreach (EnemyWaveEntry entry in _group.Entries)
                for (int i = 0; i < entry.Count; i++)
                {
                    Transform point = _spawnPoints[requests.Count % _spawnPoints.Length];
                    if (point == null) throw new InvalidOperationException("Missing region spawn point.");
                    requests.Add(new EnemySpawnRequest(entry.EnemyPrefab, entry.EnemyDefinition,
                        _player, _playerHealth, _registry, point.position, point.rotation));
                }
            Initialize(_regionId, _spawner, requests, _playerHealth, _maxEnemiesPerFrame, _millisecondsPerFrame);
        }

        public void RequestPreparation()
        {
            if (!IsInitialized || !isActiveAndEnabled || PreparationStatus != RegionPreparationStatus.Idle)
                return;
            if (_playerHealth == null || _playerHealth.IsDead) { Cancel(); return; }
            _ownedRoot = new GameObject("Prepared region " + Region.RegionId);
            _ownedRoot.transform.SetParent(transform, true);
            _inactiveRoot = new GameObject("Inactive staging");
            _inactiveRoot.transform.SetParent(_ownedRoot.transform, false);
            _inactiveRoot.SetActive(false);
            PreparationStatus = RegionPreparationStatus.Preparing;
        }

        // One activation volume supplies aggregate player presence; leaving only clears intent.
        public void SetPlayerInside(bool inside)
        {
            _playerInside = inside;
            if (!inside) { Region?.Leave(); return; }
            RequestPreparation();
        }

        private void Update()
        {
            LastStepEnemyCount = 0;
            if (!IsInitialized || Time.timeScale <= 0) return;
            if (_playerHealth == null || _playerHealth.IsDead) { Cancel(); return; }
            if (PreparationStatus == RegionPreparationStatus.Preparing)
                PrepareStep();
            if (IsReadyForPassage && _playerInside &&
                !Region.IsPlayerInside &&
                Region.Status != RegionEncounterStatus.Failed)
            {
                try { Region.Enter(); }
                catch (Exception exception) { Fail(exception); }
            }
        }

        private void PrepareStep()
        {
            long started = Stopwatch.GetTimestamp();
            using (PrepareMarker.Auto())
            {
                try
                {
                    while (_prepared.Count < _requests.Length)
                    {
                        EnemySpawnRequest request = _requests[_prepared.Count];
                        EnemySpawnResult result = _spawner.SpawnInactive(in request, _inactiveRoot.transform);
                        // Keep ownership before any warmup operation that may fail.
                        var dormant = new DormantEnemy(result);
                        _prepared.Add(dormant);
                        _enemies.Add(result.EnemyController);
                        dormant.Warm(_ownedRoot.transform);
                        LastStepEnemyCount++;
                        if (LastStepEnemyCount >= _maxEnemiesPerFrame ||
                            Elapsed(started) >= _millisecondsPerFrame) break;
                    }
                    if (_prepared.Count == _requests.Length)
                    {
                        Region.Prepare(this);
                        PreparationStatus = RegionPreparationStatus.Ready;
                    }
                }
                catch (Exception exception) { Fail(exception); }
            }
            LastPreparationStepMilliseconds = Elapsed(started);
            MaxPreparationStepMilliseconds = Math.Max(MaxPreparationStepMilliseconds, LastPreparationStepMilliseconds);
        }

        public void Begin()
        {
            if (!IsReadyForPassage || Status != ArenaSessionStatus.Ready || _playerHealth.IsDead)
                throw new InvalidOperationException("Prepare the group before activating combat.");
            long started = Stopwatch.GetTimestamp();
            using (ActivateMarker.Auto())
            {
                try
                {
                    foreach (EnemyController enemy in _enemies)
                        if (enemy == null || enemy.Health.IsDead)
                            throw new InvalidOperationException("A prepared enemy is no longer valid.");
                    _tracker.TrackWave(_enemies);
                    Status = ArenaSessionStatus.Running;
                    foreach (DormantEnemy enemy in _prepared) enemy.Activate();
                }
                catch (Exception exception) { Fail(exception); throw; }
                finally { LastActivationMilliseconds = Elapsed(started); }
            }
        }

        public void Cancel()
        {
            if (!IsInitialized || PreparationStatus == RegionPreparationStatus.Cancelled ||
                PreparationStatus == RegionPreparationStatus.Failed) return;
            PreparationStatus = RegionPreparationStatus.Cancelled;
            Status = ArenaSessionStatus.Defeat;
            _playerInside = false;
            Cleanup();
            Defeat?.Invoke();
        }

        private void Fail(Exception exception)
        {
            if (PreparationStatus == RegionPreparationStatus.Failed) return;
            LastError = exception;
            PreparationStatus = RegionPreparationStatus.Failed;
            Status = ArenaSessionStatus.Defeat;
            _playerInside = false;
            Cleanup();
            Defeat?.Invoke();
        }

        private void HandleVictory()
        {
            if (Status != ArenaSessionStatus.Running) return;
            Status = ArenaSessionStatus.Victory;
            Victory?.Invoke();
        }
        private void HandlePlayerDied(DamageInfo info, DamageResult result) => Cancel();
        private static double Elapsed(long started) =>
            (Stopwatch.GetTimestamp() - started) * 1000d / Stopwatch.Frequency;

        private void Cleanup()
        {
            _tracker.Clear();
            if (_ownedRoot != null)
            {
                _ownedRoot.SetActive(false);
                Destroy(_ownedRoot);
            }
            _ownedRoot = null;
            _inactiveRoot = null;
            _prepared.Clear();
            _enemies.Clear();
        }
        private void OnDisable() => Cancel();
        private void OnDestroy()
        {
            if (_playerHealth != null) _playerHealth.Died -= HandlePlayerDied;
            _tracker.AllEnemiesDefeated -= HandleVictory;
            Cleanup();
            Region?.Dispose();
        }

        private sealed class DormantEnemy
        {
            private readonly EnemySpawnResult _result;
            private EnemyMotor _motor;
            private Collider[] _colliders;
            private bool[] _enabledColliders;
            public DormantEnemy(EnemySpawnResult result) => _result = result;

            public void Warm(Transform parent)
            {
                _motor = _result.Instance.GetComponent<EnemyMotor>();
                _result.EnemyController.enabled = false;
                _result.AttackController.enabled = false;
                _motor.enabled = false;
                _motor.Stop();
                _result.AttackController.Stop();
                _colliders = _result.Instance.GetComponentsInChildren<Collider>(true);
                _enabledColliders = new bool[_colliders.Length];
                for (int i = 0; i < _colliders.Length; i++)
                {
                    _enabledColliders[i] = _colliders[i].enabled;
                    _colliders[i].enabled = false;
                }
                _result.Instance.transform.SetParent(parent, true);
                _result.Instance.SetActive(true);
                _motor.Stop();
            }

            public void Activate()
            {
                for (int i = 0; i < _colliders.Length; i++)
                    if (_colliders[i] != null) _colliders[i].enabled = _enabledColliders[i];
                _motor.enabled = true;
                _result.EnemyController.enabled = true;
                _result.AttackController.enabled = true;
            }
        }
    }
}
