using System;
using ProjectFirstRun.Stats;

namespace ProjectFirstRun.Abilities
{
    public static class AbilityCooldownScaling
    {
        // AbilityCooldown uses negative additive percentages against a duration multiplier of one.
        public static float Multiplier(PlayerStatCollection stats) =>
            ClampMultiplier(stats == null ? 1f : stats.Evaluate(PlayerStatType.AbilityCooldown, 1f));

        public static float ClampMultiplier(float multiplier)
        {
            if (!float.IsFinite(multiplier)) throw new ArgumentOutOfRangeException(nameof(multiplier));
            return Math.Clamp(multiplier, .25f, 1f);
        }
    }
}
