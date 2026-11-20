using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public static void ValidateUpgradeId(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length > 128) throw new ArgumentException("Assign a stable upgrade ID.", nameof(id));
            foreach (char c in id) if (!(char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_'))
                throw new ArgumentException("Invalid upgrade ID.", nameof(id));
        }
        public int GetUpgradeLevel(string id) => UpgradeLevels.TryGetValue(id, out int level) ? level : 0;
        public long Balance { get; }
        public IReadOnlyCollection<string> SettledRuns { get; }
        public IReadOnlyDictionary<string, int> UpgradeLevels { get; }
        public MetaWalletSnapshot(long balance, IEnumerable<string> settledRuns, IEnumerable<KeyValuePair<string, int>> upgrades = null)
        {
            if (balance < 0) throw new ArgumentOutOfRangeException(nameof(balance));
            if (settledRuns == null) throw new ArgumentNullException(nameof(settledRuns));
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (string value in settledRuns)
            {
                if (!Guid.TryParseExact(value, "N", out var id) || id == Guid.Empty || !ids.Add(id.ToString("N")))
                    throw new ArgumentException("Invalid or duplicate settled run identity.", nameof(settledRuns));
            }
            var levels = new Dictionary<string, int>(StringComparer.Ordinal);
            if (upgrades != null) foreach (var pair in upgrades)
            {
                ValidateUpgradeId(pair.Key);
                if (pair.Value <= 0 || !levels.TryAdd(pair.Key, pair.Value))
                    throw new ArgumentException("Invalid or duplicate upgrade level.", nameof(upgrades));
            }
            UpgradeLevels = new ReadOnlyDictionary<string, int>(levels);
            Balance = balance;
            SettledRuns = new List<string>(ids).AsReadOnly();
        }
    }
}
