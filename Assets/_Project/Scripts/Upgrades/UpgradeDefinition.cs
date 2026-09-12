using System;
using System.Collections.Generic;
using ProjectFirstRun.Items;
using ProjectFirstRun.Stats;
using UnityEngine;

namespace ProjectFirstRun.Upgrades
{
    [CreateAssetMenu(
        fileName = "UD_NewUpgrade",
        menuName = "Project First Run/Items/Upgrade")]
    public sealed class UpgradeDefinition :
        ItemDefinition
    {
        [Header("Stat Modifiers")]
        [SerializeField]
        private List<UpgradeStatModifierData> _statModifiers =
            new List<UpgradeStatModifierData>();

        public override ItemCategory Category =>
            ItemCategory.Upgrade;

        public IReadOnlyList<UpgradeStatModifierData>
            StatModifiers =>
                _statModifiers;

        public int ModifierCount =>
            _statModifiers.Count;

        public StatModifier[] CreateRuntimeModifiers()
        {
            ValidateStableId();

            StatModifier[] runtimeModifiers =
                new StatModifier[
                    _statModifiers.Count];

            for (int index = 0;
                 index < _statModifiers.Count;
                 index++)
            {
                UpgradeStatModifierData modifierData =
                    _statModifiers[index];

                if (modifierData == null)
                {
                    throw new InvalidOperationException(
                        $"Upgrade definition '{name}' contains " +
                        $"a null stat modifier at index {index}.");
                }

                runtimeModifiers[index] =
                    modifierData.CreateRuntimeModifier(
                        StableId);
            }

            return runtimeModifiers;
        }

        private void ValidateStableId()
        {
            if (string.IsNullOrWhiteSpace(
                    StableId))
            {
                throw new InvalidOperationException(
                    $"Upgrade definition '{name}' " +
                    "requires a stable ID.");
            }

            if (!string.Equals(
                    StableId,
                    StableId.Trim(),
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Upgrade definition '{name}' stable ID " +
                    "cannot contain leading or trailing whitespace.");
            }
        }
    }
}