using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace ProjectFirstRun.Progression
{
    /// <summary>Single-profile local wallet. Call from one owner on the main thread.</summary>
    public sealed class PersistentMetaWallet
    {
        private const int CurrentVersion = 1;
        private readonly string _path;
        private string _savedText;
        public long Balance { get; private set; }
        public string SavePath => _path;

        [Serializable]
        private sealed class Record
        {
            public int version;
            // Strings make missing values distinguishable from a valid zero balance.
            public string balance;
        }

        private PersistentMetaWallet(string path, string savedText, long balance)
        {
            _path = path;
            _savedText = savedText;
            Balance = balance;
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
                    return new PersistentMetaWallet(path, text, ReadBalance(text));
                }
                if (File.Exists(path + ".bak") || File.Exists(path + ".tmp"))
                    throw new InvalidDataException("Wallet is missing but recovery files exist. Preserve them and recover explicitly.");
                string initial = Serialize(0);
                Write(path, initial);
                return new PersistentMetaWallet(path, initial, 0);
            }
        }

        /// <summary>Storage primitive, not a run reward handler. Run settlement must deduplicate before calling.</summary>
        public void Credit(long amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Credit must be positive.");
            long next = checked(Balance + amount);
            string text = Serialize(next);
            using (AcquireLock(_path))
            {
                if (!File.Exists(_path) || File.ReadAllText(_path, Encoding.UTF8) != _savedText)
                    throw new InvalidOperationException("Wallet changed on disk. Reopen it before making changes.");
                Write(_path, text);
                // Publish only after the durable write has completed.
                _savedText = text;
                Balance = next;
            }
        }

        private static FileStream AcquireLock(string path) =>
            new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

        private static string Serialize(long balance) => JsonUtility.ToJson(new Record
        {
            version = CurrentVersion,
            balance = balance.ToString(System.Globalization.CultureInfo.InvariantCulture)
        }, true);

        private static long ReadBalance(string text)
        {
            Record record;
            try { record = JsonUtility.FromJson<Record>(text); }
            catch (ArgumentException error) { throw new InvalidDataException("Wallet JSON is invalid. Original file was preserved.", error); }
            if (record == null || record.version != CurrentVersion)
                throw new InvalidDataException("Unsupported or missing wallet version. Original file was preserved.");
            if (!long.TryParse(record.balance, System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out long balance))
                throw new InvalidDataException("Wallet balance is missing, negative or outside its supported range.");
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
