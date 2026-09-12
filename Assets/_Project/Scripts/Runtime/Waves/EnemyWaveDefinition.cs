using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirstRun.Waves
{
    [CreateAssetMenu(
        fileName = "EnemyWaveDefinition",
        menuName = "Project First Run/Waves/Enemy Wave Definition")]
    public sealed class EnemyWaveDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string _stableId;

        [SerializeField]
        private string _displayName;

        [Header("Enemies")]
        [SerializeField]
        private List<EnemyWaveEntry> _entries =
            new List<EnemyWaveEntry>();

        public string StableId =>
            _stableId;

        public string DisplayName =>
            _displayName;

        public IReadOnlyList<EnemyWaveEntry> Entries =>
            _entries;

        public int EntryCount =>
            _entries != null
                ? _entries.Count
                : 0;

        public int TotalEnemyCount
        {
            get
            {
                Validate();

                int totalEnemyCount = 0;

                foreach (EnemyWaveEntry entry in _entries)
                {
                    totalEnemyCount =
                        checked(totalEnemyCount + entry.Count);
                }

                return totalEnemyCount;
            }
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(_stableId))
            {
                throw new InvalidOperationException(
                    $"{nameof(EnemyWaveDefinition)} '{name}' " +
                    "requires a stable identifier.");
            }

            if (string.IsNullOrWhiteSpace(_displayName))
            {
                throw new InvalidOperationException(
                    $"{nameof(EnemyWaveDefinition)} '{name}' " +
                    "requires a display name.");
            }

            if (_entries == null ||
                _entries.Count == 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(EnemyWaveDefinition)} '{name}' " +
                    "requires at least one enemy entry.");
            }

            for (int index = 0;
                 index < _entries.Count;
                 index++)
            {
                EnemyWaveEntry entry =
                    _entries[index];

                if (entry == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(EnemyWaveDefinition)} '{name}' " +
                        $"contains a null entry at index {index}.");
                }

                try
                {
                    entry.Validate();
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(
                        $"{nameof(EnemyWaveDefinition)} '{name}' " +
                        $"contains an invalid entry at index {index}.",
                        exception);
                }
            }
        }

        private void OnValidate()
        {
            _stableId =
                _stableId?.Trim();

            _displayName =
                _displayName?.Trim();

            _entries ??=
                new List<EnemyWaveEntry>();
        }
    }
}