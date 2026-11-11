using System;
using System.IO;
using NUnit.Framework;
using ProjectFirstRun.Progression;

namespace ProjectFirstRun.Tests.EditMode.Progression
{
    public sealed class PersistentMetaWalletTests
    {
        private string _directory;
        [SetUp] public void SetUp() => _directory = Path.Combine(Path.GetTempPath(), "PFR-wallet-tests", Guid.NewGuid().ToString("N"));
        [TearDown] public void TearDown()
        {
            if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
        }

        [Test]
        public void FirstLaunchAndReopenPreserveBalanceAndPreviousBackup()
        {
            var wallet = new PersistentMetaWallet(new JsonMetaWalletStore(_directory));
            Assert.That(wallet.Balance, Is.Zero);
            wallet.Credit(40);
            string previous = File.ReadAllText(System.IO.Path.Combine(_directory, "meta-wallet.json"));
            wallet.Credit(100);
            Assert.That(new PersistentMetaWallet(new JsonMetaWalletStore(_directory)).Balance, Is.EqualTo(140));
            Assert.That(File.ReadAllText(System.IO.Path.Combine(_directory, "meta-wallet.json") + ".bak"), Is.EqualTo(previous));
        }

        [TestCase("broken json")]
        [TestCase("{}")]
        [TestCase("{\"version\":2,\"balance\":\"100\"}")]
        [TestCase("{\"version\":1}")]
        [TestCase("{\"version\":1,\"balance\":\"-1\"}")]
        public void InvalidSaveIsNeverOverwritten(string text)
        {
            var wallet = new PersistentMetaWallet(new JsonMetaWalletStore(_directory));
            File.WriteAllText(System.IO.Path.Combine(_directory, "meta-wallet.json"), text);
            Assert.Throws<InvalidDataException>(() => new PersistentMetaWallet(new JsonMetaWalletStore(_directory)));
            Assert.That(File.ReadAllText(System.IO.Path.Combine(_directory, "meta-wallet.json")), Is.EqualTo(text));
        }

        [Test]
        public void MissingPrimaryWithBackupDoesNotResetWallet()
        {
            var wallet = new PersistentMetaWallet(new JsonMetaWalletStore(_directory));
            wallet.Credit(10);
            File.Delete(System.IO.Path.Combine(_directory, "meta-wallet.json"));
            Assert.Throws<InvalidDataException>(() => new PersistentMetaWallet(new JsonMetaWalletStore(_directory)));
            Assert.That(File.Exists(System.IO.Path.Combine(_directory, "meta-wallet.json")), Is.False);
            Assert.That(File.Exists(System.IO.Path.Combine(_directory, "meta-wallet.json") + ".bak"), Is.True);
        }

        [Test]
        public void FailedWritePreservesMemoryAndPrimaryAndCanRetry()
        {
            var wallet = new PersistentMetaWallet(new JsonMetaWalletStore(_directory));
            wallet.Credit(25);
            string previous = File.ReadAllText(System.IO.Path.Combine(_directory, "meta-wallet.json"));
            Directory.CreateDirectory(System.IO.Path.Combine(_directory, "meta-wallet.json") + ".tmp");
            var error = Assert.Catch(() => wallet.Credit(5));
            Assert.That(error is IOException || error is UnauthorizedAccessException, Is.True);
            Assert.That(wallet.Balance, Is.EqualTo(25));
            Assert.That(File.ReadAllText(System.IO.Path.Combine(_directory, "meta-wallet.json")), Is.EqualTo(previous));
            Directory.Delete(System.IO.Path.Combine(_directory, "meta-wallet.json") + ".tmp");
            wallet.Credit(5);
            Assert.That(new PersistentMetaWallet(new JsonMetaWalletStore(_directory)).Balance, Is.EqualTo(30));
        }

        [Test]
        public void StaleOwnerCannotOverwriteNewerBalance()
        {
            var first = new PersistentMetaWallet(new JsonMetaWalletStore(_directory));
            var stale = new PersistentMetaWallet(new JsonMetaWalletStore(_directory));
            first.Credit(100);
            Assert.Throws<InvalidOperationException>(() => stale.Credit(5));
            Assert.That(stale.Balance, Is.Zero);
            Assert.That(new PersistentMetaWallet(new JsonMetaWalletStore(_directory)).Balance, Is.EqualTo(100));
        }

        [Test]
        public void InvalidAmountAndOverflowDoNotChangeSavedBalance()
        {
            var wallet = new PersistentMetaWallet(new JsonMetaWalletStore(_directory));
            Assert.Throws<ArgumentOutOfRangeException>(() => wallet.Credit(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => wallet.Credit(0));
            wallet.Credit(long.MaxValue);
            Assert.Throws<OverflowException>(() => wallet.Credit(1));
            Assert.That(new PersistentMetaWallet(new JsonMetaWalletStore(_directory)).Balance, Is.EqualTo(long.MaxValue));
        }
    }
}
