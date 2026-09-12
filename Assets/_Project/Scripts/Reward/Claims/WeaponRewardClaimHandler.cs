using System;
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

        public WeaponRewardClaimHandler(
            Func<WeaponDefinition, WeaponAcquireResult>
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