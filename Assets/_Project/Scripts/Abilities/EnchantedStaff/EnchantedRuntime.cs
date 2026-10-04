using System;
using System.Collections.Generic;
using ProjectFirstRun.Abilities.Execution;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;
namespace ProjectFirstRun.Abilities.EnchantedStaff
{
    public sealed class EnchantedRuntime : IAbilityExecutor, IMapEnemyRegistryBinding
    {
        private EnemyRegistry _registry;
        private readonly GameObject _source;
        private readonly PlayerStatCollection _stats;
        private readonly EnchantedDefinition _definition;
        private readonly System.Random _random = new System.Random();
        private readonly List<EnchantedBeam> _beams = new List<EnchantedBeam>();
        private EnchantedConfig _config;
        public EnchantedRuntime(EnchantedDefinition definition, EnemyRegistry registry, GameObject source, PlayerStatCollection stats)
        {
            _definition = definition != null ? definition : throw new ArgumentNullException(nameof(definition));
            _config = definition.CreateLevelConfigs()[0];
            _source = source != null ? source : throw new ArgumentNullException(nameof(source));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            BindEnemyRegistry(registry);
        }
        public void BindEnemyRegistry(EnemyRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            foreach (var beam in _beams) if (beam != null) beam.End();
            _beams.Clear(); _registry = registry;
        }
        internal void ApplyConfiguration(EnchantedConfig config) => _config = config;
        public AbilityExecutionResult TryExecute(in AbilityExecutionContext context)
        {
            if (_source == null || !_source.activeInHierarchy || _registry == null || Time.timeScale <= 0 ||
                (_source.GetComponent<HealthComponent>() != null && _source.GetComponent<HealthComponent>().IsDead))
                return AbilityExecutionResult.Failed;
            float damage = _stats.Evaluate(PlayerStatType.AbilityDamage, _config.Damage);
            if (!float.IsFinite(damage) || damage <= 0) throw new InvalidOperationException("Invalid enchanted damage.");
            // Start at the body, not a muzzle that could already be on the other side of cover.
            var collider = _source.GetComponent<Collider>();
            Vector3 origin = collider != null && collider.enabled ? collider.bounds.center : _source.transform.position + Vector3.up;
            _beams.RemoveAll(x => x == null || x.HasEnded);
            for (int i = 0; i < _config.Count; i++)
            {
                float angle = (float)_random.NextDouble() * Mathf.PI * 2;
                _beams.Add(EnchantedBeam.Launch(_source, _registry, _definition, _config, damage, origin,
                    new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle))));
            }
            return AbilityExecutionResult.Performed;
        }
    }
}
