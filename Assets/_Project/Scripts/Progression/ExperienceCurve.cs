using System;

namespace ProjectFirstRun.Progression
{
    public sealed class ExperienceCurve
    {
        public int FirstLevelCost { get; }
        public int CostIncreasePerLevel { get; }

        public ExperienceCurve(int firstLevelCost, int costIncreasePerLevel)
        {
            if (firstLevelCost <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(firstLevelCost));
            }

            if (costIncreasePerLevel <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(costIncreasePerLevel));
            }

            FirstLevelCost = firstLevelCost;
            CostIncreasePerLevel = costIncreasePerLevel;
        }

        public long GetRequiredExperience(int level)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            return FirstLevelCost + (long)(level - 1) * CostIncreasePerLevel;
        }
    }
}
