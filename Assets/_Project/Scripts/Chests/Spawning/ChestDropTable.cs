using System;
using System.Collections.Generic;
using ProjectFirstRun.Rewards;
using UnityEngine;

namespace ProjectFirstRun.Chests.Spawning
{
    [CreateAssetMenu(fileName = "CDT_NewTable", menuName = "Project First Run/Chests/Drop Table")]
    public sealed class ChestDropTable : ScriptableObject
    {
        [SerializeField] private WeightedChestEntry[] _entries = Array.Empty<WeightedChestEntry>();
        public IReadOnlyList<WeightedChestEntry> Entries => _entries;

        public int Validate()
        {
            if (_entries == null || _entries.Length == 0)
                throw new InvalidOperationException("Chest drop table requires weighted entries.");
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

        public ChestDefinition Select(IRandomSource random)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            int total = Validate();
            int roll = random.Next(0, total);
            if (roll < 0 || roll >= total)
                throw new InvalidOperationException("Random source returned a value outside the requested range.");
            foreach (WeightedChestEntry entry in _entries)
            {
                if (roll < entry.Weight) return entry.Definition;
                roll -= entry.Weight;
            }
            throw new InvalidOperationException("Chest weight selection failed.");
        }

        public ChestDefinition Select(IRandomSource random, float luckMultiplier)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (!float.IsFinite(luckMultiplier) || luckMultiplier < 1f)
                throw new ArgumentOutOfRangeException(nameof(luckMultiplier));
            if (luckMultiplier == 1f) return Select(random);
            Validate();
            // Keep fractional weights; discretize only the final random draw (one-billionth resolution).
            double total = 0;
            foreach (WeightedChestEntry entry in _entries)
                total += entry.Weight * (entry.Definition.AffectedByLuck ? (double)luckMultiplier : 1d);
            const int resolution = 1000000000;
            int sample = random.Next(0, resolution);
            if (sample < 0 || sample >= resolution)
                throw new InvalidOperationException("Random source returned a value outside the requested range.");
            double cumulative = 0;
            foreach (WeightedChestEntry entry in _entries)
            {
                cumulative += entry.Weight * (entry.Definition.AffectedByLuck ? (double)luckMultiplier : 1d);
                if (sample < cumulative / total * resolution) return entry.Definition;
            }
            throw new InvalidOperationException("Luck-adjusted chest selection failed.");
        }
    }
}
