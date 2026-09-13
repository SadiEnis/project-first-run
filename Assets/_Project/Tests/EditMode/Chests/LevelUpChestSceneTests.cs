#if UNITY_EDITOR
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Progression;
using ProjectFirstRun.UI.Rewards;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Tests.EditMode.Chests
{
    public sealed class LevelUpChestSceneTests
    {
        [Test]
        public void TestWaves_WiresRuntimeSourceAndBootstrapToExistingPlayerAndSpawner()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Tests/Test_Waves.unity", OpenSceneMode.Additive);
            try
            {
                LevelUpChestSource source = FindSingle<LevelUpChestSource>(scene);
                ChestSpawnPlacement placement = FindSingle<ChestSpawnPlacement>(scene);
                ChestSpawner spawner = FindSingle<ChestSpawner>(scene);
                PlayerExperienceController xp = FindSingle<PlayerExperienceController>(scene);
                var serialized = new SerializedObject(source);
                Assert.That(serialized.FindProperty("_playerExperience").objectReferenceValue, Is.EqualTo(xp));
                Assert.That(serialized.FindProperty("_playerHealth").objectReferenceValue,
                    Is.EqualTo(xp.GetComponent<HealthComponent>()));
                Assert.That(serialized.FindProperty("_spawner").objectReferenceValue, Is.EqualTo(spawner));
                Assert.That(serialized.FindProperty("_placement").objectReferenceValue, Is.EqualTo(placement));
                ChestDefinition definition = AssetDatabase.LoadAssetAtPath<ChestDefinition>(
                    "Assets/_Project/Data/Chests/Dev/CD_DevelopmentChest.asset");
                Assert.That(serialized.FindProperty("_definition").objectReferenceValue, Is.EqualTo(definition));
                Assert.DoesNotThrow(() => placement.ValidatePrefab(definition.WorldPrefab));
                var bootstrap = new SerializedObject(FindSingle<ChestSpawnerBootstrap>(scene));
                Assert.That(bootstrap.FindProperty("_spawner").objectReferenceValue, Is.EqualTo(spawner));
                Assert.That(bootstrap.FindProperty("_playerBuild").objectReferenceValue,
                    Is.EqualTo(xp.GetComponent<PlayerBuildController>()));
                Assert.That(bootstrap.FindProperty("_selection").objectReferenceValue,
                    Is.EqualTo(FindSingle<RewardSelectionController>(scene)));
                Physics.SyncTransforms();
                Assert.That(placement.TryFind(xp.transform, definition.WorldPrefab, out _, out _), Is.True,
                    "The actual test arena must contain a valid level-up chest location.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static T FindSingle<T>(Scene scene) where T : Component =>
            scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)).Single();
    }
}
#endif
