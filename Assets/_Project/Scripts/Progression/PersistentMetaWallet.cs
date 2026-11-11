using System;
using System.Collections.Generic;
namespace ProjectFirstRun.Progression
{
    /// <summary>Balance and exactly-once settlement rules, independent of the storage format.</summary>
    public sealed class PersistentMetaWallet
    {
        private readonly IMetaWalletStore _store;
        private HashSet<string> _settledRuns;
        public long Balance { get; private set; }
        public PersistentMetaWallet(IMetaWalletStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            var snapshot = store.Load();
            Balance = snapshot.Balance;
            _settledRuns = new HashSet<string>(snapshot.SettledRuns, StringComparer.Ordinal);
        }
        public void Credit(long amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Commit(checked(Balance + amount), _settledRuns);
        }
        public bool Settle(RunEconomyResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (_settledRuns.Contains(result.RunId)) return false;
            var runs = new HashSet<string>(_settledRuns, StringComparer.Ordinal) { result.RunId };
            Commit(checked(Balance + result.Transferred), runs);
            return true;
        }
        private void Commit(long balance, HashSet<string> runs)
        {
            _store.Save(new MetaWalletSnapshot(balance, runs));
            // Publish only after persistence succeeds. A failed settlement remains retryable.
            Balance = balance;
            _settledRuns = runs;
        }
    }
}
