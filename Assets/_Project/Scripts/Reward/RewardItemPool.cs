using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ProjectFirstRun.Items;
using UnityEngine;

namespace ProjectFirstRun.Rewards
{
    [CreateAssetMenu(
        fileName = "RP_NewRewardPool",
        menuName = "Project First Run/Rewards/Reward Item Pool")]
    public sealed class RewardItemPool :
        ScriptableObject
    {
        [SerializeField]
        private List<ItemDefinition> _items =
            new List<ItemDefinition>();

        public int ItemCount =>
            _items.Count;

        public IReadOnlyList<ItemDefinition>
            GetValidatedItems()
        {
            ValidateContents();

            return new ReadOnlyCollection<ItemDefinition>(
                new List<ItemDefinition>(_items));
        }

        public void ValidateContents()
        {
            for (int index = 0;
                 index < _items.Count;
                 index++)
            {
                ItemDefinition item =
                    _items[index];

                ValidateItem(
                    item,
                    index);

                EnsureUniqueIdentity(
                    item,
                    index);
            }
        }

        private void EnsureUniqueIdentity(
            ItemDefinition candidate,
            int candidateIndex)
        {
            for (int index = 0;
                 index < candidateIndex;
                 index++)
            {
                ItemDefinition existing =
                    _items[index];

                if (existing.Category !=
                    candidate.Category)
                {
                    continue;
                }

                if (!string.Equals(
                        existing.StableId,
                        candidate.StableId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                throw new InvalidOperationException(
                    $"Reward pool '{name}' contains duplicate " +
                    $"item identity " +
                    $"'{candidate.Category}:{candidate.StableId}'.");
            }
        }

        private void ValidateItem(
            ItemDefinition item,
            int index)
        {
            if (item == null)
            {
                throw new InvalidOperationException(
                    $"Reward pool '{name}' contains a null " +
                    $"item at index {index}.");
            }

            ValidateCategory(
                item.Category,
                index);

            ValidateStableId(
                item.StableId,
                index);
        }

        private void ValidateCategory(
            ItemCategory category,
            int index)
        {
            switch (category)
            {
                case ItemCategory.Weapon:
                case ItemCategory.Ability:
                case ItemCategory.Upgrade:
                    return;

                default:
                    throw new InvalidOperationException(
                        $"Reward pool '{name}' contains an item " +
                        $"with unsupported category '{category}' " +
                        $"at index {index}.");
            }
        }

        private void ValidateStableId(
            string stableId,
            int index)
        {
            if (string.IsNullOrWhiteSpace(
                    stableId))
            {
                throw new InvalidOperationException(
                    $"Reward pool '{name}' contains an item " +
                    $"without a valid stable ID at index {index}.");
            }

            if (!string.Equals(
                    stableId,
                    stableId.Trim(),
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Reward pool '{name}' contains an item " +
                    $"with an untrimmed stable ID at index {index}.");
            }
        }
    }
}