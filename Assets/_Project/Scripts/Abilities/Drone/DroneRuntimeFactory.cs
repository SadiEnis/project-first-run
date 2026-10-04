using System;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;
namespace ProjectFirstRun.Abilities.Drone
{
    public sealed class DroneRuntimeFactory : IAbilityRuntimeFactory, IMapEnemyRegistryBinding
    {
        private EnemyRegistry _registry;
        private readonly GameObject _source;
        private readonly PlayerStatCollection _stats;
        public DroneRuntimeFactory(EnemyRegistry registry, GameObject source, PlayerStatCollection stats)
        {
            BindEnemyRegistry(registry);
            _source = source != null ? source : throw new ArgumentNullException(nameof(source));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
        }
        public void BindEnemyRegistry(EnemyRegistry registry) => _registry = registry != null
            ? registry : throw new ArgumentNullException(nameof(registry));
        public bool Supports(AbilityDefinition definition) => definition is DroneDefinition;
        public AbilityRuntimeEntry Create(AbilityDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (!(definition is DroneDefinition drone)) throw new ArgumentException("Unsupported ability.");
            var configs = drone.CreateLevelConfigs();
            var runtime = new DroneRuntime(drone, _registry, _source, _stats);
            return new AbilityRuntimeEntry(drone, null, runtime, new Levels(configs, runtime, drone.CreateRuntimeConfig()));
        }
        private sealed class Levels : IAbilityLevelRuntime
        {
            private readonly DroneConfig[] _levels;
            private readonly DroneRuntime _runtime;
            public int Level { get; private set; } = 1;
            public int MaximumLevel => _levels.Length;
            public AbilityRuntimeConfig InitialConfig { get; }
            public Levels(DroneConfig[] levels, DroneRuntime runtime, AbilityRuntimeConfig config)
            { _levels = levels; _runtime = runtime; InitialConfig = config; }
            public void ValidateNextLevel(AbilityRuntimeState state)
            {
                if (state == null) throw new ArgumentNullException(nameof(state));
                if (Level >= MaximumLevel) throw new InvalidOperationException("Drone is at maximum level.");
            }
            public void AdvanceLevel(AbilityRuntimeState state)
            { ValidateNextLevel(state); _runtime.ApplyConfiguration(_levels[Level]); Level++; }
        }
    }
}
