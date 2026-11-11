using System;
using System.Collections.Generic;
namespace ProjectFirstRun.Progression
{
    /// <summary>One wallet owner per store instance. Load before Save; save balance and history atomically.
    /// Implementations must reject stale writes and throw on failure, preserving the previous state.</summary>
    public interface IMetaWalletStore
    {
        MetaWalletSnapshot Load();
        void Save(MetaWalletSnapshot snapshot);
    }
    public sealed class MetaWalletSnapshot
    {
        public long Balance { get; }
        public IReadOnlyCollection<string> SettledRuns { get; }
        public MetaWalletSnapshot(long balance, IEnumerable<string> settledRuns)
        {
            if (balance < 0) throw new ArgumentOutOfRangeException(nameof(balance));
            if (settledRuns == null) throw new ArgumentNullException(nameof(settledRuns));
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (string value in settledRuns)
            {
                if (!Guid.TryParseExact(value, "N", out var id) || id == Guid.Empty || !ids.Add(id.ToString("N")))
                    throw new ArgumentException("Invalid or duplicate settled run identity.", nameof(settledRuns));
            }
            Balance = balance;
            SettledRuns = new List<string>(ids).AsReadOnly();
        }
    }
}
