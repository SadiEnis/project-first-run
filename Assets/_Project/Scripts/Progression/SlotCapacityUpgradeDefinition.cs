using System;
using UnityEngine;
namespace ProjectFirstRun.Progression
{
    [CreateAssetMenu(menuName = "Project First Run/Meta/Slot Capacity Upgrade")]
    public sealed class SlotCapacityUpgradeDefinition : ScriptableObject
    {
        [SerializeField] private string _stableId;
        [SerializeField] private string _displayName;
        [SerializeField, Min(1)] private int _startingCapacity;
        [SerializeField] private int[] _costs = Array.Empty<int>();
        public string StableId => _stableId;
        public string DisplayName => _displayName;
        public int MaxLevel => _costs.Length;
        public void Validate(int maximumCapacity)
        {
            MetaWalletSnapshot.ValidateUpgradeId(_stableId);
            if (string.IsNullOrWhiteSpace(_displayName) || _costs == null || _costs.Length == 0 ||
                _startingCapacity < 1 || _costs.Length > maximumCapacity - _startingCapacity)
                throw new InvalidOperationException("Assign a slot name, starting capacity and levels within the build limit.");
            foreach (int cost in _costs)
                if (cost <= 0) throw new InvalidOperationException("Slot upgrade costs must be positive.");
        }
        public int GetCapacity(int level)
        {
            if (level < 0 || level > MaxLevel) throw new ArgumentOutOfRangeException(nameof(level));
            return checked(_startingCapacity + level);
        }
        public int GetNextCost(int level)
        {
            if (level < 0 || level >= MaxLevel) throw new ArgumentOutOfRangeException(nameof(level));
            return _costs[level];
        }
    }
}
