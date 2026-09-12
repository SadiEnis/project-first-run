using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;

namespace ProjectFirstRun.Rewards
{
    public sealed class RewardCandidateFilter
    {
        public IReadOnlyList<ItemDefinition> GetEligibleCandidates(
            IReadOnlyList<ItemDefinition> candidates,
            PlayerBuild playerBuild)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(
                    nameof(candidates));
            }

            if (playerBuild == null)
            {
                throw new ArgumentNullException(
                    nameof(playerBuild));
            }

            List<ItemDefinition> eligibleCandidates =
                new List<ItemDefinition>();

            for (int index = 0;
                 index < candidates.Count;
                 index++)
            {
                ItemDefinition candidate =
                    candidates[index];

                ValidateCandidate(
                    candidate,
                    index);

                if (!IsEligible(
                        candidate,
                        playerBuild))
                {
                    continue;
                }

                eligibleCandidates.Add(
                    candidate);
            }

            return new ReadOnlyCollection<ItemDefinition>(
                eligibleCandidates);
        }

        public bool IsEligible(
            ItemDefinition candidate,
            PlayerBuild playerBuild)
        {
            if (candidate == null)
            {
                throw new ArgumentNullException(
                    nameof(candidate));
            }

            if (playerBuild == null)
            {
                throw new ArgumentNullException(
                    nameof(playerBuild));
            }

            ValidateCategory(
                candidate.Category);

            ValidateStableId(
                candidate.StableId);

            if (playerBuild.Contains(
                    candidate.Category,
                    candidate.StableId))
            {
                return false;
            }

            if (!playerBuild.HasFreeSlot(
                    candidate.Category))
            {
                return false;
            }

            return true;
        }

        private static void ValidateCandidate(
            ItemDefinition candidate,
            int index)
        {
            if (candidate == null)
            {
                throw new ArgumentException(
                    $"Reward candidate at index {index} cannot be null.",
                    nameof(candidate));
            }

            ValidateCategory(
                candidate.Category);

            ValidateStableId(
                candidate.StableId);
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
                    "Reward candidate stable ID is required.",
                    nameof(stableId));
            }

            if (!string.Equals(
                    stableId,
                    stableId.Trim(),
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Reward candidate stable ID cannot contain " +
                    "leading or trailing whitespace.",
                    nameof(stableId));
            }
        }
    }
}