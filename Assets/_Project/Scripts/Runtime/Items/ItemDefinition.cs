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
