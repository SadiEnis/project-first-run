using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirstRun.Progression
{
    /// <summary>Single-profile local wallet. Call from one owner on the main thread.</summary>
    public sealed class PersistentMetaWallet
    {
        private const int CurrentVersion = 2;
        private readonly string _path;
        private string _savedText;
        private HashSet<string> _settledRuns;
        public long Balance { get; private set; }
        public string SavePath => _path;

        [Serializable]
        private sealed class Record
        {
            public int version;
            // Strings make missing values distinguishable from a valid zero balance.
            public string balance;
            public string[] settledRuns;
        }

        private PersistentMetaWallet(string path, string savedText, long balance, HashSet<string> settledRuns)
        {
            _path = path;
            _savedText = savedText;
            Balance = balance;
            _settledRuns = settledRuns;
        }

        public static PersistentMetaWallet OpenDefault() => Open(Application.persistentDataPath);

        public static PersistentMetaWallet Open(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("Assign a save directory.", nameof(directory));
            string path = Path.Combine(Path.GetFullPath(directory), "meta-wallet.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            // Exclusive access also prevents two processes from settling into the same file at once.
            using (AcquireLock(path))
            {
                if (File.Exists(path))
                {
                    string text = File.ReadAllText(path, Encoding.UTF8);
                    long balance = ReadBalance(text, out var settledRuns);
                    return new PersistentMetaWallet(path, text, balance, settledRuns);
                }
                if (File.Exists(path + ".bak") || File.Exists(path + ".tmp"))
                    throw new InvalidDataException("Wallet is missing but recovery files exist. Preserve them and recover explicitly.");
                var emptyRuns = new HashSet<string>(StringComparer.Ordinal);
                string initial = Serialize(0, emptyRuns);
                Write(path, initial);
                return new PersistentMetaWallet(path, initial, 0, emptyRuns);
            }
        }

        /// <summary>Storage primitive, not a run reward handler. Run settlement must deduplicate before calling.</summary>
        public void Credit(long amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Credit must be positive.");
            long next = checked(Balance + amount);
            Commit(next, _settledRuns);
        }

        /// <returns>False when this run was already paid, including after reopening the save.</returns>
        public bool Settle(RunEconomyResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (_settledRuns.Contains(result.RunId)) return false;
            long next = checked(Balance + result.Transferred);
            var settledRuns = new HashSet<string>(_settledRuns, StringComparer.Ordinal) { result.RunId };
            Commit(next, settledRuns);
            return true;
        }

        private void Commit(long next, HashSet<string> settledRuns)
        {
            string text = Serialize(next, settledRuns);
            using (AcquireLock(_path))
            {
                if (!File.Exists(_path) || File.ReadAllText(_path, Encoding.UTF8) != _savedText)
                    throw new InvalidOperationException("Wallet changed on disk. Reopen it before making changes.");
                Write(_path, text);
                // Publish only after the durable write has completed.
                _savedText = text;
                Balance = next;
                _settledRuns = settledRuns;
            }
        }

        private static FileStream AcquireLock(string path) =>
            new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

        private static string Serialize(long balance, HashSet<string> settledRuns) => JsonUtility.ToJson(new Record
        {
            version = CurrentVersion,
            balance = balance.ToString(System.Globalization.CultureInfo.InvariantCulture),
            settledRuns = new List<string>(settledRuns).ToArray()
        }, true);

        private static long ReadBalance(string text, out HashSet<string> settledRuns)
        {
            Record record;
            try { record = JsonUtility.FromJson<Record>(text); }
            catch (ArgumentException error) { throw new InvalidDataException("Wallet JSON is invalid. Original file was preserved.", error); }
            if (record == null || (record.version != 1 && record.version != CurrentVersion))
                throw new InvalidDataException("Unsupported or missing wallet version. Original file was preserved.");
            if (!long.TryParse(record.balance, System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out long balance))
                throw new InvalidDataException("Wallet balance is missing, negative or outside its supported range.");
            settledRuns = new HashSet<string>(StringComparer.Ordinal);
            if (record.version == CurrentVersion)
            {
                if (record.settledRuns == null) throw new InvalidDataException("Missing settled run history.");
                foreach (string value in record.settledRuns)
                {
                    if (!Guid.TryParseExact(value, "N", out var id) || id == Guid.Empty ||
                        !settledRuns.Add(id.ToString("N")))
                        throw new InvalidDataException("Invalid or duplicate settled run identity.");
                }
            }
            return balance;
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
