using System;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Progression;
using ProjectFirstRun.Rewards;
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
        [SerializeField] private ChestDropTable _dropTable;
        [SerializeField] private ChestSpawnPlacement _placement;

        private LevelUpChestTracker _tracker;
        private bool _isListening;
        private float _nextAttemptTime;
        private IRandomSource _random;
        private ChestDefinition _pendingDefinition;
        private bool _hasPendingSelection;

        public bool IsInitialized => _tracker != null;
        public int PendingChestCount => _tracker?.PendingCount ?? 0;
        public int SpawnedChestCount => _tracker?.SpawnedCount ?? 0;

        public void BindScenePlayer(GameObject player)
        {
            Initialize(player.GetComponent<PlayerExperienceController>(), player.GetComponent<HealthComponent>(),
                _spawner, _dropTable, _placement != null ? _placement : GetComponent<ChestSpawnPlacement>(),
                new UnityRandomSource());
        }

        public void Initialize(PlayerExperienceController experience, HealthComponent health,
            ChestSpawner spawner, ChestDropTable dropTable, ChestSpawnPlacement placement, IRandomSource random)
        {
            if (IsInitialized) throw new InvalidOperationException("Level-up chest source is already initialized.");
            if (experience == null) throw new ArgumentNullException(nameof(experience));
            if (health == null) throw new ArgumentNullException(nameof(health));
            if (spawner == null) throw new ArgumentNullException(nameof(spawner));
            if (dropTable == null) throw new ArgumentNullException(nameof(dropTable));
            if (placement == null) throw new ArgumentNullException(nameof(placement));
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (experience.gameObject != health.gameObject)
                throw new InvalidOperationException("XP and health must belong to the same player.");
            dropTable.Validate();
            foreach (WeightedChestEntry entry in dropTable.Entries)
                placement.ValidatePrefab(entry.Definition.WorldPrefab);

            _playerExperience = experience;
            _playerHealth = health;
            _spawner = spawner;
            _dropTable = dropTable;
            _random = random;
            _placement = placement;
            _tracker = new LevelUpChestTracker(experience.IsInitialized ? experience.Level : 1);
            if (isActiveAndEnabled) StartListening();
        }

        private void Awake()
        {
            if (!IsInitialized && _playerExperience != null)
                InitializeSerialized();
        }

        private void Start()
        {
            if (!IsInitialized)
                InitializeSerialized();
        }

        private void InitializeSerialized() =>
            Initialize(_playerExperience, _playerHealth, _spawner, _dropTable, _placement, new UnityRandomSource());

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

            if (!_hasPendingSelection)
            {
                if (_dropTable == null) throw new InvalidOperationException("Level-up chest drop table was destroyed.");
                _pendingDefinition = _dropTable.Select(_random);
                _hasPendingSelection = true;
            }
            if (_pendingDefinition == null)
                throw new InvalidOperationException("Selected level-up chest definition was destroyed.");

            if (!_placement.TryFind(_playerExperience.transform, _pendingDefinition.WorldPrefab,
                    out Vector3 position, out Quaternion rotation)) return false;
            ChestSpawnRequest request = new ChestSpawnRequest(_pendingDefinition, position, rotation);
            chest = _spawner.Spawn(in request).ChestController;
            _tracker.RecordSpawned();
            _pendingDefinition = null;
            _hasPendingSelection = false;
            Physics.SyncTransforms();
            return true;
        }
    }
}
