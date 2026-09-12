using System;
using System.Collections;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.Rewards.Claims;
using UnityEngine;

namespace ProjectFirstRun.Development.Rewards
{
    [DisallowMultipleComponent]
    public sealed class RewardDevelopmentController :
        MonoBehaviour
    {
        [Header("Reward")]
        [SerializeField]
        private RewardItemPool _rewardItemPool;

        [SerializeField]
        [Min(1)]
        private int _requestedChoiceCount = 3;

        [Header("Player")]
        [SerializeField]
        private PlayerBuildController _playerBuildController;

        [SerializeField]
        private PlayerRewardClaimController _rewardClaimController;

        private RewardOfferGenerator _offerGenerator;

        public RewardOffer CurrentOffer
        {
            get;
            private set;
        }

        public RewardClaimSession CurrentClaimSession
        {
            get;
            private set;
        }

        public RewardClaimResult LastClaimResult
        {
            get;
            private set;
        }

        public bool HasGeneratedOffer =>
            CurrentOffer != null;

        public bool HasClaimSession =>
            CurrentClaimSession != null;

        public bool HasClaimResult
        {
            get;
            private set;
        }

        private IEnumerator Start()
        {
            /*
             * PlayerStartingLoadoutInitializer also uses Start.
             * Wait one frame so reward eligibility sees the
             * initialized/current player build.
             */
            yield return null;

            GenerateOffer();
        }

        public RewardOffer GenerateOffer()
        {
            ValidateOfferReferences();

            if (!_playerBuildController.IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerBuildController)} " +
                    "must be initialized before generating rewards.");
            }

            EnsureGenerator();

            CurrentOffer =
                _offerGenerator.Generate(
                    _rewardItemPool,
                    _playerBuildController.Build,
                    _requestedChoiceCount);

            CurrentClaimSession =
                new RewardClaimSession(
                    CurrentOffer);

            HasClaimResult =
                false;

            return CurrentOffer;
        }

        public RewardClaimResult ClaimChoice(
            int choiceIndex)
        {
            ValidateClaimReferences();

            if (CurrentOffer == null)
            {
                throw new InvalidOperationException(
                    "A reward offer must be generated before claiming.");
            }

            if (CurrentClaimSession == null)
            {
                throw new InvalidOperationException(
                    "A reward claim session is required.");
            }

            if (choiceIndex < 0 ||
                choiceIndex >= CurrentOffer.ChoiceCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(choiceIndex));
            }

            ItemDefinition definition =
                CurrentOffer.Choices[
                    choiceIndex];

            LastClaimResult =
                _rewardClaimController.TryClaim(
                    CurrentClaimSession,
                    definition);

            HasClaimResult =
                true;

            return LastClaimResult;
        }

        private void EnsureGenerator()
        {
            if (_offerGenerator != null)
            {
                return;
            }

            _offerGenerator =
                new RewardOfferGenerator(
                    new RewardCandidateFilter(),
                    new UnityRandomSource());
        }

        private void ValidateOfferReferences()
        {
            if (_rewardItemPool == null)
            {
                throw new InvalidOperationException(
                    "Development reward item pool is required.");
            }

            if (_playerBuildController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerBuildController)} reference is required.");
            }

            if (_requestedChoiceCount <= 0)
            {
                throw new InvalidOperationException(
                    "Requested reward choice count must be greater than zero.");
            }
        }

        private void ValidateClaimReferences()
        {
            if (_rewardClaimController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerRewardClaimController)} reference is required.");
            }
        }
    }
}