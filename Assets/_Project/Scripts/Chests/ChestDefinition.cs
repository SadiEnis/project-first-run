using System;
using ProjectFirstRun.Rewards;
using UnityEngine;

namespace ProjectFirstRun.Chests
{
    [CreateAssetMenu(
        fileName = "CD_NewChest",
        menuName = "Project First Run/Chests/Chest Definition")]
    public sealed class ChestDefinition :
        ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string _stableId;

        [Header("Reward")]
        [SerializeField]
        private RewardItemPool _rewardItemPool;

        [SerializeField]
        [Min(1)]
        private int _requestedChoiceCount = 3;

        [Header("World")]
        [SerializeField]
        private GameObject _worldPrefab;

        public string StableId =>
            _stableId;

        public RewardItemPool RewardItemPool =>
            _rewardItemPool;

        public int RequestedChoiceCount =>
            _requestedChoiceCount;

        public GameObject WorldPrefab =>
            _worldPrefab;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(
                    _stableId))
            {
                throw new InvalidOperationException(
                    $"{nameof(ChestDefinition)} '{name}' " +
                    "requires a stable identifier.");
            }

            if (!string.Equals(
                    _stableId,
                    _stableId.Trim(),
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"{nameof(ChestDefinition)} '{name}' " +
                    "contains an untrimmed stable identifier.");
            }

            if (_rewardItemPool == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ChestDefinition)} '{name}' " +
                    "requires a reward item pool.");
            }

            if (_requestedChoiceCount <= 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(ChestDefinition)} '{name}' " +
                    "requires a positive choice count.");
            }

            if (_worldPrefab == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ChestDefinition)} '{name}' " +
                    "requires a world prefab.");
            }

            if (!_worldPrefab.TryGetComponent<
                    ChestController>(
                        out _))
            {
                throw new InvalidOperationException(
                    $"{nameof(ChestDefinition)} '{name}' world prefab " +
                    $"requires a {nameof(ChestController)} component.");
            }

            _rewardItemPool.ValidateContents();
        }

        private void OnValidate()
        {
            _stableId =
                _stableId?.Trim();
        }
    }
}
