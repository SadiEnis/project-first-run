using ProjectFirstRun.Chests.Spawning;
using UnityEngine;

namespace ProjectFirstRun.Enemies
{
    [CreateAssetMenu(
        fileName = "EnemyDefinition",
        menuName = "Project First Run/Enemies/Enemy Definition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string _stableId;

        [SerializeField]
        private string _displayName;

        [SerializeField] private EnemyRank _rank = EnemyRank.Normal;
        [SerializeField] private EnemyBehavior _behavior = EnemyBehavior.Chaser;
        public EnemyRank Rank => _rank;
        public EnemyBehavior Behavior => _behavior;

        [Header("Charger")]
        [SerializeField, Min(0.01f)] private float _chargeActivationRange = 7f;
        [SerializeField, Min(0.01f)] private float _chargeWindup = 0.8f;
        [SerializeField, Min(0.01f)] private float _chargeSpeed = 10f;
        [SerializeField, Min(0.01f)] private float _chargeDistance = 8f;
        [SerializeField, Min(0.01f)] private float _chargeHitRadius = 0.7f;
        [SerializeField, Min(0.01f)] private float _chargeRecovery = 1.2f;

        public EnemyChargeConfig CreateChargeConfig() => new EnemyChargeConfig(
            _chargeActivationRange, _chargeWindup, _chargeSpeed,
            _chargeDistance, _chargeHitRadius, _chargeRecovery);

        public void ValidateBehavior()
        {
            if (!System.Enum.IsDefined(typeof(EnemyRank), _rank) ||
                !System.Enum.IsDefined(typeof(EnemyBehavior), _behavior))
                throw new System.InvalidOperationException("Unknown enemy rank or behavior.");
            if (_behavior == EnemyBehavior.Charger) CreateChargeConfig();
            if (_behavior == EnemyBehavior.Ranger) CreateRangedConfig();
        }

        [Header("Ranger")]
        [SerializeField, Min(0.01f)] private float _rangedPreferredMin = 5f;
        [SerializeField, Min(0.01f)] private float _rangedPreferredMax = 9f;
        [SerializeField, Min(0.01f)] private float _rangedFireRange = 12f;
        [SerializeField, Min(0.01f)] private float _rangedWindup = 0.6f;
        [SerializeField, Min(0.01f)] private float _rangedProjectileSpeed = 7f;
        [SerializeField, Min(0.01f)] private float _rangedProjectileLifetime = 4f;
        [SerializeField, Min(0.01f)] private float _rangedProjectileRadius = 0.15f;
        public EnemyRangedConfig CreateRangedConfig() => new EnemyRangedConfig(
            _rangedPreferredMin, _rangedPreferredMax, _rangedFireRange, _rangedWindup,
            _attackCooldown, _rangedProjectileSpeed, _rangedProjectileLifetime, _rangedProjectileRadius);

        [Header("Health")]
        [SerializeField, Min(0.01f)]
        private float _maximumHealth = 100f;

        [Header("Experience")]
        [SerializeField, Min(0)]
        private int _experienceReward;

        public int ExperienceReward => _experienceReward;

        [Header("Chest drops")]
        [SerializeField] private EnemyChestDropProfile _chestDropProfile;
        public EnemyChestDropProfile ChestDropProfile => _chestDropProfile;

        [Header("Movement")]
        [SerializeField, Min(0.01f)]
        private float _movementSpeed = 3.5f;
        
        [SerializeField, Min(0.01f)]
        private float _acceleration = 12f;

        [SerializeField, Min(0.01f)]
        private float _angularSpeed = 720f;

        [SerializeField, Min(0f)]
        private float _stoppingDistance = 1.5f;

        [SerializeField, Min(0.01f)]
        private float _destinationUpdateInterval = 0.1f;
        
        [Header("Attack")]
        [SerializeField, Min(0.01f)]
        private float _attackDamage = 10f;

        [SerializeField, Min(0.01f)]
        private float _attackRange = 1.75f;

        [SerializeField, Min(0.01f)]
        private float _attackCooldown = 1f;

        public string StableId => _stableId;
        public string DisplayName => _displayName;

        public float MaximumHealth => _maximumHealth;
        public float MovementSpeed => _movementSpeed;
        public float Acceleration => _acceleration;
        public float AngularSpeed => _angularSpeed;
        public float StoppingDistance => _stoppingDistance;
        public float AttackDamage => _attackDamage;
        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;

        public float DestinationUpdateInterval =>
            _destinationUpdateInterval;

        private void OnValidate()
        {
            _stableId = _stableId?.Trim();
            _displayName = _displayName?.Trim();
            _experienceReward = Mathf.Max(0, _experienceReward);

            _maximumHealth = SanitizePositive(
                _maximumHealth,
                100f);

            _movementSpeed = SanitizePositive(
                _movementSpeed,
                3.5f);

            _acceleration = SanitizePositive(
                _acceleration,
                12f);

            _angularSpeed = SanitizePositive(
                _angularSpeed,
                720f);

            _stoppingDistance = SanitizeNonNegative(
                _stoppingDistance,
                1.5f);

            _destinationUpdateInterval = SanitizePositive(
                _destinationUpdateInterval,
                0.1f);
            
            _attackDamage = SanitizePositive(
                _attackDamage,
                10f);

            _attackRange = SanitizePositive(
                _attackRange,
                1.75f);

            _attackCooldown = SanitizePositive(
                _attackCooldown,
                1f);
        }

        private static float SanitizePositive(
            float value,
            float fallback)
        {
            if (!IsFinite(value) || value <= 0f)
            {
                return fallback;
            }

            return value;
        }

        private static float SanitizeNonNegative(
            float value,
            float fallback)
        {
            if (!IsFinite(value) || value < 0f)
            {
                return fallback;
            }

            return value;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value);
        }
        
        public EnemyAttackConfig CreateAttackConfig()
        {
            return new EnemyAttackConfig(
                _attackDamage,
                _attackRange,
                _attackCooldown);
        }
    }
}
