using System;
using System.Collections.Generic;
namespace ProjectFirstRun.Progression
{
    /// <summary>Balance and exactly-once settlement rules, independent of the storage format.</summary>
    public sealed class PersistentMetaWallet
    {
        private readonly IMetaWalletStore _store;
        private HashSet<string> _settledRuns;
        private Dictionary<string, int> _upgrades;
        public int GetUpgradeLevel(string id) => _upgrades.TryGetValue(id, out int level) ? level : 0;
        public long Balance { get; private set; }
        public PersistentMetaWallet(IMetaWalletStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            var snapshot = store.Load();
            Balance = snapshot.Balance;
            _upgrades = new Dictionary<string, int>(snapshot.UpgradeLevels, StringComparer.Ordinal);
            _settledRuns = new HashSet<string>(snapshot.SettledRuns, StringComparer.Ordinal);
        }
        public void Credit(long amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Commit(checked(Balance + amount), _settledRuns, _upgrades);
        }
        public bool Settle(RunEconomyResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (_settledRuns.Contains(result.RunId)) return false;
            var runs = new HashSet<string>(_settledRuns, StringComparer.Ordinal) { result.RunId };
            Commit(checked(Balance + result.Transferred), runs, _upgrades);
            return true;
        }
        /// <summary>Spend and advance together. The content owner supplies a validated next-level offer.</summary>
        public bool TryPurchase(string upgradeId, int expectedLevel, long cost)
        {
            MetaWalletSnapshot.ValidateUpgradeId(upgradeId);
            if (expectedLevel < 0 || expectedLevel == int.MaxValue) throw new ArgumentOutOfRangeException(nameof(expectedLevel));
            if (cost <= 0) throw new ArgumentOutOfRangeException(nameof(cost));
            if (GetUpgradeLevel(upgradeId) != expectedLevel || Balance < cost) return false;
            var upgrades = new Dictionary<string, int>(_upgrades, StringComparer.Ordinal) { [upgradeId] = expectedLevel + 1 };
            Commit(Balance - cost, _settledRuns, upgrades);
            return true;
        }
        private void Commit(long balance, HashSet<string> runs, Dictionary<string, int> upgrades)
        {
            _store.Save(new MetaWalletSnapshot(balance, runs, upgrades));
            // Publish only after persistence succeeds. A failed settlement remains retryable.
            Balance = balance;
            _settledRuns = runs;
            _upgrades = upgrades;
        }
    }
}
