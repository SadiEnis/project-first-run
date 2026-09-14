using System;
using ProjectFirstRun.Builds;
using UnityEngine;

namespace ProjectFirstRun.Evolutions
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerBuildController))]
    public sealed class PlayerEvolutionController : MonoBehaviour
    {
        private PlayerBuildController _buildController;

        public event Action<EvolutionDefinition> EvolutionApplied;

        private void Awake() => _buildController = GetComponent<PlayerBuildController>();

        public EvolutionResult TryEvolve(EvolutionDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (_buildController == null || !_buildController.IsInitialized)
                throw new InvalidOperationException("PlayerBuildController must be initialized before evolution.");
            definition.Validate();
            var source = definition.SourceItem;
            var result = definition.ResultItem;
            if (!_buildController.Build.TryGetItem(source.Category, source.StableId, out var owned))
                return EvolutionResult.SourceNotOwned;
            if (!owned.IsAtMaximumLevel) return EvolutionResult.SourceNotAtMaximum;
            if (!definition.RequirementsSatisfied(_buildController.Build)) return EvolutionResult.RequirementNotMet;
            if (_buildController.Build.Contains(result.Category, result.StableId)) return EvolutionResult.ResultAlreadyOwned;
            PlayerBuildReplaceResult replace = _buildController.Build.TryReplace(
                source.Category, source.StableId, result.StableId, result.MaximumLevel);
            if (replace == PlayerBuildReplaceResult.SourceNotOwned) return EvolutionResult.SourceNotOwned;
            if (replace == PlayerBuildReplaceResult.TargetAlreadyOwned) return EvolutionResult.ResultAlreadyOwned;
            EvolutionApplied?.Invoke(definition);
            return EvolutionResult.Evolved;
        }
    }
}
