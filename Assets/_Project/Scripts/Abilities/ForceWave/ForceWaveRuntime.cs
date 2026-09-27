using System;
using System.Collections.Generic;
using ProjectFirstRun.Abilities.Execution;
using ProjectFirstRun.Abilities.Targeting;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;

namespace ProjectFirstRun.Abilities.ForceWave
{
    /// <summary>One shared availability/execution view of the current map and horizontal facing.</summary>
    public sealed class ForceWaveRuntime : IAbilityTargetSelector, IAbilityExecutor, IMapEnemyRegistryBinding
    {
        private EnemyRegistry _registry;
        private readonly GameObject _source;
        private readonly PlayerStatCollection _stats;
        private readonly float _angle, _height;
        private readonly int _worldMask;
        private readonly Material _visual;
        private ForceWaveConfig _config;
        public ForceWaveConfig Configuration => _config;
        public ForceWaveRuntime(ForceWaveDefinition definition, EnemyRegistry registry, GameObject source, PlayerStatCollection stats)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            _config = definition.CreateLevelConfigs()[0];
            _source = source != null ? source : throw new ArgumentNullException(nameof(source));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            BindEnemyRegistry(registry);
            _angle = definition.Angle; _height = definition.HeightTolerance;
            _worldMask = definition.WorldMask; _visual = definition.VisualMaterial;
        }
        public void BindEnemyRegistry(EnemyRegistry registry) => _registry = registry != null
            ? registry : throw new ArgumentNullException(nameof(registry));
        internal void ApplyConfiguration(ForceWaveConfig config) => _config = config;
        private bool SourceAlive()
        {
            if (_source == null || !_source.activeInHierarchy) return false;
            var health = _source.GetComponent<HealthComponent>();
            return health == null || !health.IsDead;
        }
        public bool TrySelectTarget(Vector3 origin, out Transform target)
        {
            var candidates = Collect(origin);
            target = candidates.Count > 0 ? candidates[0].transform : null;
            return target != null;
        }
        public static bool InSector(Vector3 origin, Vector3 forward, Vector3 point, float reach, float angle, float height)
        {
            Vector3 delta = point - origin;
            if (Mathf.Abs(delta.y) > height) return false;
            delta.y = 0; forward.y = 0;
            if (forward.sqrMagnitude <= .000001f || delta.sqrMagnitude > reach * reach) return false;
            return delta.sqrMagnitude <= .000001f || Vector3.Dot(forward.normalized, delta.normalized) + .000001f >= Mathf.Cos(angle * .5f * Mathf.Deg2Rad);
        }
        private List<EnemyController> Collect(Vector3 origin)
        {
            if (!float.IsFinite(origin.x) || !float.IsFinite(origin.y) || !float.IsFinite(origin.z))
                throw new ArgumentException("Force Wave origin must be finite.");
            var result = new List<EnemyController>();
            if (!SourceAlive() || _registry == null) return result;
            foreach (var enemy in _registry.ActiveEnemies)
            {
                if (!Alive(enemy)) continue;
                Vector3 point = AimPoint(enemy);
                if (InSector(origin, _source.transform.forward, point, _config.Reach, _angle, _height) && Visible(origin, point))
                    result.Add(enemy);
            }
            return result;
        }
        private static bool Alive(EnemyController enemy) => enemy != null && enemy.IsInitialized &&
            enemy.isActiveAndEnabled && !enemy.IsDead && !enemy.Health.IsDead;
        private static Vector3 AimPoint(EnemyController enemy)
        {
            foreach (var collider in enemy.GetComponentsInChildren<Collider>())
                if (collider.enabled && !collider.isTrigger) return collider.bounds.center;
            return enemy.transform.position;
        }
        private bool World(Collider collider) => collider != null &&
            !collider.transform.IsChildOf(_source.transform) &&
            collider.GetComponentInParent<HealthComponent>() == null &&
            collider.GetComponentInParent<EnemyController>() == null;
        private bool Visible(Vector3 origin, Vector3 point)
        {
            foreach (var collider in Physics.OverlapSphere(origin, .001f, _worldMask, QueryTriggerInteraction.Ignore))
                if (World(collider)) return false;
            Vector3 delta = point - origin;
            if (delta.sqrMagnitude > .000001f)
                foreach (var hit in Physics.RaycastAll(origin, delta.normalized, delta.magnitude, _worldMask, QueryTriggerInteraction.Ignore))
                    if (World(hit.collider)) return false;
            return true;
        }
        public AbilityExecutionResult TryExecute(in AbilityExecutionContext context)
        {
            if (Time.timeScale <= 0 || !SourceAlive()) return AbilityExecutionResult.Failed;
            var targets = Collect(context.Origin);
            if (targets.Count == 0) return AbilityExecutionResult.Failed;
            float damage = _stats.Evaluate(PlayerStatType.AbilityDamage, _config.Damage);
            if (!float.IsFinite(damage) || damage <= 0) throw new InvalidOperationException("Invalid Force Wave damage.");
            var receivers = new HashSet<HealthComponent>();
            bool performed = false;
            foreach (var enemy in targets)
            {
                if (!SourceAlive()) break;
                if (!Alive(enemy) || !receivers.Add(enemy.Health)) continue;
                Vector3 point = AimPoint(enemy), direction = point - context.Origin;
                direction.y = 0;
                direction = direction.sqrMagnitude > .000001f ? direction.normalized : Vector3.ProjectOnPlane(_source.transform.forward, Vector3.up).normalized;
                var info = new DamageInfo(damage, _source, point, direction);
                var result = enemy.Health.ApplyDamage(in info);
                performed = true;
                if (result.WasApplied && !result.WasLethal && SourceAlive() && Alive(enemy))
                    enemy.GetComponent<IKnockbackReceiver>()?.TryPush(direction, _config.Push);
            }
            if (performed && SourceAlive() && _visual != null)
                ForceWaveVisual.Show(context.Origin, _source.transform.forward, _angle, _config.Reach, _visual, _source);
            return performed ? AbilityExecutionResult.Performed : AbilityExecutionResult.Failed;
        }
    }
}
