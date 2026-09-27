using System;

namespace ProjectFirstRun.Combat
{
    /// <summary>
    /// Pure C# health model containing health and death rules.
    /// </summary>
    public sealed class HealthState
    {
        public float MaximumHealth { get; private set; }
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

        public bool SetMaximumHealth(float maximumHealth)
        {
            if (!IsFinite(maximumHealth) || maximumHealth <= 0f)
                throw new ArgumentOutOfRangeException(nameof(maximumHealth));
            if (maximumHealth == MaximumHealth) return false;
            float difference = maximumHealth - MaximumHealth;
            MaximumHealth = maximumHealth;
            if (!IsDead)
                CurrentHealth = Math.Min(maximumHealth, CurrentHealth + Math.Max(0f, difference));
            return true;
        }

        public float Heal(float amount)
        {
            if (!IsFinite(amount) || amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (IsDead) return 0f;
            float restored = Math.Min(amount, MaximumHealth - CurrentHealth);
            CurrentHealth += restored;
            return restored;
        }

        public DamageResult ApplyDamage(float requestedDamage, float damageReduction = 0f)
        {
            if (!IsFinite(damageReduction)) throw new ArgumentOutOfRangeException(nameof(damageReduction));
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
                requestedDamage * (1f - Math.Clamp(damageReduction, 0f, .75f)),
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
