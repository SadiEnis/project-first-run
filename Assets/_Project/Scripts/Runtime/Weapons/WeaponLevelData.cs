using System;
using UnityEngine;

namespace ProjectFirstRun.Weapons
{
    [Serializable]
    public sealed class WeaponLevelData
    {
        [SerializeField, Min(0.01f)] private float _damage = 25f;
        [SerializeField, Min(1)] private int _magazineCapacity = 12;
        [SerializeField, Min(0.01f)] private float _shotsPerSecond = 5f;
        [SerializeField, Min(0.01f)] private float _reloadDuration = 1.5f;

        public WeaponLevelData(float damage, int magazineCapacity, float shotsPerSecond, float reloadDuration)
        {
            _damage = damage;
            _magazineCapacity = magazineCapacity;
            _shotsPerSecond = shotsPerSecond;
            _reloadDuration = reloadDuration;
        }

        internal WeaponLevelConfig CreateConfig(int startingReserveAmmo) => new WeaponLevelConfig(
            _damage, new WeaponRuntimeConfig(_magazineCapacity, startingReserveAmmo, _shotsPerSecond, _reloadDuration));
    }

    internal readonly struct WeaponLevelConfig
    {
        public float Damage { get; }
        public WeaponRuntimeConfig Runtime { get; }

        public WeaponLevelConfig(float damage, WeaponRuntimeConfig runtime)
        {
            if (float.IsNaN(damage) || float.IsInfinity(damage) || damage <= 0f)
                throw new ArgumentOutOfRangeException(nameof(damage), "Weapon damage must be finite and positive.");
            Damage = damage;
            Runtime = runtime;
        }
    }
}
