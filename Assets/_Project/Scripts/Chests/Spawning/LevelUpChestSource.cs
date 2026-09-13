using System;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Progression;
using UnityEngine;

namespace ProjectFirstRun.Chests.Spawning
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ChestSpawnPlacement))]
    public sealed class LevelUpChestSource : MonoBehaviour
    {
        [SerializeField] private PlayerExperienceController _playerExperience;
        [SerializeField] private HealthComponent _playerHealth;
        [SerializeField] private ChestSpawner _spawner;
        [SerializeField] private ChestDefinition _definition;
        [SerializeField] private ChestSpawnPlacement _placement;

        private LevelUpChestTracker _tracker;
        private bool _isListening;
        private float _nextAttemptTime;

        public bool IsInitialized => _tracker != null;
        public int PendingChestCount => _tracker?.PendingCount ?? 0;
        public int SpawnedChestCount => _tracker?.SpawnedCount ?? 0;

        public void Initialize(PlayerExperienceController experience, HealthComponent health,
            ChestSpawner spawner, ChestDefinition definition, ChestSpawnPlacement placement)
        {
            if (IsInitialized) throw new InvalidOperationException("Level-up chest source is already initialized.");
            if (experience == null) throw new ArgumentNullException(nameof(experience));
            if (health == null) throw new ArgumentNullException(nameof(health));
            if (spawner == null) throw new ArgumentNullException(nameof(spawner));
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (placement == null) throw new ArgumentNullException(nameof(placement));
            if (experience.gameObject != health.gameObject)
                throw new InvalidOperationException("XP and health must belong to the same player.");
            definition.Validate();
            placement.ValidatePrefab(definition.WorldPrefab);

            _playerExperience = experience;
            _playerHealth = health;
            _spawner = spawner;
            _definition = definition;
            _placement = placement;
            _tracker = new LevelUpChestTracker(experience.IsInitialized ? experience.Level : 1);
            if (isActiveAndEnabled) StartListening();
        }

        private void Awake()
        {
            if (!IsInitialized && _playerExperience != null)
                Initialize(_playerExperience, _playerHealth, _spawner, _definition, _placement);
        }

        private void Start()
        {
            if (!IsInitialized)
                Initialize(_playerExperience, _playerHealth, _spawner, _definition, _placement);
        }

        private void OnEnable() => StartListening();
        private void OnDisable()
        {
            if (_isListening && _playerExperience != null)
                _playerExperience.LevelChanged -= HandleLevelChanged;
            _isListening = false;
        }

        private void StartListening()
        {
            if (!IsInitialized || _isListening) return;
            _playerExperience.LevelChanged += HandleLevelChanged;
            _isListening = true;
            _nextAttemptTime = 0f;
            SynchronizeLevel();
        }

        private void HandleLevelChanged(ExperienceGainResult result)
        {
            _tracker.ObserveLevel(result.CurrentLevel);
            _nextAttemptTime = 0f;
        }

        private void SynchronizeLevel()
        {
            if (_playerExperience != null && _playerExperience.IsInitialized)
                _tracker.ObserveLevel(_playerExperience.Level);
        }

        private void Update()
        {
            if (!IsInitialized) return;
            SynchronizeLevel();
            if (PendingChestCount == 0 || Time.timeScale <= 0f || Time.time < _nextAttemptTime) return;
            try
            {
                if (!TrySpawnPendingChest(out _)) _nextAttemptTime = Time.time + 0.25f;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                enabled = false;
            }
        }

        public bool TrySpawnPendingChest(out ChestController chest)
        {
            chest = null;
            if (!IsInitialized) throw new InvalidOperationException("Level-up chest source must be initialized.");
            SynchronizeLevel();
            if (!isActiveAndEnabled || PendingChestCount == 0 || Time.timeScale <= 0f ||
                _playerHealth == null || _playerHealth.IsDead || _playerExperience == null ||
                !_playerExperience.IsInitialized || _spawner == null || !_spawner.isActiveAndEnabled ||
                !_spawner.IsInitialized || _placement == null || !_placement.isActiveAndEnabled)
                return false;

            if (!_placement.TryFind(_playerExperience.transform, _definition.WorldPrefab,
                    out Vector3 position, out Quaternion rotation)) return false;
            ChestSpawnRequest request = new ChestSpawnRequest(_definition, position, rotation);
            chest = _spawner.Spawn(in request).ChestController;
            _tracker.RecordSpawned();
            Physics.SyncTransforms();
            return true;
        }
    }
}
