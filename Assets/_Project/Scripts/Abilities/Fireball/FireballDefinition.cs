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
        [SerializeField, Range(1, 4)] private int _projectileCount = 1;
        [SerializeField, Min(.01f)] private float _targetRange = 20;
        [SerializeField] private LayerMask _worldMask = 129;
        [SerializeField] private LayerMask _collisionMask = 247;
        [SerializeField, Min(.01f)] private float _collisionRadius = .12f;
        [SerializeField, Min(.01f)] private float _travelRange = 60;
        public int ProjectileCount => _projectileCount;
        public float TargetRange => _targetRange;
        public int WorldMask => _worldMask;
        public int CollisionMask => _collisionMask;
        public float CollisionRadius => _collisionRadius;
        public float TravelRange => _travelRange;
        [SerializeField, Min(.01f)] private float _burnDamage = 5;
        [SerializeField, Range(.5f, 3)] private float _burnDuration = 1.5f;
        [SerializeField] private GameObject _burnVisual;
        public float BurnDamage => _burnDamage;
        public float BurnDuration => _burnDuration;
        public GameObject BurnVisual => _burnVisual;

        [Header("Levels 2 and above")]
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
            if (!float.IsFinite(_targetRange) || _targetRange <= 0 || !float.IsFinite(_collisionRadius) ||
                _collisionRadius <= 0 || !float.IsFinite(_travelRange) || _travelRange <= 0)
                throw new InvalidOperationException("Fireball range and radius must be finite and positive.");
            if (_additionalLevels == null || _additionalLevels.Length != MaximumLevel - 1)
                throw new InvalidOperationException("Fireball level data must match its configured maximum.");
            var levels = new FireballRuntimeConfig[MaximumLevel];
            levels[0] = new FireballRuntimeConfig(CreateRuntimeConfig(), _damage, _projectileSpeed, _projectileLifetime, _projectileCount, _burnDamage, _burnDuration);
            for (int i = 1; i < levels.Length; i++)
                levels[i] = (_additionalLevels[i - 1] ?? throw new InvalidOperationException("Missing Fireball level data.")).CreateConfig(_projectileLifetime, _burnDamage);
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
