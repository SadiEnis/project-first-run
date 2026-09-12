#if UNITY_EDITOR

using System.Collections;
using ProjectFirstRun.Rewards.Claims;
using ProjectFirstRun.UI.Rewards;
using UnityEngine;

namespace ProjectFirstRun.Development.Rewards
{
    [DisallowMultipleComponent]
    public sealed class RewardSelectionDevelopmentBootstrap :
        MonoBehaviour
    {
        [SerializeField]
        private RewardDevelopmentController
            _rewardDevelopmentController;

        [SerializeField]
        private RewardSelectionController
            _rewardSelectionController;

        private IEnumerator Start()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                yield break;
            }

            while (!_rewardDevelopmentController.HasClaimSession)
            {
                yield return null;
            }

            RewardClaimSession session =
                _rewardDevelopmentController.CurrentClaimSession;

            if (session.Offer.IsEmpty)
            {
                Debug.LogWarning(
                    "Reward selection UI was not opened because " +
                    "the development offer has no eligible choices.",
                    this);

                yield break;
            }

            _rewardSelectionController.Open(
                session);
        }

        public void Initialize(
            RewardDevelopmentController rewardDevelopmentController,
            RewardSelectionController rewardSelectionController)
        {
            _rewardDevelopmentController =
                rewardDevelopmentController;

            _rewardSelectionController =
                rewardSelectionController;

            if (!ValidateReferences())
            {
                throw new System.InvalidOperationException(
                    "Reward selection development references are incomplete.");
            }
        }

        private bool ValidateReferences()
        {
            bool isValid = true;

            if (_rewardDevelopmentController == null)
            {
                LogMissing(
                    nameof(_rewardDevelopmentController));

                isValid = false;
            }

            if (_rewardSelectionController == null)
            {
                LogMissing(
                    nameof(_rewardSelectionController));

                isValid = false;
            }

            return isValid;
        }

        private void LogMissing(
            string fieldName)
        {
            Debug.LogError(
                $"{nameof(RewardSelectionDevelopmentBootstrap)} " +
                $"requires {fieldName}.",
                this);
        }
    }
}

#endif
