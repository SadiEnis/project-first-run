using System;
using ProjectFirstRun.Items;
using ProjectFirstRun.Player;
using ProjectFirstRun.Rewards.Claims;
using UnityEngine;

namespace ProjectFirstRun.UI.Rewards
{
    [DisallowMultipleComponent]
    public sealed class RewardSelectionController :
        MonoBehaviour
    {
        private const string AlreadyOwnedFeedback =
            "This reward is already owned.";

        private const string CapacityReachedFeedback =
            "There is no available slot for this reward.";

        [Header("Selection")]
        [SerializeField]
        private RewardSelectionView _view;

        [SerializeField]
        private PlayerRewardClaimController _claimController;

        [Header("Player")]
        [SerializeField]
        private PlayerController _playerController;

        [SerializeField]
        private PlayerDeathController _playerDeathController;

        private RewardClaimSession _activeSession;
        private float _previousTimeScale;
        private bool _previousControlEnabled;
        private bool _hasCapturedModalState;
        private bool _isListening;

        public bool IsOpen =>
            _activeSession != null;

        public RewardClaimSession ActiveSession =>
            _activeSession;

        public event Action<RewardClaimSession>
            SelectionOpened;

        public event Action<RewardClaimResult>
            SelectionClosed;

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

            if (IsOpen)
            {
                DismissWithoutClaimResult();
            }
        }

        private void OnDestroy()
        {
            StopListening();
        }

        public void Initialize(
            RewardSelectionView view,
            PlayerRewardClaimController claimController,
            PlayerController playerController,
            PlayerDeathController playerDeathController)
        {
            StopListening();

            _view = view;
            _claimController = claimController;
            _playerController = playerController;
            _playerDeathController = playerDeathController;

            ValidateReferences();
            StartListening();
        }

        public void Open(
            RewardClaimSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(
                    nameof(session));
            }

            ValidateReferences();

            if (!isActiveAndEnabled)
            {
                throw new InvalidOperationException(
                    "Reward selection controller must be enabled before opening.");
            }

            if (IsOpen)
            {
                throw new InvalidOperationException(
                    "A reward selection is already open.");
            }

            if (session.IsClaimed)
            {
                throw new InvalidOperationException(
                    "A claimed reward session cannot be opened.");
            }

            if (session.Offer.IsEmpty)
            {
                throw new ArgumentException(
                    "A reward selection requires at least one choice.",
                    nameof(session));
            }

            CaptureModalState();
            _activeSession = session;

            try
            {
                Time.timeScale = 0f;

                _playerController.SetControlEnabled(
                    false);

                _view.Show(
                    session.Offer,
                    _claimController.GetChoiceState);
            }
            catch
            {
                _view.Hide();
                _activeSession = null;
                RestoreModalState();
                throw;
            }

            SelectionOpened?.Invoke(
                session);
        }

        public RewardClaimResult Select(
            ItemDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            if (_activeSession == null)
            {
                throw new InvalidOperationException(
                    "A reward selection must be open before selecting.");
            }

            RewardClaimResult result =
                _claimController.TryClaim(
                    _activeSession,
                    definition);

            switch (result)
            {
                case RewardClaimResult.Claimed:
                case RewardClaimResult.AlreadyClaimed:
                    CloseWithResult(
                        result);

                    return result;

                case RewardClaimResult.AlreadyOwned:
                    _view.ShowFeedback(
                        AlreadyOwnedFeedback);

                    return result;

                case RewardClaimResult.CapacityReached:
                    _view.ShowFeedback(
                        CapacityReachedFeedback);

                    return result;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported reward claim result: {result}.");
            }
        }

        private void HandleChoiceSelected(
            ItemDefinition definition)
        {
            Select(
                definition);
        }

        private void CloseWithResult(
            RewardClaimResult result)
        {
            _view.Hide();
            _activeSession = null;
            RestoreModalState();

            SelectionClosed?.Invoke(
                result);
        }

        private void DismissWithoutClaimResult()
        {
            if (_view != null)
            {
                _view.Hide();
            }

            _activeSession = null;
            RestoreModalState();
        }

        private void CaptureModalState()
        {
            _previousTimeScale =
                Time.timeScale;

            _previousControlEnabled =
                _playerController.IsControlEnabled;

            _hasCapturedModalState = true;
        }

        private void RestoreModalState()
        {
            if (!_hasCapturedModalState)
            {
                return;
            }

            Time.timeScale =
                _previousTimeScale;

            if (CanRestorePlayerControl())
            {
                _playerController.SetControlEnabled(
                    _previousControlEnabled);
            }

            _hasCapturedModalState = false;
        }

        private bool CanRestorePlayerControl()
        {
            if (_playerController == null ||
                !_playerController.enabled ||
                !_playerController.gameObject.activeInHierarchy)
            {
                return false;
            }

            return !_previousControlEnabled ||
                   !_playerDeathController.IsDead;
        }

        private void StartListening()
        {
            if (_isListening ||
                _view == null)
            {
                return;
            }

            _view.ChoiceSelected +=
                HandleChoiceSelected;

            _isListening = true;
        }

        private void StopListening()
        {
            if (!_isListening)
            {
                return;
            }

            if (_view != null)
            {
                _view.ChoiceSelected -=
                    HandleChoiceSelected;
            }

            _isListening = false;
        }

        private void ValidateReferences()
        {
            if (_view == null ||
                _claimController == null ||
                _playerController == null ||
                _playerDeathController == null)
            {
                throw new InvalidOperationException(
                    "Reward selection controller references are incomplete.");
            }

            if (_playerController.gameObject !=
                _playerDeathController.gameObject)
            {
                throw new InvalidOperationException(
                    "Player control and death references must belong to the same player.");
            }
        }
    }
}
