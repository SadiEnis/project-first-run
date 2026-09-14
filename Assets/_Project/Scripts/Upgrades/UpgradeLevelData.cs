using System;
using ProjectFirstRun.Stats;
using UnityEngine;

namespace ProjectFirstRun.Upgrades
{
    [Serializable]
    public sealed class UpgradeLevelData
    {
        [SerializeField] private UpgradeStatModifierData[] _statModifiers = Array.Empty<UpgradeStatModifierData>();

        public UpgradeLevelData(params UpgradeStatModifierData[] modifiers) => _statModifiers = modifiers;

        internal StatModifier[] CreateModifiers(string sourceId)
        {
            if (_statModifiers == null) throw new InvalidOperationException("Missing upgrade level modifiers.");
            var result = new StatModifier[_statModifiers.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = (_statModifiers[i] ?? throw new InvalidOperationException("Null upgrade modifier.")).CreateRuntimeModifier(sourceId);
            return result;
        }
    }
}
