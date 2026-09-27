using System;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;
namespace ProjectFirstRun.Abilities.Lightning
{
    public sealed class LightningRuntimeFactory : IAbilityRuntimeFactory, IMapEnemyRegistryBinding
    {
        private EnemyRegistry _registry;
        private readonly GameObject _source;
        private readonly PlayerStatCollection _stats;
        public LightningRuntimeFactory(EnemyRegistry registry, GameObject source, PlayerStatCollection stats)
        {
            BindEnemyRegistry(registry);
            _source = source != null ? source : throw new ArgumentNullException(nameof(source));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
        }
        public void BindEnemyRegistry(EnemyRegistry registry) => _registry = registry != null
            ? registry : throw new ArgumentNullException(nameof(registry));
        public bool Supports(AbilityDefinition definition) => definition is LightningDefinition;
        public AbilityRuntimeEntry Create(AbilityDefinition definition)
        {
            if (!(definition is LightningDefinition lightning)) throw new ArgumentException("Unsupported ability.");
            var levels = lightning.CreateLevelConfigs();
            var runtime = new LightningRuntime(lightning, _registry, _source, _stats);
            return new AbilityRuntimeEntry(lightning, runtime, runtime, new Levels(levels, runtime));
        }
        private sealed class Levels : IAbilityLevelRuntime
        {
            private readonly LightningConfig[] _levels;
            private readonly LightningRuntime _runtime;
            public int Level { get; private set; } = 1;
            public int MaximumLevel => _levels.Length;
            public AbilityRuntimeConfig InitialConfig => _levels[0].Ability;
            public Levels(LightningConfig[] levels, LightningRuntime runtime) { _levels = levels; _runtime = runtime; }
            public void ValidateNextLevel(AbilityRuntimeState state)
            {
                if (state == null) throw new ArgumentNullException(nameof(state));
                if (Level >= MaximumLevel) throw new InvalidOperationException("Lightning Staff is at maximum level.");
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
