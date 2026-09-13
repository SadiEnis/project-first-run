#if UNITY_EDITOR

using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Development.Chests;
using ProjectFirstRun.Development.Rewards;
using ProjectFirstRun.Player;
using ProjectFirstRun.Rewards.Claims;
using ProjectFirstRun.UI.Rewards;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Tests.EditMode.UI.Rewards
{
    public sealed class RewardSelectionDevelopmentSceneTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/Tests/Test_Waves.unity";

        private const string SelectionPrefabPath =
            "Assets/_Project/Prefabs/UI/RewardSelectionUI.prefab";

        private const string ChestDefinitionPath =
            "Assets/_Project/Data/Chests/Dev/CD_WeaponChest.asset";

        [Test]
        public void TestWaves_WiresChestBootstrapToSceneOwnedSelectionUI()
        {
            SceneSetup[] previousSetup =
                EditorSceneManager.GetSceneManagerSetup();

            Scene openedScene =
                default;

            try
            {
                openedScene =
                    EditorSceneManager.OpenScene(
                        ScenePath,
                        OpenSceneMode.Additive);

                ChestDevelopmentBootstrap bootstrap =
                    FindSingleComponent<
                        ChestDevelopmentBootstrap>(
                            openedScene);

                ChestSpawner chestSpawner =
                    FindSingleComponent<
                        ChestSpawner>(
                            openedScene);

                RewardSelectionController selectionController =
                    FindSingleComponent<
                        RewardSelectionController>(
                            openedScene);

                PlayerRewardClaimController claimController =
                    FindSingleComponent<
                        PlayerRewardClaimController>(
                            openedScene);

                PlayerController playerController =
                    FindSingleComponent<
                        PlayerController>(
                            openedScene);

                PlayerDeathController deathController =
                    FindSingleComponent<
                        PlayerDeathController>(
                            openedScene);

                Assert.That(
                    FindComponents<RewardSelectionDevelopmentBootstrap>(
                        openedScene),
                    Is.Empty,
                    "Test_Waves must not open reward selection automatically.");

                Assert.That(
                    PrefabUtility
                        .GetPrefabAssetPathOfNearestInstanceRoot(
                            selectionController.gameObject),
                    Is.EqualTo(
                        SelectionPrefabPath));

                SerializedObject serializedBootstrap =
                    new SerializedObject(
                        bootstrap);

                AssertReference(
                    serializedBootstrap,
                    "_chestSpawner",
                    chestSpawner);

                AssertReference(
                    serializedBootstrap,
                    "_playerBuildController",
                    claimController.GetComponent<PlayerBuildController>());

                AssertReference(
                    serializedBootstrap,
                    "_rewardSelectionController",
                    selectionController);

                ChestDefinition chestDefinition =
                    AssetDatabase.LoadAssetAtPath<ChestDefinition>(
                        ChestDefinitionPath);

                Assert.That(chestDefinition, Is.Not.Null);

                AssertReference(
                    serializedBootstrap,
                    "_chestDefinition",
                    chestDefinition);

                Assert.That(
                    serializedBootstrap.FindProperty("_spawnPoint")
                        .objectReferenceValue,
                    Is.Not.Null);

                SerializedObject serializedSelection =
                    new SerializedObject(
                        selectionController);

                AssertReference(
                    serializedSelection,
                    "_claimController",
                    claimController);

                AssertReference(
                    serializedSelection,
                    "_playerController",
                    playerController);

                AssertReference(
                    serializedSelection,
                    "_playerDeathController",
                    deathController);
            }
            finally
            {
                if (HasActiveLoadedScene(
                        previousSetup))
                {
                    EditorSceneManager.RestoreSceneManagerSetup(
                        previousSetup);
                }
                else if (openedScene.IsValid() &&
                         openedScene.isLoaded)
                {
                    EditorSceneManager.CloseScene(
                        openedScene,
                        true);
                }
            }
        }

        private static bool HasActiveLoadedScene(
            SceneSetup[] setup)
        {
            foreach (SceneSetup sceneSetup in setup)
            {
                if (sceneSetup.isLoaded &&
                    sceneSetup.isActive)
                {
                    return true;
                }
            }

            return false;
        }

        private static T FindSingleComponent<T>(
            Scene scene)
            where T : Component
        {
            T found = null;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T candidate =
                    root.GetComponentInChildren<T>(
                        true);

                if (candidate == null)
                {
                    continue;
                }

                Assert.That(
                    found,
                    Is.Null,
                    $"{ScenePath} must contain one {typeof(T).Name}.");

                found = candidate;
            }

            Assert.That(
                found,
                Is.Not.Null,
                $"{ScenePath} requires {typeof(T).Name}.");

            return found;
        }

        private static T[] FindComponents<T>(
            Scene scene)
            where T : Component
        {
            System.Collections.Generic.List<T> found =
                new System.Collections.Generic.List<T>();

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                found.AddRange(
                    root.GetComponentsInChildren<T>(true));
            }

            return found.ToArray();
        }

        private static void AssertReference(
            SerializedObject serializedObject,
            string propertyName,
            Object expected)
        {
            SerializedProperty property =
                serializedObject.FindProperty(
                    propertyName);

            Assert.That(
                property,
                Is.Not.Null);

            Assert.That(
                property.objectReferenceValue,
                Is.SameAs(expected));
        }
    }
}

#endif
