#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Player;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.Rewards.Claims;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class MinigunArenaTests : InputTestFixture
    {
        private Scene _scene, _original;
        private ContentArenaController _arena;
        private PlayerWeaponController _weapon;
        private Mouse _mouse;
        private Keyboard _keyboard;
        private int Index => _arena.Items.ToList().FindIndex(x => x.StableId == "weapon.minigun");
        private WeaponDefinition Minigun => (WeaponDefinition)_arena.Items[Index];
        [SetUp] public override void Setup() { }
        [TearDown] public override void TearDown() { }

        [UnitySetUp]
        public IEnumerator LoadArena()
        {
            base.Setup();
            _mouse = InputSystem.AddDevice<Mouse>();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            Time.timeScale = 1;
            _original = SceneManager.GetActiveScene();
            const string path = "Assets/_Project/Scenes/Tests/Test_ContentArena.unity";
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Additive));
            _scene = SceneManager.GetSceneByPath(path);
            SceneManager.SetActiveScene(_scene);
            _arena = Find<ContentArenaController>().Single();
            for (int i = 0; i < 200 && !_arena.IsReady; i++) yield return null;
            Assert.That(_arena.IsReady, Is.True);
            Assert.That(Index, Is.GreaterThanOrEqualTo(0));
            _weapon = _arena.Player.GetComponent<PlayerWeaponController>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator UnloadArena()
        {
            if (_arena != null) Find<ContentArenaPanel>().Single().Close();
            Time.timeScale = 1;
            if (_original.IsValid() && _original.isLoaded) SceneManager.SetActiveScene(_original);
            if (_scene.IsValid() && _scene.isLoaded) yield return SceneManager.UnloadSceneAsync(_scene);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            base.TearDown();
        }

        private T[] Find<T>() where T : Component => _scene.GetRootGameObjects()
            .SelectMany(x => x.GetComponentsInChildren<T>(true)).ToArray();

        private void Equip()
        {
            Assert.That(_arena.Acquire(Index), Is.True, _arena.LastResult);
            Assert.That(_arena.LastResult, Is.EqualTo("Acquired"));
            Assert.That(_arena.Player.GetComponent<PlayerWeaponSwitcher>().TrySwitchNext(), Is.True);
            Assert.That(_weapon.ActiveDefinition, Is.SameAs(Minigun));
        }

        [UnityTest]
        public IEnumerator HoldPreparesThenConsumesOneRoundPerShotAndRecoilAccumulates()
        {
            Equip();
            int shots = 0;
            _weapon.ShotFired += x => { shots++; Assert.That(x.Pellets.Count, Is.EqualTo(1)); };
            var look = _arena.Player.GetComponent<PlayerLook>();
            float pitch = look.CurrentPitch;
            Press(_mouse.leftButton, queueEventOnly: true);
            yield return null;
            yield return null;
            Assert.That(shots, Is.Zero);
            Assert.That(_weapon.MagazineAmmo, Is.EqualTo(80));
            yield return new WaitForSeconds(.65f);
            Assert.That(shots, Is.GreaterThanOrEqualTo(3));
            Assert.That(_weapon.MagazineAmmo, Is.EqualTo(80 - shots));
            Assert.That(look.CurrentPitch, Is.EqualTo(pitch - shots * .35f).Within(.01));
            Assert.That(_arena.Player.GetComponentsInChildren<LineRenderer>(true).Length, Is.EqualTo(1));
            Release(_mouse.leftButton, queueEventOnly: true);
            yield return null;
            yield return null;
            int stopped = shots;
            yield return new WaitForSeconds(.25f);
            Assert.That(shots, Is.EqualTo(stopped));
            Assert.That(_weapon.PreparationElapsed, Is.Zero);
        }

        [UnityTest]
        public IEnumerator ShortTapSwitchAndControlLossResetPreparationButPreserveAmmo()
        {
            Equip();
            Press(_mouse.leftButton, queueEventOnly: true);
            yield return null;
            yield return null;
            Release(_mouse.leftButton, queueEventOnly: true);
            yield return null;
            yield return null;
            Assert.That(_weapon.MagazineAmmo, Is.EqualTo(80));
            Assert.That(_weapon.PreparationElapsed, Is.Zero);
            var entry = _weapon.ActiveEntry;
            entry.RuntimeState.TryFire();
            float cooldown = entry.RuntimeState.FireCooldownRemaining;
            var switcher = _arena.Player.GetComponent<PlayerWeaponSwitcher>();
            switcher.TrySwitchNext();
            switcher.TrySwitchNext();
            Assert.That(_weapon.ActiveEntry, Is.SameAs(entry));
            Assert.That(_weapon.MagazineAmmo, Is.EqualTo(79));
            Assert.That(entry.RuntimeState.FireCooldownRemaining, Is.EqualTo(cooldown));
            Press(_mouse.leftButton, queueEventOnly: true);
            yield return null;
            yield return null;
            Assert.That(_weapon.PreparationElapsed, Is.GreaterThan(0));
            _weapon.SetWeaponControlEnabled(false);
            Assert.That(_weapon.PreparationElapsed, Is.Zero);
            yield return new WaitForSeconds(.4f);
            Assert.That(_weapon.MagazineAmmo, Is.EqualTo(79));
            _weapon.SetWeaponControlEnabled(true);
            yield return null;
            Assert.That(_weapon.MagazineAmmo, Is.EqualTo(79));
        }

        [UnityTest]
        public IEnumerator ReloadPanelPauseAndDeathCannotFireOrPrepare()
        {
            Equip();
            var state = _weapon.ActiveEntry.RuntimeState;
            state.TryFire();
            Press(_keyboard.rKey, queueEventOnly: true);
            Press(_mouse.leftButton, queueEventOnly: true);
            yield return null;
            yield return null;
            Assert.That(state.IsReloading, Is.True);
            Assert.That(_weapon.PreparationElapsed, Is.Zero);
            yield return new WaitForSeconds(.4f);
            Assert.That(state.MagazineAmmo, Is.EqualTo(79));
            Release(_keyboard.rKey, queueEventOnly: true);
            Release(_mouse.leftButton, queueEventOnly: true);
            yield return null;
            state.Tick(3);
            Assert.That(state.MagazineAmmo, Is.EqualTo(80));
            var panel = Find<ContentArenaPanel>().Single();
            Assert.That(panel.TryOpen(), Is.True);
            Press(_mouse.leftButton, queueEventOnly: true);
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(state.MagazineAmmo, Is.EqualTo(80));
            Assert.That(_weapon.PreparationElapsed, Is.Zero);
            Release(_mouse.leftButton, queueEventOnly: true);
            panel.Close();
            yield return null;
            Time.timeScale = 0;
            Press(_mouse.leftButton, queueEventOnly: true);
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(state.MagazineAmmo, Is.EqualTo(80));
            Time.timeScale = 1;
            _arena.Player.GetComponent<HealthComponent>().ApplyDamage(new DamageInfo(10000, null, Vector3.zero, Vector3.forward));
            yield return new WaitForSeconds(.4f);
            Assert.That(state.MagazineAmmo, Is.EqualTo(80));
        }

        [UnityTest]
        public IEnumerator EmptyMagazineCannotPrepareAndRequiresReload()
        {
            Equip();
            var state = _weapon.ActiveEntry.RuntimeState;
            while (state.MagazineAmmo > 0) { state.Tick(1); state.TryFire(); }
            int events = 0;
            _weapon.ShotFired += _ => events++;
            Press(_mouse.leftButton, queueEventOnly: true);
            yield return new WaitForSeconds(.5f);
            Assert.That(events, Is.Zero);
            Assert.That(_weapon.PreparationElapsed, Is.Zero);
            Assert.That(state.ReserveAmmo, Is.EqualTo(240));
        }

        [UnityTest]
        public IEnumerator CriticalDamageUsesStatsOnceAndOneDamageEventPerBullet()
        {
            Equip();
            Assert.That(_arena.Acquire(3), Is.True); // +20% weapon damage
            var copy = Object.Instantiate(Minigun);
            var data = new SerializedObject(copy);
            data.FindProperty("_criticalChance").floatValue = 1;
            data.FindProperty("_recoilDegrees").floatValue = 0;
            data.ApplyModifiedPropertiesWithoutUndo();
            var target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                _weapon.Initialize(copy);
                var camera = _arena.Player.GetComponentInChildren<Camera>();
                target.transform.position = camera.transform.position + camera.transform.forward * 3;
                target.transform.localScale = Vector3.one * 2;
                var health = target.AddComponent<HealthComponent>();
                health.Initialize(1000);
                Physics.SyncTransforms();
                int shots = 0, damages = 0;
                _weapon.ShotFired += volley => { shots++; Assert.That(volley.IsCritical, Is.True); };
                _weapon.DamageApplied += hit => { damages++; Assert.That(hit.DamageResult.AppliedDamage, Is.EqualTo(19.2f).Within(.001)); };
                Press(_mouse.leftButton, queueEventOnly: true);
                yield return new WaitForSeconds(.55f);
                Assert.That(shots, Is.GreaterThan(0));
                Assert.That(damages, Is.EqualTo(shots));
                Assert.That(health.CurrentHealth, Is.EqualTo(1000 - shots * 19.2f).Within(.01));
                Assert.That(_weapon.MagazineAmmo, Is.EqualTo(80 - shots));
            }
            finally { Object.DestroyImmediate(copy); Object.DestroyImmediate(target); }
        }

        [UnityTest]
        public IEnumerator PitchLimitsAndMouseGamepadCompensationShareOneLookOwner()
        {
            var look = _arena.Player.GetComponent<PlayerLook>();
            float before = look.CurrentPitch;
            look.ApplyRecoil(1);
            Assert.That(look.CurrentPitch, Is.EqualTo(before - 1));
            look.Tick(new Vector2(0, -10), false, .1f);
            Assert.That(look.CurrentPitch, Is.EqualTo(before).Within(.001));
            look.ApplyRecoil(16);
            look.Tick(new Vector2(0, -1), true, .1f);
            Assert.That(look.CurrentPitch, Is.EqualTo(before).Within(.001));
            look.ApplyRecoil(1000);
            Assert.That(look.CurrentPitch, Is.EqualTo(-85));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => look.ApplyRecoil(float.NaN));
            yield return null;
        }

        [UnityTest]
        public IEnumerator RealChestAcquisitionLevelAndMaximumUseExistingEligibility()
        {
            Assert.That(_arena.SpawnChest(0), Is.True);
            var chest = Find<ChestController>().Single();
            typeof(ChestController).GetField("_offerGenerator", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(chest, new RewardOfferGenerator(new RewardCandidateFilter(), new FirstCandidate()));
            chest.TryOpen();
            Assert.That(_arena.Selection.Select(Minigun), Is.EqualTo(RewardClaimResult.Claimed));
            Assert.That(_arena.ItemLevel(Index), Is.EqualTo(1));
            yield return null;
            Assert.That(_arena.SpawnChest(0), Is.True);
            Find<ChestController>().Last(x => x.gameObject.activeInHierarchy).TryOpen();
            Assert.That(_arena.Selection.Select(Minigun), Is.EqualTo(RewardClaimResult.Claimed));
            Assert.That(_arena.ItemLevel(Index), Is.EqualTo(2));
            for (int i = 2; i < 8; i++) Assert.That(_arena.LevelUp(Index), Is.True);
            yield return null;
            Assert.That(_arena.SpawnChest(0), Is.True);
            Find<ChestController>().Last(x => x.gameObject.activeInHierarchy).TryOpen();
            Assert.That(_arena.Selection.ActiveSession.Offer.Choices, Has.No.Member(Minigun));
            Assert.That(_arena.Selection.ActiveSession.Offer.Choices.All(x => x.StableId != "weapon.shotgun"), Is.True);
            _arena.Selection.Select(_arena.Selection.ActiveSession.Offer.Choices[0]);
        }

        [UnityTest]
        public IEnumerator InactiveLevelsPreserveAmmoReloadAndAssetWhileFullSlotsRejectNewWeapon()
        {
            Equip();
            var entry = _weapon.ActiveEntry;
            entry.RuntimeState.TryFire();
            entry.RuntimeState.TryStartReload();
            var switcher = _arena.Player.GetComponent<PlayerWeaponSwitcher>();
            switcher.TrySwitchNext();
            for (int i = 1; i < 8; i++) Assert.That(_arena.LevelUp(Index), Is.True);
            switcher.TrySwitchNext();
            Assert.That(_weapon.ActiveEntry, Is.SameAs(entry));
            Assert.That(entry.RuntimeState.MagazineAmmo, Is.EqualTo(79));
            Assert.That(entry.RuntimeState.MagazineCapacity, Is.EqualTo(100));
            Assert.That(entry.RuntimeState.ReloadTimeRemaining, Is.EqualTo(3));
            Assert.That(entry.Fire.RecoilDegrees, Is.EqualTo(.2f));
            Assert.That(new PlayerWeaponRuntimeEntry(Minigun).Fire.RecoilDegrees, Is.EqualTo(.35f));
            Assert.That(_arena.Acquire(4), Is.True);
            Assert.That(_arena.LastResult, Is.EqualTo("CapacityReached"));
            yield return null;
        }

        private sealed class FirstCandidate : IRandomSource
        {
            public int Next(int minInclusive, int maxExclusive) => minInclusive;
        }
    }
}
#endif
