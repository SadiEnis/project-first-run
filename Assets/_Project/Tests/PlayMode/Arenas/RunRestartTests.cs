#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Flow;
using ProjectFirstRun.Input;
using ProjectFirstRun.UI.Hud;
using ProjectFirstRun.Weapons;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class RunRestartTests : InputTestFixture
    {
        private const string Path = "Assets/_Project/Scenes/Demo/DeepJam_Opening.unity";
        private Scene _scene, _original;
        private DemoOpeningController _demo;
        private RunRestartController _restart;
        private float _time;
        private bool _audio, _cursor;
        private CursorLockMode _lock;
        private sealed class Loader : IRunSceneReloader
        {
            public int Calls;
            public bool Succeed;
            public bool Begin(string path)
            {
                Calls++;
                Assert.That(path, Is.EqualTo(Path));
                Assert.That(Time.timeScale, Is.Zero);
                Assert.That(AudioListener.pause, Is.True);
                return Succeed;
            }
        }
        [SetUp] public override void Setup() { }
        [TearDown] public override void TearDown() { }
        [UnitySetUp]
        public IEnumerator Load()
        {
            base.Setup();
            InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>(); InputSystem.AddDevice<Gamepad>();
            _time = Time.timeScale; _audio = AudioListener.pause; _cursor = Cursor.visible; _lock = Cursor.lockState;
            ClearTicket(); DemoEntryController.CancelSkip();
            Time.timeScale = 1; AudioListener.pause = false;
            _original = SceneManager.GetActiveScene();
            yield return LoadScene();
        }
        private IEnumerator LoadScene()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(Path, new LoadSceneParameters(LoadSceneMode.Additive));
            _scene = SceneManager.GetSceneByPath(Path);
            SceneManager.SetActiveScene(_scene);
            _demo = Find<DemoOpeningController>(); _restart = Find<RunRestartController>();
            for (int i = 0; i < 300 && !_demo.IsReady; i++) yield return null;
            Assert.That(_demo.IsReady, Is.True);
        }
        private T Find<T>() where T : Component => _scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<T>(true)).Single();
        private void Inject(Loader loader) => typeof(RunRestartController).GetMethod("SetReloader", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_restart, new object[] { loader });
        private static void ClearTicket() => typeof(RunRestartController).GetMethod("ResetSession", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);

        [UnityTest]
        public IEnumerator PauseFailurePreservesOwnersAndRetryArrivesWithFreshPlayer()
        {
            var loader = new Loader(); Inject(loader);
            Assert.That(_restart.Request(RunRestartSource.Pause), Is.False, "Entry owns controls.");
            Find<DemoEntryController>().enabled = false;
            var pause = Find<DemoPauseController>();
            pause.Open(); yield return null;
            Assert.That(pause.CanRestart, Is.True);
            var input = _demo.Health.GetComponent<PlayerInputReader>();
            var weapon = _demo.Health.GetComponent<PlayerWeaponController>();
            int originalAmmo = weapon.MagazineAmmo;
            object owner = new object(); input.SetGameplayBlocked(owner, true);
            pause.Restart();
            Assert.That(loader.Calls, Is.EqualTo(1));
            Assert.That(_restart.LastError, Is.Not.Null);
            Assert.That(pause.IsPaused, Is.True);
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(AudioListener.pause, Is.True);
            pause.enabled = false;
            Assert.That(input.IsGameplayInputEnabled, Is.False, "Failure released only restart's own locks.");
            input.SetGameplayBlocked(owner, false);
            Assert.That(input.IsGameplayInputEnabled, Is.True);
            pause.enabled = true; pause.Open(); yield return null;
            loader.Succeed = true;
            pause.Restart(); pause.Restart(); _demo.Restart();
            Assert.That(loader.Calls, Is.EqualTo(2));
            Assert.That(_restart.IsLoading, Is.True);
            pause.Resume();
            Assert.That(pause.IsPaused, Is.True);
            Assert.That(input.IsGameplayInputEnabled, Is.False);
            // Keep the test runner scene: emulate Unity's unload/arrival after accepted loading.
            SceneManager.SetActiveScene(_original);
            yield return SceneManager.UnloadSceneAsync(_scene);
            Assert.That(Time.timeScale, Is.Zero, "Old cleanup must not resume gameplay.");
            yield return LoadScene();
            Assert.That(Find<DemoEntryController>().Phase, Is.EqualTo(DemoEntryPhase.Playing));
            Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(AudioListener.pause, Is.False);
            Assert.That(_demo.Health.IsDead, Is.False);
            Assert.That(_demo.Health.GetComponent<PlayerWeaponController>().MagazineAmmo, Is.EqualTo(originalAmmo));
            Assert.That(_demo.Health.GetComponent<PlayerInputReader>().IsGameplayInputEnabled, Is.True);
            Assert.That(_restart.IsLoading, Is.False);
        }

        [UnityTest]
        public IEnumerator DeathFailureAllowsRetryWithoutResurrectingOldPlayer()
        {
            Find<DemoEntryController>().enabled = false;
            yield return null;
            var loader = new Loader(); Inject(loader);
            _demo.Restart(); Assert.That(loader.Calls, Is.Zero, "Alive is not eligible for death restart.");
            _demo.Health.ApplyDamage(new DamageInfo(10000, null, Vector3.zero, Vector3.zero));
            yield return null;
            float previous = Time.timeScale;
            _demo.Restart();
            Assert.That(loader.Calls, Is.EqualTo(1));
            Assert.That(_demo.Health.IsDead, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(previous));
            loader.Succeed = true;
            _demo.Restart(); _demo.Restart();
            Assert.That(loader.Calls, Is.EqualTo(2));
            Assert.That(_restart.IsLoading, Is.True);
            Assert.That(Time.timeScale, Is.Zero);
        }

        [UnityTest]
        public IEnumerator FinalRejectsAnimationAndRetriesFailedCompletedReplay()
        {
            Find<DemoEntryController>().enabled = false;
            yield return null;
            var final = Find<DemoFinalController>();
            var loader = new Loader(); Inject(loader);
            Assert.That(final.Session.TryBegin(true, true, true, true), Is.True);
            final.Replay(); Assert.That(loader.Calls, Is.Zero);
            Assert.That(final.Session.TryComplete(), Is.True);
            Time.timeScale = 0;
            final.Replay();
            Assert.That(loader.Calls, Is.EqualTo(1));
            Assert.That(final.Session.ReplayRequested, Is.False);
            Assert.That(final.Session.Phase, Is.EqualTo(DemoFinalPhase.Completed));
            Assert.That(Time.timeScale, Is.Zero);
            loader.Succeed = true;
            final.Replay(); final.Replay();
            Assert.That(loader.Calls, Is.EqualTo(2));
            Assert.That(final.Session.ReplayRequested, Is.True);
        }

        [UnityTearDown]
        public IEnumerator Unload()
        {
            if (_original.IsValid() && _original.isLoaded) SceneManager.SetActiveScene(_original);
            if (_scene.IsValid() && _scene.isLoaded) yield return SceneManager.UnloadSceneAsync(_scene);
            ClearTicket(); DemoEntryController.CancelSkip();
            Time.timeScale = _time; AudioListener.pause = _audio; Cursor.visible = _cursor; Cursor.lockState = _lock;
            base.TearDown();
        }
    }
}
#endif
