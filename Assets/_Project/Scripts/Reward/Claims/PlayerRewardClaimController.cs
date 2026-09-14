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

            if (session.IsClaimed || session.HasClaimed(definition))
            {
                return RewardClaimResult.AlreadyClaimed;
            }

            /*
             * Offer membership is validated before any
             * acquisition side-effect.
             */
            session.ValidateSelection(
                definition);

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

        public string GetChoiceState(ItemDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            var buildController = GetComponent<ProjectFirstRun.Builds.PlayerBuildController>();
            if (buildController == null || !buildController.IsInitialized) return "NEW";
            if (!buildController.Build.TryGetItem(definition.Category, definition.StableId, out var owned)) return "NEW";
            return owned.IsAtMaximumLevel ? "MAX" : "LEVEL UP";
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
                    weaponAcquisitionController.TryAcquire,
                    weaponAcquisitionController.TryLevelUp));

            registry.Register(
                new AbilityRewardClaimHandler(
                    abilityAcquisitionController.TryAcquire,
                    abilityAcquisitionController.TryLevelUp));

            registry.Register(
                new UpgradeRewardClaimHandler(
                    upgradeController.TryAcquire,
                    upgradeController.TryLevelUp));

            _registry = registry;
        }
    }
}
