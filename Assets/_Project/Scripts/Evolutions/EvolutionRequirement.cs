using System;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
using UnityEngine;

namespace ProjectFirstRun.Evolutions
{
    [Serializable]
    public sealed class EvolutionRequirement
    {
        [SerializeField] private ItemCategory _category;
        [SerializeField] private string _stableId;
        [SerializeField] private bool _requireMaximumLevel = true;

        public EvolutionRequirement(ItemCategory category, string stableId, bool requireMaximumLevel = true)
        {
            _category = category; _stableId = stableId; _requireMaximumLevel = requireMaximumLevel;
        }

        internal bool IsSatisfied(PlayerBuild build)
        {
            if (build == null || string.IsNullOrWhiteSpace(_stableId)) return false;
            if (!build.TryGetItem(_category, _stableId, out var item)) return false;
            return !_requireMaximumLevel || item.IsAtMaximumLevel;
        }

        internal void Validate()
        {
            if (!Enum.IsDefined(typeof(ItemCategory), _category) || string.IsNullOrWhiteSpace(_stableId) || _stableId != _stableId.Trim())
                throw new InvalidOperationException("Evolution requirement is invalid.");
        }
    }
}
