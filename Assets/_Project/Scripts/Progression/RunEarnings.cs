using System;

namespace ProjectFirstRun.Progression
{
    public sealed class RunEarnings
    {
        public string RunId { get; } = Guid.NewGuid().ToString("N");
        public long Collected { get; private set; }
        public RunEconomyResult Result { get; private set; }

        public void Add(long amount)
        {
            if (Result != null) throw new InvalidOperationException("The run has ended.");
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Collected = checked(Collected + amount);
        }

        public RunEconomyResult Finish(RunEconomyOutcome outcome, int retentionPercent = 40, long bossReward = 0)
        {
            if (Result != null) throw new InvalidOperationException("The run already has a result. Retry settlement with that result.");
            Result = RunEconomyResult.Calculate(RunId, Collected, outcome, retentionPercent, bossReward);
            return Result;
        }
    }
}
