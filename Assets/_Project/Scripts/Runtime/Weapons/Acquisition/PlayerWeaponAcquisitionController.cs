using System;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
using UnityEngine;

namespace ProjectFirstRun.Weapons
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerBuildController))]
    [RequireComponent(typeof(PlayerWeaponController))]
    public sealed class PlayerWeaponAcquisitionController :
        MonoBehaviour
    {
        private PlayerBuildController _buildController;
        private PlayerWeaponController _weaponController;
        private PlayerWeaponLoadout _loadout;

        public event Action<WeaponDefinition>
            WeaponAcquired;

        public PlayerWeaponLoadout Loadout
        {
            get
            {
                EnsureRuntimeState();

                return _loadout;
            }
        }

        private void Awake()
        {
            EnsureRuntimeState();
        }

        public WeaponAcquireResult TryAcquire(
    WeaponDefinition definition)
{
    if (definition == null)
    {
        throw new ArgumentNullException(
            nameof(definition));
    }

    EnsureReady();

    /*
     * Create and validate the complete runtime entry
     * before evaluating acquisition state.
     *
     * This preserves the previous runtime validation
     * semantics while preparing the exact state instance
     * that will be installed on success.
     */
    PlayerWeaponRuntimeEntry runtimeEntry =
        new PlayerWeaponRuntimeEntry(
            definition);

    ValidateCurrentRuntimeConsistency();

    bool buildOwnsWeapon =
        _buildController.Contains(
            definition);

    bool loadoutContainsWeapon =
        _loadout.Contains(
            definition);

    if (buildOwnsWeapon !=
        loadoutContainsWeapon)
    {
        throw new InvalidOperationException(
            $"Weapon ownership/runtime loadout mismatch " +
            $"for '{definition.StableId}'.");
    }

    if (buildOwnsWeapon)
    {
        return WeaponAcquireResult.AlreadyOwned;
    }

    if (!_buildController.Build.HasFreeSlot(
            ItemCategory.Weapon))
    {
        return WeaponAcquireResult.CapacityReached;
    }

    PlayerBuildAddResult buildResult =
        _buildController.TryAdd(
            definition);

    switch (buildResult)
    {
        case PlayerBuildAddResult.Added:
            break;

        case PlayerBuildAddResult.AlreadyOwned:
            throw new InvalidOperationException(
                $"Weapon '{definition.StableId}' became " +
                "owned after acquisition preflight.");

        case PlayerBuildAddResult.CapacityReached:
            throw new InvalidOperationException(
                "Weapon capacity changed after " +
                "acquisition preflight.");

        default:
            throw new InvalidOperationException(
                $"Unsupported player build add result: " +
                $"{buildResult}.");
    }

    bool shouldBecomeActive =
        !_loadout.HasActiveWeapon;

    _loadout.Add(
        runtimeEntry);

    if (shouldBecomeActive)
    {
        EquipFirstWeapon(
            runtimeEntry);
    }

    WeaponAcquired?.Invoke(
        definition);

    return WeaponAcquireResult.Acquired;
}

        private void EquipFirstWeapon(
            PlayerWeaponRuntimeEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(
                    nameof(entry));
            }

            _loadout.SetActive(
                entry);

            _weaponController.Equip(
                entry);
        }

        private void ValidateCurrentRuntimeConsistency()
        {
            if (_loadout.HasActiveWeapon)
            {
                if (!_weaponController.IsInitialized)
                {
                    throw new InvalidOperationException(
                        "Weapon loadout has an active weapon but " +
                        "PlayerWeaponController is not initialized.");
                }

                if (!ReferenceEquals(
                        _loadout.ActiveEntry,
                        _weaponController.ActiveEntry))
                {
                    throw new InvalidOperationException(
                        "Weapon loadout active runtime entry does not " +
                        "match PlayerWeaponController.");
                }

                if (!ReferenceEquals(
                        _loadout.ActiveDefinition,
                        _weaponController.ActiveDefinition))
                {
                    throw new InvalidOperationException(
                        "Weapon loadout active definition does not " +
                        "match PlayerWeaponController.");
                }

                return;
            }

            if (_weaponController.IsInitialized)
            {
                throw new InvalidOperationException(
                    "PlayerWeaponController has an active weapon " +
                    "that is not represented by PlayerWeaponLoadout.");
            }
        }

        private void EnsureReady()
        {
            /*
             * Do not rely only on Awake.
             *
             * Explicit initialization tests intentionally use
             * an inactive GameObject, so runtime dependencies
             * must also be resolvable on demand.
             */
            EnsureRuntimeState();

            if (_buildController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerBuildController)} is required.");
            }

            if (_weaponController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerWeaponController)} is required.");
            }

            if (_loadout == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerWeaponLoadout)} has not been created.");
            }

            if (!_buildController.IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerBuildController)} must be " +
                    "initialized before acquiring weapons.");
            }
        }

        private void EnsureRuntimeState()
        {
            /*
             * GetComponent works even when the GameObject is
             * inactive. This makes the controller safe for both
             * normal Unity lifecycle usage and explicit tests.
             */
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

            if (_loadout == null)
            {
                _loadout =
                    new PlayerWeaponLoadout();
            }
        }
    }
}