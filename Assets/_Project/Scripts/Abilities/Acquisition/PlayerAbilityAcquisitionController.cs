using System;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
using UnityEngine;

namespace ProjectFirstRun.Abilities
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(PlayerBuildController),
        typeof(PlayerAbilityController))]
    public sealed class PlayerAbilityAcquisitionController :
        MonoBehaviour
    {
        private PlayerBuildController _buildController;
        private PlayerAbilityController _abilityController;
        private AbilityRuntimeFactoryRegistry _factoryRegistry;

        public bool IsInitialized =>
            _factoryRegistry != null;

        public void BindEnemyRegistry(ProjectFirstRun.Enemies.EnemyRegistry registry) =>
            _factoryRegistry?.BindEnemyRegistry(registry);

        public event Action<AbilityDefinition>
            AbilityAcquired;

        private void Awake()
        {
            _buildController =
                GetComponent<PlayerBuildController>();

            _abilityController =
                GetComponent<PlayerAbilityController>();
        }

        public void Initialize(
            AbilityRuntimeFactoryRegistry factoryRegistry)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerAbilityAcquisitionController)} " +
                    "has already been initialized.");
            }

            _factoryRegistry =
                factoryRegistry ??
                throw new ArgumentNullException(
                    nameof(factoryRegistry));
        }

        public AbilityAcquireResult TryAcquire(
            AbilityDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            EnsureReady();

            /*
             * These are preflight queries against PlayerBuild,
             * not a second ownership/capacity state.
             *
             * PlayerBuild remains the source of truth.
             */
            if (_buildController.Contains(
                    definition))
            {
                return AbilityAcquireResult.AlreadyOwned;
            }

            if (!_buildController.Build.HasFreeSlot(
                    ItemCategory.Ability))
            {
                return AbilityAcquireResult.CapacityReached;
            }

            IAbilityRuntimeFactory factory =
                _factoryRegistry.Resolve(
                    definition);

            AbilityRuntimeEntry runtimeEntry =
                factory.Create(
                    definition);

            ValidateRuntimeEntry(
                definition,
                runtimeEntry);

            PlayerBuildAddResult buildResult =
                _buildController.TryAdd(
                    definition);

            switch (buildResult)
            {
                case PlayerBuildAddResult.Added:
                    break;

                case PlayerBuildAddResult.AlreadyOwned:
                    return AbilityAcquireResult.AlreadyOwned;

                case PlayerBuildAddResult.CapacityReached:
                    return AbilityAcquireResult.CapacityReached;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported player build add result: " +
                        $"{buildResult}.");
            }

            _abilityController.AddAbility(
                runtimeEntry);

            AbilityAcquired?.Invoke(
                definition);

            return AbilityAcquireResult.Acquired;
        }

        public ItemLevelUpResult TryLevelUp(AbilityDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            EnsureReady();
            bool owns = _buildController.Build.TryGetItem(ItemCategory.Ability, definition.StableId, out var owned);
            AbilityRuntimeEntry entry = _abilityController.GetEntry(definition);
            if (owns != (entry != null)) throw new InvalidOperationException("Ability ownership/runtime mismatch.");
            if (!owns) return ItemLevelUpResult.NotOwned;
            if (entry == null || entry.Level != owned.Level || entry.MaximumLevel != owned.MaximumLevel)
                throw new InvalidOperationException("Ability level-up requires matching definition, level and maximum.");
            if (owned.IsAtMaximumLevel) return ItemLevelUpResult.MaximumLevelReached;
            entry.ValidateNextLevel();
            ItemLevelUpResult result = _buildController.Build.TryLevelUp(ItemCategory.Ability, definition.StableId);
            if (result != ItemLevelUpResult.LevelIncreased) throw new InvalidOperationException("Ability level changed after preflight.");
            entry.AdvanceLevel();
            return result;
        }

        private void EnsureReady()
        {
            if (_buildController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerBuildController)} " +
                    "is required.");
            }

            if (_abilityController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerAbilityController)} " +
                    "is required.");
            }

            if (!_buildController.IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerBuildController)} " +
                    "must be initialized before acquiring abilities.");
            }

            if (!IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerAbilityAcquisitionController)} " +
                    "must be initialized with an ability " +
                    "runtime factory registry before use.");
            }
        }

        private void ValidateRuntimeEntry(
            AbilityDefinition requestedDefinition,
            AbilityRuntimeEntry runtimeEntry)
        {
            if (runtimeEntry == null)
            {
                throw new InvalidOperationException(
                    "Ability runtime factory returned a null " +
                    "runtime entry.");
            }

            if (!ReferenceEquals(
                    runtimeEntry.Definition,
                    requestedDefinition))
            {
                throw new InvalidOperationException(
                    "Ability runtime factory returned an entry " +
                    "for a different ability definition.");
            }

            if (_abilityController.Contains(
                    runtimeEntry))
            {
                throw new InvalidOperationException(
                    "Ability runtime factory returned an entry " +
                    "that is already installed on the player.");
            }
        }
    }
}
