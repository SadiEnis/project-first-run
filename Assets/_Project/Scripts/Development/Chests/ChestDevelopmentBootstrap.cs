#if UNITY_EDITOR

using System;
using System.Collections;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.UI.Rewards;
using UnityEngine;

namespace ProjectFirstRun.Development.Chests
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ChestSpawner))]
    public sealed class ChestDevelopmentBootstrap :
        MonoBehaviour
    {
        [SerializeField]
        private ChestSpawner _chestSpawner;

        [SerializeField]
        private PlayerBuildController
            _playerBuildController;

        [SerializeField]
        private RewardSelectionController
            _rewardSelectionController;

        [SerializeField]
        private ChestDefinition _chestDefinition;

        [SerializeField]
        private Transform _spawnPoint;

        private RewardOfferGenerator _offerGenerator;

        public ChestController SpawnedChest
        {
            get;
            private set;
        }

        public bool HasSpawnedChest =>
            SpawnedChest != null;

        private IEnumerator Start()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                yield break;
            }

            while (!_playerBuildController.IsInitialized)
            {
                yield return null;
            }

            SpawnChest();
        }

        public void Initialize(
            ChestSpawner chestSpawner,
            PlayerBuildController playerBuildController,
            RewardSelectionController rewardSelectionController,
            ChestDefinition chestDefinition,
            Transform spawnPoint,
            RewardOfferGenerator offerGenerator = null)
        {
            _chestSpawner = chestSpawner;
            _playerBuildController = playerBuildController;
            _rewardSelectionController = rewardSelectionController;
            _chestDefinition = chestDefinition;
            _spawnPoint = spawnPoint;
            _offerGenerator = offerGenerator;

            if (!ValidateReferences())
            {
                throw new InvalidOperationException(
                    "Chest development references are incomplete.");
            }
        }

        public ChestController SpawnChest()
        {
            if (!ValidateReferences())
            {
                throw new InvalidOperationException(
                    "Chest development references are incomplete.");
            }

            if (HasSpawnedChest)
            {
                throw new InvalidOperationException(
                    "The development chest has already been spawned.");
            }

            if (!_playerBuildController.IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerBuildController)} must be initialized " +
                    "before spawning the development chest.");
            }

            EnsureSpawnerInitialized();

            ChestSpawnRequest request =
                new ChestSpawnRequest(
                    _chestDefinition,
                    _spawnPoint.position,
                    _spawnPoint.rotation);

            SpawnedChest =
                _chestSpawner.Spawn(
                        in request)
                    .ChestController;

            return SpawnedChest;
        }

        private void EnsureSpawnerInitialized()
        {
            if (_chestSpawner.IsInitialized)
            {
                return;
            }

            _offerGenerator ??=
                new RewardOfferGenerator(
                    new RewardCandidateFilter(),
                    new UnityRandomSource());

            _chestSpawner.Initialize(
                _playerBuildController,
                _rewardSelectionController,
                _offerGenerator);
        }

        private bool ValidateReferences()
        {
            bool isValid = true;

            isValid &= ValidateReference(
                _chestSpawner,
                nameof(_chestSpawner));

            isValid &= ValidateReference(
                _playerBuildController,
                nameof(_playerBuildController));

            isValid &= ValidateReference(
                _rewardSelectionController,
                nameof(_rewardSelectionController));

            isValid &= ValidateReference(
                _chestDefinition,
                nameof(_chestDefinition));

            isValid &= ValidateReference(
                _spawnPoint,
                nameof(_spawnPoint));

            return isValid;
        }

        private bool ValidateReference(
            UnityEngine.Object reference,
            string fieldName)
        {
            if (reference != null)
            {
                return true;
            }

            Debug.LogError(
                $"{nameof(ChestDevelopmentBootstrap)} requires {fieldName}.",
                this);

            return false;
        }
    }
}

#endif
