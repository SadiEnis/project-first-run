using System;
using System.Collections.Generic;
using ProjectFirstRun.Abilities.Execution;
using ProjectFirstRun.Abilities.Targeting;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;
namespace ProjectFirstRun.Abilities.SniperBomb
{
    public sealed class SniperRuntime : IAbilityExecutor, IAbilityTargetSelector, IMapEnemyRegistryBinding
    {
        private EnemyRegistry _registry;
        private readonly GameObject _source;
        private readonly PlayerStatCollection _stats;
        private readonly SniperDefinition _definition;
        private readonly List<SniperProjectile> _bombs = new List<SniperProjectile>();
        private SniperConfig _config;
        public SniperRuntime(SniperDefinition definition, EnemyRegistry registry, GameObject source, PlayerStatCollection stats)
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
            foreach (var bomb in _bombs) if (bomb != null) bomb.Cancel();
            _bombs.Clear(); _registry = registry;
        }
        internal void ApplyConfiguration(SniperConfig config) => _config = config;
        private Vector3 Origin
        {
            get
            {
                var collider = _source.GetComponent<Collider>();
                return collider != null && collider.enabled ? collider.bounds.center : _source.transform.position + Vector3.up;
            }
        }
        public bool TrySelectTarget(Vector3 origin, out Transform target)
        {
            var enemy = SniperWorld.SourceAlive(_source)
                ? SniperWorld.Select(_registry, _source, Origin, _definition.Range, _definition.WorldMask) : null;
            target = enemy != null ? enemy.transform : null; return target != null;
        }
        public AbilityExecutionResult TryExecute(in AbilityExecutionContext context)
        {
            if (!SniperWorld.SourceAlive(_source) || Time.timeScale <= 0) return AbilityExecutionResult.Failed;
            var target = SniperWorld.Select(_registry, _source, Origin, _definition.Range, _definition.WorldMask);
            if (target == null) return AbilityExecutionResult.Failed;
            float damage = _stats.Evaluate(PlayerStatType.AbilityDamage, _config.Damage);
            if (!float.IsFinite(damage) || damage <= 0) throw new InvalidOperationException("Invalid sniper bomb damage.");
            _bombs.RemoveAll(x => x == null || x.HasEnded);
            for (int i = 0; i < _config.Count; i++)
                _bombs.Add(SniperProjectile.Launch(_source, _registry, _definition, _config, damage, Origin, target, i * .12f));
            return AbilityExecutionResult.Performed;
        }
    }
}
