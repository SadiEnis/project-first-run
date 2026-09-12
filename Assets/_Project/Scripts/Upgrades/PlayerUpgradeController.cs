using System;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Stats;
using UnityEngine;

namespace ProjectFirstRun.Upgrades
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(PlayerBuildController),
        typeof(PlayerStatsController))]
    public sealed class PlayerUpgradeController :
        MonoBehaviour
    {
        private PlayerBuildController _buildController;
        private PlayerStatsController _statsController;

        public event Action<UpgradeDefinition>
            UpgradeAcquired;

        public PlayerBuildController BuildController =>
            _buildController;

        public PlayerStatsController StatsController =>
            _statsController;

        private void Awake()
        {
            _buildController =
                GetComponent<PlayerBuildController>();

            _statsController =
                GetComponent<PlayerStatsController>();
        }

        public UpgradeAcquireResult TryAcquire(
            UpgradeDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            ValidateDependencies();

            /*
             * Create and validate all modifier objects before
             * changing PlayerBuild ownership.
             *
             * A malformed upgrade must not be inserted into
             * the build before its effects are known to be valid.
             */
            StatModifier[] runtimeModifiers =
                definition.CreateRuntimeModifiers();

            PlayerBuildAddResult buildResult =
                _buildController.TryAdd(
                    definition);

            switch (buildResult)
            {
                case PlayerBuildAddResult.Added:
                    ApplyModifiers(
                        runtimeModifiers);

                    UpgradeAcquired?.Invoke(
                        definition);

                    return UpgradeAcquireResult.Acquired;

                case PlayerBuildAddResult.AlreadyOwned:
                    return UpgradeAcquireResult.AlreadyOwned;

                case PlayerBuildAddResult.CapacityReached:
                    return UpgradeAcquireResult.CapacityReached;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported player build add result: " +
                        $"{buildResult}.");
            }
        }

        private void ApplyModifiers(
            StatModifier[] modifiers)
        {
            for (int index = 0;
                 index < modifiers.Length;
                 index++)
            {
                _statsController
                    .Stats
                    .Add(
                        modifiers[index]);
            }
        }

        private void ValidateDependencies()
        {
            if (_buildController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerUpgradeController)} " +
                    $"requires {nameof(PlayerBuildController)}.");
            }

            if (_statsController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerUpgradeController)} " +
                    $"requires {nameof(PlayerStatsController)}.");
            }

            if (!_buildController.IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerBuildController)} " +
                    "must be initialized before acquiring upgrades.");
            }

            if (!_statsController.IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerStatsController)} " +
                    "must be initialized before acquiring upgrades.");
            }
        }
    }
}