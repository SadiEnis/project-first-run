namespace ProjectFirstRun.Flow
{
    // A snapshot supplied by existing entry/health/final systems, not another run state machine.
    public enum GameFlowPhase { EntryMenu, EntryApproach, Playing, Dead, Ending, Completed, EncounterFailed }
    public enum GameFlowRequest { EntryMenu, EntryApproach, Pause, Reward, Death, Ending, Completed, Loading, Failure }
    public enum GameFlowCursor { LockedHidden, FreeHidden, FreeVisible }

    public readonly struct GameFlowContext
    {
        public GameFlowPhase Phase { get; }
        public bool IsReady { get; }
        public bool BaseControlEnabled { get; }
        public bool PauseClosing { get; }

        public GameFlowContext(GameFlowPhase phase, bool isReady, bool baseControlEnabled, bool pauseClosing = false)
        {
            Phase = phase;
            IsReady = isReady;
            BaseControlEnabled = baseControlEnabled;
            PauseClosing = pauseClosing;
        }
    }

    public readonly struct GameFlowAccess
    {
        // These are requests, not values to restore to Unity or the player's base permission.
        public bool FreezeWorld { get; }
        public bool MuteAudio { get; }
        public bool BlockGameplay { get; }
        public bool ProtectPlayer { get; }
        public bool AllowsGameplay { get; }
        public bool BlockScreenInteraction { get; }
        public GameFlowCursor Cursor { get; }

        internal GameFlowAccess(bool freeze, bool mute, bool blocked, bool baseControlEnabled,
            bool blockScreens, GameFlowCursor cursor, bool protectPlayer)
        {
            FreezeWorld = freeze;
            MuteAudio = mute;
            BlockGameplay = blocked;
            ProtectPlayer = protectPlayer;
            AllowsGameplay = !blocked && baseControlEnabled;
            BlockScreenInteraction = blockScreens;
            Cursor = cursor;
        }
    }
}
