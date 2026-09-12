using System;

namespace ProjectFirstRun.Enemies
{
    public readonly struct EnemyAttackConfig
    {
        public float Damage { get; }
        public float Range { get; }
        public float RangeSquared { get; }
        public float CooldownDuration { get; }

        public EnemyAttackConfig(
            float damage,
            float range,
            float cooldownDuration)
        {
            ValidatePositiveFinite(
                damage,
                nameof(damage));

            ValidatePositiveFinite(
                range,
                nameof(range));

            ValidatePositiveFinite(
                cooldownDuration,
                nameof(cooldownDuration));

            Damage = damage;
            Range = range;
            RangeSquared = range * range;
            CooldownDuration = cooldownDuration;
        }

        private static void ValidatePositiveFinite(
            float value,
            string parameterName)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value) ||
                value <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Value must be finite and greater than zero.");
            }
        }
    }
}