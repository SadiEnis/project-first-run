using System;

namespace ProjectFirstRun.Combat
{
    /// <summary>
    /// Pure C# health model containing health and death rules.
    /// </summary>
    public sealed class HealthState
    {
        public float MaximumHealth { get; }
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        public HealthState(float maximumHealth)
        {
            if (!IsFinite(maximumHealth) || maximumHealth <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumHealth),
                    maximumHealth,
                    "Maximum health must be a finite value greater than zero.");
            }

            MaximumHealth = maximumHealth;
            CurrentHealth = maximumHealth;
        }

        public DamageResult ApplyDamage(float requestedDamage)
        {
            if (!IsFinite(requestedDamage) ||
                requestedDamage <= 0f ||
                IsDead)
            {
                return DamageResult.Rejected(
                    requestedDamage,
                    CurrentHealth);
            }

            float previousHealth = CurrentHealth;
            float appliedDamage = Math.Min(
                requestedDamage,
                previousHealth);

            CurrentHealth = Math.Max(
                0f,
                previousHealth - appliedDamage);

            bool wasLethal =
                previousHealth > 0f &&
                CurrentHealth <= 0f;

            return new DamageResult(
                requestedDamage,
                appliedDamage,
                previousHealth,
                CurrentHealth,
                wasLethal);
        }

        public void ResetToMaximum()
        {
            CurrentHealth = MaximumHealth;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value);
        }
    }
}