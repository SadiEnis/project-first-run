using System;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;
namespace ProjectFirstRun.Abilities.Shuriken
{
    public sealed class ShurikenRuntimeFactory : IAbilityRuntimeFactory, IMapEnemyRegistryBinding
    {
        private EnemyRegistry _registry;
        private readonly GameObject _source;
        private readonly PlayerStatCollection _stats;
        public ShurikenRuntimeFactory(EnemyRegistry registry, GameObject source, PlayerStatCollection stats)
        {
            BindEnemyRegistry(registry);
            _source = source != null ? source : throw new ArgumentNullException(nameof(source));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
        }
        public void BindEnemyRegistry(EnemyRegistry registry) => _registry = registry != null
            ? registry : throw new ArgumentNullException(nameof(registry));
        public bool Supports(AbilityDefinition definition) => definition is ShurikenDefinition;
        public AbilityRuntimeEntry Create(AbilityDefinition definition)
        {
            if (!(definition is ShurikenDefinition shuriken)) throw new ArgumentException("Unsupported ability.");
            var configs = shuriken.CreateLevelConfigs();
            var runtime = new ShurikenRuntime(shuriken, _registry, _source, _stats);
            return new AbilityRuntimeEntry(shuriken, null, runtime, new Levels(configs, runtime));
        }
        private sealed class Levels : IAbilityLevelRuntime
        {
            private readonly ShurikenConfig[] _configs;
            private readonly ShurikenRuntime _runtime;
            public int Level { get; private set; } = 1;
            public int MaximumLevel => _configs.Length;
            public AbilityRuntimeConfig InitialConfig => new AbilityRuntimeConfig(_configs[0].Cooldown);
            public Levels(ShurikenConfig[] configs, ShurikenRuntime runtime) { _configs = configs; _runtime = runtime; }
            public void ValidateNextLevel(AbilityRuntimeState state)
            {
                if (state == null) throw new ArgumentNullException(nameof(state));
                if (Level >= MaximumLevel) throw new InvalidOperationException("Shuriken is at maximum level.");
            }
            public void AdvanceLevel(AbilityRuntimeState state)
            {
                ValidateNextLevel(state); _runtime.ApplyConfiguration(_configs[Level]); Level++;
            }
        }
    }
}
