using System;
using UnityEngine;

namespace ProjectFirstRun.Items
{
    public abstract class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string _stableId;

        [SerializeField]
        private string _displayName;

        [TextArea]
        [SerializeField]
        private string _gameplayEffect;

        [Header("Progression")]
        [SerializeField, Min(1)]
        private int _maximumLevel = 1;

        public int MaximumLevel => _maximumLevel;

        public void ValidateLevelConfiguration()
        {
            if (_maximumLevel < 1)
            {
                throw new InvalidOperationException(
                    $"Item '{_stableId}' must have a positive maximum level.");
            }
        }

        public string StableId => _stableId;
        public string DisplayName => _displayName;
        public string GameplayEffect => _gameplayEffect;
        public abstract ItemCategory Category { get; }

        protected virtual void OnValidate()
        {
            _stableId = _stableId?.Trim();
            _displayName = _displayName?.Trim();
            _gameplayEffect = _gameplayEffect?.Trim();
        }
    }
}
