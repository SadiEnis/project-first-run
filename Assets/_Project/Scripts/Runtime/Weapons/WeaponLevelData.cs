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
        [SerializeField, Range(1, 64)] private int _pelletCount = 1;
        [SerializeField, Range(0, 89)] private float _spreadHalfAngle;
        [SerializeField, Min(0)] private float _pushDistance;

        // Unity constructs inline serialized records without running the parameterized constructor.
        public WeaponLevelData() { }

        public WeaponLevelData(float damage, int magazineCapacity, float shotsPerSecond, float reloadDuration,
            int pelletCount = 1, float spreadHalfAngle = 0, float pushDistance = 0)
        {
            _damage = damage;
            _magazineCapacity = magazineCapacity;
            _shotsPerSecond = shotsPerSecond;
            _reloadDuration = reloadDuration;
            _pelletCount = pelletCount;
            _spreadHalfAngle = spreadHalfAngle;
            _pushDistance = pushDistance;
        }

        internal WeaponLevelConfig CreateConfig(int startingReserveAmmo) => new WeaponLevelConfig(
            _damage, new WeaponRuntimeConfig(_magazineCapacity, startingReserveAmmo, _shotsPerSecond, _reloadDuration),
            new WeaponShotConfig(_pelletCount, _spreadHalfAngle, _pushDistance));
    }

    internal readonly struct WeaponLevelConfig
    {
        public float Damage { get; }
        public WeaponRuntimeConfig Runtime { get; }
        public WeaponShotConfig Shot { get; }

        public WeaponLevelConfig(float damage, WeaponRuntimeConfig runtime, WeaponShotConfig shot)
        {
            if (float.IsNaN(damage) || float.IsInfinity(damage) || damage <= 0f)
                throw new ArgumentOutOfRangeException(nameof(damage), "Weapon damage must be finite and positive.");
            Damage = damage;
            Runtime = runtime;
            Shot = shot;
        }
    }
}
