using UnityEngine;

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

        public float Damage =>
            _damage;

        public FireballProjectile ProjectilePrefab =>
            _projectilePrefab;

        public float ProjectileSpeed =>
            _projectileSpeed;

        public float ProjectileLifetime =>
            _projectileLifetime;

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