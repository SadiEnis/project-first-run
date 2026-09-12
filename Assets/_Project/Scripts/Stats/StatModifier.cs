using System;

namespace ProjectFirstRun.Stats
{
    public sealed class StatModifier
    {
        public PlayerStatType StatType { get; }

        public StatModifierOperation Operation { get; }

        public float Value { get; }

        public string SourceId { get; }

        public StatModifier(
            PlayerStatType statType,
            StatModifierOperation operation,
            float value,
            string sourceId)
        {
            ValidateStatType(
                statType);

            ValidateOperation(
                operation);

            ValidateFinite(
                value,
                nameof(value));

            ValidateSourceId(
                sourceId);

            StatType = statType;
            Operation = operation;
            Value = value;
            SourceId = sourceId;
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
            float value,
            string parameterName)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Stat modifier value must be finite.");
            }
        }

        private static void ValidateSourceId(
            string sourceId)
        {
            if (string.IsNullOrWhiteSpace(sourceId))
            {
                throw new ArgumentException(
                    "Stat modifier source ID is required.",
                    nameof(sourceId));
            }

            if (!string.Equals(
                    sourceId,
                    sourceId.Trim(),
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Stat modifier source ID cannot contain " +
                    "leading or trailing whitespace.",
                    nameof(sourceId));
            }
        }
    }
}