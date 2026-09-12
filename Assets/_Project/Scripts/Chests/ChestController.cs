using System;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.Rewards.Claims;
using ProjectFirstRun.UI.Rewards;
using UnityEngine;

namespace ProjectFirstRun.Chests
{
    [DisallowMultipleComponent]
    public sealed class ChestController :
        MonoBehaviour
    {
        private ChestDefinition _definition;
        private PlayerBuildController _playerBuildController;
        private RewardSelectionController _selectionController;
        private RewardOfferGenerator _offerGenerator;

        private ChestState _state;
        private RewardClaimSession _activeSession;
        private bool _isListening;

        public event Action<ChestController>
            ChestOpened;

        public bool IsInitialized =>
            _state != null;

        public ChestDefinition Definition =>
            _definition;

        public ChestState State =>
            _state;

        public ChestStatus Status =>
            _state != null
                ? _state.Status
                : ChestStatus.Available;

        public RewardClaimSession ActiveSession =>
            _activeSession;

        public void Initialize(
            ChestDefinition definition,
            PlayerBuildController playerBuildController,
            RewardSelectionController selectionController,
            RewardOfferGenerator offerGenerator)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(ChestController)} has already been initialized.");
            }

            _definition =
                definition ??
                throw new ArgumentNullException(
                    nameof(definition));

            _playerBuildController =
                playerBuildController ??
                throw new ArgumentNullException(
                    nameof(playerBuildController));

            _selectionController =
                selectionController ??
                throw new ArgumentNullException(
                    nameof(selectionController));

            _offerGenerator =
                offerGenerator ??
                throw new ArgumentNullException(
                    nameof(offerGenerator));

            _definition.Validate();

            if (!_playerBuildController.IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerBuildController)} must be initialized " +
                    $"before {nameof(ChestController)}.");
            }

            _state =
                new ChestState();

            StartListening();
        }

        public ChestOpenResult TryOpen()
        {
            EnsureInitialized();

            if (_state.IsSelecting)
            {
                return ChestOpenResult.AlreadySelecting;
            }

            if (_state.IsOpened)
            {
                return ChestOpenResult.AlreadyOpened;
            }

            ValidateRuntimeReferences();

            if (_selectionController.IsOpen)
            {
                return ChestOpenResult.SelectionBusy;
            }

            RewardOffer offer =
                _offerGenerator.Generate(
                    _definition.RewardItemPool,
                    _playerBuildController.Build,
                    _definition.RequestedChoiceCount);

            if (offer.IsEmpty)
            {
                return ChestOpenResult.NoEligibleRewards;
            }

            RewardClaimSession session =
                new RewardClaimSession(
                    offer);

            _state.BeginSelection();
            _activeSession = session;

            try
            {
                _selectionController.Open(
                    session);
            }
            catch
            {
                _activeSession = null;
                _state.CancelSelection();
                throw;
            }

            return ChestOpenResult.SelectionOpened;
        }

        private void HandleSelectionClosed(
            RewardClaimResult result)
        {
            if (_state == null ||
                !_state.IsSelecting ||
                _activeSession == null)
            {
                return;
            }

            if (result != RewardClaimResult.Claimed &&
                result != RewardClaimResult.AlreadyClaimed)
            {
                throw new InvalidOperationException(
                    $"A chest selection cannot close with '{result}'.");
            }

            if (!_activeSession.IsClaimed)
            {
                throw new InvalidOperationException(
                    "A chest cannot open before its reward session is consumed.");
            }

            _activeSession = null;
            _state.CompleteSelection();

            ChestOpened?.Invoke(
                this);
        }

        private void StartListening()
        {
            if (_isListening)
            {
                return;
            }

            _selectionController.SelectionClosed +=
                HandleSelectionClosed;

            _isListening = true;
        }

        private void StopListening()
        {
            if (!_isListening)
            {
                return;
            }

            if (_selectionController != null)
            {
                _selectionController.SelectionClosed -=
                    HandleSelectionClosed;
            }

            _isListening = false;
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(ChestController)} must be initialized before use.");
            }
        }

        private void ValidateRuntimeReferences()
        {
            if (_definition == null ||
                _playerBuildController == null ||
                _selectionController == null ||
                _offerGenerator == null)
            {
                throw new InvalidOperationException(
                    "Chest runtime dependencies are no longer available.");
            }

            if (!_playerBuildController.IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerBuildController)} is no longer initialized.");
            }

            if (!_selectionController.isActiveAndEnabled)
            {
                throw new InvalidOperationException(
                    $"{nameof(RewardSelectionController)} must be active and enabled.");
            }
        }

        private void OnDestroy()
        {
            StopListening();
        }
    }
}
