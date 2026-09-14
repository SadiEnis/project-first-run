namespace ProjectFirstRun.Evolutions
{
    public enum EvolutionResult
    {
        Evolved = 0,
        SourceNotOwned = 1,
        SourceNotAtMaximum = 2,
        RequirementNotMet = 3,
        ResultAlreadyOwned = 4,
        InvalidDefinition = 5
    }
}
