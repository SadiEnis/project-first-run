using System;

namespace ProjectFirstRun.Arenas
{
    public sealed class RunSessionState
    {
        public int TotalArenas { get; }
        public int CurrentArenaIndex { get; private set; }
        public int CompletedArenas { get; private set; }
        public RunSessionStatus Status { get; private set; }

        public bool IsReady => Status == RunSessionStatus.Ready;
        public bool IsRunning => Status == RunSessionStatus.Running;
        public bool IsTransition => Status == RunSessionStatus.Transition;
        public bool IsVictory => Status == RunSessionStatus.Victory;
        public bool IsDefeat => Status == RunSessionStatus.Defeat;
        public bool IsFinished => IsVictory || IsDefeat;

        public RunSessionState(int totalArenas)
        {
            if (totalArenas <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(totalArenas),
                    totalArenas,
                    "The total arena count must be greater than zero.");
            }

            TotalArenas = totalArenas;
            CurrentArenaIndex = -1;
            CompletedArenas = 0;
            Status = RunSessionStatus.Ready;
        }

        public void Begin()
        {
            if (!IsReady)
            {
                throw new InvalidOperationException(
                    $"Run cannot begin while in '{Status}' state.");
            }

            CurrentArenaIndex = 0;
            Status = RunSessionStatus.Running;
        }

        public void CompleteCurrentArena()
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException(
                    "An arena can only complete while the run is running.");
            }

            CompletedArenas++;
            Status = CompletedArenas == TotalArenas
                ? RunSessionStatus.Victory
                : RunSessionStatus.Transition;
        }

        public void BeginNextArena()
        {
            if (!IsTransition)
            {
                throw new InvalidOperationException(
                    "The next arena can only begin during a transition.");
            }

            CurrentArenaIndex = CompletedArenas;
            Status = RunSessionStatus.Running;
        }

        public void MarkDefeat()
        {
            if (!IsRunning && !IsTransition)
            {
                throw new InvalidOperationException(
                    $"Run cannot become defeated while in '{Status}' state.");
            }

            Status = RunSessionStatus.Defeat;
        }

        public void CompleteFinalObjective()
        {
            if (!IsRunning) throw new InvalidOperationException("The final objective requires a running run.");
            Status = RunSessionStatus.Victory;
        }
    }
}
