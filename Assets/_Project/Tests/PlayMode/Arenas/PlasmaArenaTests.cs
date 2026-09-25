#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Player;
using ProjectFirstRun.Weapons;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class PlasmaArenaTests : InputTestFixture
    {
        private Scene _scene, _original;
        private ContentArenaController _arena;
        private PlayerWeaponController _weapon;
        private Mouse _mouse;
        private int Index => _arena.Items.ToList().FindIndex(x => x.StableId == "weapon.plasma-rifle");
        [SetUp] public override void Setup() { }
        [TearDown] public override void TearDown() { }
        [UnitySetUp] public IEnumerator Load()
        {
            base.Setup(); _mouse = InputSystem.AddDevice<Mouse>(); InputSystem.AddDevice<Keyboard>();
            Time.timeScale = 1; _original = SceneManager.GetActiveScene();
            const string path = "Assets/_Project/Scenes/Tests/Test_ContentArena.unity";
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Additive));
            _scene = SceneManager.GetSceneByPath(path); SceneManager.SetActiveScene(_scene);
            _arena = _scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<ContentArenaController>()).Single();
            for (int i = 0; i < 200 && !_arena.IsReady; i++) yield return null;
            Assert.That(_arena.IsReady, Is.True);
            _weapon = _arena.Player.GetComponent<PlayerWeaponController>();
            if (_arena.ItemLevel(Index) == 0) Assert.That(_arena.Acquire(Index), Is.True);
            if (_weapon.ActiveDefinition.StableId != "weapon.plasma-rifle")
                Assert.That(_arena.Player.GetComponent<PlayerWeaponSwitcher>().TrySwitchNext(), Is.True);
            Assert.That(_weapon.ActiveDefinition.StableId, Is.EqualTo("weapon.plasma-rifle"));
            _arena.Player.transform.position = new Vector3(0, 1000, 0); yield return null;
        }
        [UnityTearDown] public IEnumerator Unload()
        {
            Time.timeScale = 1;
            if (_original.IsValid() && _original.isLoaded) SceneManager.SetActiveScene(_original);
            if (_scene.IsValid() && _scene.isLoaded) yield return SceneManager.UnloadSceneAsync(_scene);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true; base.TearDown();
        }
        [UnityTest] public IEnumerator AutomaticFlightHasNoHitscanAndSnapshotSurvivesUpgradeAndMapCleanup()
        {
            int launches = 0, hitscan = 0; PlasmaProjectile first = null;
            _weapon.PlasmaLaunched += x => { launches++; if (first == null) first = x; };
            _weapon.ShotFired += _ => hitscan++;
            Press(_mouse.leftButton, queueEventOnly: true); yield return null; yield return null;
            yield return new WaitForSeconds(.3f);
            Release(_mouse.leftButton, queueEventOnly: true); yield return null; yield return null;
            Assert.That(launches, Is.GreaterThanOrEqualTo(2)); Assert.That(hitscan, Is.Zero);
            Assert.That(_weapon.MagazineAmmo, Is.EqualTo(12 - launches));
            Assert.That(first.gameObject.scene, Is.EqualTo(_scene));
            Assert.That(_arena.Player.GetComponent<PlayerLook>().RecoilOffset, Is.GreaterThan(0));
            Time.timeScale = 0; Vector3 position = first.transform.position;
            yield return null; yield return null;
            Assert.That(first.transform.position, Is.EqualTo(position)); Time.timeScale = 1;
            for (int i = 1; i < 8; i++) Assert.That(_arena.LevelUp(Index), Is.True, _arena.LastResult);
            Assert.That(first.Damage, Is.EqualTo(25)); Assert.That(first.Config.Burn, Is.False);
            Assert.That(_weapon.ActiveEntry.Plasma.Value.Burn, Is.True);
            Assert.That(_weapon.MagazineAmmo, Is.EqualTo(12 - launches));
            SceneManager.SetActiveScene(_original); yield return SceneManager.UnloadSceneAsync(_scene);
            Assert.That(first == null, Is.True);
        }
        [UnityTest] public IEnumerator StatAdjustedDirectAndBurnDamageAreAppliedOnceAndSurviveSwitch()
        {
            for (int i = 1; i < 8; i++) Assert.That(_arena.LevelUp(Index), Is.True);
            Assert.That(_arena.Acquire(3), Is.True); // Existing +20% damage upgrade.
            var target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            SceneManager.MoveGameObjectToScene(target, _scene);
            var camera = _arena.Player.GetComponentInChildren<Camera>();
            target.transform.position = camera.transform.position + camera.transform.forward * 3;
            target.transform.localScale = Vector3.one * 2;
            var health = target.AddComponent<ProjectFirstRun.Combat.HealthComponent>(); health.Initialize(1000);
            Physics.SyncTransforms();
            int hits = 0;
            _weapon.DamageApplied += _ => hits++;
            Press(_mouse.leftButton, queueEventOnly: true); yield return null; yield return null;
            Release(_mouse.leftButton, queueEventOnly: true); yield return null; yield return null;
            for (int i = 0; i < 30 && hits == 0; i++) yield return null;
            Assert.That(hits, Is.EqualTo(1));
            Assert.That(health.CurrentHealth, Is.EqualTo(958).Within(.01));
            var burn = target.GetComponent<PlasmaBurn>(); Assert.That(burn, Is.Not.Null);
            Assert.That(burn.State.Damage, Is.EqualTo(6).Within(.001));
            var switcher = _arena.Player.GetComponent<PlayerWeaponSwitcher>();
            if (_arena.ItemLevel(1) == 0) _arena.Acquire(1);
            Assert.That(switcher.TrySwitchNext(), Is.True);
            burn.Advance(3);
            Assert.That(health.CurrentHealth, Is.EqualTo(922).Within(.01)); Assert.That(hits, Is.EqualTo(7));
        }
    }
}
#endif
