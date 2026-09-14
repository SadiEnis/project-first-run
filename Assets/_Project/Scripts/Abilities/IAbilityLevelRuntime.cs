namespace ProjectFirstRun.Abilities
{
    public interface IAbilityLevelRuntime
    {
        int Level { get; }
        int MaximumLevel { get; }
        AbilityRuntimeConfig InitialConfig { get; }
        void ValidateNextLevel(AbilityRuntimeState state);
        void AdvanceLevel(AbilityRuntimeState state);
    }
}
