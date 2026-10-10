namespace ProjectFirstRun.Abilities
{
    /// <summary>Read-only projection of a continuous executor's real wait, never a HUD timer.</summary>
    public interface IAbilityCooldownPresentation
    {
        float CooldownFraction { get; }
    }
}
