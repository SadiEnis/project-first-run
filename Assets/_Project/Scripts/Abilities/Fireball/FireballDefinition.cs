using UnityEngine;
using System;
using ProjectFirstRun.Abilities;

namespace ProjectFirstRun.Abilities.Fireball
{
    [CreateAssetMenu(
        fileName = "FireballDefinition",
        menuName = "Project First Run/Items/Ability/Fireball Definition")]
    public sealed class FireballDefinition :
        AbilityDefinition
    {
        [Header("Damage")]
        [SerializeField, Min(0.01f)]
        private float _damage = 20f;

        [Header("Projectile")]
        [SerializeField]
        private FireballProjectile _projectilePrefab;

        [SerializeField, Min(0.01f)]
        private float _projectileSpeed = 10f;

        [SerializeField, Min(0.01f)]
        private float _projectileLifetime = 5f;

        [Header("Levels 2 and above (provisional effects)")]
        [SerializeField] private FireballLevelData[] _additionalLevels = Array.Empty<FireballLevelData>();

        public float Damage =>
            _damage;

        public FireballProjectile ProjectilePrefab =>
            _projectilePrefab;

        public float ProjectileSpeed =>
            _projectileSpeed;

        public float ProjectileLifetime =>
            _projectileLifetime;

        internal FireballRuntimeConfig[] CreateLevelConfigs()
        {
            ValidateLevelConfiguration();
            if (_additionalLevels == null || _additionalLevels.Length != MaximumLevel - 1)
                throw new InvalidOperationException("Fireball level data must match its configured maximum.");
            var levels = new FireballRuntimeConfig[MaximumLevel];
            levels[0] = new FireballRuntimeConfig(CreateRuntimeConfig(), _damage, _projectileSpeed, _projectileLifetime);
            for (int i = 1; i < levels.Length; i++)
                levels[i] = (_additionalLevels[i - 1] ?? throw new InvalidOperationException("Missing Fireball level data.")).CreateConfig(_projectileLifetime);
            return levels;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            _damage =
                SanitizePositive(
                    _damage,
                    20f);

            _projectileSpeed =
                SanitizePositive(
                    _projectileSpeed,
                    10f);

            _projectileLifetime =
                SanitizePositive(
                    _projectileLifetime,
                    5f);
        }

        private static float SanitizePositive(
            float value,
            float fallback)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value) ||
                value <= 0f)
            {
                return fallback;
            }

            return value;
        }
    }
}
