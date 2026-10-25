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

        [Test]
        public void PauseBuildRowsAreCenteredAsTwoFiveAndFourPlusFour()
        {
            var weapon = ProjectFirstRun.Items.ItemCategory.Weapon;
            var ability = ProjectFirstRun.Items.ItemCategory.Ability;
            var upgrade = ProjectFirstRun.Items.ItemCategory.Upgrade;
            Assert.That(DemoPauseController.BuildSlotPosition(weapon, 0).x + DemoPauseController.BuildSlotPosition(weapon, 1).x, Is.Zero);
            Assert.That(DemoPauseController.BuildSlotPosition(ability, 2).x, Is.Zero);
            for (int i = 0; i < 5; i++)
                Assert.That(DemoPauseController.BuildSlotPosition(ability, i).x, Is.EqualTo(-DemoPauseController.BuildSlotPosition(ability, 4 - i).x));
            for (int i = 0; i < 4; i++)
            {
                var top = DemoPauseController.BuildSlotPosition(upgrade, i);
                var bottom = DemoPauseController.BuildSlotPosition(upgrade, i + 4);
                Assert.That(top.x, Is.EqualTo(-DemoPauseController.BuildSlotPosition(upgrade, 3 - i).x));
                Assert.That(bottom.x, Is.EqualTo(top.x));
                Assert.That(bottom.y, Is.LessThan(top.y));
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
