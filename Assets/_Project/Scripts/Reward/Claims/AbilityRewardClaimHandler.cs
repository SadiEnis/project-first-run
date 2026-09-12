using System;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Items;

namespace ProjectFirstRun.Rewards.Claims
{
    public sealed class AbilityRewardClaimHandler :
        IRewardClaimHandler
    {
        private readonly Func<
            AbilityDefinition,
            AbilityAcquireResult> _tryAcquire;

        public AbilityRewardClaimHandler(
            Func<AbilityDefinition, AbilityAcquireResult>
                tryAcquire)
        {
            _tryAcquire =
                tryAcquire ??
                throw new ArgumentNullException(
                    nameof(tryAcquire));
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