using System;
using ProjectFirstRun.Weapons;
using UnityEngine;

namespace ProjectFirstRun.Builds
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerBuildController))]
    [RequireComponent(typeof(PlayerWeaponController))]
    [RequireComponent(typeof(PlayerWeaponAcquisitionController))]
    public sealed class PlayerStartingLoadoutInitializer :
        MonoBehaviour
    {
        [Header("Starting Loadout")]
        [SerializeField] private PlayerStartingLoadoutSettings _settings;
        [Tooltip("Fail closed if Settings is missing; enable on game scenes. Legacy fixtures may leave this off.")]
        [SerializeField] private bool _requireSettings;
        [Header("Legacy fixture fallback (ignored with Settings)")]
        [Tooltip("Legacy fixture fallback only. Ignored when Settings is assigned.")]
        [SerializeField]
        private WeaponDefinition _startingWeapon;

        private WeaponDefinition _initializedWeapon;
        public PlayerStartingLoadoutSettings Settings => _settings;

        private PlayerBuildController _buildController;
        private PlayerWeaponController _weaponController;

        private PlayerWeaponAcquisitionController
            _weaponAcquisitionController;

        private bool _isInitialized;

        public event Action Initialized;

        public bool IsInitialized =>
            _isInitialized;

        public WeaponDefinition StartingWeapon =>
            _isInitialized ? _initializedWeapon : (_settings != null ? _settings.StartingWeapon : _startingWeapon);

        private void Awake()
        {
            EnsureReferences();
        }

        private void Start()
        {
            if (_isInitialized)
            {
                return;
            }

            if (_requireSettings || _settings != null) InitializeConfigured();
            else Initialize(PlayerBuildCapacity.CreateDefault());
        }

        public void ValidateConfiguredSettings()
        {
            if (_settings == null)
                throw new InvalidOperationException("Assign starting loadout settings for this scene.");
            _settings.Validate();
        }

        public void InitializeConfigured()
        {
            ValidateConfiguredSettings();
            InitializeCore(_settings.CreateCapacity(), _settings.StartingWeapon);
        }

        public void Initialize(
            PlayerBuildCapacity capacity)
        {
            if (_requireSettings || _settings != null)
                throw new InvalidOperationException("Configured loadouts cannot use legacy capacity overrides. Use InitializeConfigured.");
            InitializeCore(capacity, _startingWeapon);
        }

        private void InitializeCore(PlayerBuildCapacity capacity, WeaponDefinition startingWeapon)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerStartingLoadoutInitializer)} " +
                    "has already been initialized.");
            }

            if (capacity == null)
            {
                throw new ArgumentNullException(
                    nameof(capacity));
            }

            if (startingWeapon == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerStartingLoadoutInitializer)} " +
                    "requires a starting weapon.");
            }

            EnsureReferences();

            if (_buildController.IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerBuildController)} " +
                    "must not already be initialized.");
            }

            if (_weaponController.IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerWeaponController)} " +
                    "must not already be initialized.");
            }

            /*
             * Validate weapon configuration before initializing
             * PlayerBuild so malformed starting content fails
             * before run ownership state is created.
             */
            _ = startingWeapon.CreateRuntimeConfig();

            _buildController.Initialize(
                capacity);

            WeaponAcquireResult acquireResult =
                _weaponAcquisitionController.TryAcquire(
                    startingWeapon);

            if (acquireResult !=
                WeaponAcquireResult.Acquired)
            {
                throw new InvalidOperationException(
                        $"Starting weapon '{startingWeapon.name}' " +
                    $"could not be acquired. " +
                    $"Result: {acquireResult}.");
            }

            _initializedWeapon = startingWeapon;
            _isInitialized = true;
            Initialized?.Invoke();
        }

        private void EnsureReferences()
        {
            if (_buildController == null)
            {
                _buildController =
                    GetComponent<PlayerBuildController>();
            }

            if (_weaponController == null)
            {
                _weaponController =
                    GetComponent<PlayerWeaponController>();
            }

            if (_weaponAcquisitionController == null)
            {
                _weaponAcquisitionController =
                    GetComponent<
                        PlayerWeaponAcquisitionController>();
            }
        }
    }
}
