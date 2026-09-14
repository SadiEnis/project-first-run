using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public int MaxSelections { get; }

        public int SelectionsRemaining { get; private set; }

        public IReadOnlyList<ItemDefinition> ClaimedDefinitions =>
            _readOnlyClaimedDefinitions;

        public ItemDefinition ClaimedDefinition
        {
            get;
            private set;
        }

        private readonly List<ItemDefinition> _claimedDefinitions;
        private readonly ReadOnlyCollection<ItemDefinition>
            _readOnlyClaimedDefinitions;

        public RewardClaimSession(
            RewardOffer offer)
            : this(offer, 1)
        {
        }

        public RewardClaimSession(
            RewardOffer offer,
            int maxSelections)
        {
            Offer =
                offer ??
                throw new ArgumentNullException(
                    nameof(offer));

            if (maxSelections <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxSelections),
                    maxSelections,
                    "Maximum selections must be greater than zero.");
            }

            if (offer.ChoiceCount > 0 &&
                maxSelections > offer.ChoiceCount)
            {
                throw new ArgumentException(
                    "Maximum selections cannot exceed offer choice count.",
                    nameof(maxSelections));
            }

            MaxSelections = maxSelections;
            SelectionsRemaining = maxSelections;
            _claimedDefinitions = new List<ItemDefinition>(maxSelections);
            _readOnlyClaimedDefinitions = _claimedDefinitions.AsReadOnly();
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

            if (HasClaimed(definition))
            {
                throw new InvalidOperationException(
                    $"Item '{definition.name}' has already been claimed " +
                    "from this reward offer.");
            }

            if (IsClaimed)
            {
                throw new InvalidOperationException(
                    "Reward claim session has already been completed.");
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
                ClaimedDefinition ?? definition;

            _claimedDefinitions.Add(definition);

            SelectionsRemaining--;

            IsClaimed =
                SelectionsRemaining == 0;
        }

        public bool HasClaimed(ItemDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            for (int index = 0; index < _claimedDefinitions.Count; index++)
            {
                if (ReferenceEquals(_claimedDefinitions[index], definition))
                {
                    return true;
                }
            }

            return false;
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
