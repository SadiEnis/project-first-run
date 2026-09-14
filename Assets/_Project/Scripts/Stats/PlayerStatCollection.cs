using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ProjectFirstRun.Stats
{
    public sealed class PlayerStatCollection
    {
        private readonly List<StatModifier> _modifiers =
            new List<StatModifier>();

        private readonly ReadOnlyCollection<StatModifier>
            _readOnlyModifiers;

        public IReadOnlyList<StatModifier> Modifiers =>
            _readOnlyModifiers;

        public int ModifierCount =>
            _modifiers.Count;
        
        public event Action Changed;

        public PlayerStatCollection()
        {
            _readOnlyModifiers =
                _modifiers.AsReadOnly();
        }

        public void Add(
            StatModifier modifier)
        {
            if (modifier == null)
            {
                throw new ArgumentNullException(
                    nameof(modifier));
            }

            if (_modifiers.Contains(modifier))
            {
                throw new InvalidOperationException(
                    "The same stat modifier instance " +
                    "has already been added.");
            }

            _modifiers.Add(
                modifier);
            
            Changed?.Invoke();
        }

        // Internal transaction support: callers validate, commit their state, replace, then notify.
        internal void ValidateReplacement(StatModifier[] previous, StatModifier[] next)
        {
            if (previous == null || next == null) throw new ArgumentNullException();
            var seen = new HashSet<StatModifier>();
            foreach (var modifier in previous)
                if (modifier == null || !seen.Add(modifier) || !_modifiers.Contains(modifier))
                    throw new InvalidOperationException("Expected modifier is missing or duplicated.");
            seen.Clear();
            foreach (var modifier in next)
                if (modifier == null || !seen.Add(modifier) || _modifiers.Contains(modifier))
                    throw new InvalidOperationException("Replacement modifier is null or already installed.");
        }

        internal void ReplaceWithoutNotification(StatModifier[] previous, StatModifier[] next)
        {
            ValidateReplacement(previous, next);
            foreach (var modifier in previous) _modifiers.Remove(modifier);
            _modifiers.AddRange(next);
        }

        internal void NotifyChanged() => Changed?.Invoke();

        public bool Remove(
            StatModifier modifier)
        {
            if (modifier == null)
            {
                return false;
            }

            bool removed =
                _modifiers.Remove(
                    modifier);

            if (removed)
            {
                Changed?.Invoke();
            }

            return removed;
        }

        public int RemoveBySource(
            string sourceId)
        {
            ValidateSourceId(
                sourceId);

            int removedCount = 0;

            for (int index =
                     _modifiers.Count - 1;
                 index >= 0;
                 index--)
            {
                StatModifier modifier =
                    _modifiers[index];

                if (!string.Equals(
                        modifier.SourceId,
                        sourceId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                _modifiers.RemoveAt(
                    index);

                removedCount++;
            }

            if (removedCount > 0)
            {
                Changed?.Invoke();
            }
            
            return removedCount;
        }

        public bool Contains(
            StatModifier modifier)
        {
            if (modifier == null)
            {
                return false;
            }

            return _modifiers.Contains(
                modifier);
        }

        public float Evaluate(
            PlayerStatType statType,
            float baseValue)
        {
            ValidateStatType(
                statType);

            ValidateFinite(
                baseValue,
                nameof(baseValue));

            float flatTotal = 0f;
            float additivePercentTotal = 0f;
            float multiplicativeFactor = 1f;

            foreach (StatModifier modifier
                     in _modifiers)
            {
                if (modifier.StatType !=
                    statType)
                {
                    continue;
                }

                switch (modifier.Operation)
                {
                    case StatModifierOperation.Flat:
                        flatTotal +=
                            modifier.Value;
                        break;

                    case StatModifierOperation.AdditivePercent:
                        additivePercentTotal +=
                            modifier.Value;
                        break;

                    case StatModifierOperation.MultiplicativePercent:
                        multiplicativeFactor *=
                            1f + modifier.Value;
                        break;

                    default:
                        throw new InvalidOperationException(
                            $"Unsupported stat modifier operation: " +
                            $"{modifier.Operation}.");
                }
            }

            float result =
                (baseValue + flatTotal) *
                (1f + additivePercentTotal) *
                multiplicativeFactor;

            if (float.IsNaN(result) ||
                float.IsInfinity(result))
            {
                throw new InvalidOperationException(
                    $"Evaluation of stat '{statType}' " +
                    "produced a non-finite value.");
            }

            return result;
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
                    "Stat value must be finite.");
            }
        }

        private static void ValidateSourceId(
            string sourceId)
        {
            if (string.IsNullOrWhiteSpace(sourceId))
            {
                throw new ArgumentException(
                    "Source ID is required.",
                    nameof(sourceId));
            }

            if (!string.Equals(
                    sourceId,
                    sourceId.Trim(),
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Source ID cannot contain leading " +
                    "or trailing whitespace.",
                    nameof(sourceId));
            }
        }
    }
}
