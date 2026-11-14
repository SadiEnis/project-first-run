using System;
using NUnit.Framework;
using ProjectFirstRun.Progression;
using ProjectFirstRun.UI.Hud;

namespace ProjectFirstRun.Tests.EditMode.Progression
{
    public sealed class RunEconomySummaryViewTests
    {
        [Test]
        public void DeathDisplaysFrozenAmountsAndTotalWalletRatherThanRunTransferAsBalance()
        {
            var result = RunEconomyResult.Calculate(Guid.NewGuid().ToString("N"), 103, RunEconomyOutcome.Death, 40, 50);
            string text = RunEconomySummaryView.Describe(result, true, 1041);
            Assert.That(text, Does.Contain("Collected: 103 GOLD"));
            Assert.That(text, Does.Contain("Lost: 62 GOLD"));
            Assert.That(text, Does.Contain("Boss reward: 0 GOLD"));
            Assert.That(text, Does.Contain("Transferred: 41 GOLD"));
            Assert.That(text, Does.Contain("Permanent balance: 1,041 GOLD"));
        }

        [Test]
        public void VictoryIncludesBossRewardWithoutLoss()
        {
            var result = RunEconomyResult.Calculate(Guid.NewGuid().ToString("N"), 100, RunEconomyOutcome.Victory, 40, 50);
            string text = RunEconomySummaryView.Describe(result, true, 150);
            Assert.That(text, Does.Contain("Lost: 0 GOLD"));
            Assert.That(text, Does.Contain("Boss reward: 50 GOLD"));
            Assert.That(text, Does.Contain("Transferred: 150 GOLD"));
        }

        [Test]
        public void UnconfirmedTransferNeverClaimsSuccessOrDisplaysStaleBalance()
        {
            var result = RunEconomyResult.Calculate(Guid.NewGuid().ToString("N"), 100, RunEconomyOutcome.Death);
            string text = RunEconomySummaryView.Describe(result, false, 999);
            Assert.That(text, Does.Contain("Pending transfer: 40 GOLD"));
            Assert.That(text, Does.Contain("Permanent balance: Unavailable"));
            Assert.That(text, Does.Contain("retry saving"));
            Assert.That(text, Does.Not.Contain("Saved to"));
            Assert.That(text, Does.Not.Contain("999"));
        }

        [Test]
        public void ZeroBalanceIsConfirmedButMissingResultDoesNotInventAReward()
        {
            var result = RunEconomyResult.Calculate(Guid.NewGuid().ToString("N"), 0, RunEconomyOutcome.Death);
            Assert.That(RunEconomySummaryView.Describe(result, true, 0), Does.Contain("Permanent balance: 0 GOLD"));
            Assert.That(RunEconomySummaryView.Describe(null, false, null), Is.EqualTo("Preparing run summary..."));
        }
    }
}
