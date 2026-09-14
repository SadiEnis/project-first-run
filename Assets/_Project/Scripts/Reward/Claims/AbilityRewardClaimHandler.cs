using System;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;

namespace ProjectFirstRun.Rewards.Claims
{
    public sealed class AbilityRewardClaimHandler :
        IRewardClaimHandler
    {
        private readonly Func<
            AbilityDefinition,
            AbilityAcquireResult> _tryAcquire;
        private readonly Func<AbilityDefinition, ItemLevelUpResult> _tryLevelUp;

        public AbilityRewardClaimHandler(
            Func<AbilityDefinition, AbilityAcquireResult>
                tryAcquire)
            : this(tryAcquire, null)
        {
        }

        public AbilityRewardClaimHandler(
            Func<AbilityDefinition, AbilityAcquireResult> tryAcquire,
            Func<AbilityDefinition, ItemLevelUpResult> tryLevelUp)
        {
            _tryAcquire =
                tryAcquire ??
                throw new ArgumentNullException(
                    nameof(tryAcquire));
            _tryLevelUp = tryLevelUp;
        }

        public bool Supports(
            ItemDefinition definition)
        {
            return definition is AbilityDefinition;
        }

        public RewardClaimResult TryClaim(
            ItemDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            if (definition is not AbilityDefinition
                abilityDefinition)
            {
                throw new ArgumentException(
                    $"{nameof(AbilityRewardClaimHandler)} " +
                    $"does not support item category " +
                    $"'{definition.Category}'.",
                    nameof(definition));
            }

            if (_tryLevelUp != null)
            {
                ItemLevelUpResult levelResult = _tryLevelUp(abilityDefinition);
                if (levelResult == ItemLevelUpResult.LevelIncreased) return RewardClaimResult.Claimed;
                if (levelResult == ItemLevelUpResult.MaximumLevelReached) return RewardClaimResult.AlreadyOwned;
            }

            AbilityAcquireResult result =
                _tryAcquire(
                    abilityDefinition);

            switch (result)
            {
                case AbilityAcquireResult.Acquired:
                    return RewardClaimResult.Claimed;

                case AbilityAcquireResult.AlreadyOwned:
                    return RewardClaimResult.AlreadyOwned;

                case AbilityAcquireResult.CapacityReached:
                    return RewardClaimResult.CapacityReached;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported ability acquisition result: " +
                        $"{result}.");
            }
        }
    }
}
