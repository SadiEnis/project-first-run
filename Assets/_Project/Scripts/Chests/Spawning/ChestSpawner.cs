using System;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.UI.Rewards;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Chests.Spawning
{
    [DisallowMultipleComponent]
    public sealed class ChestSpawner :
        MonoBehaviour
    {
        private PlayerBuildController
            _playerBuildController;

        private RewardSelectionController
            _selectionController;

        private RewardOfferGenerator
            _offerGenerator;

        public bool IsInitialized =>
            _offerGenerator != null;

        public void Initialize(
            PlayerBuildController playerBuildController,
            RewardSelectionController selectionController,
            RewardOfferGenerator offerGenerator)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(ChestSpawner)} has already been initialized.");
            }

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

            if (!_playerBuildController.IsInitialized)
            {
                _offerGenerator = null;

                throw new InvalidOperationException(
                    $"{nameof(PlayerBuildController)} must be initialized " +
                    $"before {nameof(ChestSpawner)}.");
            }
        }

        public ChestSpawnResult Spawn(
            in ChestSpawnRequest request)
        {
            EnsureInitialized();
            request.Validate();

            GameObject spawnedInstance = null;

            try
            {
                spawnedInstance =
                    Instantiate(
                        request.Definition.WorldPrefab,
                        request.Position,
                        request.Rotation);

                // The map-local spawner owns loot lifetime, not the active scene.
                SceneManager.MoveGameObjectToScene(spawnedInstance, gameObject.scene);

                spawnedInstance.name =
                    $"{request.Definition.WorldPrefab.name}_Instance";

                if (!spawnedInstance.TryGetComponent<
                        ChestController>(
                            out ChestController chestController))
                {
                    throw new InvalidOperationException(
                        $"Spawned chest '{spawnedInstance.name}' requires a " +
                        $"{nameof(ChestController)} component.");
                }

                chestController.Initialize(
                    request.Definition,
                    _playerBuildController,
                    _selectionController,
                    _offerGenerator);

                return new ChestSpawnResult(
                    chestController);
            }
            catch
            {
                CleanupFailedSpawn(
                    spawnedInstance);

                throw;
            }
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(ChestSpawner)} must be initialized before use.");
            }

            if (_playerBuildController == null ||
                _selectionController == null)
            {
                throw new InvalidOperationException(
                    "Chest spawner dependencies are no longer available.");
            }
        }

        private static void CleanupFailedSpawn(
            GameObject spawnedInstance)
        {
            if (spawnedInstance == null)
            {
                return;
            }

            spawnedInstance.SetActive(
                false);

            if (Application.isPlaying)
            {
                Destroy(
                    spawnedInstance);

                return;
            }

            DestroyImmediate(
                spawnedInstance);
        }
    }
}
