using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ProjectFirstRun.Items;

namespace ProjectFirstRun.Builds
{
    public sealed class PlayerBuild
    {
        private readonly Dictionary<(ItemCategory Category, string StableId), PlayerBuildItem> _itemStates =
            new Dictionary<(ItemCategory Category, string StableId), PlayerBuildItem>();

        private readonly List<string> _weapons =
            new List<string>();

        private readonly List<string> _abilities =
            new List<string>();

        private readonly List<string> _upgrades =
            new List<string>();

        private readonly ReadOnlyCollection<string> _readOnlyWeapons;
        private readonly ReadOnlyCollection<string> _readOnlyAbilities;
        private readonly ReadOnlyCollection<string> _readOnlyUpgrades;

        public PlayerBuildCapacity Capacity { get; }

        public IReadOnlyList<string> Weapons =>
            _readOnlyWeapons;

        public IReadOnlyList<string> Abilities =>
            _readOnlyAbilities;

        public IReadOnlyList<string> Upgrades =>
            _readOnlyUpgrades;

        public PlayerBuild(
            PlayerBuildCapacity capacity)
        {
            Capacity =
                capacity ??
                throw new ArgumentNullException(
                    nameof(capacity));

            _readOnlyWeapons =
                _weapons.AsReadOnly();

            _readOnlyAbilities =
                _abilities.AsReadOnly();

            _readOnlyUpgrades =
                _upgrades.AsReadOnly();
        }

        public PlayerBuildAddResult TryAdd(
            ItemCategory slotType,
            string stableId)
        {
            return TryAdd(slotType, stableId, 1);
        }

        public PlayerBuildAddResult TryAdd(
            ItemCategory slotType,
            string stableId,
            int maximumLevel)
        {
            ValidateStableId(stableId);
            if (maximumLevel < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumLevel),
                    maximumLevel, "Maximum item level must be positive.");
            }

            List<string> items =
                GetMutableItems(slotType);

            if (ContainsInternal(
                    items,
                    stableId))
            {
                return PlayerBuildAddResult.AlreadyOwned;
            }

            if (items.Count >=
                Capacity.GetCapacity(slotType))
            {
                return PlayerBuildAddResult.CapacityReached;
            }

            items.Add(stableId);
            _itemStates.Add((slotType, stableId), new PlayerBuildItem(slotType, stableId, maximumLevel));

            return PlayerBuildAddResult.Added;
        }

        public bool Contains(
            ItemCategory slotType,
            string stableId)
        {
            ValidateStableId(stableId);

            return ContainsInternal(
                GetMutableItems(slotType),
                stableId);
        }

        public bool TryGetItem(ItemCategory slotType, string stableId, out PlayerBuildItem item)
        {
            ValidateStableId(stableId);
            _ = GetMutableItems(slotType);
            return _itemStates.TryGetValue((slotType, stableId), out item);
        }

        public int GetLevel(ItemCategory slotType, string stableId)
        {
            return TryGetItem(slotType, stableId, out PlayerBuildItem item) ? item.Level : 0;
        }

        public ItemLevelUpResult TryLevelUp(ItemCategory slotType, string stableId)
        {
            if (!TryGetItem(slotType, stableId, out PlayerBuildItem item))
            {
                return ItemLevelUpResult.NotOwned;
            }

            return item.TryAdvance()
                ? ItemLevelUpResult.LevelIncreased
                : ItemLevelUpResult.MaximumLevelReached;
        }

        public int GetCount(
            ItemCategory slotType)
        {
            return GetMutableItems(slotType).Count;
        }

        public int GetCapacity(
            ItemCategory slotType)
        {
            return Capacity.GetCapacity(slotType);
        }

        public bool HasFreeSlot(
            ItemCategory slotType)
        {
            return GetCount(slotType) <
                   GetCapacity(slotType);
        }

        public bool IsFull(
            ItemCategory slotType)
        {
            return !HasFreeSlot(slotType);
        }

        public IReadOnlyList<string> GetItems(
            ItemCategory slotType)
        {
            return slotType switch
            {
                ItemCategory.Weapon =>
                    _readOnlyWeapons,

                ItemCategory.Ability =>
                    _readOnlyAbilities,

                ItemCategory.Upgrade =>
                    _readOnlyUpgrades,

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(slotType),
                        slotType,
                        "Unsupported player build slot type.")
            };
        }

        private List<string> GetMutableItems(
            ItemCategory slotType)
        {
            return slotType switch
            {
                ItemCategory.Weapon =>
                    _weapons,

                ItemCategory.Ability =>
                    _abilities,

                ItemCategory.Upgrade =>
                    _upgrades,

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(slotType),
                        slotType,
                        "Unsupported player build slot type.")
            };
        }

        private static bool ContainsInternal(
            IReadOnlyList<string> items,
            string stableId)
        {
            for (int index = 0;
                 index < items.Count;
                 index++)
            {
                if (string.Equals(
                        items[index],
                        stableId,
                        StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateStableId(
            string stableId)
        {
            if (string.IsNullOrWhiteSpace(stableId))
            {
                throw new ArgumentException(
                    "Stable item id cannot be null, empty or whitespace.",
                    nameof(stableId));
            }

            if (!string.Equals(
                    stableId,
                    stableId.Trim(),
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Stable item id cannot contain leading or trailing whitespace.",
                    nameof(stableId));
            }
        }
    }
}
