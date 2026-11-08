using System;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;

namespace ProjectFirstRun.Abilities
{
    /// <summary>Installs ability factories after the player's chosen loadout is ready.</summary>
    [DisallowMultipleComponent]
    public sealed class PlayerAbilityRuntimeInstaller : MonoBehaviour
    {
        [SerializeField] private PlayerAbilityAcquisitionController _playerAbilityAcquisitionController;
        [SerializeField] private PlayerStatsController _playerStatsController;
        [SerializeField] private GameObject _damageSource;
        [SerializeField] private EnemyRegistry _enemyRegistry;
        private PlayerStartingLoadoutInitializer _loadout;
        private bool _started;
        private bool _listening;

        public bool IsInstalled { get; private set; }
        public event Action Installed;

        public void Configure(PlayerAbilityAcquisitionController acquisition,
            PlayerStatsController stats, GameObject damageSource, EnemyRegistry enemies)
        {
            if (_started || IsInstalled || _listening)
                throw new InvalidOperationException("Cannot reconfigure an active ability installer.");
            _playerAbilityAcquisitionController = acquisition;
            _playerStatsController = stats;
            _damageSource = damageSource;
            _enemyRegistry = enemies;
        }

        private void Start()
        {
            _started = true;
            Begin();
        }

        private void OnEnable()
        {
            if (_started) Begin();
        }

        private void OnDisable() => StopListening();
        private void OnDestroy() => StopListening();

        public void CancelPendingInstallation() => StopListening();

        public void Begin()
        {
            if (IsInstalled) return;
            ValidateDependencies();
            if (_loadout.IsInitialized)
            {
                Install();
                return;
            }
            if (!_listening)
            {
                _loadout.Initialized += HandleLoadoutReady;
                _listening = true;
            }
        }

        private void HandleLoadoutReady()
        {
            StopListening();
            Install();
        }

        private void StopListening()
        {
            if (_listening && _loadout != null)
                _loadout.Initialized -= HandleLoadoutReady;
            _listening = false;
        }

        public void Install()
        {
            if (IsInstalled)
                throw new InvalidOperationException("Ability factories have already been installed.");
            ValidateDependencies();
            if (!_loadout.IsInitialized || !_playerStatsController.IsInitialized)
                throw new InvalidOperationException("Loadout and stats must be ready before ability installation.");
            if (_playerAbilityAcquisitionController.IsInitialized)
                throw new InvalidOperationException("Ability acquisition already has an installation owner.");

            var registry = new AbilityRuntimeFactoryRegistry();
            var stats = _playerStatsController.Stats;
            registry.Register(new Fireball.FireballAbilityRuntimeFactory(_enemyRegistry, _damageSource, stats));
            registry.Register(new ForceWave.ForceWaveRuntimeFactory(_enemyRegistry, _damageSource, stats));
            registry.Register(new Drone.DroneRuntimeFactory(_enemyRegistry, _damageSource, stats));
            registry.Register(new AcidBottle.AcidBottleRuntimeFactory(_enemyRegistry, _damageSource, stats));
            registry.Register(new Lightning.LightningRuntimeFactory(_enemyRegistry, _damageSource, stats));
            registry.Register(new Shuriken.ShurikenRuntimeFactory(_enemyRegistry, _damageSource, stats));
            registry.Register(new EnchantedStaff.EnchantedRuntimeFactory(_enemyRegistry, _damageSource, stats));
            registry.Register(new SniperBomb.SniperRuntimeFactory(_enemyRegistry, _damageSource, stats));
            _playerAbilityAcquisitionController.Initialize(registry);
            StopListening();
            IsInstalled = true;
            Installed?.Invoke();
        }

        private void ValidateDependencies()
        {
            if (_playerAbilityAcquisitionController == null || _playerStatsController == null ||
                _damageSource == null || _enemyRegistry == null)
                throw new InvalidOperationException("Assign acquisition, stats, damage source and enemy registry.");
            _loadout = _playerAbilityAcquisitionController.GetComponent<PlayerStartingLoadoutInitializer>();
            if (_loadout == null)
                throw new InvalidOperationException("The ability player requires a starting loadout initializer.");
            if (_playerStatsController.gameObject != _playerAbilityAcquisitionController.gameObject)
                throw new InvalidOperationException("Ability acquisition and stats must belong to the same player.");
        }
    }
}
