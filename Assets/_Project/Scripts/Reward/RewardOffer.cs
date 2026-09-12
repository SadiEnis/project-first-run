using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ProjectFirstRun.Items;

namespace ProjectFirstRun.Rewards
{
    public sealed class RewardOffer
    {
        private readonly List<ItemDefinition> _choices;
        private readonly ReadOnlyCollection<ItemDefinition>
            _readOnlyChoices;

        public IReadOnlyList<ItemDefinition> Choices =>
            _readOnlyChoices;

        public int ChoiceCount =>
            _choices.Count;

        public bool IsEmpty =>
            _choices.Count == 0;

        public RewardOffer(
            IReadOnlyList<ItemDefinition> choices)
        {
            if (choices == null)
            {
                throw new ArgumentNullException(
                    nameof(choices));
            }

            _choices =
                new List<ItemDefinition>(
                    choices.Count);

            for (int index = 0;
                 index < choices.Count;
                 index++)
            {
                ItemDefinition choice =
                    choices[index];

                ValidateChoice(
                    choice,
                    index);

                EnsureUniqueIdentity(
                    choice);

                _choices.Add(
                    choice);
            }

            _readOnlyChoices =
                _choices.AsReadOnly();
        }

        private static void ValidateChoice(
            ItemDefinition choice,
            int index)
        {
            if (choice == null)
            {
                throw new ArgumentException(
                    $"Reward choice at index {index} cannot be null.",
                    nameof(choice));
            }

            ValidateCategory(
                choice.Category);

            ValidateStableId(
                choice.StableId);
        }

        private void EnsureUniqueIdentity(
            ItemDefinition candidate)
        {
            for (int index = 0;
                 index < _choices.Count;
                 index++)
            {
                ItemDefinition existing =
                    _choices[index];

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

                throw new ArgumentException(
                    $"Reward offer contains duplicate item identity " +
                    $"'{candidate.Category}:{candidate.StableId}'.");
            }
        }

        private static void ValidateCategory(
            ItemCategory category)
        {
            switch (category)
            {
                case ItemCategory.Weapon:
                case ItemCategory.Ability:
                case ItemCategory.Upgrade:
                    return;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(category),
                        category,
                        "Unsupported reward item category.");
            }
        }

        private static void ValidateStableId(
            string stableId)
        {
            if (string.IsNullOrWhiteSpace(
                    stableId))
            {
                throw new ArgumentException(
                    "Reward item stable ID is required.",
                    nameof(stableId));
            }

            if (!string.Equals(
                    stableId,
                    stableId.Trim(),
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Reward item stable ID cannot contain " +
                    "leading or trailing whitespace.",
                    nameof(stableId));
            }
        }
    }
}