using System;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;
namespace ProjectFirstRun.Abilities.SniperBomb
{
    public sealed class SniperRuntimeFactory : IAbilityRuntimeFactory, IMapEnemyRegistryBinding
    {
        private EnemyRegistry _registry;
        private readonly GameObject _source;
        private readonly PlayerStatCollection _stats;
        public SniperRuntimeFactory(EnemyRegistry registry, GameObject source, PlayerStatCollection stats)
        {
            BindEnemyRegistry(registry);
            _source = source != null ? source : throw new ArgumentNullException(nameof(source));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
        }
        public void BindEnemyRegistry(EnemyRegistry registry) => _registry = registry != null
            ? registry : throw new ArgumentNullException(nameof(registry));
        public bool Supports(AbilityDefinition definition) => definition is SniperDefinition;
        public AbilityRuntimeEntry Create(AbilityDefinition definition)
        {
            if (!(definition is SniperDefinition sniper)) throw new ArgumentException("Unsupported ability.");
            var levels = sniper.CreateLevelConfigs();
            var runtime = new SniperRuntime(sniper, _registry, _source, _stats);
            return new AbilityRuntimeEntry(sniper, runtime, runtime, new Levels(levels, runtime));
        }
        private sealed class Levels : IAbilityLevelRuntime
        {
            private readonly SniperConfig[] _levels;
            private readonly SniperRuntime _runtime;
            public int Level { get; private set; } = 1;
            public int MaximumLevel => _levels.Length;
            public AbilityRuntimeConfig InitialConfig => _levels[0].Ability;
            public Levels(SniperConfig[] levels, SniperRuntime runtime) { _levels = levels; _runtime = runtime; }
            public void ValidateNextLevel(AbilityRuntimeState state)
            {
                if (state == null) throw new ArgumentNullException(nameof(state));
                if (Level >= MaximumLevel) throw new InvalidOperationException("Sniper Staff is at maximum level.");
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
