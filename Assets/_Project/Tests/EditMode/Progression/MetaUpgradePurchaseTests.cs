using System;
using System.IO;
using NUnit.Framework;
using ProjectFirstRun.Progression;
namespace ProjectFirstRun.Tests.EditMode.Progression
{
    public sealed class MetaUpgradePurchaseTests
    {
        private string _directory;
        private string PathName => Path.Combine(_directory, "meta-wallet.json");
        private PersistentMetaWallet Open() => new(new JsonMetaWalletStore(_directory));
        [SetUp] public void Setup() => _directory = Path.Combine(Path.GetTempPath(), "PFR-upgrade-tests", Guid.NewGuid().ToString("N"));
        [TearDown] public void Cleanup() { if (Directory.Exists(_directory)) Directory.Delete(_directory, true); }
        [Test] public void PurchasePersistsLevelAndCostTogetherAndRejectsRepeatedOfferOrInsufficientFunds()
        {
            var wallet = Open(); wallet.Credit(100);
            Assert.That(wallet.TryPurchase("ammo", 0, 50), Is.True);
            Assert.That(wallet.TryPurchase("ammo", 0, 50), Is.False);
            Assert.That(wallet.TryPurchase("ammo", 1, 100), Is.False);
            var reopened = Open();
            Assert.That(reopened.Balance, Is.EqualTo(50));
            Assert.That(reopened.GetUpgradeLevel("ammo"), Is.EqualTo(1));
            Assert.That(reopened.TryPurchase("future-upgrade", 0, 10), Is.True);
            var result = RunEconomyResult.Calculate(Guid.NewGuid().ToString("N"), 100, RunEconomyOutcome.Death);
            reopened.Settle(result); reopened.Credit(20);
            var latest = Open();
            Assert.That(latest.Balance, Is.EqualTo(100));
            Assert.That(latest.GetUpgradeLevel("ammo"), Is.EqualTo(1));
            Assert.That(latest.GetUpgradeLevel("future-upgrade"), Is.EqualTo(1));
            Assert.That(latest.Settle(result), Is.False);
        }
        [Test] public void FailedPurchaseLeavesMemoryAndDiskUnchangedThenRetriesOnce()
        {
            var wallet = Open(); wallet.Credit(100);
            string previous = File.ReadAllText(PathName);
            Directory.CreateDirectory(PathName + ".tmp");
            Assert.Catch(() => wallet.TryPurchase("ammo", 0, 50));
            Assert.That(wallet.Balance, Is.EqualTo(100));
            Assert.That(wallet.GetUpgradeLevel("ammo"), Is.Zero);
            Assert.That(File.ReadAllText(PathName), Is.EqualTo(previous));
            Directory.Delete(PathName + ".tmp");
            Assert.That(wallet.TryPurchase("ammo", 0, 50), Is.True);
            Assert.That(Open().GetUpgradeLevel("ammo"), Is.EqualTo(1));
        }
        [Test] public void StaleWalletCannotOverwritePurchasedLevel()
        {
            var wallet = Open(); wallet.Credit(100);
            var stale = Open();
            wallet.TryPurchase("ammo", 0, 50);
            Assert.Throws<InvalidOperationException>(() => stale.TryPurchase("ammo", 0, 50));
            Assert.That(stale.GetUpgradeLevel("ammo"), Is.Zero);
            Assert.That(Open().Balance, Is.EqualTo(50));
        }
        [Test] public void VersionTwoMigratesWithoutLosingBalanceOrSettledRunHistory()
        {
            Directory.CreateDirectory(_directory);
            string id = Guid.NewGuid().ToString("N");
            string old = "{\"version\":2,\"balance\":\"100\",\"settledRuns\":[\"" + id + "\"]}";
            File.WriteAllText(PathName, old);
            var wallet = Open();
            Assert.That(wallet.GetUpgradeLevel("ammo"), Is.Zero);
            wallet.TryPurchase("ammo", 0, 50);
            var reopened = Open();
            Assert.That(reopened.GetUpgradeLevel("ammo"), Is.EqualTo(1));
            Assert.That(reopened.Settle(RunEconomyResult.Calculate(id, 100, RunEconomyOutcome.Victory)), Is.False);
            Assert.That(reopened.Balance, Is.EqualTo(50));
            Assert.That(File.ReadAllText(PathName + ".bak"), Is.EqualTo(old));
        }
        [TestCase("{\"version\":3,\"balance\":\"10\",\"settledRuns\":[]}")]
        [TestCase("{\"version\":3,\"balance\":\"10\",\"settledRuns\":[],\"upgrades\":[{\"id\":\"ammo\",\"level\":0}]}")]
        [TestCase("{\"version\":3,\"balance\":\"10\",\"settledRuns\":[],\"upgrades\":[{\"id\":\"ammo\",\"level\":1},{\"id\":\"ammo\",\"level\":2}]}")]
        public void InvalidUpgradeHistoryDoesNotResetOrOverwriteSave(string json)
        {
            Directory.CreateDirectory(_directory); File.WriteAllText(PathName, json);
            Assert.Throws<InvalidDataException>(() => Open());
            Assert.That(File.ReadAllText(PathName), Is.EqualTo(json));
        }
    }
}
