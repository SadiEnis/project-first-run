using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ProjectFirstRun.Weapons
{
    public sealed class PlayerWeaponLoadout
    {
        private readonly List<PlayerWeaponRuntimeEntry>
            _entries =
                new List<PlayerWeaponRuntimeEntry>();

        private readonly ReadOnlyCollection<
            PlayerWeaponRuntimeEntry>
            _readOnlyEntries;

        /*
         * Compatibility view for existing callers.
         * Runtime entries remain authoritative.
         */
        private readonly List<WeaponDefinition>
            _weapons =
                new List<WeaponDefinition>();

        private readonly ReadOnlyCollection<
            WeaponDefinition>
            _readOnlyWeapons;

        public IReadOnlyList<PlayerWeaponRuntimeEntry>
            Entries =>
                _readOnlyEntries;

        public IReadOnlyList<WeaponDefinition>
            Weapons =>
                _readOnlyWeapons;

        public int WeaponCount =>
            _entries.Count;

        public PlayerWeaponRuntimeEntry ActiveEntry
        {
            get;
            private set;
        }

        public WeaponDefinition ActiveDefinition =>
            ActiveEntry?.Definition;

        public bool HasActiveWeapon =>
            ActiveEntry != null;

        public PlayerWeaponLoadout()
        {
            _readOnlyEntries =
                _entries.AsReadOnly();

            _readOnlyWeapons =
                _weapons.AsReadOnly();
        }

        public PlayerWeaponRuntimeEntry Add(
            WeaponDefinition definition)
        {
            ValidateDefinition(
                definition);

            PlayerWeaponRuntimeEntry entry =
                new PlayerWeaponRuntimeEntry(
                    definition);

            Add(
                entry);

            return entry;
        }

        public void Add(
            PlayerWeaponRuntimeEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(
                    nameof(entry));
            }

            ValidateDefinition(
                entry.Definition);

            if (Contains(
                    entry.Definition))
            {
                throw new InvalidOperationException(
                    $"Weapon '{entry.Definition.StableId}' " +
                    "is already present in the runtime loadout.");
            }

            _entries.Add(
                entry);

            _weapons.Add(
                entry.Definition);
        }

        public bool Contains(
            WeaponDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            ValidateStableId(
                definition.StableId);

            for (int index = 0;
                 index < _entries.Count;
                 index++)
            {
                WeaponDefinition existing =
                    _entries[index].Definition;

                if (string.Equals(
                        existing.StableId,
                        definition.StableId,
                        StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public PlayerWeaponRuntimeEntry GetEntry(
            WeaponDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            ValidateStableId(
                definition.StableId);

            for (int index = 0;
                 index < _entries.Count;
                 index++)
            {
                PlayerWeaponRuntimeEntry entry =
                    _entries[index];

                if (ReferenceEquals(
                        entry.Definition,
                        definition))
                {
                    return entry;
                }
            }

            return null;
        }

        public void SetActive(
            WeaponDefinition definition)
        {
            ValidateDefinition(
                definition);

            PlayerWeaponRuntimeEntry entry =
                GetEntry(
                    definition);

            if (entry == null)
            {
                throw new InvalidOperationException(
                    $"Weapon '{definition.StableId}' " +
                    "must belong to the runtime loadout " +
                    "before it can become active.");
            }

            ActiveEntry =
                entry;
        }

        public void SetActive(
            PlayerWeaponRuntimeEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(
                    nameof(entry));
            }

            if (!ContainsExactEntry(
                    entry))
            {
                throw new InvalidOperationException(
                    "Weapon runtime entry must belong " +
                    "to the loadout before it can become active.");
            }

            ActiveEntry =
                entry;
        }

        public PlayerWeaponRuntimeEntry GetNextEntry()
        {
            if (_entries.Count == 0)
            {
                return null;
            }

            if (ActiveEntry == null)
            {
                return _entries[0];
            }

            if (_entries.Count == 1)
            {
                return ActiveEntry;
            }

            int activeIndex =
                _entries.IndexOf(
                    ActiveEntry);

            if (activeIndex < 0)
            {
                throw new InvalidOperationException(
                    "Active weapon entry does not belong " +
                    "to the runtime loadout.");
            }

            int nextIndex =
                (activeIndex + 1) %
                _entries.Count;

            return _entries[
                nextIndex];
        }

        private bool ContainsExactEntry(
            PlayerWeaponRuntimeEntry entry)
        {
            for (int index = 0;
                 index < _entries.Count;
                 index++)
            {
                if (ReferenceEquals(
                        _entries[index],
                        entry))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateDefinition(
            WeaponDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            ValidateStableId(
                definition.StableId);
        }

        private static void ValidateStableId(
            string stableId)
        {
            if (string.IsNullOrWhiteSpace(
                    stableId))
            {
                throw new ArgumentException(
                    "Weapon stable ID cannot be null, " +
                    "empty or whitespace.",
                    nameof(stableId));
            }

            if (!string.Equals(
                    stableId,
                    stableId.Trim(),
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Weapon stable ID cannot contain leading " +
                    "or trailing whitespace.",
                    nameof(stableId));
            }
        }
    }
}