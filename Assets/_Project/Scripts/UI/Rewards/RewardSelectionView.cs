using System;
using ProjectFirstRun.Items;
using ProjectFirstRun.Rewards;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectFirstRun.UI.Rewards
{
    [DisallowMultipleComponent]
    public sealed class RewardSelectionView : MonoBehaviour
    {
        private const string HeaderLabel = "Choose a Reward";
        private const string RemainingSelectionsLabelFormat =
            "Selections Remaining: {0}";

        [SerializeField]
        private GameObject _modalRoot;

        [SerializeField]
        private Text _headerText;

        [SerializeField]
        private Text _remainingSelectionsText;

        [SerializeField]
        private Text _feedbackText;

        [SerializeField]
        private RewardChoiceView[] _choiceViews;

        private bool _isListening;

        public bool IsVisible =>
            _modalRoot != null &&
            _modalRoot.activeSelf;

        public event Action<ItemDefinition> ChoiceSelected;

        private void Awake()
        {
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
            GameObject modalRoot,
            Text headerText,
            Text remainingSelectionsText,
            Text feedbackText,
            RewardChoiceView[] choiceViews)
        {
            StopListening();

            _modalRoot = modalRoot;
            _headerText = headerText;
            _remainingSelectionsText = remainingSelectionsText;
            _feedbackText = feedbackText;
            _choiceViews = choiceViews;

            ValidateReferences();
            StartListening();
        }

        public void Show(RewardOffer offer)
        {
            Show(offer, null, 1);
        }

        public void Show(RewardOffer offer, Func<ItemDefinition, string> stateResolver)
        {
            Show(offer, stateResolver, 1);
        }

        public void Show(
            RewardOffer offer,
            Func<ItemDefinition, string> stateResolver,
            int selectionsRemaining)
        {
            if (offer == null)
            {
                throw new ArgumentNullException(
                    nameof(offer));
            }

            if (offer.IsEmpty)
            {
                throw new ArgumentException(
                    "Reward offer must contain at least one choice.",
                    nameof(offer));
            }

            ValidateReferences();

            if (offer.ChoiceCount > _choiceViews.Length)
            {
                throw new InvalidOperationException(
                    "Reward offer exceeds the configured choice view count.");
            }

            if (selectionsRemaining <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(selectionsRemaining),
                    selectionsRemaining,
                    "Selections remaining must be greater than zero.");
            }

            for (int index = 0;
                 index < _choiceViews.Length;
                 index++)
            {
                if (index < offer.ChoiceCount)
                {
                    ItemDefinition choice = offer.Choices[index];
                    _choiceViews[index].Bind(
                        choice,
                        stateResolver == null ? "NEW" : stateResolver(choice));
                }
                else
                {
                    _choiceViews[index].Clear();
                }
            }

            _headerText.text = HeaderLabel;
            _remainingSelectionsText.text =
                string.Format(
                    RemainingSelectionsLabelFormat,
                    selectionsRemaining);
            _feedbackText.text = string.Empty;
            _modalRoot.SetActive(true);

            SelectFirstChoice();
        }

        public void MarkSelectionCommitted(
            ItemDefinition definition,
            int selectionsRemaining)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (selectionsRemaining <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(selectionsRemaining),
                    selectionsRemaining,
                    "Selections remaining must be greater than zero.");
            }

            for (int index = 0; index < _choiceViews.Length; index++)
            {
                RewardChoiceView choiceView = _choiceViews[index];
                if (choiceView != null &&
                    ReferenceEquals(choiceView.Definition, definition))
                {
                    choiceView.SetInteractable(false);
                    break;
                }
            }

            _remainingSelectionsText.text =
                string.Format(
                    RemainingSelectionsLabelFormat,
                    selectionsRemaining);
        }

        public void Hide()
        {
            if (_modalRoot != null)
            {
                _modalRoot.SetActive(false);
            }
        }

        public void ShowFeedback(string message)
        {
            if (_feedbackText == null)
            {
                throw new InvalidOperationException(
                    "Reward selection feedback text is not configured.");
            }

            _feedbackText.text = message ?? string.Empty;
        }

        private void HandleChoiceSelected(
            ItemDefinition definition)
        {
            ChoiceSelected?.Invoke(
                definition);
        }

        private void StartListening()
        {
            if (_isListening ||
                _choiceViews == null)
            {
                return;
            }

            for (int index = 0;
                 index < _choiceViews.Length;
                 index++)
            {
                RewardChoiceView choiceView =
                    _choiceViews[index];

                if (choiceView != null)
                {
                    choiceView.Selected +=
                        HandleChoiceSelected;
                }
            }

            _isListening = true;
        }

        private void StopListening()
        {
            if (!_isListening ||
                _choiceViews == null)
            {
                return;
            }

            for (int index = 0;
                 index < _choiceViews.Length;
                 index++)
            {
                RewardChoiceView choiceView =
                    _choiceViews[index];

                if (choiceView != null)
                {
                    choiceView.Selected -=
                        HandleChoiceSelected;
                }
            }

            _isListening = false;
        }

        private void SelectFirstChoice()
        {
            if (EventSystem.current == null)
            {
                return;
            }

            Button firstButton =
                _choiceViews[0].Button;

            if (firstButton != null &&
                firstButton.isActiveAndEnabled)
            {
                EventSystem.current.SetSelectedGameObject(
                    firstButton.gameObject);
            }
        }

        private void ValidateReferences()
        {
            if (_modalRoot == null ||
                _headerText == null ||
                _remainingSelectionsText == null ||
                _feedbackText == null ||
                _choiceViews == null ||
                _choiceViews.Length == 0)
            {
                throw new InvalidOperationException(
                    "Reward selection view references are incomplete.");
            }

            for (int index = 0;
                 index < _choiceViews.Length;
                 index++)
            {
                if (_choiceViews[index] == null)
                {
                    throw new InvalidOperationException(
                        $"Reward choice view at index {index} is missing.");
                }
            }
        }
    }
}
