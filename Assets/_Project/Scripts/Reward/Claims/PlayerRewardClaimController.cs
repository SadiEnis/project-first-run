using System;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Items;
using ProjectFirstRun.Upgrades;
using ProjectFirstRun.Weapons;
using UnityEngine;

namespace ProjectFirstRun.Rewards.Claims
{
    [DisallowMultipleComponent]
    public sealed class PlayerRewardClaimController :
        MonoBehaviour
    {
        private RewardClaimHandlerRegistry _registry;

        public event Action<ItemDefinition> RewardClaimed;

        public bool IsInitialized =>
            _registry != null;

        private void Awake()
        {
            EnsureRegistry();
        }

        public void Initialize(
            RewardClaimHandlerRegistry registry)
        {
            if (registry == null)
            {
                throw new ArgumentNullException(
                    nameof(registry));
            }

            if (_registry != null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerRewardClaimController)} " +
                    "has already been initialized.");
            }

            _registry = registry;
        }

        public RewardClaimResult TryClaim(
            RewardClaimSession session,
            ItemDefinition definition)
        {
            if (session == null)
            {
                throw new ArgumentNullException(
                    nameof(session));
            }

            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            EnsureRegistry();

            /*
             * Offer membership is validated before any
             * acquisition side-effect.
             */
            session.ValidateSelection(
                definition);

            if (session.IsClaimed)
            {
                return RewardClaimResult.AlreadyClaimed;
            }

            IRewardClaimHandler handler =
                _registry.Resolve(
                    definition);

            RewardClaimResult result =
                handler.TryClaim(
                    definition);

            switch (result)
            {
                case RewardClaimResult.Claimed:
                    CommitSuccessfulClaim(
                        session,
                        definition);

                    return RewardClaimResult.Claimed;

                case RewardClaimResult.AlreadyOwned:
                    return RewardClaimResult.AlreadyOwned;

                case RewardClaimResult.CapacityReached:
                    return RewardClaimResult.CapacityReached;

                case RewardClaimResult.AlreadyClaimed:
                    throw new InvalidOperationException(
                        "Reward claim handlers must not return " +
                        $"{nameof(RewardClaimResult.AlreadyClaimed)}.");

                default:
                    throw new InvalidOperationException(
                        $"Unsupported reward claim result: {result}.");
            }
        }

        private void CommitSuccessfulClaim(
            RewardClaimSession session,
            ItemDefinition definition)
        {
            session.Commit(
                definition);

            RewardClaimed?.Invoke(
                definition);
        }

        private void EnsureRegistry()
        {
            if (_registry != null)
            {
                return;
            }

            PlayerWeaponAcquisitionController
                weaponAcquisitionController =
                    GetComponent<
                        PlayerWeaponAcquisitionController>();

            PlayerAbilityAcquisitionController
                abilityAcquisitionController =
                    GetComponent<
                        PlayerAbilityAcquisitionController>();

            PlayerUpgradeController
                upgradeController =
                    GetComponent<
                        PlayerUpgradeController>();

            if (weaponAcquisitionController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerWeaponAcquisitionController)} " +
                    "is required.");
            }

            if (abilityAcquisitionController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerAbilityAcquisitionController)} " +
                    "is required.");
            }

            if (upgradeController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerUpgradeController)} is required.");
            }

            RewardClaimHandlerRegistry registry =
                new RewardClaimHandlerRegistry();

            registry.Register(
                new WeaponRewardClaimHandler(
                    weaponAcquisitionController.TryAcquire));

            registry.Register(
                new AbilityRewardClaimHandler(
                    abilityAcquisitionController.TryAcquire));

            registry.Register(
                new UpgradeRewardClaimHandler(
                    upgradeController.TryAcquire));

            _registry = registry;
        }
    }
}