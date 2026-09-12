namespace ProjectFirstRun.Rewards
{
    public interface IRandomSource
    {
        int Next(
            int minInclusive,
            int maxExclusive);
    }
}