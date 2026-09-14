using System;

namespace ProjectFirstRun.Enemies
{
    public readonly struct EnemyChargeConfig
    {
        public float ActivationRange { get; }
        public float Windup { get; }
        public float Speed { get; }
        public float Distance { get; }
        public float HitRadius { get; }
        public float Recovery { get; }

        public EnemyChargeConfig(float activationRange, float windup, float speed,
            float distance, float hitRadius, float recovery)
        {
            Validate(activationRange); Validate(windup); Validate(speed);
            Validate(distance); Validate(hitRadius); Validate(recovery);
            ActivationRange = activationRange; Windup = windup; Speed = speed;
            Distance = distance; HitRadius = hitRadius; Recovery = recovery;
        }

        private static void Validate(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Charge settings must be finite and positive.");
        }
    }
}
