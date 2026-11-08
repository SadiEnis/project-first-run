namespace ProjectFirstRun.Arenas
{
    public enum DemoFinalPhase { Playing, Ending, Completed, Dead }

    /// <summary>First accepted terminal outcome wins; opening the gate is not completion.</summary>
    public sealed class DemoFinalSession
    {
        public DemoFinalPhase Phase { get; private set; } = DemoFinalPhase.Playing;
        public bool ReplayRequested { get; private set; }
        public bool TryRequestReplay()
        {
            if (Phase != DemoFinalPhase.Completed || ReplayRequested) return false;
            ReplayRequested = true;
            return true;
        }
        public void CancelFailedReplay() => ReplayRequested = false;
        public bool TryBegin(bool alive, bool eligible, bool allowed, bool inside)
        {
            if (Phase != DemoFinalPhase.Playing) return false;
            if (!alive) { TryDie(); return false; }
            if (!eligible || !allowed || !inside) return false;
            Phase = DemoFinalPhase.Ending;
            return true;
        }
        public bool TryDie()
        {
            if (Phase != DemoFinalPhase.Playing) return false;
            Phase = DemoFinalPhase.Dead;
            return true;
        }
        public bool TryComplete()
        {
            if (Phase != DemoFinalPhase.Ending) return false;
            Phase = DemoFinalPhase.Completed;
            return true;
        }
    }
}
