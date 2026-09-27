using System;
using System.Collections.Generic;
using ProjectFirstRun.Abilities.Execution;
using ProjectFirstRun.Abilities.Targeting;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;
namespace ProjectFirstRun.Abilities.Lightning
{
    public sealed class LightningRuntime : IAbilityExecutor, IAbilityTargetSelector, IMapEnemyRegistryBinding
    {
        private EnemyRegistry _registry;
        private readonly GameObject _source;
        private readonly PlayerStatCollection _stats;
        private readonly LightningDefinition _definition;
        private readonly System.Random _random = new System.Random();
        private LightningConfig _config;
        public LightningRuntime(LightningDefinition definition, EnemyRegistry registry, GameObject source, PlayerStatCollection stats)
        {
            _definition = definition != null ? definition : throw new ArgumentNullException(nameof(definition));
            _config = definition.CreateLevelConfigs()[0];
            _source = source != null ? source : throw new ArgumentNullException(nameof(source));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            BindEnemyRegistry(registry);
        }
        public void BindEnemyRegistry(EnemyRegistry registry) => _registry = registry != null
            ? registry : throw new ArgumentNullException(nameof(registry));
        internal void ApplyConfiguration(LightningConfig config) => _config = config;
        private bool SourceAlive => _source != null && _source.activeInHierarchy &&
            (_source.GetComponent<HealthComponent>() == null || !_source.GetComponent<HealthComponent>().IsDead);
        private static bool Alive(EnemyController enemy) => enemy != null && enemy.IsInitialized &&
            enemy.isActiveAndEnabled && !enemy.IsDead && !enemy.Health.IsDead;
        private bool World(Collider collider) => collider != null && !collider.isTrigger &&
            !collider.transform.IsChildOf(_source.transform) &&
            collider.GetComponentInParent<HealthComponent>() == null &&
            collider.GetComponentInParent<EnemyController>() == null;
        private bool Visible(Vector3 from, Vector3 to)
        {
            foreach (var collider in Physics.OverlapSphere(from, .02f, _definition.WorldMask, QueryTriggerInteraction.Ignore))
                if (World(collider)) return false;
            Vector3 delta = to - from;
            if (delta.sqrMagnitude < .000001f) return true;
            foreach (var hit in Physics.RaycastAll(from, delta.normalized, delta.magnitude, _definition.WorldMask, QueryTriggerInteraction.Ignore))
                if (World(hit.collider)) return false;
            return true;
        }
        private List<EnemyController> Candidates(Vector3 origin)
        {
            if (!float.IsFinite(origin.x) || !float.IsFinite(origin.y) || !float.IsFinite(origin.z))
                throw new ArgumentException("Lightning origin must be finite.");
            var result = new List<EnemyController>();
            if (!SourceAlive || _registry == null) return result;
            var collider = _source.GetComponent<Collider>();
            Vector3 anchor = collider != null && collider.enabled ? collider.bounds.center : _source.transform.position;
            if (!Visible(anchor, origin)) return result;
            foreach (var enemy in _registry.ActiveEnemies)
                if (Alive(enemy) && (enemy.transform.position - origin).sqrMagnitude <= _definition.Range * _definition.Range &&
                    Visible(origin, enemy.transform.position + Vector3.up))
                    result.Add(enemy);
            return result;
        }
        public bool TrySelectTarget(Vector3 origin, out Transform target)
        {
            var candidates = Candidates(origin);
            target = candidates.Count == 0 ? null : candidates[0].transform;
            return target != null;
        }
        private bool InArea(Vector3 impact, EnemyController enemy, float range)
        {
            Vector3 delta = enemy.transform.position - impact;
            if (Mathf.Abs(delta.y) > .6f) return false;
            delta.y = 0;
            return delta.sqrMagnitude <= range * range &&
                Visible(impact + Vector3.up * .2f, enemy.transform.position + Vector3.up * .2f);
        }
        private void Hit(EnemyController enemy, float damage)
        {
            if (!SourceAlive || !Alive(enemy)) return;
            var info = new DamageInfo(damage, _source, enemy.transform.position, Vector3.down);
            var result = enemy.Health.ApplyDamage(in info);
            if (result.WasApplied && Alive(enemy) && _config.Stun > 0)
                enemy.GetComponent<EnemyMotor>().ApplyStun(_config.Stun);
        }
        public AbilityExecutionResult TryExecute(in AbilityExecutionContext context)
        {
            if (Time.timeScale <= 0) return AbilityExecutionResult.Failed;
            var candidates = Candidates(context.Origin);
            if (candidates.Count == 0) return AbilityExecutionResult.Failed;
            float damage = _stats.Evaluate(PlayerStatType.AbilityDamage, _config.Damage);
            if (!float.IsFinite(damage) || damage <= 0) throw new InvalidOperationException("Invalid lightning damage.");
            // Plan impacts before lethal damage can unregister selected targets.
            var positions = new List<Vector3>();
            var available = new List<EnemyController>(candidates);
            for (int i = 0; i < _config.Strikes; i++)
            {
                if (available.Count == 0) available.AddRange(candidates);
                int index = _random.Next(available.Count);
                positions.Add(available[index].transform.position); available.RemoveAt(index);
            }
            foreach (var impact in positions)
            {
                if (!SourceAlive || _registry == null) break;
                var scene = _registry.gameObject.scene;
                LightningVisual.Show(impact + Vector3.up * 5, impact + Vector3.up * .1f, _definition.Material, scene);
                // Registry snapshot deduplicates colliders and tolerates lethal unregisters.
                var enemies = new List<EnemyController>(_registry.ActiveEnemies);
                var hit = new HashSet<EnemyController>();
                foreach (var enemy in enemies)
                    if (SourceAlive && Alive(enemy) && InArea(impact, enemy, _definition.Radius))
                    { hit.Add(enemy); Hit(enemy, damage); }
                if (_config.Chains == 0 || !SourceAlive) continue;
                EnemyController nearest = null; float distance = float.PositiveInfinity;
                foreach (var enemy in enemies)
                {
                    if (!Alive(enemy) || hit.Contains(enemy) || !InArea(impact, enemy, _definition.ChainRange)) continue;
                    float candidateDistance = (enemy.transform.position - impact).sqrMagnitude;
                    if (candidateDistance < distance) { nearest = enemy; distance = candidateDistance; }
                }
                if (nearest != null)
                {
                    LightningVisual.Show(impact + Vector3.up, nearest.transform.position + Vector3.up, _definition.Material, scene);
                    Hit(nearest, damage);
                }
            }
            return AbilityExecutionResult.Performed;
        }
    }
}
