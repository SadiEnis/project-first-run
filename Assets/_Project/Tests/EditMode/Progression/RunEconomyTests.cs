using System;
using System.IO;
using NUnit.Framework;
using ProjectFirstRun.Progression;

namespace ProjectFirstRun.Tests.EditMode.Progression
{
    public sealed class RunEconomyTests
    {
        private string _directory;
        [SetUp] public void SetUp() => _directory = Path.Combine(Path.GetTempPath(), "PFR-economy-tests", Guid.NewGuid().ToString("N"));
        [TearDown] public void TearDown()
        {
            if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
        }

        [Test]
        public void DeathFloorsRetentionAndNeverPaysBossReward()
        {
            var run = new RunEarnings();
            run.Add(103);
            var result = run.Finish(RunEconomyOutcome.Death, bossReward: 500);
            Assert.That(result.Transferred, Is.EqualTo(41));
            Assert.That(result.Lost, Is.EqualTo(62));
            Assert.That(result.BossReward, Is.Zero);
            Assert.Throws<InvalidOperationException>(() => run.Add(1));
            Assert.Throws<InvalidOperationException>(() => run.Finish(RunEconomyOutcome.Victory));
            var next = new RunEarnings();
            Assert.That(next.Collected, Is.Zero);
            Assert.That(next.RunId, Is.Not.EqualTo(run.RunId));
        }

        [Test]
        public void VictoryPaysOnceEvenAfterAnotherRunAndReopen()
        {
            var wallet = new PersistentMetaWallet(new JsonMetaWalletStore(_directory));
            wallet.Credit(200);
            var run = new RunEarnings();
            run.Add(100);
            var result = run.Finish(RunEconomyOutcome.Victory, bossReward: 50);
            Assert.That(result.Lost, Is.Zero);
            Assert.That(wallet.Settle(result), Is.True);
            Assert.That(wallet.Settle(result), Is.False);
            wallet.Settle(new RunEarnings().Finish(RunEconomyOutcome.Death));
            var reopened = new PersistentMetaWallet(new JsonMetaWalletStore(_directory));
            Assert.That(reopened.Settle(result), Is.False);
            Assert.That(reopened.Balance, Is.EqualTo(350));
            var changed = RunEconomyResult.Calculate(result.RunId, 999, RunEconomyOutcome.Victory);
            Assert.That(reopened.Settle(changed), Is.False);
        }

        [Test]
        public void FailedSettlementCanRetryWithoutLosingOrDuplicatingReward()
        {
            var wallet = new PersistentMetaWallet(new JsonMetaWalletStore(_directory));
            var run = new RunEarnings();
            run.Add(100);
            var result = run.Finish(RunEconomyOutcome.Death);
            Directory.CreateDirectory(System.IO.Path.Combine(_directory, "meta-wallet.json") + ".tmp");
            Assert.Catch(() => wallet.Settle(result));
            Assert.That(wallet.Balance, Is.Zero);
            Assert.That(new PersistentMetaWallet(new JsonMetaWalletStore(_directory)).Balance, Is.Zero);
            Directory.Delete(System.IO.Path.Combine(_directory, "meta-wallet.json") + ".tmp");
            Assert.That(wallet.Settle(result), Is.True);
            Assert.That(new PersistentMetaWallet(new JsonMetaWalletStore(_directory)).Settle(result), Is.False);
            Assert.That(wallet.Balance, Is.EqualTo(40));
        }

        [Test]
        public void VersionOneBalanceMigratesWithFirstSettlement()
        {
            Directory.CreateDirectory(_directory);
            var path = Path.Combine(_directory, "meta-wallet.json");
            File.WriteAllText(path, "{\"version\":1,\"balance\":\"200\"}");
            var wallet = new PersistentMetaWallet(new JsonMetaWalletStore(_directory));
            var run = new RunEarnings();
            run.Add(100);
            var result = run.Finish(RunEconomyOutcome.Death);
            wallet.Settle(result);
            var reopened = new PersistentMetaWallet(new JsonMetaWalletStore(_directory));
            Assert.That(reopened.Balance, Is.EqualTo(240));
            Assert.That(reopened.Settle(result), Is.False);
            Assert.That(File.ReadAllText(path + ".bak"), Does.Contain("\"version\":1"));
        }

        [Test]
        public void LargeAmountsAndInvalidParametersAreHandledWithoutOverflowOrPartialSettlement()
        {
            var id = Guid.NewGuid().ToString("N");
            Assert.That(RunEconomyResult.Calculate(id, long.MaxValue, RunEconomyOutcome.Death, 100).Transferred,
                Is.EqualTo(long.MaxValue));
            Assert.That(RunEconomyResult.Calculate(id, 100, RunEconomyOutcome.Death, 0).Transferred, Is.Zero);
            Assert.Throws<ArgumentOutOfRangeException>(() => RunEconomyResult.Calculate(id, 1, RunEconomyOutcome.Death, 101));
            Assert.Throws<ArgumentOutOfRangeException>(() => RunEconomyResult.Calculate(id, -1, RunEconomyOutcome.Death));
            Assert.Throws<ArgumentOutOfRangeException>(() => RunEconomyResult.Calculate(id, 1, (RunEconomyOutcome)99));
            Assert.Throws<OverflowException>(() => RunEconomyResult.Calculate(id, long.MaxValue, RunEconomyOutcome.Victory, bossReward: 1));
            var wallet = new PersistentMetaWallet(new JsonMetaWalletStore(_directory));
            wallet.Credit(long.MaxValue);
            var result = RunEconomyResult.Calculate(id, 1, RunEconomyOutcome.Victory);
            Assert.Throws<OverflowException>(() => wallet.Settle(result));
            Assert.That(new PersistentMetaWallet(new JsonMetaWalletStore(_directory)).Balance, Is.EqualTo(long.MaxValue));
        }
    }
}
