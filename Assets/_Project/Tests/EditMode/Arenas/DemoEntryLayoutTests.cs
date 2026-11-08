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
                var pause = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<DemoPauseController>(true)).Single();
                Assert.DoesNotThrow(pause.ValidateConfiguration);
                Assert.That(pause.GetComponent<Canvas>().renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
                Assert.That(pause.GetComponentsInChildren<UnityEngine.UI.Button>(true).Length, Is.EqualTo(3));
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

        [TestCase(ProjectFirstRun.Items.ItemCategory.Weapon, 1)]
        [TestCase(ProjectFirstRun.Items.ItemCategory.Weapon, 2)]
        [TestCase(ProjectFirstRun.Items.ItemCategory.Ability, 1)]
        [TestCase(ProjectFirstRun.Items.ItemCategory.Ability, 3)]
        [TestCase(ProjectFirstRun.Items.ItemCategory.Ability, 5)]
        [TestCase(ProjectFirstRun.Items.ItemCategory.Upgrade, 1)]
        [TestCase(ProjectFirstRun.Items.ItemCategory.Upgrade, 4)]
        [TestCase(ProjectFirstRun.Items.ItemCategory.Upgrade, 5)]
        [TestCase(ProjectFirstRun.Items.ItemCategory.Upgrade, 6)]
        [TestCase(ProjectFirstRun.Items.ItemCategory.Upgrade, 7)]
        [TestCase(ProjectFirstRun.Items.ItemCategory.Upgrade, 8)]
        public void PauseBuildRowsCenterEachActualRow(ProjectFirstRun.Items.ItemCategory category, int slots)
        {
            int columns = category == ProjectFirstRun.Items.ItemCategory.Upgrade ? 4 : slots;
            for (int start = 0; start < slots; start += columns)
            {
                int count = System.Math.Min(columns, slots - start);
                for (int i = 0; i < count; i++)
                {
                    var left = DemoPauseController.BuildSlotPosition(category, start + i, slots);
                    var right = DemoPauseController.BuildSlotPosition(category, start + count - 1 - i, slots);
                    Assert.That(left.x, Is.EqualTo(-right.x));
                    Assert.That(left.y, Is.EqualTo(right.y));
                    if (start > 0)
                        Assert.That(left.y, Is.LessThan(DemoPauseController.BuildSlotPosition(category, 0, slots).y));
                }
            }
        }

        [Test]
        public void PauseOnlyOpensDuringLiveGameplayAndNavigationWrapsThreeButtons()
        {
            Assert.That(DemoPauseController.CanOpen(true, false, false, false, false, 1), Is.True);
            Assert.That(DemoPauseController.CanOpen(false, false, false, false, false, 1), Is.False);
            Assert.That(DemoPauseController.CanOpen(true, true, false, false, false, 1), Is.False);
            Assert.That(DemoPauseController.CanOpen(true, false, true, false, false, 1), Is.False);
            Assert.That(DemoPauseController.CanOpen(true, false, false, true, false, 1), Is.False);
            Assert.That(DemoPauseController.CanOpen(true, false, false, false, true, 1), Is.False);
            Assert.That(DemoPauseController.CanOpen(true, false, false, false, false, 0), Is.False);
            Assert.That(DemoPauseController.MoveSelection(0, -1), Is.EqualTo(2));
            Assert.That(DemoPauseController.MoveSelection(2, 1), Is.Zero);
            Assert.That(DemoPauseController.MoveSelection(0, 1), Is.EqualTo(1));
        }
    }
}
