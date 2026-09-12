using System;
using ProjectFirstRun.Combat;

namespace ProjectFirstRun.Player
{
    public interface IPlayerDeathSource
    {
        bool IsDead { get; }

        event Action<DamageInfo, DamageResult> PlayerDied;
    }
}