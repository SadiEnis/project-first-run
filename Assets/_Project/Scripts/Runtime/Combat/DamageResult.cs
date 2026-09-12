namespace ProjectFirstRun.Combat
{
    /// <summary>
    /// Represents the result of a damage operation.
    /// </summary>
    public readonly struct DamageResult
    {
        public float RequestedDamage { get; }
        public float AppliedDamage { get; }
        public float PreviousHealth { get; }
        public float CurrentHealth { get; }
        public bool WasApplied => AppliedDamage > 0f;
        public bool WasLethal { get; }

        public DamageResult(
            float requestedDamage,
            float appliedDamage,
            float previousHealth,
            float currentHealth,
            bool wasLethal)
        {
            RequestedDamage = requestedDamage;
            AppliedDamage = appliedDamage;
            PreviousHealth = previousHealth;
            CurrentHealth = currentHealth;
            WasLethal = wasLethal;
        }

        public static DamageResult Rejected(
            float requestedDamage,
            float currentHealth)
        {
            return new DamageResult(
                requestedDamage,
                0f,
                currentHealth,
                currentHealth,
                false);
        }
    }
}