using System;
using ProjectFirstRun.Abilities.Execution;
using ProjectFirstRun.Stats;
using UnityEngine;

namespace ProjectFirstRun.Abilities.Fireball
{
    public sealed class FireballAbilityExecutor :
        IAbilityExecutor
    {
        private readonly FireballDefinition _definition;
        private readonly GameObject _damageSource;
        private readonly PlayerStatCollection _stats;
        private FireballRuntimeConfig _config;

        public FireballAbilityExecutor(
            FireballDefinition definition,
            GameObject damageSource,
            PlayerStatCollection stats)
        {
            _definition =
                definition ??
                throw new ArgumentNullException(
                    nameof(definition));

            _damageSource =
                damageSource != null
                    ? damageSource
                    : throw new ArgumentNullException(
                        nameof(damageSource));

            _stats =
                stats ??
                throw new ArgumentNullException(
                    nameof(stats));

            _config = new FireballRuntimeConfig(
                definition.CreateRuntimeConfig(), definition.Damage,
                definition.ProjectileSpeed, definition.ProjectileLifetime);
        }

        internal void ApplyConfiguration(FireballRuntimeConfig config) => _config = config;

        public AbilityExecutionResult TryExecute(
            in AbilityExecutionContext context)
        {
            ValidateOrigin(
                context.Origin);

            if (!context.HasTarget)
            {
                return AbilityExecutionResult.Failed;
            }

            if (_definition.ProjectilePrefab == null)
            {
                throw new InvalidOperationException(
                    $"Fireball definition '{_definition.name}' " +
                    "requires a projectile prefab.");
            }

            Vector3 direction =
                context.Target.position -
                context.Origin;

            if (direction.sqrMagnitude <=
                Mathf.Epsilon)
            {
                direction =
                    _damageSource.transform.forward;
            }

            if (direction.sqrMagnitude <=
                Mathf.Epsilon)
            {
                return AbilityExecutionResult.Failed;
            }

            float damage =
                EvaluateDamage();

            Quaternion rotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up);

            FireballProjectile projectile =
                UnityEngine.Object.Instantiate(
                    _definition.ProjectilePrefab,
                    context.Origin,
                    rotation);

            try
            {
                projectile.Initialize(
                    direction,
                    damage,
                    _config.ProjectileSpeed,
                    _config.ProjectileLifetime,
                    _damageSource);
            }
            catch
            {
                CleanupFailedProjectile(
                    projectile);

                throw;
            }

            return AbilityExecutionResult.Performed;
        }

        private float EvaluateDamage()
        {
            float damage =
                _stats.Evaluate(
                    PlayerStatType.AbilityDamage,
                    _config.Damage);

            if (float.IsNaN(damage) ||
                float.IsInfinity(damage) ||
                damage <= 0f)
            {
                throw new InvalidOperationException(
                    $"Evaluated Fireball damage for " +
                    $"'{_definition.name}' must be finite " +
                    $"and greater than zero. Value: {damage}.");
            }

            return damage;
        }

        private static void CleanupFailedProjectile(
            FireballProjectile projectile)
        {
            if (projectile == null)
            {
                return;
            }

            projectile.gameObject.SetActive(
                false);

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(
                    projectile.gameObject);

                return;
            }

            UnityEngine.Object.DestroyImmediate(
                projectile.gameObject);
        }

        private static void ValidateOrigin(
            Vector3 origin)
        {
            if (!IsFinite(origin.x) ||
                !IsFinite(origin.y) ||
                !IsFinite(origin.z))
            {
                throw new ArgumentException(
                    "Fireball origin must contain finite values.",
                    nameof(origin));
            }
        }

        private static bool IsFinite(
            float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value);
        }
    }
}
