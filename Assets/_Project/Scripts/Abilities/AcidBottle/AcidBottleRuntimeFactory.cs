using System;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;
namespace ProjectFirstRun.Abilities.AcidBottle
{
    public sealed class AcidBottleRuntimeFactory : IAbilityRuntimeFactory, IMapEnemyRegistryBinding
    {
        private EnemyRegistry _registry;
        private readonly GameObject _source;
        private readonly PlayerStatCollection _stats;
        public AcidBottleRuntimeFactory(EnemyRegistry registry, GameObject source, PlayerStatCollection stats)
        {
            BindEnemyRegistry(registry);
            _source = source != null ? source : throw new ArgumentNullException(nameof(source));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
        }
        public void BindEnemyRegistry(EnemyRegistry registry) => _registry = registry != null
            ? registry : throw new ArgumentNullException(nameof(registry));
        public bool Supports(AbilityDefinition definition) => definition is AcidBottleDefinition;
        public AbilityRuntimeEntry Create(AbilityDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (!(definition is AcidBottleDefinition acid)) throw new ArgumentException("Unsupported ability.");
            var levels = acid.CreateLevelConfigs();
            var runtime = new AcidBottleRuntime(acid, _registry, _source, _stats);
            return new AbilityRuntimeEntry(acid, runtime, runtime, new Levels(levels, runtime, acid.CreateRuntimeConfig()));
        }
        private sealed class Levels : IAbilityLevelRuntime
        {
            private readonly AcidConfig[] _levels;
            private readonly AcidBottleRuntime _runtime;
            public int Level { get; private set; } = 1;
            public int MaximumLevel => _levels.Length;
            public AbilityRuntimeConfig InitialConfig { get; }
            public Levels(AcidConfig[] levels, AcidBottleRuntime runtime, AbilityRuntimeConfig config)
            { _levels = levels; _runtime = runtime; InitialConfig = config; }
            public void ValidateNextLevel(AbilityRuntimeState state)
            {
                if (state == null) throw new ArgumentNullException(nameof(state));
                if (Level >= MaximumLevel) throw new InvalidOperationException("Acid Bottle is at maximum level.");
            }
            public void AdvanceLevel(AbilityRuntimeState state)
            { ValidateNextLevel(state); _runtime.ApplyConfiguration(_levels[Level]); Level++; }
        }
    }
}
