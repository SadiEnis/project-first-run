using System;
using ProjectFirstRun.Abilities.Targeting;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;

namespace ProjectFirstRun.Abilities.Fireball
{
    public sealed class FireballAbilityRuntimeFactory :
        IAbilityRuntimeFactory, IMapEnemyRegistryBinding
    {
        private EnemyRegistry _enemyRegistry;

        public void BindEnemyRegistry(EnemyRegistry registry) => _enemyRegistry = registry != null
            ? registry : throw new ArgumentNullException(nameof(registry));
        private readonly GameObject _damageSource;
        private readonly PlayerStatCollection _stats;

        public FireballAbilityRuntimeFactory(
            EnemyRegistry enemyRegistry,
            GameObject damageSource,
            PlayerStatCollection stats)
        {
            _enemyRegistry =
                enemyRegistry != null
                    ? enemyRegistry
                    : throw new ArgumentNullException(
                        nameof(enemyRegistry));

            _damageSource =
                damageSource != null
                    ? damageSource
                    : throw new ArgumentNullException(
                        nameof(damageSource));

            _stats =
                stats ??
                throw new ArgumentNullException(
                    nameof(stats));
        }

        public bool Supports(
            AbilityDefinition definition)
        {
            return definition is FireballDefinition;
        }

        AbilityRuntimeEntry IAbilityRuntimeFactory.Create(
            AbilityDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            if (!(definition is FireballDefinition
                    fireballDefinition))
            {
                throw new ArgumentException(
                    $"{nameof(FireballAbilityRuntimeFactory)} " +
                    "only supports Fireball definitions.",
                    nameof(definition));
            }

            return Create(
                fireballDefinition);
        }

        public AbilityRuntimeEntry Create(
            FireballDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            ValidateRuntimeDependencies();

            NearestEnemyTargetSelector targetSelector =
                new NearestEnemyTargetSelector(
                    _enemyRegistry);

            FireballAbilityExecutor executor =
                new FireballAbilityExecutor(
                    definition,
                    _damageSource,
                    _stats);

            FireballRuntimeConfig[] levels = definition.CreateLevelConfigs();
            executor.ApplyConfiguration(levels[0]);
            return new AbilityRuntimeEntry(
                definition,
                targetSelector,
                executor,
                new FireballAbilityLevelRuntime(levels, executor));
        }

        private void ValidateRuntimeDependencies()
        {
            if (_enemyRegistry == null)
            {
                throw new InvalidOperationException(
                    "The enemy registry is no longer available.");
            }

            if (_damageSource == null)
            {
                throw new InvalidOperationException(
                    "The Fireball damage source is no longer available.");
            }
        }
    }
}
