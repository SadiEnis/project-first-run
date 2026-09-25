using System;
using ProjectFirstRun.Items;
using UnityEngine;

namespace ProjectFirstRun.Weapons
{
    [CreateAssetMenu(
        fileName = "WeaponDefinition",
        menuName = "Project First Run/Items/Weapon Definition")]
    public sealed class WeaponDefinition : ItemDefinition
    {
        [Header("Delivery")]
        [SerializeField] private WeaponDeliveryMode _deliveryMode;
        [SerializeField] private RocketProjectile _rocketPrefab;
        [SerializeField] private RocketLevelData _rocket = new RocketLevelData();
        public WeaponDeliveryMode DeliveryMode => _deliveryMode;
        public RocketProjectile RocketPrefab => _rocketPrefab;
        [Header("Trigger")]
        [SerializeField]
        private WeaponTriggerMode _triggerMode =
            WeaponTriggerMode.Automatic;

        [Header("Damage")]
        [SerializeField, Min(0.01f)]
        private float _baseDamage = 25f;

        [SerializeField, Min(0.1f)]
        private float _range = 100f;

        [SerializeField]
        private LayerMask _damageMask = -1;

        [Header("Shot pattern")]
        [SerializeField, Range(1, 64)] private int _pelletCount = 1;
        [SerializeField, Range(0, 89)] private float _spreadHalfAngle;
        [SerializeField, Min(0)] private float _pushDistance;

        [Header("Preparation, critical hits and recoil")]
        [SerializeField, Min(0)] private float _preparationDuration;
        [SerializeField, Range(0, 1)] private float _criticalChance;
        [SerializeField, Min(1)] private float _criticalMultiplier = 2;
        [SerializeField, Min(0)] private float _recoilDegrees;
        [SerializeField, Min(0)] private float _recoilRecoveryDelay = .08f;
        [SerializeField, Min(.01f)] private float _recoilRecoveryDuration = .3f;
        [SerializeField, Min(.01f)] private float _recoilMaximumOffset = 12;

        [Header("Ammunition")]
        [SerializeField, Min(1)]
        private int _magazineCapacity = 12;

        [SerializeField, Min(0)]
        private int _startingReserveAmmo = 48;

        [Header("Timing")]
        [SerializeField, Min(0.01f)]
        private float _shotsPerSecond = 5f;

        [SerializeField, Min(0.01f)]
        private float _reloadDuration = 1.5f;

        [Header("Levels 2 and above (absolute values)")]
        [SerializeField] private WeaponLevelData[] _additionalLevels = Array.Empty<WeaponLevelData>();

        internal WeaponLevelConfig[] CreateLevelConfigs()
        {
            ValidateLevelConfiguration();
            if (!Enum.IsDefined(typeof(WeaponDeliveryMode), _deliveryMode))
                throw new InvalidOperationException("Unknown weapon delivery mode.");
            if (_additionalLevels == null || _additionalLevels.Length != MaximumLevel - 1)
                throw new InvalidOperationException("Weapon level data must match its configured maximum.");
            var levels = new WeaponLevelConfig[MaximumLevel];
            levels[0] = new WeaponLevelConfig(_baseDamage, CreateRuntimeConfig(),
                new WeaponShotConfig(_pelletCount, _spreadHalfAngle, _pushDistance),
                new WeaponFireProfile(_preparationDuration, _criticalChance, _criticalMultiplier, _recoilDegrees,
                    _recoilRecoveryDelay, _recoilRecoveryDuration, _recoilMaximumOffset),
                _deliveryMode == WeaponDeliveryMode.Rocket ? (_rocket ?? throw new InvalidOperationException("Missing rocket data.")).CreateConfig() : (RocketConfig?)null);
            for (int i = 1; i < levels.Length; i++)
            {
                if (_additionalLevels[i - 1] == null)
                    throw new InvalidOperationException("Weapon level data is missing.");
                levels[i] = _additionalLevels[i - 1].CreateConfig(_startingReserveAmmo, _deliveryMode);
                if (levels[i].Runtime.MagazineCapacity < levels[i - 1].Runtime.MagazineCapacity)
                    throw new InvalidOperationException("Weapon level progression cannot reduce magazine capacity.");
            }
            foreach (var level in levels)
            {
                if (level.Fire.PreparationDuration > 0 && _triggerMode != WeaponTriggerMode.Automatic)
                    throw new InvalidOperationException("Preparation requires an automatic weapon.");
                if (_deliveryMode == WeaponDeliveryMode.Rocket)
                {
                    if (_triggerMode != WeaponTriggerMode.SemiAutomatic || level.Shot.PelletCount != 1 ||
                        level.Shot.HalfAngle != 0 || level.Shot.PushDistance != 0 ||
                        level.Fire.CriticalChance != 0 || level.Fire.PreparationDuration != 0)
                        throw new InvalidOperationException("Rocket delivery requires a single, non-critical semi-automatic shot.");
                    if (_rocketPrefab == null) throw new InvalidOperationException("Rocket prefab is required.");
                    _rocketPrefab.ValidatePrefab(level.Rocket.Value.FragmentCount > 0);
                }
            }
            return levels;
        }

        public override ItemCategory Category =>
            ItemCategory.Weapon;

        public WeaponTriggerMode TriggerMode =>
            _triggerMode;

        public float BaseDamage =>
            _baseDamage;

        public float Range =>
            _range;

        public LayerMask DamageMask =>
            _damageMask;

        public int MagazineCapacity =>
            _magazineCapacity;

        public int StartingReserveAmmo =>
            _startingReserveAmmo;

        public float ShotsPerSecond =>
            _shotsPerSecond;

        public float ReloadDuration =>
            _reloadDuration;

        public WeaponRuntimeConfig CreateRuntimeConfig()
        {
            return new WeaponRuntimeConfig(
                _magazineCapacity,
                _startingReserveAmmo,
                _shotsPerSecond,
                _reloadDuration);
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            _baseDamage = SanitizePositiveFloat(
                _baseDamage,
                25f);

            _range = SanitizePositiveFloat(
                _range,
                100f);

            _magazineCapacity = Mathf.Max(
                1,
                _magazineCapacity);

            _startingReserveAmmo = Mathf.Max(
                0,
                _startingReserveAmmo);

            _shotsPerSecond = SanitizePositiveFloat(
                _shotsPerSecond,
                5f);

            _reloadDuration = SanitizePositiveFloat(
                _reloadDuration,
                1.5f);
        }

        private static float SanitizePositiveFloat(
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
