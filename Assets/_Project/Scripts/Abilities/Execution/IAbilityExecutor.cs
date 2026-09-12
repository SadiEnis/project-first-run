namespace ProjectFirstRun.Abilities.Execution
{
    public interface IAbilityExecutor
    {
        AbilityExecutionResult TryExecute(
            in AbilityExecutionContext context);
    }
}