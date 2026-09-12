using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirstRun.Waves
{
    [CreateAssetMenu(
        fileName = "ArenaWaveDefinition",
        menuName = "Project First Run/Waves/Arena Wave Definition")]
    public sealed class ArenaWaveDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string _stableId;

        [Header("Waves")]
        [SerializeField]
        private List<EnemyWaveDefinition> _waves =
            new List<EnemyWaveDefinition>();

        public string StableId =>
            _stableId;

        public IReadOnlyList<EnemyWaveDefinition> Waves =>
            _waves;

        public int WaveCount =>
            _waves != null
                ? _waves.Count
                : 0;

        public int TotalPlannedEnemyCount
        {
            get
            {
                Validate();

                int totalEnemyCount = 0;

                foreach (EnemyWaveDefinition wave in _waves)
                {
                    totalEnemyCount =
                        checked(
                            totalEnemyCount +
                            wave.TotalEnemyCount);
                }

                return totalEnemyCount;
            }
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(_stableId))
            {
                throw new InvalidOperationException(
                    $"{nameof(ArenaWaveDefinition)} '{name}' " +
                    "requires a stable identifier.");
            }

            if (_waves == null ||
                _waves.Count == 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(ArenaWaveDefinition)} '{name}' " +
                    "requires at least one wave.");
            }

            for (int index = 0;
                 index < _waves.Count;
                 index++)
            {
                EnemyWaveDefinition wave =
                    _waves[index];

                if (wave == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(ArenaWaveDefinition)} '{name}' " +
                        $"contains a null wave at index {index}.");
                }

                try
                {
                    wave.Validate();
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(
                        $"{nameof(ArenaWaveDefinition)} '{name}' " +
                        $"contains an invalid wave at index {index}.",
                        exception);
                }
            }
        }

        private void OnValidate()
        {
            _stableId =
                _stableId?.Trim();

            _waves ??=
                new List<EnemyWaveDefinition>();
        }
    }
}