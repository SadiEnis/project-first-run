#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using ProjectFirstRun.Player;
using ProjectFirstRun.Progression;
using ProjectFirstRun.Rewards.Claims;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Weapons;
using ProjectFirstRun.Waves;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    /// <summary>Loads the actual authored scene; no substitute player, spawner or wave.</summary>
    public sealed class PlayableMapFixturePlayModeTests
    {
        private const string SourcePath = "Assets/_Project/Scenes/Playable/PlayableMapFixture.unity";
        private Scene _original, _source;
        private PlayableMapFixtureBootstrap _fixture;
        private GameObject _player;
        private SceneTravelController _travel;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            Time.timeScale = 1;
            _original = SceneManager.GetActiveScene();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(SourcePath, new LoadSceneParameters(LoadSceneMode.Additive));
            _source = SceneManager.GetSceneByPath(SourcePath);
            SceneManager.SetActiveScene(_source);
            _fixture = _source.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<PlayableMapFixtureBootstrap>()).Single();
            _travel = _source.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<SceneTravelController>()).Single();
            _player = _travel.gameObject;
            _player.GetComponent<PlayerController>().enabled = false;
            yield return null;
            yield return null;
            Assert.That(_player.GetComponent<PlayerExperienceController>().IsInitialized, Is.True);
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            Time.timeScale = 1;
            if (_travel != null && !_travel.CurrentTravel.IsCompleted)
            {
                _travel.Cancel();
                for (int i=0;i<1000 && !_travel.CurrentTravel.IsCompleted;i++) yield return null;
            }
            if (_original.IsValid() && _original.isLoaded) SceneManager.SetActiveScene(_original);
            if (_player != null) Object.Destroy(_player);
            if (_source.IsValid() && _source.isLoaded) yield return SceneManager.UnloadSceneAsync(_source);
            var target = SceneManager.GetSceneByPath(PlayableMapFixtureBootstrap.TargetScene);
            if (target.IsValid() && target.isLoaded) yield return SceneManager.UnloadSceneAsync(target);
            Cursor.lockState = CursorLockMode.None;
        }

        private PreparedRegionEncounter Encounter(string id) => _fixture.Encounters.Single(x => x.Region.RegionId == id);
        private IEnumerator At(float x, float z)
        {
            _player.GetComponent<PlayerMotor>().Teleport(new Vector3(x,.1f,z), Quaternion.identity);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return null;
        }
        private IEnumerator Ready(PreparedRegionEncounter encounter)
        {
            for (int i=0;i<120 && !encounter.IsReadyForPassage;i++)
            {
                Assert.That(encounter.LastError, Is.Null, encounter.LastError?.ToString());
                yield return null;
            }
            Assert.That(encounter.PreparationStatus, Is.EqualTo(RegionPreparationStatus.Ready));
            TestContext.WriteLine($"{encounter.Region.RegionId}: preparation max {encounter.MaxPreparationStepMilliseconds:F4} ms");
        }
        private IEnumerator Walk(Vector3 delta, int steps)
        {
            for (int i=0;i<steps;i++)
            {
                _player.GetComponent<CharacterController>().Move(delta);
                yield return new WaitForFixedUpdate();
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator PreparedEnemiesAreOccludedNavigateOnActivation_OneWayDoorBlocksReturn()
        {
            var encounter = Encounter("second-main");
            yield return At(8,0);
            yield return Ready(encounter);
            Assert.That(encounter.Enemies, Has.Count.EqualTo(4));
            var registry = _fixture.MapRoot.Content.GetComponentInChildren<EnemyRegistry>();
            Assert.That(registry.ActiveCount, Is.Zero);
            foreach (var enemy in encounter.Enemies)
            {
                Assert.That(enemy.GetComponent<NavMeshAgent>().isOnNavMesh, Is.True);
                Assert.That(enemy.GetComponent<EnemyMotor>().IsMovementEnabled, Is.False);
                Assert.That(Physics.Linecast(new Vector3(8,1.6f,0), enemy.transform.position+Vector3.up,
                    out var hit, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore), Is.True);
                Assert.That(hit.collider.name, Is.EqualTo("Spawn sight screen").Or.EqualTo("Boundary wall"));
            }
            yield return Walk(new Vector3(.25f,0,0), 48);
            Assert.That(_fixture.Map.Session.CurrentRegionId, Is.EqualTo("second-main"));
            yield return Walk(new Vector3(-.25f,0,0), 48);
            Assert.That(_player.transform.position.x, Is.GreaterThan(15));
            Assert.That(_fixture.Map.Session.CurrentRegionId, Is.EqualTo("second-main"));
            yield return At(27,6);
            Assert.That(encounter.Status, Is.EqualTo(ArenaSessionStatus.Running));
            Assert.That(registry.ActiveCount, Is.EqualTo(4));
            var first = encounter.Enemies[0];
            Vector3 initial = first.transform.position;
            yield return new WaitForSeconds(.4f);
            Assert.That(Vector3.Distance(first.transform.position,initial), Is.GreaterThan(.1f));
            TestContext.WriteLine($"Activation {encounter.LastActivationMilliseconds:F4} ms");
        }

        [UnityTest]
        public IEnumerator SideReentryPreservesEnemies_CollectedXpCreatesClaimableLevelUpChest()
        {
            var encounter = Encounter("side-north");
            yield return At(0,8);
            yield return Ready(encounter);
            yield return Walk(new Vector3(0,0,.25f), 40);
            Assert.That(_fixture.Map.Session.CurrentRegionId, Is.EqualTo("side-north"));
            yield return At(5,25);
            Assert.That(encounter.Status, Is.EqualTo(ArenaSessionStatus.Running));
            var first = encounter.Enemies[0];
            first.Health.ApplyDamage(new DamageInfo(1,_player,first.transform.position,Vector3.forward));
            float health = first.Health.CurrentHealth;
            yield return At(0,18);
            yield return Walk(new Vector3(0,0,-.25f), 40);
            Assert.That(_fixture.Map.Session.CurrentRegionId, Is.EqualTo("main"));
            yield return Walk(new Vector3(0,0,.25f), 40);
            Assert.That(_fixture.Map.Session.CurrentRegionId, Is.EqualTo("side-north"));
            Assert.That(encounter.Enemies[0], Is.SameAs(first));
            Assert.That(first.Health.CurrentHealth, Is.EqualTo(health));
            foreach (var enemy in encounter.Enemies.ToArray())
                enemy.Health.ApplyDamage(new DamageInfo(1000,_player,enemy.transform.position,Vector3.forward));
            Assert.That(encounter.Status, Is.EqualTo(ArenaSessionStatus.Victory));
            yield return null;
            var pickups = Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None)
                .Where(x=>x.gameObject.scene == _source).ToArray();
            Assert.That(pickups, Has.Length.EqualTo(4));
            var collector = _player.GetComponent<PlayerExperienceCollector>();
            foreach (var pickup in pickups) Assert.That(pickup.TryCollect(collector), Is.True);
            var xp = _player.GetComponent<PlayerExperienceController>();
            Assert.That(xp.TotalExperience, Is.EqualTo(100));
            Assert.That(xp.Level, Is.EqualTo(2));
            var levelSource = _fixture.MapRoot.Content.GetComponentInChildren<LevelUpChestSource>();
            for (int i=0;i<120 && levelSource.SpawnedChestCount == 0;i++) yield return null;
            Assert.That(levelSource.SpawnedChestCount, Is.EqualTo(1));
            var chest = Object.FindObjectsByType<ChestController>(FindObjectsSortMode.None)
                .Single(x=>x.gameObject.scene == _source);
            chest.TryOpen();
            var selection = _fixture.MapRoot.Content.GetComponentInChildren<RewardSelectionController>(true);
            Assert.That(selection.IsOpen, Is.True);
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(selection.Select(selection.ActiveSession.Offer.Choices[0]), Is.EqualTo(RewardClaimResult.Claimed));
            Assert.That(selection.IsOpen, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator CrowdedGroupHonorsSpawnBudgetAndActivatesExistingInstances()
        {
            var wave = AssetDatabase.LoadAssetAtPath<EnemyWaveDefinition>(
                "Assets/_Project/Data/Waves/Test/EW_PlayableFixture.asset");
            var entry = wave.Entries[0];
            var go = new GameObject("Crowded preparation probe");
            go.transform.SetParent(_fixture.MapRoot.Content.transform);
            var spawner = go.AddComponent<EnemySpawner>();
            var encounter = go.AddComponent<PreparedRegionEncounter>();
            var registry = _fixture.MapRoot.Content.GetComponentInChildren<EnemyRegistry>();
            var requests = new EnemySpawnRequest[32];
            for (int i = 0; i < requests.Length; i++)
                requests[i] = new EnemySpawnRequest(entry.EnemyPrefab, entry.EnemyDefinition, _player.transform,
                    _player.GetComponent<HealthComponent>(), registry, new Vector3(27 + i % 4 * 3, 0, -7 + i / 4 * 2), Quaternion.identity);
            encounter.Initialize("stress-probe", spawner, requests, _player.GetComponent<HealthComponent>(), 2, 2);
            long memoryBefore = System.GC.GetTotalMemory(false);
            encounter.RequestPreparation();
            int previous = 0;
            for (int i = 0; i < 100 && !encounter.IsReadyForPassage; i++)
            {
                yield return null;
                Assert.That(encounter.LastError, Is.Null, encounter.LastError?.ToString());
                Assert.That(encounter.Enemies.Count - previous, Is.InRange(0, 2));
                previous = encounter.Enemies.Count;
            }
            Assert.That(encounter.IsReadyForPassage, Is.True);
            Assert.That(encounter.Enemies, Has.Count.EqualTo(32));
            Assert.That(registry.ActiveCount, Is.Zero);
            var identities = encounter.Enemies.Select(x => x.GetInstanceID()).ToArray();
            encounter.SetPlayerInside(true);
            yield return null;
            Assert.That(encounter.Status, Is.EqualTo(ArenaSessionStatus.Running));
            Assert.That(registry.ActiveCount, Is.EqualTo(32));
            Assert.That(encounter.Enemies.Select(x => x.GetInstanceID()).ToArray(), Is.EqualTo(identities));
            TestContext.WriteLine($"32 real prefab enemies: preparation max {encounter.MaxPreparationStepMilliseconds:F4} ms; " +
                $"activation {encounter.LastActivationMilliseconds:F4} ms; whole-probe managed heap delta " +
                $"{System.GC.GetTotalMemory(false) - memoryBefore} bytes (not an allocation counter or FPS guarantee).");
        }

        [UnityTest]
        public IEnumerator FreeExitTravelsWithLivingEnemies_PreservesPlayerAndBindsDestinationServices()
        {
            var xp = _player.GetComponent<PlayerExperienceController>();
            xp.GainExperience(25);
            var weapon = _player.GetComponent<PlayerWeaponController>().ActiveEntry;
            weapon.RuntimeState.TryFire();
            int ammo = weapon.RuntimeState.MagazineAmmo;
            var build = _player.GetComponent<PlayerBuildController>().Build;
            int playerId = _player.GetInstanceID();
            yield return At(8,0); yield return Ready(Encounter("second-main"));
            yield return Walk(new Vector3(.25f,0,0),48);
            yield return At(27,6);
            Assert.That(Encounter("second-main").Status, Is.EqualTo(ArenaSessionStatus.Running));
            // The exit's actual collider initiates travel without waiting for encounter victory.
            yield return At(38,7);
            for (int i=0;i<1500 && _travel.Status != SceneTravelStatus.Succeeded;i++)
            {
                Assert.That(_travel.LastError, Is.Null, _travel.LastError);
                yield return null;
            }
            Assert.That(_travel.Status, Is.EqualTo(SceneTravelStatus.Succeeded), _travel.LastError);
            Assert.That(_source.isLoaded, Is.False);
            Assert.That(_player.GetInstanceID(), Is.EqualTo(playerId));
            Assert.That(_player.GetComponent<PlayerBuildController>().Build, Is.SameAs(build));
            Assert.That(_player.GetComponent<PlayerWeaponController>().ActiveEntry, Is.SameAs(weapon));
            Assert.That(weapon.RuntimeState.MagazineAmmo, Is.EqualTo(ammo));
            Assert.That(xp.TotalExperience, Is.EqualTo(25));
            Assert.That(Physics.Raycast(_player.transform.position+Vector3.up,Vector3.down,3,1<<7), Is.True);
            var target = _travel.CurrentMap;
            var levelSource = target.Content.GetComponentInChildren<LevelUpChestSource>();
            Assert.That(levelSource.SpawnedChestCount, Is.Zero);
            xp.GainExperience(75);
            for (int i=0;i<120 && levelSource.SpawnedChestCount == 0;i++) yield return null;
            Assert.That(levelSource.SpawnedChestCount, Is.EqualTo(1));
        }
    }
}
#endif
