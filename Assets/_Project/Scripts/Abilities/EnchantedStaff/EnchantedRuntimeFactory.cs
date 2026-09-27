using System;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;
namespace ProjectFirstRun.Abilities.EnchantedStaff
{
    public sealed class EnchantedRuntimeFactory : IAbilityRuntimeFactory, IMapEnemyRegistryBinding
    {
        private EnemyRegistry _registry;
        private readonly GameObject _source;
        private readonly PlayerStatCollection _stats;
        public EnchantedRuntimeFactory(EnemyRegistry registry, GameObject source, PlayerStatCollection stats)
        {
            BindEnemyRegistry(registry);
            _source = source != null ? source : throw new ArgumentNullException(nameof(source));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
        }
        public void BindEnemyRegistry(EnemyRegistry registry) => _registry = registry != null
            ? registry : throw new ArgumentNullException(nameof(registry));
        public bool Supports(AbilityDefinition definition) => definition is EnchantedDefinition;
        public AbilityRuntimeEntry Create(AbilityDefinition definition)
        {
            if (!(definition is EnchantedDefinition enchanted)) throw new ArgumentException("Unsupported ability.");
            var levels = enchanted.CreateLevelConfigs();
            var runtime = new EnchantedRuntime(enchanted, _registry, _source, _stats);
            return new AbilityRuntimeEntry(enchanted, null, runtime, new Levels(levels, runtime));
        }
        private sealed class Levels : IAbilityLevelRuntime
        {
            private readonly EnchantedConfig[] _levels;
            private readonly EnchantedRuntime _runtime;
            public int Level { get; private set; } = 1;
            public int MaximumLevel => _levels.Length;
            public AbilityRuntimeConfig InitialConfig => _levels[0].Ability;
            public Levels(EnchantedConfig[] levels, EnchantedRuntime runtime) { _levels = levels; _runtime = runtime; }
            public void ValidateNextLevel(AbilityRuntimeState state)
            {
                if (state == null) throw new ArgumentNullException(nameof(state));
                if (Level >= MaximumLevel) throw new InvalidOperationException("Enchanted Staff is at maximum level.");
                state.ValidateConfiguration(_levels[Level].Ability);
            }
            public void AdvanceLevel(AbilityRuntimeState state)
            {
                ValidateNextLevel(state);
                var config = _levels[Level]; state.ApplyConfiguration(config.Ability);
                _runtime.ApplyConfiguration(config); Level++;
            }
        }
    }
}
