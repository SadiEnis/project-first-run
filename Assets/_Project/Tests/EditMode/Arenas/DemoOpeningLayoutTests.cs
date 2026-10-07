using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Development.Weapons;
using ProjectFirstRun.Editor;
using ProjectFirstRun.Player;
using ProjectFirstRun.Enemies;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class DemoOpeningLayoutTests
    {
        [Test]
        public void SavedOpeningHasRuntimeWiringAndNoContentControlPanel()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(DeepJamOpeningSceneBuilder.ScenePath) == null)
                Assert.Ignore("Run Project First Run/Demo/Create Opening Greybox once before scene acceptance.");
            var scene = EditorSceneManager.OpenScene(DeepJamOpeningSceneBuilder.ScenePath, OpenSceneMode.Additive);
            try
            {
                var roots = scene.GetRootGameObjects();
                var demo = roots.SelectMany(x => x.GetComponentsInChildren<DemoOpeningController>(true)).Single();
                Assert.DoesNotThrow(demo.ValidateConfiguration);
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<PlayerController>(true)).Count(), Is.EqualTo(1));
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<ContentArenaController>(true)), Is.Empty);
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<ContentArenaPanel>(true)), Is.Empty);
                Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<WeaponSwitchingDevelopmentBootstrap>(true)), Is.Empty);
                var surface = roots.SelectMany(x => x.GetComponentsInChildren<NavMeshSurface>(true)).Single();
                Assert.That(surface.navMeshData, Is.Not.Null);
                Assert.That(AssetDatabase.GetAssetPath(surface.navMeshData), Does.StartWith(DeepJamOpeningSceneBuilder.Folder));
                var encounter = new SerializedObject(demo.Encounter);
                foreach (string name in new[] { "_group", "_spawner", "_registry", "_player", "_playerHealth" })
                    Assert.That(encounter.FindProperty(name).objectReferenceValue, Is.Not.Null, name);
                Assert.That(encounter.FindProperty("_spawnPoints").arraySize, Is.EqualTo(4));
                var spawner = new SerializedObject(encounter.FindProperty("_spawner").objectReferenceValue);
                var profile = spawner.FindProperty("_perceptionProfile").objectReferenceValue as EnemyPerceptionProfile;
                Assert.That(profile, Is.Not.Null);
                Assert.DoesNotThrow(profile.Validate);
                Assert.That(AssetDatabase.GetAssetPath(profile), Is.EqualTo(DeepJamOpeningSceneBuilder.PerceptionPath));
                for (int i = 0; i < 4; i++)
                {
                    var point = (Transform)encounter.FindProperty("_spawnPoints").GetArrayElementAtIndex(i).objectReferenceValue;
                    Assert.That(Vector3.Dot(point.forward, Vector3.back), Is.GreaterThan(.99f));
                }
                var trigger = roots.SelectMany(x => x.GetComponentsInChildren<RegionPreparationTrigger>(true))
                    .Single(x => new SerializedObject(x).FindProperty("_encounter").objectReferenceValue == demo.Encounter);
                var properties = new SerializedObject(trigger);
                Assert.That(properties.FindProperty("_encounter").objectReferenceValue, Is.SameAs(demo.Encounter));
                Assert.That(properties.FindProperty("_volume").objectReferenceValue, Is.Not.Null);
                Assert.That(properties.FindProperty("_player").objectReferenceValue, Is.Not.Null);
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
