#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Player;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class ArenaTransitionShowcaseTests
    {
        private Scene _showcase, _scratch, _previous;

        [UnityTest]
        public IEnumerator TestWaves_RealWavesUnlockExit_AndSecondArenaCompletesRun()
        {
            _previous = SceneManager.GetActiveScene();
            _scratch = SceneManager.CreateScene("Transition showcase instances");
            SceneManager.SetActiveScene(_scratch);
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/_Project/Scenes/Tests/Test_Waves.unity", new LoadSceneParameters(LoadSceneMode.Additive));
            _showcase = SceneManager.GetSceneByPath("Assets/_Project/Scenes/Tests/Test_Waves.unity");
            yield return null;
            var roots = _showcase.GetRootGameObjects();
            var run = roots.SelectMany(r => r.GetComponentsInChildren<RunSessionController>()).Single();
            var transitions = roots.SelectMany(r => r.GetComponentsInChildren<ArenaTransitionController>()).Single();
            var exit = roots.SelectMany(r => r.GetComponentsInChildren<ArenaTransitionTrigger>()).Single();
            var registry = roots.SelectMany(r => r.GetComponentsInChildren<EnemyRegistry>()).Single();
            var player = roots.SelectMany(r => r.GetComponentsInChildren<PlayerDeathController>()).Single();
            Assert.That(exit.IsAvailable, Is.False);
            for (int wave = 0; wave < 2; wave++) KillWave(registry);
            Assert.That(run.Status, Is.EqualTo(RunSessionStatus.Transition));
            Assert.That(exit.IsAvailable, Is.True);
            Assert.That(transitions.TryTransition(0, player.GetComponent<CharacterController>()), Is.True);
            Assert.That(run.State.CurrentArenaIndex, Is.EqualTo(1));
            for (int wave = 0; wave < 2; wave++) KillWave(registry);
            Assert.That(run.Status, Is.EqualTo(RunSessionStatus.Victory));
            Assert.That(exit.IsAvailable, Is.False);
        }

        private static void KillWave(EnemyRegistry registry)
        {
            Assert.That(registry.ActiveCount, Is.GreaterThan(0));
            foreach (var enemy in registry.ActiveEnemies.ToArray())
                enemy.Health.ApplyDamage(new DamageInfo(10000, null, enemy.transform.position, Vector3.forward));
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            Time.timeScale = 1f;
            if (_previous.IsValid() && _previous.isLoaded) SceneManager.SetActiveScene(_previous);
            if (_showcase.IsValid() && _showcase.isLoaded) yield return SceneManager.UnloadSceneAsync(_showcase);
            if (_scratch.IsValid() && _scratch.isLoaded) yield return SceneManager.UnloadSceneAsync(_scratch);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
#endif
