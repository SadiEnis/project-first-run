#if UNITY_EDITOR
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using ProjectFirstRun.Progression;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Waves;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Tests.EditMode.Chests
{
    public sealed class LevelUpChestSceneTests
    {
        [Test]
        public void EnemyDrops_HaveIndependentProfilesAndExplicitSceneAndPrefabDependencies()
        {
            var normal = AssetDatabase.LoadAssetAtPath<EnemyChestDropProfile>(
                "Assets/_Project/Data/Chests/Dev/CDP_DevelopmentNormal.asset");
            var elite = AssetDatabase.LoadAssetAtPath<EnemyChestDropProfile>(
                "Assets/_Project/Data/Chests/Dev/CDP_DevelopmentElite.asset");
            Assert.That(normal, Is.Not.Null);
            Assert.That(elite, Is.Not.Null.And.Not.EqualTo(normal));
            Assert.That(normal.ChanceBasisPoints, Is.EqualTo(2500));
            Assert.That(elite.ChanceBasisPoints, Is.EqualTo(5000));
            Assert.That(normal.Validate(), Is.EqualTo(1));
            Assert.That(elite.Validate(), Is.EqualTo(1));
            var enemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Project/Data/Enemies/ED_ChaserChestDropTest.asset");
            Assert.That(enemy.ChestDropProfile, Is.EqualTo(normal));
            foreach (string path in new[] { "EW_Test_01", "EW_Test_02" })
            {
                var wave = AssetDatabase.LoadAssetAtPath<EnemyWaveDefinition>($"Assets/_Project/Data/Waves/Test/{path}.asset");
                Assert.That(wave.Entries, Is.Not.Empty);
                Assert.That(wave.Entries.All(entry => entry.EnemyDefinition == enemy), Is.True);
            }
            Assert.That(AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Project/Data/Enemies/ED_ChaserBasic.asset").ChestDropProfile,
                Is.Null, "Legacy attack-only scenes must not require the chest scene services.");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/Enemy_ChaserBasic.prefab");
            Assert.That(prefab.GetComponent<EnemyChestDrop>(), Is.Not.Null);
            Scene scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Tests/Test_Waves.unity", OpenSceneMode.Additive);
            try
            {
                var source = FindSingle<EnemyChestDropSource>(scene);
                var serialized = new SerializedObject(source);
                Assert.That(serialized.FindProperty("_spawner").objectReferenceValue, Is.EqualTo(FindSingle<ChestSpawner>(scene)));
                Assert.That(serialized.FindProperty("_placement").objectReferenceValue, Is.EqualTo(FindSingle<ChestSpawnPlacement>(scene)));
                Assert.That(serialized.FindProperty("_playerHealth").objectReferenceValue,
                    Is.EqualTo(FindSingle<PlayerExperienceController>(scene).GetComponent<HealthComponent>()));
                Assert.That(new SerializedObject(FindSingle<EnemySpawner>(scene)).FindProperty("_chestDropSource").objectReferenceValue,
                    Is.EqualTo(source));
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }

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
