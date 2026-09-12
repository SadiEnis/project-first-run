using System;
using System.Collections.Generic;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;

namespace ProjectFirstRun.Rewards
{
    public sealed class RewardOfferGenerator
    {
        private readonly RewardCandidateFilter _candidateFilter;
        private readonly IRandomSource _randomSource;

        public RewardOfferGenerator(
            RewardCandidateFilter candidateFilter,
            IRandomSource randomSource)
        {
            _candidateFilter =
                candidateFilter ??
                throw new ArgumentNullException(
                    nameof(candidateFilter));

            _randomSource =
                randomSource ??
                throw new ArgumentNullException(
                    nameof(randomSource));
        }

        public RewardOffer Generate(
            RewardItemPool itemPool,
            PlayerBuild playerBuild,
            int requestedChoiceCount)
        {
            if (itemPool == null)
            {
                throw new ArgumentNullException(
                    nameof(itemPool));
            }

            if (playerBuild == null)
            {
                throw new ArgumentNullException(
                    nameof(playerBuild));
            }

            if (requestedChoiceCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestedChoiceCount),
                    requestedChoiceCount,
                    "Requested reward choice count must be greater than zero.");
            }

            IReadOnlyList<ItemDefinition> poolItems =
                itemPool.GetValidatedItems();

            IReadOnlyList<ItemDefinition> eligibleCandidates =
                _candidateFilter.GetEligibleCandidates(
                    poolItems,
                    playerBuild);

            if (eligibleCandidates.Count == 0)
            {
                return new RewardOffer(
                    Array.Empty<ItemDefinition>());
            }

            int choiceCount =
                Math.Min(
                    requestedChoiceCount,
                    eligibleCandidates.Count);

            List<ItemDefinition> remainingCandidates =
                new List<ItemDefinition>(
                    eligibleCandidates);

            List<ItemDefinition> selectedChoices =
                new List<ItemDefinition>(
                    choiceCount);

            for (int index = 0;
                 index < choiceCount;
                 index++)
            {
                int selectedIndex =
                    _randomSource.Next(
                        0,
                        remainingCandidates.Count);

                ValidateRandomIndex(
                    selectedIndex,
                    remainingCandidates.Count);

                selectedChoices.Add(
                    remainingCandidates[selectedIndex]);

                remainingCandidates.RemoveAt(
                    selectedIndex);
            }

            return new RewardOffer(
                selectedChoices);
        }

        private static void ValidateRandomIndex(
            int selectedIndex,
            int candidateCount)
        {
            if (selectedIndex >= 0 &&
                selectedIndex < candidateCount)
            {
                return;
            }

            throw new InvalidOperationException(
                $"Random source returned index " +
                $"{selectedIndex} for candidate count " +
                $"{candidateCount}.");
        }
    }
}