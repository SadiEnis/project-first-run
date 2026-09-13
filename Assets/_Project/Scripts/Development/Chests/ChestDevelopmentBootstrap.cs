#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
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

        [SerializeField] private ChestDefinition[] _additionalChestDefinitions = Array.Empty<ChestDefinition>();
        private readonly List<ChestController> _spawnedChests = new List<ChestController>();
        private bool _hasSpawnedBatch;
        public IReadOnlyList<ChestController> SpawnedChests => _spawnedChests.AsReadOnly();

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

            if (_hasSpawnedBatch)
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
            _chestDefinition.Validate();
            if (_additionalChestDefinitions == null)
                throw new InvalidOperationException("Additional development chest definitions cannot be null.");
            foreach (ChestDefinition definition in _additionalChestDefinitions)
            {
                if (definition == null) throw new InvalidOperationException("Additional development chest definition is missing.");
                definition.Validate();
            }
            try
            {
                for (int i = 0; i <= _additionalChestDefinitions.Length; i++)
                {
                    ChestDefinition definition = i == 0 ? _chestDefinition : _additionalChestDefinitions[i - 1];
                    float offset = i == 0 ? 0f : ((i + 1) / 2) * 3f * (i % 2 == 1 ? -1f : 1f);
                    var request = new ChestSpawnRequest(definition,
                        _spawnPoint.position + _spawnPoint.right * offset, _spawnPoint.rotation);
                    _spawnedChests.Add(_chestSpawner.Spawn(in request).ChestController);
                }
                SpawnedChest = _spawnedChests[0];
                _hasSpawnedBatch = true;
                return SpawnedChest;
            }
            catch
            {
                foreach (ChestController chest in _spawnedChests)
                {
                    if (chest == null) continue;
                    chest.gameObject.SetActive(false);
                    if (Application.isPlaying) Destroy(chest.gameObject);
                    else DestroyImmediate(chest.gameObject);
                }
                _spawnedChests.Clear();
                throw;
            }
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
