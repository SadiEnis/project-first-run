using System;
using System.Collections.Generic;
using ProjectFirstRun.Abilities.Execution;
using ProjectFirstRun.Abilities.Targeting;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;
namespace ProjectFirstRun.Abilities.AcidBottle
{
    public sealed class AcidBottleRuntime : IAbilityExecutor, IAbilityTargetSelector, IMapEnemyRegistryBinding
    {
        private sealed class Plan { public EnemyController Enemy; public Vector3 Ground; }
        private EnemyRegistry _registry;
        private readonly GameObject _source;
        private readonly PlayerStatCollection _stats;
        private readonly Material _material;
        private readonly float _range;
        private readonly int _mask;
        private readonly System.Random _random = new System.Random();
        private AcidConfig _config;
        private AcidFieldSystem _system;
        public AcidBottleRuntime(AcidBottleDefinition definition, EnemyRegistry registry, GameObject source, PlayerStatCollection stats)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            _config = definition.CreateLevelConfigs()[0];
            _source = source != null ? source : throw new ArgumentNullException(nameof(source));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _material = definition.Material; _range = definition.TargetRange; _mask = definition.WorldMask;
            BindEnemyRegistry(registry);
        }
        public void BindEnemyRegistry(EnemyRegistry registry)
        {
            _registry = registry != null ? registry : throw new ArgumentNullException(nameof(registry));
            if (_system != null) _system.Shutdown(); _system = null;
        }
        internal void ApplyConfiguration(AcidConfig config) => _config = config;
        private List<Plan> Plans(Vector3 origin)
        {
            if (!float.IsFinite(origin.x) || !float.IsFinite(origin.y) || !float.IsFinite(origin.z))
                throw new ArgumentException("Acid origin must be finite.");
            var result = new List<Plan>();
            if (!AcidWorld.SourceAlive(_source) || _registry == null) return result;
            var sourceCollider = _source.GetComponent<Collider>();
            Vector3 anchor = sourceCollider != null && sourceCollider.enabled ? sourceCollider.bounds.center : _source.transform.position;
            if (!AcidWorld.Visible(anchor, origin, _mask, _source)) return result;
            foreach (var enemy in _registry.ActiveEnemies)
            {
                if (!AcidWorld.Alive(enemy) || (enemy.transform.position - origin).sqrMagnitude > _range * _range) continue;
                if (!AcidWorld.Visible(origin, enemy.transform.position + Vector3.up, _mask, _source)) continue;
                if (AcidWorld.Ground(enemy.transform.position, _mask, _source, out var point, out _))
                    result.Add(new Plan { Enemy = enemy, Ground = point });
            }
            return result;
        }
        public bool TrySelectTarget(Vector3 origin, out Transform target)
        {
            var plans = Plans(origin); target = plans.Count > 0 ? plans[0].Enemy.transform : null; return target != null;
        }
        public AbilityExecutionResult TryExecute(in AbilityExecutionContext context)
        {
            if (Time.timeScale <= 0) return AbilityExecutionResult.Failed;
            var plans = Plans(context.Origin);
            if (plans.Count == 0) return AbilityExecutionResult.Failed;
            float damage = _stats.Evaluate(PlayerStatType.AbilityDamage, _config.Damage);
            if (!float.IsFinite(damage) || damage <= 0) throw new InvalidOperationException("Invalid acid damage.");
            if (_system == null) _system = AcidFieldSystem.Create(_source, _registry, _mask, _material);
            var available = new List<Plan>(plans); var launched = new List<AcidBottleFlight>();
            try
            {
                for (int i = 0; i < _config.Bottles; i++)
                {
                    if (available.Count == 0) available.AddRange(plans);
                    int index = _random.Next(available.Count); var plan = available[index]; available.RemoveAt(index);
                    launched.Add(AcidBottleFlight.Launch(_system, context.Origin, plan.Ground, _config, damage));
                }
            }
            catch
            {
                foreach (var flight in launched)
                    if (flight != null) { flight.gameObject.SetActive(false); UnityEngine.Object.Destroy(flight.gameObject); }
                throw;
            }
            return AbilityExecutionResult.Performed;
        }
    }
}
