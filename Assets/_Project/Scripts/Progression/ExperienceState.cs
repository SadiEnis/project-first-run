using System;

namespace ProjectFirstRun.Progression
{
    public sealed class ExperienceState
    {
        private readonly ExperienceCurve _curve;

        public int Level { get; private set; } = 1;
        public long CurrentExperience { get; private set; }
        public long TotalExperience { get; private set; }
        public long RequiredExperience => _curve.GetRequiredExperience(Level);

        public ExperienceState(ExperienceCurve curve)
        {
            _curve = curve ?? throw new ArgumentNullException(nameof(curve));
        }

        public ExperienceGainResult GainExperience(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            int previousLevel = Level;
            int nextLevel = Level;
            long nextTotal = checked(TotalExperience + amount);
            long remaining = checked(CurrentExperience + amount);
            long required = _curve.GetRequiredExperience(nextLevel);

            // Calculate before committing so a numeric error cannot partly advance a run.
            while (remaining >= required)
            {
                remaining -= required;
                nextLevel = checked(nextLevel + 1);
                required = _curve.GetRequiredExperience(nextLevel);
            }

            Level = nextLevel;
            CurrentExperience = remaining;
            TotalExperience = nextTotal;

            return new ExperienceGainResult(
                amount, previousLevel, Level, CurrentExperience,
                TotalExperience, required);
        }
    }
}
