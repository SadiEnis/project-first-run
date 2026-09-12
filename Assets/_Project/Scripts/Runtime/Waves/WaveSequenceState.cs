using System;

namespace ProjectFirstRun.Waves
{
    public sealed class WaveSequenceState
    {
        public int TotalWaves
        {
            get;
        }

        public int CurrentWaveIndex
        {
            get;
            private set;
        }

        public int CompletedWaves
        {
            get;
            private set;
        }

        public WaveSequenceStatus Status
        {
            get;
            private set;
        }

        public bool HasActiveWave =>
            Status == WaveSequenceStatus.Running;

        public bool HasRemainingWaves =>
            CompletedWaves < TotalWaves;

        public bool IsCompleted =>
            Status == WaveSequenceStatus.Completed;

        public bool IsFailed =>
            Status == WaveSequenceStatus.Failed;

        public WaveSequenceState(int totalWaves)
        {
            if (totalWaves <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(totalWaves),
                    totalWaves,
                    "The total wave count must be greater than zero.");
            }

            TotalWaves = totalWaves;
            CurrentWaveIndex = -1;
            CompletedWaves = 0;
            Status = WaveSequenceStatus.Ready;
        }

        public void BeginNextWave()
        {
            if (Status == WaveSequenceStatus.Running)
            {
                throw new InvalidOperationException(
                    "A new wave cannot begin while another wave is active.");
            }

            if (Status == WaveSequenceStatus.Completed)
            {
                throw new InvalidOperationException(
                    "A new wave cannot begin after the sequence has completed.");
            }

            if (Status == WaveSequenceStatus.Failed)
            {
                throw new InvalidOperationException(
                    "A new wave cannot begin after the sequence has failed.");
            }

            if (!HasRemainingWaves)
            {
                throw new InvalidOperationException(
                    "There are no remaining waves to begin.");
            }

            CurrentWaveIndex = CompletedWaves;
            Status = WaveSequenceStatus.Running;
        }

        public void CompleteCurrentWave()
        {
            if (Status != WaveSequenceStatus.Running)
            {
                throw new InvalidOperationException(
                    "A wave can only be completed while the sequence is running.");
            }

            CompletedWaves++;

            Status =
                CompletedWaves == TotalWaves
                    ? WaveSequenceStatus.Completed
                    : WaveSequenceStatus.Ready;
        }

        public void MarkFailed()
        {
            if (Status == WaveSequenceStatus.Completed)
            {
                throw new InvalidOperationException(
                    "A completed wave sequence cannot be marked as failed.");
            }

            if (Status == WaveSequenceStatus.Failed)
            {
                throw new InvalidOperationException(
                    "The wave sequence has already been marked as failed.");
            }

            Status = WaveSequenceStatus.Failed;
        }
    }
}