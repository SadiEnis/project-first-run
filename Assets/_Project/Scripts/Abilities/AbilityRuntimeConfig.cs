using System;

namespace ProjectFirstRun.Abilities
{
    public readonly struct AbilityRuntimeConfig
    {
        public float Cooldown { get; }

        public AbilityRuntimeConfig(
            float cooldown)
        {
            if (float.IsNaN(cooldown) ||
                float.IsInfinity(cooldown) ||
                cooldown <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cooldown),
                    cooldown,
                    "Ability cooldown must be a finite value greater than zero.");
            }

            Cooldown = cooldown;
        }
    }
}