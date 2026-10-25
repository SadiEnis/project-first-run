using System;
using ProjectFirstRun.Weapons;
using UnityEngine;

namespace ProjectFirstRun.Builds
{
    [CreateAssetMenu(fileName = "StartingLoadout", menuName = "Project First Run/Builds/Starting Loadout")]
    public sealed class PlayerStartingLoadoutSettings : ScriptableObject
    {
        [SerializeField] private WeaponDefinition _startingWeapon;
        [SerializeField, Range(1, PlayerBuildCapacity.MaximumWeaponSlots)] private int _weaponSlots = 1;
        [SerializeField, Range(1, PlayerBuildCapacity.MaximumAbilitySlots)] private int _abilitySlots = 3;
        [SerializeField, Range(1, PlayerBuildCapacity.MaximumUpgradeSlots)] private int _upgradeSlots = 5;

        public WeaponDefinition StartingWeapon => _startingWeapon;

        public PlayerBuildCapacity CreateCapacity() =>
            new PlayerBuildCapacity(_weaponSlots, _abilitySlots, _upgradeSlots);

        public void Validate()
        {
            if (_startingWeapon == null)
                throw new InvalidOperationException("Starting loadout settings require a weapon.");
            _ = CreateCapacity();
            _ = _startingWeapon.CreateRuntimeConfig();
        }
    }
}
