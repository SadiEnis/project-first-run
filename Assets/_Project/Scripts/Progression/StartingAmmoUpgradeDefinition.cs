using System;
using UnityEngine;
namespace ProjectFirstRun.Progression
{
    [CreateAssetMenu(menuName = "Project First Run/Meta/Starting Ammo Upgrade")]
    public sealed class StartingAmmoUpgradeDefinition : ScriptableObject
    {
        [Serializable] private struct Level
        {
            [Min(1)] public int cost;
            [Min(1)] public int totalBonusMagazines;
        }
        [SerializeField] private string _stableId = "meta.starting-ammo";
        [SerializeField] private string _displayName = "Reserve Cache";
        [SerializeField] private Level[] _levels = Array.Empty<Level>();
        public string StableId => _stableId;
        public string DisplayName => _displayName;
        public int MaxLevel => _levels.Length;
        public void Validate()
        {
            MetaWalletSnapshot.ValidateUpgradeId(_stableId);
            if (string.IsNullOrWhiteSpace(_displayName) || _levels == null || _levels.Length == 0)
                throw new InvalidOperationException("Assign upgrade name and levels.");
            int previous = 0;
            foreach (var level in _levels)
            {
                if (level.cost <= 0 || level.totalBonusMagazines <= previous)
                    throw new InvalidOperationException("Upgrade costs must be positive and total magazine bonuses must increase.");
                previous = level.totalBonusMagazines;
            }
        }
        public int GetBonusMagazines(int level)
        {
            Validate();
            if (level < 0 || level > MaxLevel) throw new ArgumentOutOfRangeException(nameof(level));
            return level == 0 ? 0 : _levels[level - 1].totalBonusMagazines;
        }
        public int GetNextCost(int currentLevel)
        {
            Validate();
            if (currentLevel < 0 || currentLevel >= MaxLevel) throw new ArgumentOutOfRangeException(nameof(currentLevel));
            return _levels[currentLevel].cost;
        }
    }
}
