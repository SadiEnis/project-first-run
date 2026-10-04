#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Development.Arenas;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class DemoOpeningTests : InputTestFixture
    {
        private Scene _scene, _original;
        private DemoOpeningController _demo;
        private const string Path = "Assets/_Project/Scenes/Demo/DeepJam_Opening.unity";
        [SetUp] public override void Setup() { }
        [TearDown] public override void TearDown() { }

        [UnitySetUp]
        public IEnumerator Load()
        {
            base.Setup();
            InputSystem.AddDevice<Keyboard>(); InputSystem.AddDevice<Mouse>();
            _original = SceneManager.GetActiveScene();
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Path) == null)
                Assert.Ignore("Create Opening Greybox before running demo scene acceptance.");
            Time.timeScale = 1;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(Path, new LoadSceneParameters(LoadSceneMode.Additive));
            _scene = SceneManager.GetSceneByPath(Path);
            SceneManager.SetActiveScene(_scene);
            _demo = _scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<DemoOpeningController>()).Single();
            for (int i = 0; i < 300 && (!_demo.IsReady || !_demo.Encounter.IsReadyForPassage); i++) yield return null;
            Assert.That(_demo.IsReady, Is.True);
            Assert.That(_demo.Encounter.IsReadyForPassage, Is.True, _demo.Encounter.LastError?.ToString());
        }

        [UnityTest]
        public IEnumerator StartIsSafeAndEnteringCorridorActivatesThePreparedEnemies()
        {
            Assert.That(_demo.Encounter.Status, Is.EqualTo(ArenaSessionStatus.Ready));
            Assert.That(_demo.Encounter.Enemies.Count, Is.EqualTo(4));
            Assert.That(_demo.Reward, Is.Null);
            var body = _demo.Health.GetComponent<CharacterController>();
            body.enabled = false;
            _demo.Health.transform.position = new Vector3(2, .1f, 12);
            body.enabled = true;
            for (int i = 0; i < 60 && _demo.Encounter.Status != ArenaSessionStatus.Running; i++)
                yield return new WaitForFixedUpdate();
            Assert.That(_demo.Encounter.Status, Is.EqualTo(ArenaSessionStatus.Running));
            foreach (var enemy in _demo.Encounter.Enemies.ToArray())
                enemy.Health.ApplyDamage(new DamageInfo(10000, _demo.Health.gameObject, enemy.transform.position, Vector3.forward));
            yield return null;
            Assert.That(_demo.Encounter.Status, Is.EqualTo(ArenaSessionStatus.Victory));
            Assert.That(_demo.Reward, Is.Not.Null);
            var reward = _demo.Reward;
            Assert.That(reward.Status, Is.EqualTo(ChestStatus.Available));
            _demo.Encounter.SetPlayerInside(false); _demo.Encounter.SetPlayerInside(true);
            yield return null;
            Assert.That(_demo.Reward, Is.SameAs(reward), "Re-entry must not create another guaranteed reward.");
            Assert.That(_demo.RewardClaimed, Is.False, "Spawning a chest is not claiming its reward.");
        }

        [UnityTest]
        public IEnumerator DeathBeforeCombatCancelsEncounterWithoutGrantingReward()
        {
            _demo.Health.ApplyDamage(new DamageInfo(10000, null, _demo.Health.transform.position, Vector3.zero));
            yield return null;
            Assert.That(_demo.Health.IsDead, Is.True);
            Assert.That(_demo.Encounter.Status, Is.EqualTo(ArenaSessionStatus.Defeat));
            Assert.That(_demo.Reward, Is.Null);
        }

        [UnityTearDown]
        public IEnumerator Unload()
        {
            Time.timeScale = 1;
            if (_original.IsValid() && _original.isLoaded) SceneManager.SetActiveScene(_original);
            if (_scene.IsValid() && _scene.isLoaded) yield return SceneManager.UnloadSceneAsync(_scene);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            base.TearDown();
        }
    }
}
#endif
