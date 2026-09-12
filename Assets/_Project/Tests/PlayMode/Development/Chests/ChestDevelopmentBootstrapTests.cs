#if UNITY_EDITOR

using System.Collections;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Development.Chests;
using ProjectFirstRun.Player;
using ProjectFirstRun.Rewards.Claims;
using ProjectFirstRun.UI.Rewards;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Development.Chests
{
    public sealed class ChestDevelopmentBootstrapTests
    {
        private const string PlayerPrefabPath =
            "Assets/_Project/Prefabs/Player/Player.prefab";

        private const string SelectionPrefabPath =
            "Assets/_Project/Prefabs/UI/RewardSelectionUI.prefab";

        private const string DefinitionPath =
            "Assets/_Project/Data/Chests/Dev/CD_DevelopmentChest.asset";

        private GameObject _playerObject;
        private GameObject _selectionObject;
        private GameObject _bootstrapObject;

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;

            DestroyImmediateIfExists(_bootstrapObject);
            DestroyImmediateIfExists(_selectionObject);
            DestroyImmediateIfExists(_playerObject);

            ChestController[] remainingChests =
                Object.FindObjectsByType<ChestController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            foreach (ChestController chest in remainingChests)
            {
                DestroyImmediateIfExists(chest.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator Start_WhenPlayerBuildIsReady_SpawnsDevelopmentChest()
        {
            CreatePlayer();
            RewardSelectionController selectionController =
                CreateSelectionController();

            ChestDefinition definition =
                AssetDatabase.LoadAssetAtPath<ChestDefinition>(
                    DefinitionPath);

            Assert.That(definition, Is.Not.Null);

            _bootstrapObject =
                new GameObject("ChestDevelopmentBootstrap_Test");

            _bootstrapObject.SetActive(false);

            ChestSpawner spawner =
                _bootstrapObject.AddComponent<ChestSpawner>();

            ChestDevelopmentBootstrap bootstrap =
                _bootstrapObject.AddComponent<ChestDevelopmentBootstrap>();

            Transform spawnPoint =
                new GameObject("SpawnPoint").transform;

            spawnPoint.SetParent(_bootstrapObject.transform);
            spawnPoint.position = new Vector3(2f, 0.75f, 4f);
            spawnPoint.rotation = Quaternion.Euler(0f, 35f, 0f);

            bootstrap.Initialize(
                spawner,
                _playerObject.GetComponent<PlayerBuildController>(),
                selectionController,
                definition,
                spawnPoint);

            _bootstrapObject.SetActive(true);

            for (int frame = 0;
                 frame < 5 && !bootstrap.HasSpawnedChest;
                 frame++)
            {
                yield return null;
            }

            Assert.That(bootstrap.HasSpawnedChest, Is.True);
            Assert.That(bootstrap.SpawnedChest.IsInitialized, Is.True);
            Assert.That(bootstrap.SpawnedChest.Definition, Is.SameAs(definition));
            Assert.That(
                bootstrap.SpawnedChest.transform.position,
                Is.EqualTo(spawnPoint.position));
            Assert.That(selectionController.IsOpen, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        private void CreatePlayer()
        {
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    PlayerPrefabPath);

            Assert.That(prefab, Is.Not.Null);

            _playerObject = Object.Instantiate(prefab);

            SerializedObject serializedPlayer =
                new SerializedObject(
                    _playerObject.GetComponent<PlayerController>());

            serializedPlayer.FindProperty("_lockCursorWhileControlled")
                .boolValue = false;

            serializedPlayer.ApplyModifiedPropertiesWithoutUndo();
        }

        private RewardSelectionController CreateSelectionController()
        {
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    SelectionPrefabPath);

            Assert.That(prefab, Is.Not.Null);

            _selectionObject = Object.Instantiate(prefab);

            RewardSelectionController controller =
                _selectionObject.GetComponent<RewardSelectionController>();

            controller.Initialize(
                _selectionObject.GetComponent<RewardSelectionView>(),
                _playerObject.GetComponent<PlayerRewardClaimController>(),
                _playerObject.GetComponent<PlayerController>(),
                _playerObject.GetComponent<PlayerDeathController>());

            return controller;
        }

        private static void DestroyImmediateIfExists(
            GameObject target)
        {
            if (target != null)
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}

#endif
