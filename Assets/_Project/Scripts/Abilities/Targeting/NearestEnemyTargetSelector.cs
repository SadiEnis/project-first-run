using System;
using ProjectFirstRun.Enemies;
using UnityEngine;

namespace ProjectFirstRun.Abilities.Targeting
{
    public sealed class NearestEnemyTargetSelector :
        IAbilityTargetSelector, IMapEnemyRegistryBinding
    {
        private EnemyRegistry _enemyRegistry;

        public void BindEnemyRegistry(EnemyRegistry registry) => _enemyRegistry = registry != null
            ? registry : throw new ArgumentNullException(nameof(registry));

        public NearestEnemyTargetSelector(
            EnemyRegistry enemyRegistry)
        {
            _enemyRegistry =
                enemyRegistry != null
                    ? enemyRegistry
                    : throw new ArgumentNullException(
                        nameof(enemyRegistry));
        }

        public bool TrySelectTarget(
            Vector3 origin,
            out Transform target)
        {
            ValidateOrigin(origin);
            EnsureRegistryAvailable();

            EnemyController nearestEnemy = null;

            float nearestDistanceSquared =
                float.PositiveInfinity;

            foreach (EnemyController enemy
                     in _enemyRegistry.ActiveEnemies)
            {
                if (!IsValidCandidate(enemy))
                {
                    continue;
                }

                Vector3 offset =
                    enemy.transform.position -
                    origin;

                float distanceSquared =
                    offset.sqrMagnitude;

                if (distanceSquared >=
                    nearestDistanceSquared)
                {
                    continue;
                }

                nearestEnemy = enemy;

                nearestDistanceSquared =
                    distanceSquared;
            }

            if (nearestEnemy == null)
            {
                target = null;
                return false;
            }

            target =
                nearestEnemy.transform;

            return true;
        }

        private static bool IsValidCandidate(
            EnemyController enemy)
        {
            return enemy != null &&
                   enemy.IsInitialized &&
                   !enemy.IsDead &&
                   enemy.isActiveAndEnabled;
        }

        private void EnsureRegistryAvailable()
        {
            if (_enemyRegistry == null)
            {
                throw new InvalidOperationException(
                    "The enemy registry is no longer available.");
            }
        }

        private static void ValidateOrigin(
            Vector3 origin)
        {
            if (!IsFinite(origin.x) ||
                !IsFinite(origin.y) ||
                !IsFinite(origin.z))
            {
                throw new ArgumentException(
                    "Target-selection origin must contain finite values.",
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
