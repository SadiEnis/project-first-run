using ProjectFirstRun.Items;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.Rewards.Claims;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectFirstRun.Development.Rewards
{
    [DisallowMultipleComponent]
    public sealed class RewardDebugPresenter :
        MonoBehaviour
    {
        private bool _interactionMode;

        private CursorLockMode _previousCursorLockMode;
        private bool _previousCursorVisible;
        
        [Header("Source")]
        [SerializeField]
        private RewardDevelopmentController _rewardController;

        [Header("Debug Panel")]
        [SerializeField]
        private Rect _panelRect =
            new Rect(
                1375f,
                390f,
                320f,
                320f);

        private void Update()
        {
            Keyboard keyboard =
                Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (keyboard.f8Key.wasPressedThisFrame)
            {
                SetInteractionMode(
                    !_interactionMode);
            }
        }
        
        private void OnGUI()
        {
            GUI.Box(
                _panelRect,
                "Reward Debug");

            Rect contentRect =
                new Rect(
                    _panelRect.x + 12f,
                    _panelRect.y + 28f,
                    _panelRect.width - 24f,
                    _panelRect.height - 36f);

            GUILayout.BeginArea(
                contentRect);

            DrawContent();

            GUILayout.EndArea();
        }
        
        private void SetInteractionMode(
            bool enabled)
        {
            if (_interactionMode == enabled)
            {
                return;
            }

            _interactionMode =
                enabled;

            if (_interactionMode)
            {
                _previousCursorLockMode =
                    Cursor.lockState;

                _previousCursorVisible =
                    Cursor.visible;

                Cursor.lockState =
                    CursorLockMode.None;

                Cursor.visible =
                    true;

                return;
            }

            Cursor.lockState =
                _previousCursorLockMode;

            Cursor.visible =
                _previousCursorVisible;
        }

        private void OnDisable()
        {
            if (_interactionMode)
            {
                SetInteractionMode(
                    false);
            }
        }

        private void DrawContent()
        {
            if (_rewardController == null)
            {
                GUILayout.Label(
                    "Reward Controller: Missing");

                return;
            }

            if (!_rewardController.HasGeneratedOffer)
            {
                GUILayout.Label(
                    "Offer: Not Generated");

                return;
            }

            RewardOffer offer =
                _rewardController.CurrentOffer;

            GUILayout.Label(
                $"Choices: {offer.ChoiceCount}");

            DrawClaimState();

            GUILayout.Space(
                6f);

            if (offer.IsEmpty)
            {
                GUILayout.Label(
                    "No eligible rewards");

                return;
            }

            for (int index = 0;
                 index < offer.ChoiceCount;
                 index++)
            {
                DrawChoice(
                    offer,
                    index);
            }

            GUILayout.Space(
                8f);

            if (GUILayout.Button(
                    "Generate New Offer"))
            {
                _rewardController.GenerateOffer();
            }
        }

        private void DrawChoice(
            RewardOffer offer,
            int index)
        {
            ItemDefinition choice =
                offer.Choices[index];

            GUILayout.BeginHorizontal();

            GUILayout.Label(
                $"{index + 1}. " +
                $"{choice.DisplayName} " +
                $"[{choice.Category}]");

            if (GUILayout.Button(
                    "Claim",
                    GUILayout.Width(60f)))
            {
                _rewardController.ClaimChoice(
                    index);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawClaimState()
        {
            if (!_rewardController.HasClaimSession)
            {
                GUILayout.Label(
                    "Session: Missing");

                return;
            }

            RewardClaimSession session =
                _rewardController.CurrentClaimSession;

            GUILayout.Label(
                $"Session Claimed: {session.IsClaimed}");

            if (session.ClaimedDefinition != null)
            {
                GUILayout.Label(
                    $"Claimed: " +
                    $"{session.ClaimedDefinition.DisplayName}");
            }

            if (_rewardController.HasClaimResult)
            {
                RewardClaimResult result =
                    _rewardController.LastClaimResult;

                GUILayout.Label(
                    $"Last Claim: {result}");
            }
        }
    }
}