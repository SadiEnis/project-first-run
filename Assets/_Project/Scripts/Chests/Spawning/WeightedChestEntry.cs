using System;
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
}
