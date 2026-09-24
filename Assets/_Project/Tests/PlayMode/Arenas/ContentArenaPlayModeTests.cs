#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Input;
using ProjectFirstRun.Player;
using ProjectFirstRun.Progression;
using ProjectFirstRun.Rewards.Claims;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class ContentArenaPlayModeTests
    {
        private const string Path = "Assets/_Project/Scenes/Tests/Test_ContentArena.unity";
        private Scene _scene, _original;
        private ContentArenaController _arena;
        private ContentArenaPanel _panel;
        private GameObject _player;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            Time.timeScale = 1;
            _original = SceneManager.GetActiveScene();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(Path, new LoadSceneParameters(LoadSceneMode.Additive));
            _scene = SceneManager.GetSceneByPath(Path);
            SceneManager.SetActiveScene(_scene);
            _arena = Find<ContentArenaController>().Single();
            _panel = Find<ContentArenaPanel>().Single();
            _player = _arena.Player;
            for (int i = 0; i < 200 && !_arena.IsReady; i++) yield return null;
            Assert.That(_arena.IsReady, Is.True);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            if (_panel != null) _panel.Close();
            Time.timeScale = 1;
            if (_original.IsValid() && _original.isLoaded) SceneManager.SetActiveScene(_original);
            if (_scene.IsValid() && _scene.isLoaded) yield return SceneManager.UnloadSceneAsync(_scene);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private T[] Find<T>() where T : Component => _scene.GetRootGameObjects()
            .SelectMany(x => x.GetComponentsInChildren<T>(true)).ToArray();

        [UnityTest]
        public IEnumerator StartupUsesThreeFamiliesAndRealNavigationWithoutFreeRewards()
        {
            Assert.That(_arena.Enemies.Count, Is.EqualTo(4));
            Assert.That(_arena.Registry.ActiveCount, Is.EqualTo(4));
            Assert.That(_arena.Enemies.Select(x => x.Definition).Distinct().Count(), Is.EqualTo(3));
            Assert.That(_player.GetComponent<PlayerAbilityController>().AbilityCount, Is.Zero);
            Assert.That(_player.GetComponent<PlayerExperienceController>().TotalExperience, Is.Zero);
            Assert.That(Find<ChestController>(), Is.Empty);
            Assert.That(_arena.Selection.IsOpen, Is.False);
            var first = _arena.Enemies[0];
            var position = first.transform.position;
            foreach (var enemy in _arena.Enemies)
            {
                Assert.That(enemy.Target, Is.EqualTo(_player.transform));
                Assert.That(enemy.GetComponent<NavMeshAgent>().isOnNavMesh, Is.True);
            }
            yield return new WaitForSeconds(.4f);
            Assert.That(Vector3.Distance(first.transform.position, position), Is.GreaterThan(.1f));
        }

        [UnityTest]
        public IEnumerator ReplaceGroupPreservesLootHealthAmmoAndProgressionWithoutKillRewards()
        {
            Assert.That(_arena.GrantExperience(25), Is.True);
            Assert.That(_arena.SpawnChest(0), Is.True, _arena.LastResult);
            var chest = Find<ChestController>().Single();
            var weapon = _player.GetComponent<PlayerWeaponController>().ActiveEntry;
            weapon.RuntimeState.TryFire();
            int ammo = weapon.RuntimeState.MagazineAmmo;
            var health = _player.GetComponent<HealthComponent>();
            health.ApplyDamage(new DamageInfo(7, null, Vector3.zero, Vector3.forward));
            float remainingHealth = health.CurrentHealth;
            var old = _arena.Enemies.ToArray();
            Assert.That(_arena.ReplaceGroup(2), Is.True, _arena.LastResult);
            Assert.That(_arena.Registry.ActiveCount, Is.EqualTo(4));
            Assert.That(_arena.Enemies.All(x => x.Definition == _arena.EnemyDefinitions[2]), Is.True);
            yield return null;
            Assert.That(old.All(x => x == null), Is.True);
            Assert.That(Find<ExperiencePickup>(), Is.Empty);
            Assert.That(Find<ChestController>().Single(), Is.SameAs(chest));
            Assert.That(_player.GetComponent<PlayerExperienceController>().TotalExperience, Is.EqualTo(25));
            Assert.That(health.CurrentHealth, Is.EqualTo(remainingHealth));
            Assert.That(weapon.RuntimeState.MagazineAmmo, Is.EqualTo(ammo));
            Assert.That(_arena.ReplaceGroup(-2), Is.False);
            Assert.That(_arena.Registry.ActiveCount, Is.EqualTo(4));
        }

        [UnityTest]
        public IEnumerator ActualKillsCollectXpAndProduceClaimableLevelUpChest()
        {
            foreach (var enemy in _arena.Enemies.ToArray())
                enemy.Health.ApplyDamage(new DamageInfo(10000, _player, enemy.transform.position, Vector3.forward));
            yield return null;
            var pickups = Find<ExperiencePickup>();
            Assert.That(pickups, Has.Length.EqualTo(4));
            foreach (var pickup in pickups)
                Assert.That(pickup.TryCollect(_player.GetComponent<PlayerExperienceCollector>()), Is.True);
            var source = Find<LevelUpChestSource>().Single();
            for (int i = 0; i < 120 && source.SpawnedChestCount == 0; i++) yield return null;
            Assert.That(_player.GetComponent<PlayerExperienceController>().Level, Is.EqualTo(2));
            Assert.That(source.SpawnedChestCount, Is.EqualTo(1));
            var chest = Find<ChestController>().First();
            chest.TryOpen();
            Assert.That(_arena.Selection.IsOpen, Is.True);
            Assert.That(Time.timeScale, Is.Zero);
            foreach (var choice in _arena.Selection.ActiveSession.Offer.Choices.ToArray())
            {
                if (!_arena.Selection.IsOpen) break;
                Assert.That(_arena.Selection.Select(choice), Is.EqualTo(RewardClaimResult.Claimed));
            }
            Assert.That(_arena.Selection.IsOpen, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ItemCommandsUseRuntimeLevelsPreserveAmmoAndRejectInvalidRequests()
        {
            Assert.That(_arena.LevelUp(1), Is.True);
            Assert.That(_arena.LastResult, Is.EqualTo("NotOwned"));
            Assert.That(_arena.Acquire(1), Is.True);
            Assert.That(_arena.LastResult, Is.EqualTo("Acquired"));
            var weapon = _player.GetComponent<PlayerWeaponAcquisitionController>().Loadout.GetEntry((WeaponDefinition)_arena.Items[1]);
            weapon.RuntimeState.TryFire();
            int ammo = weapon.RuntimeState.MagazineAmmo;
            for (int i = 1; i < weapon.MaximumLevel; i++) Assert.That(_arena.LevelUp(1), Is.True);
            Assert.That(_arena.ItemLevel(1), Is.EqualTo(weapon.MaximumLevel));
            Assert.That(weapon.RuntimeState.MagazineAmmo, Is.EqualTo(ammo));
            Assert.That(_arena.LevelUp(1), Is.True);
            Assert.That(_arena.LastResult, Is.EqualTo("MaximumLevelReached"));
            Assert.That(_arena.Acquire(2), Is.True);
            Assert.That(_arena.Acquire(3), Is.True);
            Assert.That(_arena.LevelUp(2), Is.True);
            Assert.That(_arena.LevelUp(3), Is.True);
            Assert.That(_arena.ItemLevel(2), Is.EqualTo(2));
            Assert.That(_arena.ItemLevel(3), Is.EqualTo(2));
            Assert.That(_arena.Acquire(-1), Is.False);
            Assert.That(_arena.LevelUp(100), Is.False);
            Assert.That(_arena.GrantExperience(-10), Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PanelReservesRewardUiRestoresStatesAndPermitsRealChestAfterClose()
        {
            var control = _player.GetComponent<PlayerController>();
            var weapon = _player.GetComponent<PlayerWeaponController>();
            weapon.SetWeaponControlEnabled(false);
            Time.timeScale = .5f;
            for (int i = 0; i < 3; i++)
            {
                Assert.That(_panel.TryOpen(), Is.True);
                Assert.That(_panel.TryOpen(), Is.False);
                Assert.That(Time.timeScale, Is.Zero);
                Assert.That(control.IsControlEnabled, Is.False);
                Assert.That(_arena.Selection.enabled, Is.False);
                Assert.That(_player.GetComponent<PlayerInputReader>().IsGameplayInputEnabled, Is.False);
                Assert.That(_player.GetComponent<PlayerAbilityController>().IsAbilityControlEnabled, Is.False);
                _panel.Close();
                Assert.That(Time.timeScale, Is.EqualTo(.5f));
                Assert.That(control.IsControlEnabled, Is.True);
                Assert.That(weapon.IsWeaponControlEnabled, Is.False);
            }
            Assert.That(_panel.TryOpen(), Is.True);
            Assert.That(_arena.SpawnChest(1), Is.True, _arena.LastResult);
            _panel.Close();
            Find<ChestController>().Single().TryOpen();
            Assert.That(_arena.Selection.IsOpen, Is.True);
            Assert.That(_panel.TryOpen(), Is.False);
            Assert.That(_arena.GrantExperience(100), Is.False);
            Assert.That(_arena.Acquire(1), Is.False);
            Assert.That(_arena.ReplaceGroup(0), Is.False);
            Assert.That(_arena.Selection.Select(_arena.Selection.ActiveSession.Offer.Choices[0]), Is.EqualTo(RewardClaimResult.Claimed));
            Assert.That(Time.timeScale, Is.EqualTo(.5f));
            Assert.That(control.IsControlEnabled, Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FullWeaponSlotsReportCapacityWithoutAddingAnItem()
        {
            Assert.That(_arena.Acquire(1), Is.True);
            var extra = Object.Instantiate((WeaponDefinition)_arena.Items[1]);
            try
            {
                var item = new SerializedObject(extra);
                item.FindProperty("_stableId").stringValue = "content-test-capacity-only";
                item.ApplyModifiedPropertiesWithoutUndo();
                var catalog = new SerializedObject(_arena);
                var items = catalog.FindProperty("_items");
                int index = items.arraySize++;
                items.GetArrayElementAtIndex(index).objectReferenceValue = extra;
                catalog.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(_arena.Acquire(index), Is.True);
                Assert.That(_arena.LastResult, Is.EqualTo("CapacityReached"));
                Assert.That(_arena.ItemLevel(index), Is.Zero);
                Assert.That(_arena.Acquire(1), Is.True);
                Assert.That(_arena.LastResult, Is.EqualTo("AlreadyOwned"));
            }
            finally { Object.Destroy(extra); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator PanelXpCreatesRealChestAndDisablingPanelReleasesControls()
        {
            Assert.That(_panel.TryOpen(), Is.True);
            Assert.That(_arena.GrantExperience(100), Is.True);
            Assert.That(_player.GetComponent<PlayerExperienceController>().Level, Is.EqualTo(2));
            Assert.That(Find<LevelUpChestSource>().Single().PendingChestCount, Is.EqualTo(1));
            // The normal chest source intentionally waits until the game is no longer paused.
            Assert.That(Find<ChestController>(), Is.Empty);
            _panel.enabled = false;
            for (int i = 0; i < 120 && Find<LevelUpChestSource>().Single().SpawnedChestCount == 0; i++) yield return null;
            Assert.That(_player.GetComponent<PlayerExperienceController>().Level, Is.EqualTo(2));
            Assert.That(Find<LevelUpChestSource>().Single().SpawnedChestCount, Is.EqualTo(1));
            Assert.That(Find<ChestController>(), Has.Length.EqualTo(1));
            Assert.That(_panel.IsOpen, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(_arena.Selection.enabled, Is.True);
            Assert.That(_player.GetComponent<PlayerController>().IsControlEnabled, Is.True);
            Assert.That(_player.GetComponent<PlayerInputReader>().IsGameplayInputEnabled, Is.True);
        }

        [UnityTest]
        public IEnumerator DeathClosesPanelWithoutRestoringControlsOrAllowingMutations()
        {
            Assert.That(_panel.TryOpen(), Is.True);
            _player.GetComponent<HealthComponent>().ApplyDamage(new DamageInfo(10000, null, Vector3.zero, Vector3.forward));
            yield return null;
            Assert.That(_panel.IsOpen, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(_player.GetComponent<PlayerController>().IsControlEnabled, Is.False);
            Assert.That(_player.GetComponent<PlayerWeaponController>().IsWeaponControlEnabled, Is.False);
            Assert.That(_player.GetComponent<PlayerAbilityController>().IsAbilityControlEnabled, Is.False);
            Assert.That(_panel.TryOpen(), Is.False);
            Assert.That(_arena.GrantExperience(100), Is.False);
            Assert.That(_arena.ReplaceGroup(0), Is.False);
        }
    }
}
#endif
