using System;

namespace ProjectFirstRun.Progression
{
    public enum RunEconomyOutcome { Death, Victory }

    /// <summary>Immutable outcome calculation, independent of storage and presentation.</summary>
    public sealed class RunEconomyResult
    {
        public string RunId { get; }
        public RunEconomyOutcome Outcome { get; }
        public long Collected { get; }
        public long Lost { get; }
        public long BossReward { get; }
        public long Transferred { get; }

        private RunEconomyResult(string id, RunEconomyOutcome outcome, long collected, long retained, long bossReward)
        {
            RunId = id;
            Outcome = outcome;
            Collected = collected;
            Lost = collected - retained;
            BossReward = bossReward;
            Transferred = checked(retained + bossReward);
        }

        public static RunEconomyResult Calculate(string runId, long collected, RunEconomyOutcome outcome,
            int retentionPercent = 40, long bossReward = 0)
        {
            if (!Guid.TryParseExact(runId, "N", out var id) || id == Guid.Empty)
                throw new ArgumentException("A nonempty run GUID in N format is required.", nameof(runId));
            if (collected < 0) throw new ArgumentOutOfRangeException(nameof(collected));
            if (retentionPercent < 0 || retentionPercent > 100) throw new ArgumentOutOfRangeException(nameof(retentionPercent));
            if (bossReward < 0) throw new ArgumentOutOfRangeException(nameof(bossReward));
            if (outcome != RunEconomyOutcome.Death && outcome != RunEconomyOutcome.Victory)
                throw new ArgumentOutOfRangeException(nameof(outcome));
            long retained = outcome == RunEconomyOutcome.Victory ? collected :
                (long)decimal.Floor((decimal)collected * retentionPercent / 100m);
            return new RunEconomyResult(id.ToString("N"), outcome, collected, retained,
                outcome == RunEconomyOutcome.Victory ? bossReward : 0);
        }
    }
}
