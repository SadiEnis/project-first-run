using System;
using ProjectFirstRun.Items;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectFirstRun.UI.Rewards
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class RewardChoiceView : MonoBehaviour
    {
        [SerializeField]
        private Button _button;

        [SerializeField]
        private Text _nameText;

        [SerializeField]
        private Text _categoryText;

        [SerializeField]
        private Text _effectText;

        [SerializeField]
        private Text _stateText;

        private bool _isListening;

        public ItemDefinition Definition { get; private set; }
        public Button Button => _button;

        public event Action<ItemDefinition> Selected;

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            StartListening();
        }

        private void OnEnable()
        {
            StartListening();
        }

        private void OnDisable()
        {
            StopListening();
        }

        private void OnDestroy()
        {
            StopListening();
        }

        public void Initialize(
            Button button,
            Text nameText,
            Text categoryText,
            Text effectText,
            Text stateText)
        {
            StopListening();

            _button = button;
            _nameText = nameText;
            _categoryText = categoryText;
            _effectText = effectText;
            _stateText = stateText;

            ValidateReferences();
            StartListening();
        }

        public void Bind(ItemDefinition definition)
        {
            Bind(definition, "NEW");
        }

        public void Bind(ItemDefinition definition, string stateLabel)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            ValidateReferences();

            Definition = definition;
            _nameText.text = definition.DisplayName;
            _categoryText.text = definition.Category.ToString();
            _effectText.text = definition.GameplayEffect;
            _stateText.text = string.IsNullOrWhiteSpace(stateLabel) ? "NEW" : stateLabel;

            gameObject.SetActive(true);
        }

        public void Clear()
        {
            Definition = null;

            if (_nameText != null)
            {
                _nameText.text = string.Empty;
            }

            if (_categoryText != null)
            {
                _categoryText.text = string.Empty;
            }

            if (_effectText != null)
            {
                _effectText.text = string.Empty;
            }

            if (_stateText != null)
            {
                _stateText.text = string.Empty;
            }

            gameObject.SetActive(false);
        }

        private void HandleClick()
        {
            if (Definition != null)
            {
                Selected?.Invoke(Definition);
            }
        }

        private void StartListening()
        {
            if (_isListening ||
                _button == null ||
                !isActiveAndEnabled)
            {
                return;
            }

            _button.onClick.AddListener(
                HandleClick);

            _isListening = true;
        }

        private void StopListening()
        {
            if (!_isListening)
            {
                return;
            }

            if (_button != null)
            {
                _button.onClick.RemoveListener(
                    HandleClick);
            }

            _isListening = false;
        }

        private void ValidateReferences()
        {
            if (_button == null ||
                _nameText == null ||
                _categoryText == null ||
                _effectText == null ||
                _stateText == null)
            {
                throw new InvalidOperationException(
                    "Reward choice view references are incomplete.");
            }
        }
    }
}
