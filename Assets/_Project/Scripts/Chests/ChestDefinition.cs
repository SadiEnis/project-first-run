using System;
using ProjectFirstRun.Items;
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

        [SerializeField] private string _displayName;
        [SerializeField] private ChestRewardCategory _rewardCategory;
        public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? _stableId : _displayName;
        public ChestRewardCategory RewardCategory => _rewardCategory;

        [SerializeField] private ChestRarity _rarity = ChestRarity.Common;
        public ChestRarity Rarity => _rarity;

        [Header("Reward")]
        [SerializeField]
        private RewardItemPool _rewardItemPool;

        [SerializeField]
        [Min(1)]
        private int _requestedChoiceCount = 3;

        [SerializeField]
        [Min(1)]
        private int _maxSelections = 1;

        [Header("World")]
        [SerializeField]
        private GameObject _worldPrefab;

        public string StableId =>
            _stableId;

        public RewardItemPool RewardItemPool =>
            _rewardItemPool;

        public int RequestedChoiceCount =>
            _requestedChoiceCount;

        public int MaxSelections =>
            _maxSelections;

        public GameObject WorldPrefab =>
            _worldPrefab;

        public void Validate()
        {
            if (!Enum.IsDefined(typeof(ChestRarity), _rarity))
                throw new InvalidOperationException("Chest rarity must be a supported serialized value.");
            if (!Enum.IsDefined(typeof(ChestRewardCategory), _rewardCategory))
                throw new InvalidOperationException("Chest reward category must be a supported serialized value.");

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

            if (_maxSelections <= 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(ChestDefinition)} '{name}' " +
                    "requires a positive maximum selection count.");
            }

            if (_maxSelections > _requestedChoiceCount)
            {
                throw new InvalidOperationException(
                    $"{nameof(ChestDefinition)} '{name}' " +
                    "cannot allow more selections than requested choices.");
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

            foreach (ItemDefinition item in _rewardItemPool.GetValidatedItems())
                if (!AllowsCategory(item.Category))
                    throw new InvalidOperationException($"Chest '{_stableId}' contains an item outside its {_rewardCategory} category.");
        }

        public bool AllowsCategory(ItemCategory category) => _rewardCategory switch
        {
            ChestRewardCategory.Mixed => category == ItemCategory.Weapon || category == ItemCategory.Ability || category == ItemCategory.Upgrade,
            ChestRewardCategory.Weapon => category == ItemCategory.Weapon,
            ChestRewardCategory.Ability => category == ItemCategory.Ability,
            ChestRewardCategory.Upgrade => category == ItemCategory.Upgrade,
            _ => throw new InvalidOperationException("Unsupported chest reward category.")
        };

        private void OnValidate()
        {
            _stableId =
                _stableId?.Trim();
            _displayName = _displayName?.Trim();
        }
    }
}
