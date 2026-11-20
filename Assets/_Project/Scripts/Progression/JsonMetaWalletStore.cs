using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
namespace ProjectFirstRun.Progression
{
    /// <summary>Versioned, atomic local storage. Each instance tracks its last loaded revision.</summary>
    public sealed class JsonMetaWalletStore : IMetaWalletStore
    {
        private const int CurrentVersion = 3;
        private readonly string _path;
        private string _savedText;
        [Serializable] private sealed class Record
        {
            public int version;
            public string balance;
            public string[] settledRuns;
            public UpgradeRecord[] upgrades;
        }
        [Serializable] private sealed class UpgradeRecord { public string id; public int level; }
        public JsonMetaWalletStore(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("Assign a save directory.", nameof(directory));
            _path = Path.Combine(Path.GetFullPath(directory), "meta-wallet.json");
        }
        public MetaWalletSnapshot Load()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            using (AcquireLock(_path))
            {
                if (!File.Exists(_path))
                {
                    if (File.Exists(_path + ".bak") || File.Exists(_path + ".tmp"))
                        throw new InvalidDataException("Wallet is missing but recovery files exist. Preserve them and recover explicitly.");
                    Write(_path, Serialize(new MetaWalletSnapshot(0, Array.Empty<string>())));
                }
                string text = File.ReadAllText(_path, Encoding.UTF8);
                var snapshot = ReadSnapshot(text);
                _savedText = text;
                return snapshot;
            }
        }
        public void Save(MetaWalletSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            string text = Serialize(snapshot);
            using (AcquireLock(_path))
            {
                if (_savedText == null || !File.Exists(_path) || File.ReadAllText(_path, Encoding.UTF8) != _savedText)
                    throw new InvalidOperationException("Wallet changed on disk. Reopen it before making changes.");
                Write(_path, text);
                _savedText = text;
            }
        }
        private static FileStream AcquireLock(string path) =>
            new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

        private static string Serialize(MetaWalletSnapshot snapshot)
        {
            var upgrades = new List<UpgradeRecord>();
            foreach (var pair in snapshot.UpgradeLevels) upgrades.Add(new UpgradeRecord { id = pair.Key, level = pair.Value });
            return JsonUtility.ToJson(new Record {
                version = CurrentVersion,
                balance = snapshot.Balance.ToString(System.Globalization.CultureInfo.InvariantCulture),
                settledRuns = new List<string>(snapshot.SettledRuns).ToArray(),
                upgrades = upgrades.ToArray()
            }, true);
        }
        private static MetaWalletSnapshot ReadSnapshot(string text)
        {
            Record record;
            try { record = JsonUtility.FromJson<Record>(text); }
            catch (ArgumentException error) { throw new InvalidDataException("Wallet JSON is invalid. Original file was preserved.", error); }
            if (record == null || record.version < 1 || record.version > CurrentVersion)
                throw new InvalidDataException("Unsupported or missing wallet version. Original file was preserved.");
            if (!long.TryParse(record.balance, System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out long balance))
                throw new InvalidDataException("Wallet balance is missing, negative or outside its supported range.");
            if (record.version >= 2 && record.settledRuns == null) throw new InvalidDataException("Missing settled run history.");
            if (record.version >= 3 && record.upgrades == null) throw new InvalidDataException("Missing upgrade history.");
            var upgrades = new List<KeyValuePair<string, int>>();
            if (record.version >= 3) foreach (var upgrade in record.upgrades)
            {
                if (upgrade == null) throw new InvalidDataException("Missing upgrade entry.");
                upgrades.Add(new KeyValuePair<string, int>(upgrade.id, upgrade.level));
            }
            try { return new MetaWalletSnapshot(balance, record.version >= 2 ? record.settledRuns : Array.Empty<string>(), upgrades); }
            catch (ArgumentException error) { throw new InvalidDataException("Invalid wallet history. Original file was preserved.", error); }
        }

        private static void Write(string path, string text)
        {
            string temporary = path + ".tmp";
            byte[] bytes = new UTF8Encoding(false).GetBytes(text);
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
            else File.Move(temporary, path);
        }
    }
}
