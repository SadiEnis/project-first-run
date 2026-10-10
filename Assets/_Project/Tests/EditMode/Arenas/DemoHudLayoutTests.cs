#if UNITY_EDITOR
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.UI.Hud;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class DemoHudLayoutTests
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/UI/DemoHUD.prefab";

        [Test]
        public void CommittedTimersKeepTheirDenominatorWhenConfigurationChanges()
        {
            var ability = new ProjectFirstRun.Abilities.AbilityRuntimeState(
                new ProjectFirstRun.Abilities.AbilityRuntimeConfig(8));
            ability.TryCommitCast(.5f);
            ability.Tick(1);
            ability.ApplyConfiguration(new ProjectFirstRun.Abilities.AbilityRuntimeConfig(2));
            Assert.That(ability.CommittedCooldownDuration, Is.EqualTo(4));
            Assert.That(ability.CooldownRemaining, Is.EqualTo(3));
            var weapon = new ProjectFirstRun.Weapons.WeaponRuntimeState(
                new ProjectFirstRun.Weapons.WeaponRuntimeConfig(4, 12, 2, 4));
            weapon.TryFire();
            weapon.TryStartReload();
            weapon.Tick(1);
            weapon.ApplyConfiguration(new ProjectFirstRun.Weapons.WeaponRuntimeConfig(4, 12, 2, 2));
            Assert.That(weapon.CommittedReloadDuration, Is.EqualTo(4));
            Assert.That(weapon.ReloadTimeRemaining, Is.EqualTo(3));
        }

        [Test]
        public void RadialBoundaryStartsAtTopAndMovesClockwise()
        {
            var rect = new Rect(-50, -30, 100, 60);
            Assert.That(Vector2.Distance(HudRadialShade.Boundary(rect, 0), new Vector2(0, 30)), Is.LessThan(.001));
            Assert.That(Vector2.Distance(HudRadialShade.Boundary(rect, 90), new Vector2(50, 0)), Is.LessThan(.001));
            Assert.That(Vector2.Distance(HudRadialShade.Boundary(rect, 180), new Vector2(0, -30)), Is.LessThan(.001));
            Assert.That(Vector2.Distance(HudRadialShade.Boundary(rect, 270), new Vector2(-50, 0)), Is.LessThan(.001));
        }

        [Test]
        public void CarouselIncomingUsesUpperArcAndOutgoingUsesLowerArc()
        {
            Assert.That(HudWeaponCarousel.Position(Vector2.zero, 84, 90).y, Is.GreaterThan(0));
            Assert.That(HudWeaponCarousel.Position(Vector2.zero, 84, -90).y, Is.LessThan(0));
            Assert.That(HudWeaponCarousel.ClockwiseTarget(180, true), Is.Zero);
            Assert.That(HudWeaponCarousel.ClockwiseTarget(0, false), Is.EqualTo(-180));
            Assert.That(HudWeaponCarousel.ClockwiseTarget(-35, true), Is.EqualTo(-360));
        }

        [Test]
        public void CatalogueCoversReleasedItemsAndHealthUsesSlantedGeometry()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<HudIconCatalog>("Assets/_Project/Prefabs/UI/DemoHudIcons.asset");
            Assert.That(catalog, Is.Not.Null);
            foreach (string root in new[] { "Assets/_Project/Data/Abilities", "Assets/_Project/Data/Upgrade", "Assets/_Project/Data/Items/Weapons" })
                foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { root }))
                {
                    var item = AssetDatabase.LoadAssetAtPath<ProjectFirstRun.Items.ItemDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                    if (item == null || item.StableId.Contains("development")) continue;
                    var icon = catalog.Find(item.StableId);
                    Assert.That(icon, Is.Not.Null, item.StableId);
                    Assert.That(icon.Sprite != null || !string.IsNullOrWhiteSpace(icon.Strokes), Is.True, item.StableId);
                }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab.transform.Find("Health track").GetComponent<HudSlantedImage>(), Is.Not.Null);
            Assert.That(prefab.transform.Find("Health track/Health fill").GetComponent<HudSlantedImage>(), Is.Not.Null);
        }

        [Test]
        public void SavedPrefabHasExplicitPresentationAndDoesNotInterceptInput()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.DoesNotThrow(prefab.GetComponent<DemoHudView>().ValidateConfiguration);
            Assert.That(prefab.GetComponent<Canvas>().renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
            Assert.That(prefab.GetComponent<CanvasScaler>().uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Assert.That(prefab.GetComponent<GraphicRaycaster>(), Is.Null);
            Assert.That(prefab.GetComponentsInChildren<Graphic>(true).All(x => !x.raycastTarget), Is.True);
            Assert.That(prefab.GetComponentsInChildren<Text>(true).All(x => x.font != null), Is.True);
            Assert.That(prefab.GetComponent<CanvasGroup>().blocksRaycasts, Is.False);
        }

        [TestCase(0, 0)]
        [TestCase(3, 5)]
        [TestCase(5, 8)]
        public void SlotsFollowCapacityWithoutCreatingGameplayItems(int abilities, int upgrades)
        {
            var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            try
            {
                var view = instance.GetComponent<DemoHudView>();
                view.SetCapacity(abilities, upgrades);
                Assert.That(view.VisibleAbilitySlots, Is.EqualTo(abilities));
                Assert.That(view.VisibleUpgradeSlots, Is.EqualTo(upgrades));
                view.SetCapacity(0, 0);
                Assert.That(view.VisibleAbilitySlots + view.VisibleUpgradeSlots, Is.Zero);
                Assert.That(instance.GetComponent<PlayerBuildController>(), Is.Null);
            }
            finally { Object.DestroyImmediate(instance); }
        }

        [TestCase(false, false, false, false, false)]
        [TestCase(true, false, false, false, true)]
        [TestCase(true, true, false, false, false)]
        [TestCase(true, false, true, false, false)]
        [TestCase(true, false, false, true, false)]
        public void VisibilityFollowsRunAndModalState(bool ready, bool dead, bool reward, bool ending, bool expected)
        {
            Assert.That(DemoHudController.ShouldShow(ready, dead, reward, ending), Is.EqualTo(expected));
        }

        [Test]
        public void WideSceneBindsOneSavedHudAndPreservesLegacyController()
        {
            const string path = "Assets/_Project/Scenes/Demo/DeepJam_WideCombat.unity";
            var existing = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
            bool opened = !existing.IsValid() || !existing.isLoaded;
            var scene = opened ? EditorSceneManager.OpenScene(path, OpenSceneMode.Additive) : existing;
            try
            {
                var controllers = scene.GetRootGameObjects()
                    .SelectMany(x => x.GetComponentsInChildren<DemoHudController>(true)).ToArray();
                Assert.That(controllers.Length, Is.EqualTo(1));
                var controller = controllers[0];
                Assert.DoesNotThrow(controller.ValidateConfiguration);
                var data = new SerializedObject(controller);
                var view = (DemoHudView)data.FindProperty("_view").objectReferenceValue;
                Assert.That(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(view.gameObject), Is.EqualTo(PrefabPath));
                var demo = (DemoOpeningController)data.FindProperty("_demo").objectReferenceValue;
                Assert.That(new SerializedObject(demo).FindProperty("_hud").objectReferenceValue, Is.SameAs(controller));
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
#endif
