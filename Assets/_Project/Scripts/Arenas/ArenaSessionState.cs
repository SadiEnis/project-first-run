using System;

namespace ProjectFirstRun.Arenas
{
    public sealed class ArenaSessionState
    {
        public ArenaSessionStatus Status { get; private set; }

        public bool IsReady =>
            Status == ArenaSessionStatus.Ready;

        public bool IsRunning =>
            Status == ArenaSessionStatus.Running;

        public bool IsVictory =>
            Status == ArenaSessionStatus.Victory;

        public bool IsDefeat =>
            Status == ArenaSessionStatus.Defeat;

        public bool IsFinished =>
            IsVictory || IsDefeat;

        public ArenaSessionState()
        {
            Status = ArenaSessionStatus.Ready;
        }

        public void Begin()
        {
            if (!IsReady)
            {
                throw new InvalidOperationException(
                    $"Arena session cannot begin while in " +
                    $"'{Status}' state.");
            }

            Status = ArenaSessionStatus.Running;
        }

        public void MarkVictory()
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException(
                    $"Arena session cannot become victorious while in " +
                    $"'{Status}' state.");
            }

            Status = ArenaSessionStatus.Victory;
        }

        public void MarkDefeat()
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException(
                    $"Arena session cannot become defeated while in " +
                    $"'{Status}' state.");
            }

            Status = ArenaSessionStatus.Defeat;
        }
    }
}