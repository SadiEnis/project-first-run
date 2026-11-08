#if UNITY_EDITOR
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Development.Abilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Tests.EditMode.Abilities
{
    public sealed class AbilityRuntimeInstallerLayoutTests
    {
        [TestCase("Demo/DeepJam_Opening")]
        [TestCase("Demo/DeepJam_WideCombat")]
        [TestCase("Playable/PlayableMapFixture")]
        [TestCase("Tests/Test_ContentArena")]
        public void SavedScene_UsesProductionInstallerWithExplicitPlayerAndWorldReferences(string name)
        {
            string path = "Assets/_Project/Scenes/" + name + ".unity";
            var scene = SceneManager.GetSceneByPath(path);
            bool openedHere = !scene.IsValid() || !scene.isLoaded;
            if (openedHere) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                var roots = scene.GetRootGameObjects();
                Assert.That(roots.SelectMany(go =>
                    go.GetComponentsInChildren<FireballDevelopmentBootstrap>(true)), Is.Empty);
                var installer = roots.SelectMany(go =>
                    go.GetComponentsInChildren<PlayerAbilityRuntimeInstaller>(true)).Single();
                Assert.That(installer.enabled, Is.True);
                var serialized = new SerializedObject(installer);
                foreach (string field in new[] { "_playerAbilityAcquisitionController",
                    "_playerStatsController", "_damageSource", "_enemyRegistry" })
                    Assert.That(serialized.FindProperty(field).objectReferenceValue, Is.Not.Null, field);
                var acquisition = (PlayerAbilityAcquisitionController)serialized
                    .FindProperty("_playerAbilityAcquisitionController").objectReferenceValue;
                Assert.That(acquisition.GetComponent<PlayerStartingLoadoutInitializer>(), Is.Not.Null);
            }
            finally
            {
                if (openedHere) EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
#endif
