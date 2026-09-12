namespace ProjectFirstRun.Progression
{
    public readonly struct ExperienceGainResult
    {
        public int Amount { get; }
        public int PreviousLevel { get; }
        public int CurrentLevel { get; }
        public int LevelsGained => CurrentLevel - PreviousLevel;
        public long CurrentExperience { get; }
        public long TotalExperience { get; }
        public long RequiredExperience { get; }

        internal ExperienceGainResult(
            int amount,
            int previousLevel,
            int currentLevel,
            long currentExperience,
            long totalExperience,
            long requiredExperience)
        {
            Amount = amount;
            PreviousLevel = previousLevel;
            CurrentLevel = currentLevel;
            CurrentExperience = currentExperience;
            TotalExperience = totalExperience;
            RequiredExperience = requiredExperience;
        }
    }
}
