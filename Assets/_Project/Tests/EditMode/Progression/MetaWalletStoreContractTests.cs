using System;
using NUnit.Framework;
using ProjectFirstRun.Progression;
namespace ProjectFirstRun.Tests.EditMode.Progression
{
    public sealed class MetaWalletStoreContractTests
    {
        private sealed class MemoryStore : IMetaWalletStore
        {
            public MetaWalletSnapshot State = new(0, Array.Empty<string>());
            public bool Fail;
            public MetaWalletSnapshot Load() => State;
            public void Save(MetaWalletSnapshot state)
            {
                if (Fail) throw new InvalidOperationException("Simulated storage failure");
                State = state;
            }
        }
        [Test]
        public void FailedSettlementDoesNotPublishBalanceOrConsumeRunIdentity()
        {
            var store = new MemoryStore();
            var wallet = new PersistentMetaWallet(store);
            var run = new RunEarnings();
            run.Add(103);
            run.Finish(RunEconomyOutcome.Death, 40, 0);
            store.Fail = true;
            Assert.Throws<InvalidOperationException>(() => wallet.Settle(run.Result));
            Assert.That(wallet.Balance, Is.Zero);
            Assert.That(store.State.SettledRuns, Is.Empty);
            store.Fail = false;
            Assert.That(wallet.Settle(run.Result), Is.True);
            Assert.That(wallet.Balance, Is.EqualTo(41));
            Assert.That(new PersistentMetaWallet(store).Settle(run.Result), Is.False);
        }
        [Test]
        public void SnapshotCopiesHistoryAndRejectsInvalidState()
        {
            var ids = new[] { Guid.NewGuid().ToString("N") };
            var snapshot = new MetaWalletSnapshot(10, ids);
            ids[0] = "changed";
            Assert.That(snapshot.SettledRuns, Does.Not.Contain("changed"));
            Assert.Throws<ArgumentOutOfRangeException>(() => new MetaWalletSnapshot(-1, Array.Empty<string>()));
        }
    }
}
