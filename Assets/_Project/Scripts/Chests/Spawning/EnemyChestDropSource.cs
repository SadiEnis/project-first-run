using System;
using System.Collections.Generic;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Rewards;
using UnityEngine;

namespace ProjectFirstRun.Chests.Spawning
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ChestSpawnPlacement))]
    public sealed class EnemyChestDropSource : MonoBehaviour
    {
        [SerializeField] private HealthComponent _playerHealth;
        [SerializeField] private ChestSpawner _spawner;
        [SerializeField] private ChestSpawnPlacement _placement;

        private readonly Queue<PendingDrop> _pending = new Queue<PendingDrop>();
        private IRandomSource _random;
        private float _nextAttemptTime;

        public bool IsInitialized => _random != null;
        public int PendingChestCount => _pending.Count;
        public int SpawnedChestCount { get; private set; }

        private readonly struct PendingDrop
        {
            public readonly ChestDefinition Definition;
            public readonly Vector3 Position;
            public readonly Vector3 Forward;
            public PendingDrop(ChestDefinition definition, Vector3 position, Vector3 forward)
            {
                Definition = definition;
                Position = position;
                Forward = forward;
            }
        }

        public void Initialize(HealthComponent health, ChestSpawner spawner,
            ChestSpawnPlacement placement, IRandomSource random)
        {
            if (IsInitialized) throw new InvalidOperationException("Enemy chest source is already initialized.");
            if (health == null) throw new ArgumentNullException(nameof(health));
            if (spawner == null) throw new ArgumentNullException(nameof(spawner));
            if (placement == null) throw new ArgumentNullException(nameof(placement));
            if (random == null) throw new ArgumentNullException(nameof(random));
            _playerHealth = health;
            _spawner = spawner;
            _placement = placement;
            _random = random;
        }

        private void Awake()
        {
            if (!IsInitialized && _playerHealth != null) InitializeSerialized();
        }
        private void Start()
        {
            if (!IsInitialized) InitializeSerialized();
        }
        private void InitializeSerialized() =>
            Initialize(_playerHealth, _spawner, _placement, new UnityRandomSource());
        private void OnEnable() => _nextAttemptTime = 0f;

        public bool TryQueueDrop(EnemyChestDropProfile profile, Vector3 position, Vector3 forward)
        {
            // Serialized sources may receive a death before their own Awake (scene order is unspecified).
            if (!IsInitialized) InitializeSerialized();
            if (profile == null) return false;
            if (!IsFinite(position) || !IsFinite(forward))
                throw new ArgumentException("Death position and facing must be finite.");
            if (!profile.TryRoll(_random, out ChestDefinition definition)) return false;
            _placement.ValidatePrefab(definition.WorldPrefab);
            _pending.Enqueue(new PendingDrop(definition, position, forward));
            return true;
        }

        private void Update()
        {
            if (!IsInitialized || _pending.Count == 0 || Time.timeScale <= 0f || Time.time < _nextAttemptTime) return;
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
            if (!IsInitialized) throw new InvalidOperationException("Enemy chest source must be initialized.");
            if (!isActiveAndEnabled || _pending.Count == 0 || Time.timeScale <= 0f ||
                _playerHealth == null || _playerHealth.IsDead || _spawner == null ||
                !_spawner.isActiveAndEnabled || !_spawner.IsInitialized ||
                _placement == null || !_placement.isActiveAndEnabled) return false;

            PendingDrop drop = _pending.Peek();
            if (drop.Definition == null) throw new InvalidOperationException("Queued chest definition was destroyed.");
            if (!_placement.TryFind(drop.Position, drop.Forward, drop.Definition.WorldPrefab, true,
                    out Vector3 position, out Quaternion rotation))
            {
                _pending.Enqueue(_pending.Dequeue());
                return false;
            }
            var request = new ChestSpawnRequest(drop.Definition, position, rotation);
            chest = _spawner.Spawn(in request).ChestController;
            _pending.Dequeue();
            SpawnedChestCount++;
            Physics.SyncTransforms();
            return true;
        }

        private static bool IsFinite(Vector3 value) =>
            float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
    }
}
