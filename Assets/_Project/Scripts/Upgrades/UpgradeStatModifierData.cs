using System;
using ProjectFirstRun.Stats;
using UnityEngine;

namespace ProjectFirstRun.Upgrades
{
    [Serializable]
    public sealed class UpgradeStatModifierData
    {
        [SerializeField]
        private PlayerStatType _statType;

        [SerializeField]
        private StatModifierOperation _operation;

        [SerializeField]
        private float _value;

        public PlayerStatType StatType =>
            _statType;

        public StatModifierOperation Operation =>
            _operation;

        public float Value =>
            _value;

        public UpgradeStatModifierData(
            PlayerStatType statType,
            StatModifierOperation operation,
            float value)
        {
            ValidateStatType(
                statType);

            ValidateOperation(
                operation);

            ValidateFinite(
                value);

            _statType = statType;
            _operation = operation;
            _value = value;
        }

        public StatModifier CreateRuntimeModifier(
            string sourceId)
        {
            ValidateStatType(
                _statType);

            ValidateOperation(
                _operation);

            ValidateFinite(
                _value);

            return new StatModifier(
                _statType,
                _operation,
                _value,
                sourceId);
        }

        private static void ValidateStatType(
            PlayerStatType statType)
        {
            if (!Enum.IsDefined(
                    typeof(PlayerStatType),
                    statType))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(statType),
                    statType,
                    "Unsupported player stat type.");
            }
        }

        private static void ValidateOperation(
            StatModifierOperation operation)
        {
            if (!Enum.IsDefined(
                    typeof(StatModifierOperation),
                    operation))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(operation),
                    operation,
                    "Unsupported stat modifier operation.");
            }
        }

        private static void ValidateFinite(
            float value)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Upgrade stat modifier value must be finite.");
            }
        }
    }
}