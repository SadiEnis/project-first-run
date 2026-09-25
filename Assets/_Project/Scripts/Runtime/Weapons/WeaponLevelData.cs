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
        [SerializeField, Min(0)] private float _preparationDuration;
        [SerializeField, Range(0, 1)] private float _criticalChance;
        [SerializeField, Min(1)] private float _criticalMultiplier = 2;
        [SerializeField, Min(0)] private float _recoilDegrees;
        [SerializeField, Min(0)] private float _recoilRecoveryDelay = .08f;
        [SerializeField, Min(.01f)] private float _recoilRecoveryDuration = .3f;
        [SerializeField, Min(.01f)] private float _recoilMaximumOffset = 12;
        [SerializeField] private RocketLevelData _rocket = new RocketLevelData();

        // Unity constructs inline serialized records without running the parameterized constructor.
        public WeaponLevelData() { }

        public WeaponLevelData(float damage, int magazineCapacity, float shotsPerSecond, float reloadDuration,
            int pelletCount = 1, float spreadHalfAngle = 0, float pushDistance = 0,
            float preparationDuration = 0, float criticalChance = 0, float criticalMultiplier = 2, float recoilDegrees = 0,
            float recoilRecoveryDelay = .08f, float recoilRecoveryDuration = .3f, float recoilMaximumOffset = 12)
        {
            _damage = damage;
            _magazineCapacity = magazineCapacity;
            _shotsPerSecond = shotsPerSecond;
            _reloadDuration = reloadDuration;
            _pelletCount = pelletCount;
            _spreadHalfAngle = spreadHalfAngle;
            _pushDistance = pushDistance;
            _preparationDuration = preparationDuration;
            _criticalChance = criticalChance;
            _criticalMultiplier = criticalMultiplier;
            _recoilDegrees = recoilDegrees;
            _recoilRecoveryDelay = recoilRecoveryDelay;
            _recoilRecoveryDuration = recoilRecoveryDuration;
            _recoilMaximumOffset = recoilMaximumOffset;
        }

        internal WeaponLevelConfig CreateConfig(int startingReserveAmmo, WeaponDeliveryMode delivery = WeaponDeliveryMode.Hitscan) => new WeaponLevelConfig(
            _damage, new WeaponRuntimeConfig(_magazineCapacity, startingReserveAmmo, _shotsPerSecond, _reloadDuration),
            new WeaponShotConfig(_pelletCount, _spreadHalfAngle, _pushDistance),
            new WeaponFireProfile(_preparationDuration, _criticalChance, _criticalMultiplier, _recoilDegrees,
                _recoilRecoveryDelay, _recoilRecoveryDuration, _recoilMaximumOffset),
            delivery == WeaponDeliveryMode.Rocket ? (_rocket ?? throw new InvalidOperationException("Missing rocket level data.")).CreateConfig() : (RocketConfig?)null);
    }

    internal readonly struct WeaponLevelConfig
    {
        public float Damage { get; }
        public WeaponRuntimeConfig Runtime { get; }
        public WeaponShotConfig Shot { get; }
        public WeaponFireProfile Fire { get; }
        public RocketConfig? Rocket { get; }

        public WeaponLevelConfig(float damage, WeaponRuntimeConfig runtime, WeaponShotConfig shot, WeaponFireProfile fire, RocketConfig? rocket = null)
        {
            if (float.IsNaN(damage) || float.IsInfinity(damage) || damage <= 0f)
                throw new ArgumentOutOfRangeException(nameof(damage), "Weapon damage must be finite and positive.");
            Damage = damage;
            Runtime = runtime;
            Shot = shot;
            if (!float.IsFinite(damage * shot.PelletCount * fire.CriticalMultiplier))
                throw new ArgumentOutOfRangeException(nameof(damage), "Maximum critical volley damage must be finite.");
            Fire = fire;
            Rocket = rocket;
        }
    }
}
