namespace ProjectFirstRun.Abilities
{
    public interface IAbilityRuntimeFactory
    {
        bool Supports(
            AbilityDefinition definition);

        AbilityRuntimeEntry Create(
            AbilityDefinition definition);
    }
}