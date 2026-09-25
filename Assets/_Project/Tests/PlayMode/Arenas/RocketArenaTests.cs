#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Combat;
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
    public sealed class RocketArenaTests : InputTestFixture
    {
        private Scene _scene, _original;
        private ContentArenaController _arena;
        private PlayerWeaponController _weapon;
        private Mouse _mouse;
        private int Index => _arena.Items.ToList().FindIndex(x => x.StableId == "weapon.rocket-launcher");
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
            Assert.That(_arena.Acquire(Index), Is.True);
            _weapon = _arena.Player.GetComponent<PlayerWeaponController>();
            Assert.That(_arena.Player.GetComponent<PlayerWeaponSwitcher>().TrySwitchNext(), Is.True);
            // Test flight in open air without an unrelated enemy ending the run.
            _arena.Player.transform.position = new Vector3(0, 1000, 0);
            yield return null;
        }
        [UnityTearDown] public IEnumerator Unload()
        {
            Time.timeScale = 1;
            if (_original.IsValid() && _original.isLoaded) SceneManager.SetActiveScene(_original);
            if (_scene.IsValid() && _scene.isLoaded) yield return SceneManager.UnloadSceneAsync(_scene);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true; base.TearDown();
        }
        [UnityTest] public IEnumerator LaunchIsSemiAutomaticAndSnapshotSurvivesLevelAndSwitchThenMapUnloadsIt()
        {
            RocketProjectile rocket = null; int launches = 0, hitscan = 0;
            _weapon.ProjectileLaunched += x => { rocket = x; launches++; };
            _weapon.ShotFired += _ => hitscan++;
            Press(_mouse.leftButton, queueEventOnly: true); yield return null; yield return null;
            Assert.That(launches, Is.EqualTo(1)); Assert.That(hitscan, Is.Zero);
            Assert.That(_weapon.MagazineAmmo, Is.EqualTo(2));
            Assert.That(rocket.gameObject.scene, Is.EqualTo(_scene));
            Assert.That(_arena.Player.GetComponent<PlayerLook>().RecoilOffset, Is.GreaterThan(0));
            Time.timeScale = 0; var position = rocket.transform.position;
            yield return null; yield return null;
            Assert.That(rocket.transform.position, Is.EqualTo(position)); Time.timeScale = 1;
            for (int i = 1; i < 8; i++) Assert.That(_arena.LevelUp(Index), Is.True, _arena.LastResult);
            Assert.That(_weapon.MagazineAmmo, Is.EqualTo(2)); // Capacity gain is not free ammo.
            Assert.That(rocket.Damage, Is.EqualTo(80)); Assert.That(rocket.Config.FragmentCount, Is.Zero);
            Assert.That(_weapon.ActiveEntry.Rocket.Value.FragmentCount, Is.EqualTo(8));
            Assert.That(_arena.Player.GetComponent<PlayerWeaponSwitcher>().TrySwitchNext(), Is.True);
            Release(_mouse.leftButton, queueEventOnly: true); yield return null; yield return null;
            Assert.That(rocket, Is.Not.Null); Assert.That(launches, Is.EqualTo(1));
            SceneManager.SetActiveScene(_original); yield return SceneManager.UnloadSceneAsync(_scene);
            Assert.That(rocket == null, Is.True);
        }
        [UnityTest] public IEnumerator EmptyMagazineReloadsAndDeathDisarmsFlight()
        {
            var state = _weapon.ActiveEntry.RuntimeState;
            while (state.MagazineAmmo > 1) { state.Tick(2); state.TryFire(); }
            state.Tick(2);
            RocketProjectile rocket = null;
            _weapon.ProjectileLaunched += x => rocket = x;
            Press(_mouse.leftButton, queueEventOnly: true); yield return null; yield return null;
            Assert.That(_weapon.MagazineAmmo, Is.Zero); Assert.That(state.IsReloading, Is.True);
            Release(_mouse.leftButton, queueEventOnly: true); yield return null; yield return null;
            var health = _arena.Player.GetComponent<HealthComponent>();
            health.ApplyDamage(new DamageInfo(health.MaximumHealth, null, Vector3.zero, Vector3.forward));
            yield return null; yield return null;
            Assert.That(rocket == null, Is.True);
        }
    }
}
#endif
