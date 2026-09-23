#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.Fireball;
using ProjectFirstRun.Abilities.Targeting;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests.Interaction;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Input;
using ProjectFirstRun.Player;
using ProjectFirstRun.Progression;
using ProjectFirstRun.Weapons;
using ProjectFirstRun.Upgrades;
using ProjectFirstRun.Stats;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class SceneTravelTests
    {
        private const string TargetPath = "Assets/_Project/Scenes/Tests/Test_SceneTravelTarget.unity";
        private readonly List<Scene> _scenes = new List<Scene>();
        private readonly List<ScriptableObject> _assets = new List<ScriptableObject>();
        private Scene _original, _source, _target;
        private GameObject _player;
        private MapSceneRoot _sourceMap, _targetMap;
        private SceneTravelController _travel;
        private ControlledLoader _loader;
        private SceneTravelDestination Destination(string entry = "arrival") => new SceneTravelDestination(TargetPath, entry);

        [SetUp]
        public void Setup()
        {
            Time.timeScale = 1;
            _original = SceneManager.GetActiveScene();
            _source = NewScene("Travel source");
            _target = NewScene("Travel target");
            _sourceMap = CreateMap(_source);
            _sourceMap.Activate();
            _targetMap = CreateMap(_target);
            _player = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Player/Player.prefab"));
            SceneManager.MoveGameObjectToScene(_player, _source);
            _player.GetComponent<PlayerStartingLoadoutInitializer>().Initialize(new PlayerBuildCapacity(2, 3, 5));
            _player.GetComponent<PlayerController>().enabled = false;
            var xp = _player.GetComponent<PlayerExperienceController>();
            if (xp == null) xp = _player.AddComponent<PlayerExperienceController>();
            if (!xp.IsInitialized) xp.Initialize(new ExperienceState(new ExperienceCurve(100, 50)));
            xp.GainExperience(125);
            _travel = _player.AddComponent<SceneTravelController>();
            _loader = new ControlledLoader();
            _travel.ConfigureLoader(_loader);
        }

        private Scene NewScene(string name)
        {
            Scene scene = SceneManager.CreateScene(name + Guid.NewGuid().ToString("N"));
            _scenes.Add(scene);
            return scene;
        }

        private static GameObject NewIn(Scene scene, string name)
        {
            var go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, scene);
            return go;
        }

        private static MapSceneRoot CreateMap(Scene scene)
        {
            var content = NewIn(scene, "Content");
            content.SetActive(false);
            var map = content.AddComponent<MapTraversalController>();
            content.AddComponent<EnemyRegistry>();
            map.Initialize("main", new[] { "main", "side" });
            var point = NewIn(scene, "Entry").transform;
            point.SetParent(content.transform);
            point.position = new Vector3(7, 0, 11);
            var metadata = NewIn(scene, "Metadata").AddComponent<MapSceneRoot>();
            metadata.Configure(content, map, new[] { new MapSceneEntry("arrival", "side", point) });
            return metadata;
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            if (_travel != null && !_travel.CurrentTravel.IsCompleted)
            {
                _travel.Cancel();
                _loader?.Pending.TrySetResult(_target);
                for (int i = 0; i < 1000 && !_travel.CurrentTravel.IsCompleted; i++) yield return null;
            }
            Time.timeScale = 1;
            if (_original.IsValid() && _original.isLoaded) SceneManager.SetActiveScene(_original);
            if (_player != null) Object.DestroyImmediate(_player);
            foreach (var scene in _scenes)
                if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            var fixture = SceneManager.GetSceneByPath(TargetPath);
            if (fixture.IsValid() && fixture.isLoaded) yield return SceneManager.UnloadSceneAsync(fixture);
            _scenes.Clear();
            foreach (var asset in _assets) if (asset != null) Object.DestroyImmediate(asset);
            _assets.Clear();
        }

        private IEnumerator Finish()
        {
            for (int i = 0; i < 2000 && !_travel.CurrentTravel.IsCompleted; i++) yield return null;
            Assert.That(_travel.CurrentTravel.IsCompleted, Is.True, "Travel did not finish.");
            Assert.That(_travel.CurrentTravel.IsFaulted, Is.False, _travel.CurrentTravel.Exception?.ToString());
        }

        [UnityTest]
        public IEnumerator SuccessPreservesPlayerRuntimeAndDisposesOnlyOldMap()
        {
            var build = _player.GetComponent<PlayerBuildController>().Build;
            var weapons = _player.GetComponent<PlayerWeaponController>();
            _player.GetComponent<PlayerWeaponAcquisitionController>().TryLevelUp(weapons.ActiveDefinition);
            var entry = weapons.ActiveEntry;
            entry.RuntimeState.TryFire();
            entry.RuntimeState.TryStartReload();
            float reload = entry.RuntimeState.ReloadTimeRemaining;
            int ammo = entry.RuntimeState.MagazineAmmo;
            var health = _player.GetComponent<HealthComponent>();
            health.ApplyDamage(new DamageInfo(17, null, Vector3.zero, Vector3.forward));
            var oldLoot = NewIn(_source, "Unclaimed old map loot");
            oldLoot.AddComponent<ExperiencePickup>().Initialize(50);
            int identity = _player.GetInstanceID();
            var mapSession = _sourceMap.Map.Session;
            Time.timeScale = .5f;
            Assert.That(_travel.TryTravel(_sourceMap, Destination()), Is.True);
            Assert.That(mapSession.IsTransitioning, Is.True);
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(weapons.enabled, Is.False);
            var interactor = _player.GetComponent<PlayerChestInteractor>();
            if (interactor != null) Assert.That(interactor.enabled, Is.False);
            Assert.That(_targetMap.Content.activeSelf, Is.False);
            Assert.That(_travel.TryTravel(_sourceMap, Destination()), Is.False);
            yield return null;
            Assert.That(entry.RuntimeState.ReloadTimeRemaining, Is.EqualTo(reload));
            _loader.Pending.SetResult(_target);
            yield return Finish();
            Assert.That(_travel.Status, Is.EqualTo(SceneTravelStatus.Succeeded), _travel.LastError);
            Assert.That(_source.isLoaded, Is.False);
            Assert.That(oldLoot == null, Is.True);
            Assert.That(_original.isLoaded, Is.True, "Unrelated scene must survive.");
            Assert.That(_player.GetInstanceID(), Is.EqualTo(identity));
            Assert.That(_player.scene, Is.EqualTo(_target));
            Assert.That(SceneManager.GetActiveScene(), Is.EqualTo(_target));
            Assert.That(_player.transform.position, Is.EqualTo(new Vector3(7, 0, 11)));
            Assert.That(_targetMap.Map.Session.CurrentRegionId, Is.EqualTo("side"));
            Assert.That(_targetMap.Content.activeSelf, Is.True);
            Assert.That(mapSession.IsTransitioning, Is.False);
            Assert.That(_player.GetComponent<PlayerBuildController>().Build, Is.SameAs(build));
            Assert.That(weapons.ActiveEntry, Is.SameAs(entry));
            Assert.That(entry.Level, Is.EqualTo(2));
            Assert.That(entry.RuntimeState.MagazineAmmo, Is.EqualTo(ammo));
            Assert.That(health.CurrentHealth, Is.EqualTo(83));
            Assert.That(_player.GetComponent<PlayerExperienceController>().TotalExperience, Is.EqualTo(125));
            Assert.That(Time.timeScale, Is.EqualTo(.5f));
            Assert.That(_player.GetComponent<PlayerController>().enabled, Is.False);
            Assert.That(weapons.enabled, Is.True);
        }

        [UnityTest]
        public IEnumerator AbilityTargetsRebindWithoutResettingLevelsCooldownOrUpgradeModifiers()
        {
            var stats = _player.GetComponent<PlayerStatsController>().Stats;
            var upgrades = _player.GetComponent<PlayerUpgradeController>();
            if (upgrades == null) upgrades = _player.AddComponent<PlayerUpgradeController>();
            var upgrade = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(
                "Assets/_Project/Data/Upgrade/Dev/UD_DevelopmentDamageBoost.asset");
            upgrades.TryAcquire(upgrade);
            upgrades.TryLevelUp(upgrade);
            float damage = stats.Evaluate(PlayerStatType.AbilityDamage, 100);
            int modifiers = stats.ModifierCount;
            var acquisition = _player.GetComponent<PlayerAbilityAcquisitionController>();
            if (acquisition == null) acquisition = _player.AddComponent<PlayerAbilityAcquisitionController>();
            var factory = new FireballAbilityRuntimeFactory(
                _sourceMap.Content.GetComponent<EnemyRegistry>(), _player, stats);
            var factories = new AbilityRuntimeFactoryRegistry();
            factories.Register(factory);
            acquisition.Initialize(factories);
            var definition = AssetDatabase.LoadAssetAtPath<FireballDefinition>(
                "Assets/_Project/Data/Abilities/Fireball/AD_Fireball.asset");
            acquisition.TryAcquire(definition);
            acquisition.TryLevelUp(definition);
            var abilities = _player.GetComponent<PlayerAbilityController>();
            abilities.enabled = false;
            var entry = abilities.Entries[0];
            entry.State.TryCommitCast();
            float cooldown = entry.State.CooldownRemaining;
            var enemyObject = NewIn(_target, "New map enemy");
            enemyObject.transform.SetParent(_targetMap.Content.transform);
            enemyObject.transform.position = new Vector3(9, 0, 11);
            var enemy = enemyObject.AddComponent<EnemyController>();
            var enemyDefinition = ScriptableObject.CreateInstance<EnemyDefinition>();
            _assets.Add(enemyDefinition);
            enemy.Initialize(enemyDefinition, _player.transform, _targetMap.Content.GetComponent<EnemyRegistry>());
            _travel.TryTravel(_sourceMap, Destination());
            _loader.Pending.SetResult(_target);
            yield return Finish();
            Assert.That(_travel.Status, Is.EqualTo(SceneTravelStatus.Succeeded), _travel.LastError);
            Assert.That(abilities.Entries[0], Is.SameAs(entry));
            Assert.That(entry.Level, Is.EqualTo(2));
            Assert.That(entry.State.CooldownRemaining, Is.EqualTo(cooldown));
            Assert.That(stats.ModifierCount, Is.EqualTo(modifiers));
            Assert.That(stats.Evaluate(PlayerStatType.AbilityDamage, 100), Is.EqualTo(damage));
            var selectorField = typeof(AbilityRuntimeEntry).GetField("_targetSelector", BindingFlags.Instance | BindingFlags.NonPublic);
            var selector = (IAbilityTargetSelector)selectorField.GetValue(entry);
            Assert.That(selector.TrySelectTarget(_player.transform.position, out var target), Is.True);
            Assert.That(target, Is.EqualTo(enemy.transform));
            var futureEntry = factory.Create(definition);
            var futureSelector = (IAbilityTargetSelector)selectorField.GetValue(futureEntry);
            Assert.That(futureSelector.TrySelectTarget(_player.transform.position, out target), Is.True);
            Assert.That(target, Is.EqualTo(enemy.transform));
        }

        [UnityTest]
        public IEnumerator DisabledControllerCancelsAndRestoresTimeAfterCleanup()
        {
            _travel.TryTravel(_sourceMap, Destination());
            _travel.enabled = false;
            _loader.Pending.SetResult(_target);
            yield return Finish();
            Assert.That(_travel.Status, Is.EqualTo(SceneTravelStatus.Cancelled));
            Assert.That(_player.scene, Is.EqualTo(_source));
            Assert.That(Time.timeScale, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator FailedRollbackCleanupBlocksRetriesUntilRejectedSceneIsRemoved()
        {
            _loader.FailUnload = true;
            _travel.TryTravel(_sourceMap, Destination("missing"));
            _loader.Pending.SetResult(_target);
            yield return Finish();
            Assert.That(_travel.Status, Is.EqualTo(SceneTravelStatus.CleanupFailed));
            Assert.That(_player.scene, Is.EqualTo(_source));
            Assert.That(_travel.IsBusy, Is.True);
            Assert.That(_targetMap.Content.activeInHierarchy, Is.False);
            _loader.FailUnload = false;
            _travel.RetryCleanup();
            yield return Finish();
            Assert.That(_target.isLoaded, Is.False);
            Assert.That(_travel.IsBusy, Is.False);
            Assert.That(_source.isLoaded, Is.True);
        }

        [UnityTest]
        public IEnumerator GenericExitIgnoresOtherBodiesAndStartsOnlyOneLoadForPlayer()
        {
            var exitObject = NewIn(_source, "Generic map exit");
            var volume = exitObject.AddComponent<BoxCollider>();
            volume.size = Vector3.one * 4;
            volume.isTrigger = true;
            var body = exitObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            var exit = exitObject.AddComponent<MapSceneExit>();
            exit.Configure(_travel, _sourceMap, Destination(), "main");
            _player.transform.position = new Vector3(20, 0, 20);
            var other = NewIn(_source, "Not the player");
            other.AddComponent<BoxCollider>();
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(_loader.LoadCount, Is.Zero);
            _player.transform.position = Vector3.zero;
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.That(_loader.LoadCount, Is.EqualTo(1));
            _loader.Pending.SetResult(_target);
            yield return Finish();
            Assert.That(_travel.Status, Is.EqualTo(SceneTravelStatus.Succeeded));
            Assert.That(_loader.LoadCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DestinationLevelUpSourceBindsCurrentXpWithoutRegrantingPastLevels()
        {
            var holder = NewIn(_target, "Destination level rewards");
            holder.transform.SetParent(_targetMap.Content.transform);
            var spawner = holder.AddComponent<ChestSpawner>();
            var source = holder.AddComponent<LevelUpChestSource>();
            typeof(LevelUpChestSource).GetField("_spawner", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(source, spawner);
            typeof(LevelUpChestSource).GetField("_dropTable", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(source, AssetDatabase.LoadAssetAtPath<ChestDropTable>(
                    "Assets/_Project/Data/Chests/Dev/CDT_DevelopmentLevelUp.asset"));
            _travel.TryTravel(_sourceMap, Destination());
            _loader.Pending.SetResult(_target);
            yield return Finish();
            Assert.That(_travel.Status, Is.EqualTo(SceneTravelStatus.Succeeded), _travel.LastError);
            Assert.That(source.IsInitialized, Is.True);
            Assert.That(source.PendingChestCount, Is.Zero);
            _player.GetComponent<PlayerExperienceController>().GainExperience(150);
            Assert.That(source.PendingChestCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator MissingEntryRollsBackAndUnloadsRejectedTarget()
        {
            var position = _player.transform.position;
            Assert.That(_travel.TryTravel(_sourceMap, Destination("missing")), Is.True);
            _loader.Pending.SetResult(_target);
            yield return Finish();
            Assert.That(_travel.Status, Is.EqualTo(SceneTravelStatus.Failed));
            Assert.That(_travel.LastError, Does.Contain("entry"));
            Assert.That(_player.scene, Is.EqualTo(_source));
            Assert.That(_player.transform.position, Is.EqualTo(position));
            Assert.That(_source.isLoaded, Is.True);
            Assert.That(_target.isLoaded, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1));
            Assert.That(_sourceMap.Map.Session.IsTransitioning, Is.False);
        }

        [UnityTest]
        public IEnumerator CancellationWaitsForLoadThenCleansTarget()
        {
            _travel.TryTravel(_sourceMap, Destination());
            _travel.Cancel();
            yield return null;
            Assert.That(_travel.IsBusy, Is.True);
            _loader.Pending.SetResult(_target);
            yield return Finish();
            Assert.That(_travel.Status, Is.EqualTo(SceneTravelStatus.Cancelled));
            Assert.That(_player.scene, Is.EqualTo(_source));
            Assert.That(_target.isLoaded, Is.False);
        }

        [UnityTest]
        public IEnumerator DeathDuringLoadDoesNotEnableDeadPlayerInput()
        {
            yield return null; // Complete the player's initial Start lifecycle.
            _travel.TryTravel(_sourceMap, Destination());
            _player.GetComponent<HealthComponent>().ApplyDamage(new DamageInfo(1000, null, Vector3.zero, Vector3.forward));
            _loader.Pending.SetResult(_target);
            yield return Finish();
            Assert.That(_travel.Status, Is.EqualTo(SceneTravelStatus.Cancelled));
            Assert.That(_player.scene, Is.EqualTo(_source));
            Assert.That(_player.GetComponent<PlayerInputReader>().IsGameplayInputEnabled, Is.False);
            Assert.That(_player.GetComponent<PlayerWeaponController>().IsWeaponControlEnabled, Is.False);
        }

        [UnityTest]
        public IEnumerator LoadExceptionRestoresSourceAndReleasesLock()
        {
            _travel.TryTravel(_sourceMap, Destination());
            _loader.Pending.SetException(new InvalidOperationException("Simulated load failure"));
            yield return Finish();
            Assert.That(_travel.Status, Is.EqualTo(SceneTravelStatus.Failed));
            Assert.That(_travel.LastError, Does.Contain("Simulated"));
            Assert.That(_sourceMap.Map.Session.IsTransitioning, Is.False);
            Assert.That(_player.scene, Is.EqualTo(_source));
            Assert.That(Time.timeScale, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator CleanupFailureKeepsDestinationAndSupportsRetry()
        {
            _loader.FailUnload = true;
            _travel.TryTravel(_sourceMap, Destination());
            _loader.Pending.SetResult(_target);
            yield return Finish();
            Assert.That(_travel.Status, Is.EqualTo(SceneTravelStatus.CleanupFailed));
            Assert.That(_player.scene, Is.EqualTo(_target));
            Assert.That(_sourceMap.Content.activeInHierarchy, Is.False);
            Assert.That(_travel.IsBusy, Is.True);
            Assert.That(_travel.TryTravel(_targetMap, Destination()), Is.False);
            _loader.FailUnload = false;
            Assert.That(_travel.RetryCleanup(), Is.True);
            yield return Finish();
            Assert.That(_travel.Status, Is.EqualTo(SceneTravelStatus.Succeeded));
            Assert.That(_source.isLoaded, Is.False);
            Assert.That(_travel.IsBusy, Is.False);
        }

        [UnityTest]
        public IEnumerator RealAdditiveLoadResolvesSerializedEntryAndUnloadsSource()
        {
            _travel.ConfigureLoader(new EditorLoader());
            Assert.That(_travel.TryTravel(_sourceMap, Destination()), Is.True);
            yield return Finish();
            Assert.That(_travel.Status, Is.EqualTo(SceneTravelStatus.Succeeded), _travel.LastError);
            Assert.That(_player.scene.path, Is.EqualTo(TargetPath));
            Assert.That(_player.transform.position, Is.EqualTo(new Vector3(7, 0, 11)));
            Assert.That(_travel.CurrentMap.Map.Session.CurrentRegionId, Is.EqualTo("side"));
            Assert.That(_source.isLoaded, Is.False);
            Assert.That(_target.isLoaded, Is.True, "Unrelated pre-existing map must not be unloaded.");
            Assert.That(_travel.TryTravel(_travel.CurrentMap, Destination()), Is.False,
                "An already loaded/current map must not be loaded a second time.");
        }

        [Test]
        public void InvalidTargetAndPendingWalkNeverStartLoading()
        {
            _loader.Available = false;
            Assert.That(_travel.TryTravel(_sourceMap, Destination()), Is.False);
            Assert.That(_loader.LoadCount, Is.Zero);
            _loader.Available = true;
            var route = new RegionTransition("main", "side", 0, 0, RegionTransitionDirection.Returnable);
            _sourceMap.Map.Session.TryBegin(route, true, true, out var attempt);
            Assert.That(_travel.TryTravel(_sourceMap, Destination()), Is.False);
            Assert.That(_loader.LoadCount, Is.Zero);
            _sourceMap.Map.Session.Cancel(route, attempt);
            Time.timeScale = 0;
            Assert.That(_travel.TryTravel(_sourceMap, Destination()), Is.False);
        }

        [Test]
        public void EntryValidationRejectsDuplicatesUnknownRegionAndActiveContent()
        {
            var entry = _targetMap.ValidateDestination("arrival");
            _targetMap.Configure(_targetMap.Content, _targetMap.Map, new[] { entry, entry });
            Assert.Throws<InvalidOperationException>(() => _targetMap.ValidateDestination("arrival"));
            _targetMap.Configure(_targetMap.Content, _targetMap.Map,
                new[] { new MapSceneEntry("arrival", "missing", entry.Point) });
            Assert.Throws<InvalidOperationException>(() => _targetMap.ValidateDestination("arrival"));
            _targetMap.Configure(_targetMap.Content, _targetMap.Map, new[] { entry });
            _targetMap.Content.SetActive(true);
            Assert.Throws<InvalidOperationException>(() => _targetMap.ValidateDestination("arrival"));
        }

        private sealed class ControlledLoader : IMapSceneLoader
        {
            public readonly TaskCompletionSource<Scene> Pending = new TaskCompletionSource<Scene>();
            public bool Available = true, FailUnload;
            public int LoadCount;
            public bool CanLoad(string path) => Available;
            public Task<Scene> LoadAsync(string path) { LoadCount++; return Pending.Task; }
            public Task UnloadAsync(Scene scene) => FailUnload
                ? Task.FromException(new InvalidOperationException("Simulated cleanup failure"))
                : new UnityMapSceneLoader().UnloadAsync(scene);
        }

        private sealed class EditorLoader : UnityMapSceneLoader
        {
            public override bool CanLoad(string path) => AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null;
            protected override AsyncOperation BeginLoad(string path) =>
                EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Additive));
        }
    }
}
#endif
