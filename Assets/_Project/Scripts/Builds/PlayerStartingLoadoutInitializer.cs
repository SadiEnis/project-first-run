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
        [SerializeField]
        private WeaponDefinition _startingWeapon;

        private PlayerBuildController _buildController;
        private PlayerWeaponController _weaponController;

        private PlayerWeaponAcquisitionController
            _weaponAcquisitionController;

        private bool _isInitialized;

        public bool IsInitialized =>
            _isInitialized;

        public WeaponDefinition StartingWeapon =>
            _startingWeapon;

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

            Initialize(
                PlayerBuildCapacity.CreateDefault());
        }

        public void Initialize(
            PlayerBuildCapacity capacity)
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

            if (_startingWeapon == null)
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
            _ = _startingWeapon.CreateRuntimeConfig();

            _buildController.Initialize(
                capacity);

            WeaponAcquireResult acquireResult =
                _weaponAcquisitionController.TryAcquire(
                    _startingWeapon);

            if (acquireResult !=
                WeaponAcquireResult.Acquired)
            {
                throw new InvalidOperationException(
                    $"Starting weapon '{_startingWeapon.name}' " +
                    $"could not be acquired. " +
                    $"Result: {acquireResult}.");
            }

            _isInitialized = true;
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