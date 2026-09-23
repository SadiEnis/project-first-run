using System;

namespace ProjectFirstRun.Arenas
{
    public interface IArenaSession
    {
        bool IsInitialized { get; }
        ArenaSessionStatus Status { get; }

        event Action Victory;
        event Action Defeat;

        void Begin();
    }
}
