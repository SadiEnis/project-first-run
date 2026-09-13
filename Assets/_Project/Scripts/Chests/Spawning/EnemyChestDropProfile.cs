using System;
using ProjectFirstRun.Rewards;
using UnityEngine;

namespace ProjectFirstRun.Chests.Spawning
{
    [Serializable]
    public sealed class WeightedChestEntry
    {
        [SerializeField] private ChestDefinition _definition;
        [SerializeField, Min(1)] private int _weight = 1;

        public ChestDefinition Definition => _definition;
        public int Weight => _weight;

        public WeightedChestEntry(ChestDefinition definition, int weight)
        {
            _definition = definition;
            _weight = weight;
        }
    }

    [CreateAssetMenu(fileName = "ChestDropProfile", menuName = "Project First Run/Chests/Enemy Drop Profile")]
    public sealed class EnemyChestDropProfile : ScriptableObject
    {
        [SerializeField, Range(0, 10000)] private int _chanceBasisPoints;
        [SerializeField] private WeightedChestEntry[] _entries = Array.Empty<WeightedChestEntry>();

        public int ChanceBasisPoints => _chanceBasisPoints;

        public int Validate()
        {
            if (_chanceBasisPoints < 0 || _chanceBasisPoints > 10000)
                throw new InvalidOperationException("Chest drop chance must be between 0 and 10000 basis points.");
            if (_chanceBasisPoints == 0) return 0;
            if (_entries == null || _entries.Length == 0)
                throw new InvalidOperationException("An enabled chest drop profile requires weighted entries.");
            long total = 0;
            foreach (WeightedChestEntry entry in _entries)
            {
                if (entry == null || entry.Definition == null || entry.Weight <= 0)
                    throw new InvalidOperationException("Chest entries require a definition and a positive weight.");
                entry.Definition.Validate();
                total += entry.Weight;
                if (total > int.MaxValue)
                    throw new InvalidOperationException("Chest weight total exceeds the supported random range.");
            }
            return (int)total;
        }

        public bool TryRoll(IRandomSource random, out ChestDefinition definition)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            definition = null;
            int total = Validate();
            if (_chanceBasisPoints == 0) return false;
            if (_chanceBasisPoints < 10000 && Next(random, 10000) >= _chanceBasisPoints) return false;
            int roll = Next(random, total);
            foreach (WeightedChestEntry entry in _entries)
            {
                if (roll < entry.Weight)
                {
                    definition = entry.Definition;
                    return true;
                }
                roll -= entry.Weight;
            }
            throw new InvalidOperationException("Chest weight selection failed.");
        }

        private static int Next(IRandomSource random, int maximum)
        {
            int value = random.Next(0, maximum);
            if (value < 0 || value >= maximum)
                throw new InvalidOperationException("Random source returned a value outside the requested range.");
            return value;
        }
    }
}
