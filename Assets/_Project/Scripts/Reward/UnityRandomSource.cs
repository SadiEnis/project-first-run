using System;

namespace ProjectFirstRun.Rewards
{
    public sealed class UnityRandomSource :
        IRandomSource
    {
        public int Next(
            int minInclusive,
            int maxExclusive)
        {
            if (minInclusive >= maxExclusive)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxExclusive),
                    maxExclusive,
                    "Maximum value must be greater than minimum value.");
            }

            return UnityEngine.Random.Range(
                minInclusive,
                maxExclusive);
        }
    }
}