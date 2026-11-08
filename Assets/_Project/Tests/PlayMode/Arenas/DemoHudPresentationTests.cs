#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using ProjectFirstRun.UI.Hud;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class DemoHudPresentationTests
    {
        private GameObject _instance;
        private DemoHudView _view;
        [SetUp]
        public void Create()
        {
            _instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/UI/DemoHUD.prefab"));
            _view = _instance.GetComponent<DemoHudView>();
        }
        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            Object.Destroy(_instance);
            yield return null;
        }
        private Text Label(string path) => _instance.transform.Find(path).GetComponent<Text>();
        private Image Bar(string path) => _instance.transform.Find(path).GetComponent<Image>();

        [UnityTest]
        public IEnumerator OwnedItemsLevelsAndKeyFollowBuildWithoutChangingIt()
        {
            var build = new ProjectFirstRun.Builds.PlayerBuild(new ProjectFirstRun.Builds.PlayerBuildCapacity(2, 3, 5));
            build.TryAdd(ProjectFirstRun.Items.ItemCategory.Ability, "ability.fireball", 8);
            build.TryAdd(ProjectFirstRun.Items.ItemCategory.Upgrade, "upgrade.loaded_dice", 3);
            _view.SetCapacity(3, 5);
            _view.SetItems(build, null, true);
            _view.SetVisible(true);
            yield return null;
            var ability = _instance.transform.Find("Ability slot 1").GetComponent<HudItemCard>();
            Assert.That(ability.ItemId, Is.EqualTo("ability.fireball"));
            Assert.That(_instance.transform.Find("Upgrade slot 1").GetComponent<HudItemCard>().ItemId, Is.EqualTo("upgrade.loaded_dice"));
            Assert.That(_instance.transform.Find("Owned key").gameObject.activeSelf, Is.True);
            build.TryLevelUp(ProjectFirstRun.Items.ItemCategory.Ability, "ability.fireball");
            _view.SetItems(build, null, false);
            Assert.That(Label("Ability slot 1/Item level").text, Is.EqualTo("2"));
            Assert.That(build.Abilities.Count, Is.EqualTo(1));
            Assert.That(_instance.transform.Find("Owned key").gameObject.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator WeaponCardsReadReloadAndSettleOnLatestSelection()
        {
            var first = new ProjectFirstRun.Weapons.PlayerWeaponRuntimeEntry(
                AssetDatabase.LoadAssetAtPath<ProjectFirstRun.Weapons.WeaponDefinition>("Assets/_Project/Data/Items/Weapons/WD_Shotgun.asset"));
            var second = new ProjectFirstRun.Weapons.PlayerWeaponRuntimeEntry(
                AssetDatabase.LoadAssetAtPath<ProjectFirstRun.Weapons.WeaponDefinition>("Assets/_Project/Data/Items/Weapons/WD_Minigun.asset"));
            _view.SetVisible(true);
            _view.SetWeaponCard(first);
            first.RuntimeState.TryFire();
            first.RuntimeState.TryStartReload();
            _view.SetWeaponCard(first);
            var shade = _instance.transform.Find("Weapon card/Clockwise cooldown").GetComponent<HudRadialShade>();
            Assert.That(shade.Remaining, Is.EqualTo(1));
            first.RuntimeState.Tick(first.RuntimeState.CommittedReloadDuration * .5f);
            _view.SetWeaponCard(first);
            Assert.That(shade.Remaining, Is.EqualTo(.5f).Within(.001));
            _view.SetWeaponCard(second);
            yield return null;
            _view.SetWeaponCard(first);
            for (int i = 0; i < 30; i++) { _view.SetWeaponCard(first); yield return null; }
            _view.SetVisible(false); // Hide/finale must settle the latest selection, not leave half a card.
            var primary = (RectTransform)_instance.transform.Find("Weapon card");
            Assert.That(primary.anchoredPosition.x, Is.EqualTo(84).Within(.001));
            Assert.That(primary.anchoredPosition.y, Is.EqualTo(144).Within(.001));
            Assert.That(first.RuntimeState.ReloadTimeRemaining, Is.EqualTo(first.RuntimeState.CommittedReloadDuration * .5f));
        }

        [UnityTest]
        public IEnumerator HealthExperienceAndSwitchingUpdateWithoutAnimationDelay()
        {
            Assert.That(_view.IsVisible, Is.False);
            _view.SetVisible(true);
            _view.SetHealth(75, 150);
            _view.SetExperience(3, 25, 200);
            _view.SetWeapon("Shotgun", 2, 6, 24, false);
            _view.SetCapacity(3, 5);
            yield return null;
            Assert.That(Bar("Health track/Health fill").rectTransform.anchorMax.x, Is.EqualTo(.5f));
            Assert.That(Bar("XP track/XP fill").rectTransform.anchorMax.x, Is.EqualTo(.125f));
            Assert.That(Label("Health track/Health value").text, Is.EqualTo("75 / 150"));
            Assert.That(Label("Level").text, Is.EqualTo("LEVEL 3"));
            Assert.That(Label("Weapon card/Weapon name and level").text, Does.Contain("Shotgun"));
            _view.SetWeapon("Minigun", 1, 80, 0, false);
            Assert.That(Label("Ammunition").text, Is.EqualTo("80 / 0"));
            Assert.That(Label("Weapon status").text, Does.Not.Contain("NO AMMO"));
            _view.SetWeapon("Minigun", 1, 0, 80, true);
            Assert.That(Label("Weapon status").text, Is.EqualTo("RELOADING"));
            _view.SetWeapon("Minigun", 1, 0, 0, false);
            Assert.That(Label("Weapon status").text, Is.EqualTo("NO AMMO"));
        }

        [UnityTest]
        public IEnumerator HiddenPresentationCanResumeWithUpdatedCapacityAndValues()
        {
            _view.SetVisible(true);
            _view.SetCapacity(5, 8);
            _view.SetHealth(5, 100);
            _view.SetExperience(8, 40, 450);
            _view.SetVisible(false);
            yield return null;
            Assert.That(_view.IsVisible, Is.False);
            Assert.That(_instance.GetComponent<CanvasGroup>().blocksRaycasts, Is.False);
            _view.SetCapacity(3, 5);
            _view.SetHealth(100, 100);
            _view.SetExperience(1, 0, 100);
            _view.SetVisible(true);
            Assert.That(_view.VisibleAbilitySlots, Is.EqualTo(3));
            Assert.That(_view.VisibleUpgradeSlots, Is.EqualTo(5));
            Assert.That(Bar("XP track/XP fill").rectTransform.anchorMax.x, Is.Zero);
            Assert.That(Label("Health track/Health value").text, Is.EqualTo("100 / 100"));
        }
    }
}
#endif
