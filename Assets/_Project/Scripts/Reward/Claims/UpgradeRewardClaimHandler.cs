using System;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
using ProjectFirstRun.Upgrades;

namespace ProjectFirstRun.Rewards.Claims
{
    public sealed class UpgradeRewardClaimHandler :
        IRewardClaimHandler
    {
        private readonly Func<
            UpgradeDefinition,
            UpgradeAcquireResult> _tryAcquire;
        private readonly Func<UpgradeDefinition, ItemLevelUpResult> _tryLevelUp;

        public UpgradeRewardClaimHandler(
            Func<UpgradeDefinition, UpgradeAcquireResult>
                tryAcquire)
            : this(tryAcquire, null)
        {
        }

        public UpgradeRewardClaimHandler(
            Func<UpgradeDefinition, UpgradeAcquireResult> tryAcquire,
            Func<UpgradeDefinition, ItemLevelUpResult> tryLevelUp)
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
            return definition is UpgradeDefinition;
        }

        public RewardClaimResult TryClaim(
            ItemDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            if (definition is not UpgradeDefinition
                upgradeDefinition)
            {
                throw new ArgumentException(
                    $"{nameof(UpgradeRewardClaimHandler)} " +
                    $"does not support item category " +
                    $"'{definition.Category}'.",
                    nameof(definition));
            }

            if (_tryLevelUp != null)
            {
                ItemLevelUpResult levelResult = _tryLevelUp(upgradeDefinition);
                if (levelResult == ItemLevelUpResult.LevelIncreased) return RewardClaimResult.Claimed;
                if (levelResult == ItemLevelUpResult.MaximumLevelReached) return RewardClaimResult.AlreadyOwned;
            }

            UpgradeAcquireResult result =
                _tryAcquire(
                    upgradeDefinition);

            switch (result)
            {
                case UpgradeAcquireResult.Acquired:
                    return RewardClaimResult.Claimed;

                case UpgradeAcquireResult.AlreadyOwned:
                    return RewardClaimResult.AlreadyOwned;

                case UpgradeAcquireResult.CapacityReached:
                    return RewardClaimResult.CapacityReached;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported upgrade acquisition result: " +
                        $"{result}.");
            }
        }
    }
}
