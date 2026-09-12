using System;
using ProjectFirstRun.Items;

namespace ProjectFirstRun.Builds
{
    public sealed class PlayerBuildCapacity
    {
        public const int DefaultWeaponSlots = 1;
        public const int DefaultAbilitySlots = 3;
        public const int DefaultUpgradeSlots = 5;

        public const int MaximumWeaponSlots = 2;
        public const int MaximumAbilitySlots = 5;
        public const int MaximumUpgradeSlots = 8;

        public int WeaponSlots { get; }
        public int AbilitySlots { get; }
        public int UpgradeSlots { get; }

        public PlayerBuildCapacity(
            int weaponSlots,
            int abilitySlots,
            int upgradeSlots)
        {
            ValidateCapacity(
                nameof(weaponSlots),
                weaponSlots,
                MaximumWeaponSlots);

            ValidateCapacity(
                nameof(abilitySlots),
                abilitySlots,
                MaximumAbilitySlots);

            ValidateCapacity(
                nameof(upgradeSlots),
                upgradeSlots,
                MaximumUpgradeSlots);

            WeaponSlots = weaponSlots;
            AbilitySlots = abilitySlots;
            UpgradeSlots = upgradeSlots;
        }

        public static PlayerBuildCapacity CreateDefault()
        {
            return new PlayerBuildCapacity(
                DefaultWeaponSlots,
                DefaultAbilitySlots,
                DefaultUpgradeSlots);
        }

        public int GetCapacity(
            ItemCategory  category)
        {
            return category switch
            {
                ItemCategory.Weapon =>
                    WeaponSlots,

                ItemCategory.Ability =>
                    AbilitySlots,

                ItemCategory.Upgrade =>
                    UpgradeSlots,

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(category),
                        category,
                        "Unsupported player build slot type.")
            };
        }

        private static void ValidateCapacity(
            string parameterName,
            int capacity,
            int maximumCapacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    capacity,
                    "Build capacity must be greater than zero.");
            }

            if (capacity > maximumCapacity)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    capacity,
                    $"Build capacity cannot exceed {maximumCapacity}.");
            }
        }
    }
}