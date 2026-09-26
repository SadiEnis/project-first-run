using System;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;

namespace ProjectFirstRun.Abilities.ForceWave
{
    public sealed class ForceWaveRuntimeFactory : IAbilityRuntimeFactory, IMapEnemyRegistryBinding
    {
        private EnemyRegistry _registry;
        private readonly GameObject _source;
        private readonly PlayerStatCollection _stats;
        public ForceWaveRuntimeFactory(EnemyRegistry registry, GameObject source, PlayerStatCollection stats)
        {
            BindEnemyRegistry(registry);
            _source = source != null ? source : throw new ArgumentNullException(nameof(source));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
        }
        public void BindEnemyRegistry(EnemyRegistry registry) => _registry = registry != null
            ? registry : throw new ArgumentNullException(nameof(registry));
        public bool Supports(AbilityDefinition definition) => definition is ForceWaveDefinition;
        public AbilityRuntimeEntry Create(AbilityDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (!(definition is ForceWaveDefinition wave)) throw new ArgumentException("Unsupported ability.");
            var levels = wave.CreateLevelConfigs();
            var runtime = new ForceWaveRuntime(wave, _registry, _source, _stats);
            return new AbilityRuntimeEntry(wave, runtime, runtime, new Levels(levels, runtime));
        }
        private sealed class Levels : IAbilityLevelRuntime
        {
            private readonly ForceWaveConfig[] _levels;
            private readonly ForceWaveRuntime _runtime;
            public int Level { get; private set; } = 1;
            public int MaximumLevel => _levels.Length;
            public AbilityRuntimeConfig InitialConfig => _levels[0].Ability;
            public Levels(ForceWaveConfig[] levels, ForceWaveRuntime runtime) { _levels = levels; _runtime = runtime; }
            public void ValidateNextLevel(AbilityRuntimeState state)
            {
                if (state == null) throw new ArgumentNullException(nameof(state));
                if (Level >= MaximumLevel) throw new InvalidOperationException("Force Wave is at maximum level.");
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
