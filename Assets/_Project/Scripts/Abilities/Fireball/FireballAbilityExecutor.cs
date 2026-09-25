using System;
using System.Collections.Generic;
using ProjectFirstRun.Abilities.Execution;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Stats;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Abilities.Fireball
{
    public sealed class FireballAbilityExecutor : IAbilityExecutor
    {
        private readonly FireballProjectile _prefab;
        private readonly GameObject _damageSource;
        private readonly PlayerStatCollection _stats;
        private readonly FireballTargetSelector _targets;
        private readonly float _radius, _range;
        private readonly int _mask;
        private FireballRuntimeConfig _config;

        public FireballAbilityExecutor(FireballDefinition definition, GameObject damageSource,
            PlayerStatCollection stats, FireballTargetSelector targets = null)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            _damageSource = damageSource != null ? damageSource : throw new ArgumentNullException(nameof(damageSource));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _prefab = definition.ProjectilePrefab; _targets = targets;
            _radius = definition.CollisionRadius; _range = definition.TravelRange; _mask = definition.CollisionMask;
            _config = new FireballRuntimeConfig(definition.CreateRuntimeConfig(), definition.Damage,
                definition.ProjectileSpeed, definition.ProjectileLifetime, definition.ProjectileCount);
        }
        internal void ApplyConfiguration(FireballRuntimeConfig config) => _config = config;

        public AbilityExecutionResult TryExecute(in AbilityExecutionContext context)
        {
            Vector3 origin = context.Origin;
            if (!Finite(origin)) throw new ArgumentException("Fireball origin must be finite.");
            if (!context.HasTarget || !SourceAlive()) return AbilityExecutionResult.Failed;
            if (_prefab == null) throw new InvalidOperationException("Fireball requires a projectile prefab.");
            var targets = _targets != null ? _targets.SelectVolley(origin, _config.ProjectileCount) : new List<Transform>();
            if (_targets == null)
                for (int i = 0; i < _config.ProjectileCount; i++) targets.Add(context.Target);
            if (targets.Count == 0) return AbilityExecutionResult.Failed;

            float damage = _stats.Evaluate(PlayerStatType.AbilityDamage, _config.Damage);
            if (!float.IsFinite(damage) || damage <= 0) throw new InvalidOperationException("Evaluated Fireball damage must be finite and positive.");
            if (!float.IsFinite(_radius) || _radius <= 0 || !float.IsFinite(_range) || _range <= 0)
                throw new InvalidOperationException("Invalid Fireball movement bounds.");
            var directions = new List<Vector3>(targets.Count);
            foreach (var target in targets)
            {
                if (target == null || (_targets != null && !_targets.Eligible(target, origin))) return AbilityExecutionResult.Failed;
                Vector3 direction = FireballTargetSelector.AimPoint(target) - origin;
                if (direction.sqrMagnitude <= Mathf.Epsilon) direction = _damageSource.transform.forward;
                if (!Finite(direction) || !float.IsFinite(direction.sqrMagnitude) || direction.sqrMagnitude <= Mathf.Epsilon)
                    return AbilityExecutionResult.Failed;
                directions.Add(direction.normalized);
            }
            var sourceCollider = _damageSource.GetComponent<Collider>();
            Vector3 anchor = sourceCollider != null && sourceCollider.enabled
                ? sourceCollider.bounds.center : _damageSource.transform.position;
            Vector3 start = origin, gap = origin - anchor;
            Collider blocker = null;
            if (FireballProjectile.Sweep(anchor, _radius,
                gap.sqrMagnitude > .000001f ? gap.normalized : _damageSource.transform.forward,
                gap.magnitude, _mask, _damageSource, out blocker, out float travel))
                start = anchor + gap.normalized * travel;
            var spawned = new List<FireballProjectile>(directions.Count);
            try
            {
                foreach (var direction in directions)
                {
                    var projectile = UnityEngine.Object.Instantiate(_prefab, start, Quaternion.LookRotation(direction, Vector3.up));
                    spawned.Add(projectile);
                    SceneManager.MoveGameObjectToScene(projectile.gameObject, SceneManager.GetActiveScene());
                    projectile.Initialize(direction, damage, _config.ProjectileSpeed, _config.ProjectileLifetime,
                        _damageSource, _radius, _range, _mask);
                    projectile.SetLaunchBlocker(blocker);
                }
            }
            catch
            {
                foreach (var projectile in spawned)
                {
                    if (projectile == null) continue;
                    projectile.gameObject.SetActive(false);
                    if (Application.isPlaying) UnityEngine.Object.Destroy(projectile.gameObject);
                    else UnityEngine.Object.DestroyImmediate(projectile.gameObject);
                }
                throw;
            }
            return AbilityExecutionResult.Performed;
        }
        private bool SourceAlive()
        {
            if (_damageSource == null) return false;
            var health = _damageSource.GetComponent<HealthComponent>();
            return health == null || !health.IsDead;
        }
        private static bool Finite(Vector3 value) => float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
    }
}
