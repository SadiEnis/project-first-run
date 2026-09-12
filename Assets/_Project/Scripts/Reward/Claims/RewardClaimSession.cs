using System;
using ProjectFirstRun.Items;

namespace ProjectFirstRun.Rewards.Claims
{
    public sealed class RewardClaimSession
    {
        public RewardOffer Offer
        {
            get;
        }

        public bool IsClaimed
        {
            get;
            private set;
        }

        public ItemDefinition ClaimedDefinition
        {
            get;
            private set;
        }

        public RewardClaimSession(
            RewardOffer offer)
        {
            Offer =
                offer ??
                throw new ArgumentNullException(
                    nameof(offer));
        }

        public void ValidateSelection(
            ItemDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            if (!ContainsExactChoice(
                    definition))
            {
                throw new InvalidOperationException(
                    $"Item '{definition.name}' does not belong " +
                    "to this reward offer.");
            }
        }

        public void Commit(
            ItemDefinition definition)
        {
            ValidateSelection(
                definition);

            if (IsClaimed)
            {
                throw new InvalidOperationException(
                    "Reward claim session has already been claimed.");
            }

            ClaimedDefinition =
                definition;

            IsClaimed =
                true;
        }

        private bool ContainsExactChoice(
            ItemDefinition definition)
        {
            for (int index = 0;
                 index < Offer.ChoiceCount;
                 index++)
            {
                if (ReferenceEquals(
                        Offer.Choices[index],
                        definition))
                {
                    return true;
                }
            }

            return false;
        }
    }
}