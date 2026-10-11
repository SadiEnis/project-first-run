#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Input;
using ProjectFirstRun.Weapons;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class DemoEntryTests : InputTestFixture
    {
        private Scene _scene, _original;
        private DemoEntryController _entry;
        private DemoOpeningController _demo;
        private float _oldTime;
        private CursorLockMode _oldLock;
        private bool _oldCursor;
        [SetUp] public override void Setup() { }
        [TearDown] public override void TearDown() { }

        [UnitySetUp]
        public IEnumerator Load()
        {
            base.Setup();
            InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>(); InputSystem.AddDevice<Gamepad>();
            _oldTime = Time.timeScale; _oldLock = Cursor.lockState; _oldCursor = Cursor.visible;
            Time.timeScale = 1;
            DemoEntryController.CancelSkip();
            _original = SceneManager.GetActiveScene();
            const string path = "Assets/_Project/Scenes/Demo/DeepJam_Opening.unity";
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Additive));
            _scene = SceneManager.GetSceneByPath(path);
            SceneManager.SetActiveScene(_scene);
            _entry = _scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<DemoEntryController>()).Single();
            _demo = _entry.GetComponent<DemoOpeningController>();
            for (int i = 0; i < 300 && !_demo.IsReady; i++) yield return null;
            Assert.That(_demo.IsReady, Is.True);
        }

        [UnityTest]
        public IEnumerator MenuBlocksGameplayAndStartHandsBackOriginalCameraOnlyOnce()
        {
            var player = _demo.Health.gameObject;
            var input = player.GetComponent<PlayerInputReader>();
            var weapon = player.GetComponent<PlayerWeaponController>();
            var camera = weapon.AimCamera;
            Assert.That(_entry.Phase, Is.EqualTo(DemoEntryPhase.Menu));
            Assert.That(_demo.IsEntryBlocking, Is.True);
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(input.IsGameplayInputEnabled, Is.False);
            Assert.That(weapon.IsWeaponControlEnabled, Is.False);
            var hit = _demo.Health.ApplyDamage(new DamageInfo(10, null, player.transform.position, Vector3.forward));
            Assert.That(hit.WasApplied, Is.False);
            Assert.That(camera.transform.localPosition.sqrMagnitude, Is.GreaterThan(.1f));
            Assert.That(_entry.Begin(), Is.True);
            Assert.That(_entry.Begin(), Is.False);
            float deadline = Time.realtimeSinceStartup + 8;
            while (_entry.IsBlocking && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(_entry.Phase, Is.EqualTo(DemoEntryPhase.Playing));
            Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(input.IsGameplayInputEnabled, Is.True);
            Assert.That(weapon.IsWeaponControlEnabled, Is.True);
            Assert.That(camera.transform.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(Quaternion.Angle(camera.transform.localRotation, Quaternion.identity), Is.LessThan(.001f));
            Assert.That(camera.fieldOfView, Is.EqualTo(75));
            Assert.That(_entry.Begin(), Is.False);
        }

        [UnityTest]
        public IEnumerator HeldConfirmCannotLeakIntoGameplayAndOtherOwnersRemainBlocked()
        {
            var input = _demo.Health.GetComponent<PlayerInputReader>();
            object otherOwner = new object();
            input.SetGameplayBlocked(otherOwner, true);
            Press(Gamepad.current.buttonSouth);
            Assert.That(_entry.Begin(), Is.True);
            float deadline = Time.realtimeSinceStartup + 8;
            while (_entry.Progress < 1 && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(_entry.Progress, Is.EqualTo(1));
            Assert.That(_entry.IsBlocking, Is.True);
            Release(Gamepad.current.buttonSouth);
            deadline = Time.realtimeSinceStartup + 3;
            while (_entry.IsBlocking && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(_entry.IsBlocking, Is.False);
            Assert.That(input.IsGameplayInputEnabled, Is.False, "Entry must not release another owner's lock.");
            input.SetGameplayBlocked(otherOwner, false);
            Assert.That(input.IsGameplayInputEnabled, Is.True);
        }

        [UnityTest]
        public IEnumerator DisablingEntryRestoresCameraAndTime()
        {
            var camera = _demo.Health.GetComponent<PlayerWeaponController>().AimCamera;
            _entry.enabled = false;
            yield return null;
            Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(camera.transform.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(_demo.IsEntryBlocking, Is.False);
        }

        [UnityTearDown]
        public IEnumerator Unload()
        {
            if (_entry != null) _entry.enabled = false;
            if (_original.IsValid() && _original.isLoaded) SceneManager.SetActiveScene(_original);
            if (_scene.IsValid() && _scene.isLoaded) yield return SceneManager.UnloadSceneAsync(_scene);
            DemoEntryController.CancelSkip();
            Time.timeScale = _oldTime; Cursor.lockState = _oldLock; Cursor.visible = _oldCursor;
            base.TearDown();
        }
    }
}
#endif
