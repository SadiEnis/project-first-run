using System;
using ProjectFirstRun.Input;
using UnityEngine;

namespace ProjectFirstRun.Weapons
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(PlayerInputReader))]
    [RequireComponent(
        typeof(PlayerWeaponAcquisitionController))]
    [RequireComponent(
        typeof(PlayerWeaponController))]
    public sealed class PlayerWeaponSwitcher :
        MonoBehaviour
    {
        private PlayerInputReader
            _inputReader;

        private PlayerWeaponAcquisitionController
            _acquisitionController;

        private PlayerWeaponController
            _weaponController;

        public event Action<
            WeaponDefinition,
            WeaponDefinition>
            ActiveWeaponChanged;

        private void Awake()
        {
            EnsureReferences();
        }

        private void Update()
        {
            if (!_inputReader
                    .WasSwitchWeaponPressedThisFrame)
            {
                return;
            }

            TrySwitchNext();
        }

        public bool TrySwitchNext()
        {
            EnsureReady();

            if (!_weaponController
                    .IsWeaponControlEnabled)
            {
                return false;
            }

            PlayerWeaponLoadout loadout =
                _acquisitionController.Loadout;

            if (loadout.WeaponCount <= 1)
            {
                return false;
            }

            PlayerWeaponRuntimeEntry currentEntry =
                loadout.ActiveEntry;

            if (currentEntry == null)
            {
                throw new InvalidOperationException(
                    "Weapon switching requires an active " +
                    "runtime entry.");
            }

            if (!ReferenceEquals(
                    currentEntry,
                    _weaponController.ActiveEntry))
            {
                throw new InvalidOperationException(
                    "Weapon loadout active runtime entry does not " +
                    "match PlayerWeaponController.");
            }

            PlayerWeaponRuntimeEntry nextEntry =
                loadout.GetNextEntry();

            if (nextEntry == null)
            {
                throw new InvalidOperationException(
                    "Weapon loadout could not resolve the next " +
                    "runtime entry.");
            }

            if (ReferenceEquals(
                    currentEntry,
                    nextEntry))
            {
                return false;
            }

            WeaponDefinition previousDefinition =
                currentEntry.Definition;

            loadout.SetActive(
                nextEntry);

            _weaponController.Equip(
                nextEntry);

            ValidateResult(
                loadout,
                nextEntry);

            ActiveWeaponChanged?.Invoke(
                previousDefinition,
                nextEntry.Definition);

            return true;
        }

        private void ValidateResult(
            PlayerWeaponLoadout loadout,
            PlayerWeaponRuntimeEntry expectedEntry)
        {
            if (!ReferenceEquals(
                    loadout.ActiveEntry,
                    expectedEntry))
            {
                throw new InvalidOperationException(
                    "Weapon loadout did not activate the expected " +
                    "runtime entry.");
            }

            if (!ReferenceEquals(
                    _weaponController.ActiveEntry,
                    expectedEntry))
            {
                throw new InvalidOperationException(
                    "PlayerWeaponController did not equip the " +
                    "expected runtime entry.");
            }
        }

        private void EnsureReady()
        {
            EnsureReferences();

            if (_acquisitionController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerWeaponAcquisitionController)} " +
                    "is required.");
            }

            if (_weaponController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerWeaponController)} is required.");
            }
        }

        private void EnsureReferences()
        {
            if (_inputReader == null)
            {
                _inputReader =
                    GetComponent<PlayerInputReader>();
            }

            if (_acquisitionController == null)
            {
                _acquisitionController =
                    GetComponent<
                        PlayerWeaponAcquisitionController>();
            }

            if (_weaponController == null)
            {
                _weaponController =
                    GetComponent<
                        PlayerWeaponController>();
            }
        }
    }
}
