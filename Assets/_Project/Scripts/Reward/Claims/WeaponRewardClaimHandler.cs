using System;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
using ProjectFirstRun.Weapons;

namespace ProjectFirstRun.Rewards.Claims
{
    public sealed class WeaponRewardClaimHandler :
        IRewardClaimHandler
    {
        private readonly Func<
            WeaponDefinition,
            WeaponAcquireResult> _tryAcquire;
        private readonly Func<WeaponDefinition, ItemLevelUpResult> _tryLevelUp;

        public WeaponRewardClaimHandler(
            Func<WeaponDefinition, WeaponAcquireResult>
                tryAcquire)
            : this(tryAcquire, null)
        {
        }

        public WeaponRewardClaimHandler(
            Func<WeaponDefinition, WeaponAcquireResult> tryAcquire,
            Func<WeaponDefinition, ItemLevelUpResult> tryLevelUp)
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
            return definition is WeaponDefinition;
        }

        public RewardClaimResult TryClaim(
            ItemDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            if (definition is not WeaponDefinition
                weaponDefinition)
            {
                throw new ArgumentException(
                    $"{nameof(WeaponRewardClaimHandler)} " +
                    $"does not support item category " +
                    $"'{definition.Category}'.",
                    nameof(definition));
            }

            if (_tryLevelUp != null)
            {
                ItemLevelUpResult levelResult = _tryLevelUp(weaponDefinition);
                if (levelResult == ItemLevelUpResult.LevelIncreased) return RewardClaimResult.Claimed;
                if (levelResult == ItemLevelUpResult.MaximumLevelReached) return RewardClaimResult.AlreadyOwned;
            }

            WeaponAcquireResult result =
                _tryAcquire(
                    weaponDefinition);

            switch (result)
            {
                case WeaponAcquireResult.Acquired:
                    return RewardClaimResult.Claimed;

                case WeaponAcquireResult.AlreadyOwned:
                    return RewardClaimResult.AlreadyOwned;

                case WeaponAcquireResult.CapacityReached:
                    return RewardClaimResult.CapacityReached;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported weapon acquisition result: " +
                        $"{result}.");
            }
        }
    }
}
