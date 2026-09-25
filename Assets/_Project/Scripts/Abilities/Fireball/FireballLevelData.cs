using System;
using UnityEngine;

namespace ProjectFirstRun.Abilities.Fireball
{
    [Serializable]
    public sealed class FireballLevelData
    {
        [SerializeField, Min(0.01f)] private float _cooldown = 2f;
        [SerializeField, Min(0.01f)] private float _damage = 25f;
        [SerializeField, Min(0.01f)] private float _projectileSpeed = 12f;
        [SerializeField, Range(1, 4)] private int _projectileCount = 1;

        public FireballLevelData(float cooldown, float damage, float projectileSpeed, int projectileCount = 1)
        {
            _cooldown = cooldown;
            _damage = damage;
            _projectileSpeed = projectileSpeed;
            _projectileCount = projectileCount;
        }

        internal FireballRuntimeConfig CreateConfig(float lifetime) =>
            new FireballRuntimeConfig(new AbilityRuntimeConfig(_cooldown), _damage, _projectileSpeed, lifetime, _projectileCount);
    }

    internal readonly struct FireballRuntimeConfig
    {
        public AbilityRuntimeConfig Ability { get; }
        public float Damage { get; }
        public float ProjectileSpeed { get; }
        public float ProjectileLifetime { get; }
        public int ProjectileCount { get; }

        public FireballRuntimeConfig(AbilityRuntimeConfig ability, float damage, float projectileSpeed, float projectileLifetime, int projectileCount = 1)
        {
            if (float.IsNaN(damage) || float.IsInfinity(damage) || damage <= 0f) throw new ArgumentOutOfRangeException(nameof(damage));
            if (float.IsNaN(projectileSpeed) || float.IsInfinity(projectileSpeed) || projectileSpeed <= 0f) throw new ArgumentOutOfRangeException(nameof(projectileSpeed));
            if (float.IsNaN(projectileLifetime) || float.IsInfinity(projectileLifetime) || projectileLifetime <= 0f) throw new ArgumentOutOfRangeException(nameof(projectileLifetime));
            Ability = ability; Damage = damage; ProjectileSpeed = projectileSpeed; ProjectileLifetime = projectileLifetime;
            if (projectileCount < 1 || projectileCount > 4) throw new ArgumentOutOfRangeException(nameof(projectileCount));
            ProjectileCount = projectileCount;
        }
    }
}
