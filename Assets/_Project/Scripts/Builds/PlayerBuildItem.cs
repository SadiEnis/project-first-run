using ProjectFirstRun.Items;

namespace ProjectFirstRun.Builds
{
    /// <summary>Run-owned progression. Only PlayerBuild advances an owned item.</summary>
    public sealed class PlayerBuildItem
    {
        public ItemCategory Category { get; }
        public string StableId { get; }
        public int Level { get; private set; } = 1;
        public int MaximumLevel { get; }
        public bool IsAtMaximumLevel => Level == MaximumLevel;

        internal PlayerBuildItem(ItemCategory category, string stableId, int maximumLevel)
        {
            Category = category;
            StableId = stableId;
            MaximumLevel = maximumLevel;
        }

        internal bool TryAdvance()
        {
            if (IsAtMaximumLevel) return false;
            Level++;
            return true;
        }
    }
}
