using System;
using System.Collections.Generic;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
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
        private readonly Dictionary<string, RuntimeEntry> _entries = new Dictionary<string, RuntimeEntry>(StringComparer.Ordinal);

        private sealed class RuntimeEntry
        {
            public UpgradeDefinition Definition;
            public StatModifier[][] Levels;
            public int Level = 1;
        }

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
            StatModifier[][] levels = definition.CreateLevelModifiers();
            _statsController.Stats.ValidateReplacement(Array.Empty<StatModifier>(), levels[0]);

            PlayerBuildAddResult buildResult =
                _buildController.TryAdd(
                    definition);

            switch (buildResult)
            {
                case PlayerBuildAddResult.Added:
                    _entries.Add(definition.StableId, new RuntimeEntry { Definition = definition, Levels = levels });
                    _statsController.Stats.ReplaceWithoutNotification(Array.Empty<StatModifier>(), levels[0]);
                    _statsController.Stats.NotifyChanged();

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

        public ItemLevelUpResult TryLevelUp(UpgradeDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            ValidateDependencies();
            bool owns = _buildController.Build.TryGetItem(ItemCategory.Upgrade, definition.StableId, out var owned);
            bool installed = _entries.TryGetValue(definition.StableId, out var entry);
            if (owns != installed) throw new InvalidOperationException("Upgrade ownership/runtime mismatch.");
            if (!owns) return ItemLevelUpResult.NotOwned;
            if (!ReferenceEquals(entry.Definition, definition) || entry.Level != owned.Level || entry.Levels.Length != owned.MaximumLevel)
                throw new InvalidOperationException("Upgrade definition or level mismatch.");
            var previous = entry.Levels[entry.Level - 1];
            // Validate installed effects even when capped.
            _statsController.Stats.ValidateReplacement(previous, Array.Empty<StatModifier>());
            if (owned.IsAtMaximumLevel) return ItemLevelUpResult.MaximumLevelReached;
            var next = entry.Levels[entry.Level];
            _statsController.Stats.ValidateReplacement(previous, next);
            var result = _buildController.Build.TryLevelUp(ItemCategory.Upgrade, definition.StableId);
            if (result != ItemLevelUpResult.LevelIncreased) throw new InvalidOperationException("Upgrade level changed after preflight.");
            entry.Level++;
            _statsController.Stats.ReplaceWithoutNotification(previous, next);
            _statsController.Stats.NotifyChanged();
            return result;
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
