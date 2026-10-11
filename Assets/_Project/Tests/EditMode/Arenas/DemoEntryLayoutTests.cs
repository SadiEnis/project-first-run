using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.UI.Hud;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class DemoEntryLayoutTests
    {
        [TestCase("DeepJam_Opening")]
        [TestCase("DeepJam_WideCombat")]
        public void BothDemoScenesContainWiredEntry(string name)
        {
            var scene = EditorSceneManager.OpenPreviewScene($"Assets/_Project/Scenes/Demo/{name}.unity");
            try
            {
                var entry = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<DemoEntryController>(true)).Single();
                Assert.DoesNotThrow(entry.ValidateConfiguration);
                var demo = entry.GetComponent<DemoOpeningController>();
                var serialized = new SerializedObject(demo);
                Assert.That(serialized.FindProperty("_entry").objectReferenceValue, Is.EqualTo(entry));
                Assert.That(entry.GetComponentsInChildren<Camera>().Length, Is.Zero, "No second camera on the menu service.");
                var view = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<DemoEntryView>(true)).Single();
                Assert.DoesNotThrow(view.ValidateConfiguration);
                Assert.That(view.GetComponent<Canvas>().renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
                Assert.That(view.GetComponentsInChildren<UnityEngine.UI.Button>(true).Length, Is.EqualTo(2));
                Assert.That(view.GetComponentsInChildren<EntryButtonFeedback>(true).Length, Is.EqualTo(2));
                var configuration = new SerializedObject(entry);
                Assert.That(configuration.FindProperty("_view").objectReferenceValue, Is.EqualTo(view));
                var opening = (Component)configuration.FindProperty("_entryCamera").objectReferenceValue;
                var handoff = (Component)configuration.FindProperty("_fpsCamera").objectReferenceValue;
                Assert.That(opening.transform.position.y, Is.GreaterThan(handoff.transform.position.y));
                Assert.That(opening.GetComponent<Camera>(), Is.Null);
                Assert.That(handoff.GetComponent<Camera>(), Is.Null);
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }

        [Test]
        public void CameraEaseReachesEndpointsWithoutOvershoot()
        {
            Assert.That(DemoEntryController.Ease(-1), Is.Zero);
            Assert.That(DemoEntryController.Ease(0), Is.Zero);
            Assert.That(DemoEntryController.Ease(.5f), Is.EqualTo(.5f));
            Assert.That(DemoEntryController.Ease(1), Is.EqualTo(1));
            Assert.That(DemoEntryController.Ease(2), Is.EqualTo(1));
        }
    }
}
