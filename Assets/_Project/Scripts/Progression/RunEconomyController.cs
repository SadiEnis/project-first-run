using System;
using UnityEngine;

namespace ProjectFirstRun.Progression
{
    /// <summary>Scene lifetime bridge between run earnings and the persistent wallet.</summary>
    [DisallowMultipleComponent]
    public sealed class RunEconomyController : MonoBehaviour
    {
        [SerializeField, Range(0, 100)] private int _deathRetentionPercent = 40;
        [SerializeField, Min(0)] private int _bossReward;
        private readonly RunEarnings _earnings = new();
        private PersistentMetaWallet _wallet;
        private IMetaWalletStore _store;
        public RunEconomyResult Result => _earnings.Result;
        public long Collected => _earnings.Collected;
        public long? PermanentBalance => _wallet?.Balance;
        public bool IsSettled { get; private set; }
        public string LastError { get; private set; }

        public void Add(long amount) => _earnings.Add(amount);

        // A terminal result is frozen even if saving fails; retries must use this same result.
        public bool Complete(RunEconomyOutcome outcome)
        {
            if (IsSettled) return true;
            try
            {
                if (Result == null) _earnings.Finish(outcome, _deathRetentionPercent, _bossReward);
                _wallet ??= new PersistentMetaWallet(_store ?? throw new InvalidOperationException("Assign a wallet store at scene startup."));
                _wallet.Settle(Result);
                IsSettled = true;
                LastError = null;
                return true;
            }
            catch (Exception error)
            {
                LastError = error.Message;
                // Reopen on retry: a prior owner or interrupted operation may have saved already.
                _wallet = null;
                return false;
            }
        }

        // Tests and development fixtures can isolate their saves before any terminal result.
        public void ConfigureStore(IMetaWalletStore store)
        {
            if (Result != null || _wallet != null) throw new InvalidOperationException("Configure storage before ending the run.");
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }
    }
}
