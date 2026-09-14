using System;
using ProjectFirstRun.Items;
using UnityEngine;

namespace ProjectFirstRun.Evolutions
{
    [CreateAssetMenu(fileName = "ED_NewEvolution", menuName = "Project First Run/Evolution")]
    public sealed class EvolutionDefinition : ScriptableObject
    {
        [SerializeField] private ItemDefinition _sourceItem;
        [SerializeField] private ItemDefinition _resultItem;
        [SerializeField] private EvolutionRequirement[] _requirements = Array.Empty<EvolutionRequirement>();

        public ItemDefinition SourceItem => _sourceItem;
        public ItemDefinition ResultItem => _resultItem;

        public void Validate()
        {
            if (_sourceItem == null || _resultItem == null) throw new InvalidOperationException("Evolution source and result are required.");
            _sourceItem.ValidateLevelConfiguration();
            _resultItem.ValidateLevelConfiguration();
            if (_sourceItem.Category != _resultItem.Category) throw new InvalidOperationException("Evolution source and result must share a category.");
            if (string.Equals(_sourceItem.StableId, _resultItem.StableId, StringComparison.Ordinal)) throw new InvalidOperationException("Evolution source and result must differ.");
            if (_requirements == null) throw new InvalidOperationException("Evolution requirements cannot be null.");
            for (int i = 0; i < _requirements.Length; i++)
            {
                if (_requirements[i] == null) throw new InvalidOperationException("Evolution requirement is missing.");
                _requirements[i].Validate();
            }
        }

        internal bool RequirementsSatisfied(ProjectFirstRun.Builds.PlayerBuild build)
        {
            for (int i = 0; i < _requirements.Length; i++) if (!_requirements[i].IsSatisfied(build)) return false;
            return true;
        }
    }
}
