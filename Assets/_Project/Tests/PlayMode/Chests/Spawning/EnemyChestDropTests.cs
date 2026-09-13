using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Lifecycle;
using ProjectFirstRun.Enemies.Spawning;
using ProjectFirstRun.Progression;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.UI.Rewards;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Chests.Spawning
{
    public sealed class EnemyChestDropTests
    {
        private readonly List<Object> _objects = new List<Object>();
        private EnemyChestDropSource _source;
        private ChestSpawner _spawner;
        private HealthComponent _health;
        private PlayerBuildController _build;
        private RewardSelectionController _selection;
        private ChestSpawnPlacement _placement;
        private EnemyChestDropProfile _profile;
        private ChestDefinition _chestDefinition;
        private RewardItemPool _pool;
        private EnemyDefinition _enemyDefinition;
        private EnemyController _enemy;
        private EnemyRegistry _registry;
        private GameObject _ground;
        private CountingRandom _random;
        private float _timeScale;

        [SetUp]
        public void SetUp()
        {
            _timeScale = Time.timeScale;
            Time.timeScale = 1f;
            var player = NewObject("Player");
            _health = player.AddComponent<HealthComponent>();
            _build = player.AddComponent<PlayerBuildController>();
            _build.Initialize(PlayerBuildCapacity.CreateDefault());
            _selection = NewObject("Selection").AddComponent<RewardSelectionController>();
            _spawner = NewObject("ChestSpawner").AddComponent<ChestSpawner>();
            InitializeSpawner();
            _pool = Asset<RewardItemPool>();
            var prefab = NewObject("ChestTemplate");
            prefab.transform.position = Vector3.one * 100f;
            prefab.AddComponent<ChestController>();
            var box = prefab.AddComponent<BoxCollider>();
            box.size = new Vector3(1.6f, 1.8f, 1.1f);
            box.center = new Vector3(0f, 0.8f, 0f);
            _chestDefinition = Asset<ChestDefinition>();
            SetField(_chestDefinition, "_stableId", "chest.enemy-test");
            SetField(_chestDefinition, "_rewardItemPool", _pool);
            SetField(_chestDefinition, "_worldPrefab", prefab);
            _profile = Asset<EnemyChestDropProfile>();
            SetField(_profile, "_chanceBasisPoints", 10000);
            SetField(_profile, "_entries", new[] { new WeightedChestEntry(_chestDefinition, 1) });
            _ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _objects.Add(_ground);
            _ground.layer = 7;
            _ground.transform.position = new Vector3(0f, -0.1f, 0f);
            _ground.transform.localScale = new Vector3(40f, 0.2f, 40f);
            _random = new CountingRandom();
            CreateSource();
            _registry = NewObject("Registry").AddComponent<EnemyRegistry>();
            _enemyDefinition = Asset<EnemyDefinition>();
            SetField(_enemyDefinition, "_chestDropProfile", _profile);
            var enemyObject = NewObject("Enemy");
            enemyObject.SetActive(false);
            _enemy = enemyObject.AddComponent<EnemyController>();
            var lifecycle = enemyObject.AddComponent<EnemyDeathLifecycle>();
            SetField(lifecycle, "_destroyDelay", 0f);
            enemyObject.AddComponent<EnemyChestDrop>().Initialize(_source);
            _enemy.Initialize(_enemyDefinition, player.transform, _registry);
            enemyObject.SetActive(true);
            Physics.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var chest in Object.FindObjectsByType<ChestController>(FindObjectsSortMode.None))
                if (chest.Definition == _chestDefinition) Object.DestroyImmediate(chest.gameObject);
            foreach (var pickup in Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None))
                if (!_objects.Contains(pickup.gameObject)) Object.DestroyImmediate(pickup.gameObject);
            for (int i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
            _objects.Clear();
            Time.timeScale = _timeScale;
        }

        [Test]
        public void LethalDeath_QueuesOnceAfterRegistryRemovalWithoutOpeningUI()
        {
            Damage(1f);
            Assert.That(_source.PendingChestCount, Is.Zero);
            Damage(1000f);
            Damage(1000f);
            Assert.That(_source.PendingChestCount, Is.EqualTo(1));
            Assert.That(_source.SpawnedChestCount, Is.Zero);
            Assert.That(_random.Calls, Is.EqualTo(1));
            Assert.That(_registry.ActiveCount, Is.Zero);
            Assert.That(_source.TrySpawnPendingChest(out var chest), Is.True);
            Assert.That(chest.Status, Is.EqualTo(ChestStatus.Available));
            Assert.That(_selection.IsOpen, Is.False);
        }

        [TestCase("no_profile")]
        [TestCase("zero_chance")]
        [TestCase("failed_roll")]
        [TestCase("disable_destroy")]
        public void NoAwardConditions_DoNotQueue(string reason)
        {
            if (reason == "no_profile") SetField(_enemyDefinition, "_chestDropProfile", null);
            if (reason == "zero_chance") SetField(_profile, "_chanceBasisPoints", 0);
            if (reason == "failed_roll")
            {
                SetField(_profile, "_chanceBasisPoints", 1);
                _random.Value = 9999;
            }
            if (reason == "disable_destroy")
            {
                _enemy.gameObject.SetActive(false);
                Object.DestroyImmediate(_enemy.gameObject);
            }
            else Damage(1000f);
            Assert.That(_source.PendingChestCount, Is.Zero);
        }

        [Test]
        public void ReenableBeforeDeath_DoesNotDuplicateSubscription()
        {
            _enemy.gameObject.SetActive(false);
            _enemy.gameObject.SetActive(true);
            Damage(1000f);
            Assert.That(_source.PendingChestCount, Is.EqualTo(1));
            Assert.That(_random.Calls, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator CorpseCleanupAndPlayerMovement_DoNotChangeQueuedDeathLocation()
        {
            _source.enabled = false;
            _enemy.transform.position = new Vector3(8f, 0f, 3f);
            Damage(1000f);
            yield return null;
            Assert.That(_enemy == null, Is.True);
            _health.transform.position = new Vector3(-10f, 0f, 0f);
            SetField(_profile, "_chanceBasisPoints", 0);
            _source.enabled = true;
            Assert.That(_source.TrySpawnPendingChest(out var chest), Is.True);
            Assert.That(chest.transform.position.x, Is.EqualTo(8f).Within(0.01f));
            Assert.That(chest.transform.position.z, Is.EqualTo(3f).Within(0.01f));
            Assert.That(chest.transform.position.y, Is.EqualTo(0.12f).Within(0.01f));
            Assert.That(_random.Calls, Is.EqualTo(1));
        }

        [TestCase("paused")]
        [TestCase("dead")]
        [TestCase("disabled")]
        [TestCase("spawner_disabled")]
        public void Gating_PreservesQueueAndResumesWithoutReroll(string reason)
        {
            if (reason == "paused") Time.timeScale = 0f;
            if (reason == "dead") _health.ApplyDamage(new DamageInfo(1000f, null, Vector3.zero, Vector3.up));
            if (reason == "disabled") _source.enabled = false;
            if (reason == "spawner_disabled") _spawner.enabled = false;
            Damage(1000f);
            Assert.That(_source.TrySpawnPendingChest(out _), Is.False);
            Assert.That(_source.PendingChestCount, Is.EqualTo(1));
            Time.timeScale = 1f;
            _health.ResetHealth();
            _source.enabled = true;
            _spawner.enabled = true;
            Assert.That(_source.TrySpawnPendingChest(out _), Is.True);
            Assert.That(_random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void BlockedDrop_RotatesSoOtherDeathLocationsCanSpawn()
        {
            _source.TryQueueDrop(_profile, new Vector3(100f, 0f, 0f), Vector3.forward);
            _source.TryQueueDrop(_profile, Vector3.zero, Vector3.forward);
            Assert.That(_source.TrySpawnPendingChest(out _), Is.False);
            Assert.That(_source.TrySpawnPendingChest(out _), Is.True);
            Assert.That(_source.PendingChestCount, Is.EqualTo(1));
            _ground.transform.position += Vector3.right * 100f;
            Physics.SyncTransforms();
            Assert.That(_source.TrySpawnPendingChest(out var chest), Is.True);
            Assert.That(chest.transform.position.x, Is.EqualTo(100f).Within(0.01f));
            Assert.That(_random.Calls, Is.EqualTo(2));
        }

        [Test]
        public void SamePositionDrops_UseDistinctUnobstructedLocations()
        {
            _source.TryQueueDrop(_profile, Vector3.zero, Vector3.forward);
            _source.TryQueueDrop(_profile, Vector3.zero, Vector3.forward);
            Assert.That(_source.TrySpawnPendingChest(out var first), Is.True);
            Assert.That(_source.TrySpawnPendingChest(out var second), Is.True);
            Assert.That(Vector3.Distance(first.transform.position, second.transform.position), Is.GreaterThan(1.6f));
        }

        [Test]
        public void LateSpawnerReadiness_PreservesDrop()
        {
            _spawner = NewObject("LateSpawner").AddComponent<ChestSpawner>();
            SetField(_source, "_spawner", _spawner);
            Damage(1000f);
            Assert.That(_source.TrySpawnPendingChest(out _), Is.False);
            InitializeSpawner();
            Assert.That(_source.TrySpawnPendingChest(out _), Is.True);
        }

        [Test]
        public void DeathDropFailure_DoesNotSuppressXPObserver()
        {
            var template = NewObject("XPTemplate").AddComponent<ExperiencePickup>();
            template.transform.position = Vector3.one * 100f;
            template.GetComponent<SphereCollider>().isTrigger = true;
            template.GetComponent<Rigidbody>().isKinematic = true;
            SetField(_enemy.gameObject.AddComponent<EnemyExperienceDrop>(), "_pickupPrefab", template);
            SetField(_enemyDefinition, "_experienceReward", 25);
            SetField(_profile, "_entries", Array.Empty<WeightedChestEntry>());
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("requires weighted entries"));
            Assert.DoesNotThrow(() => Damage(1000f));
            Assert.That(_source.PendingChestCount, Is.Zero);
            Assert.That(Array.FindAll(Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None),
                pickup => pickup != template), Has.Length.EqualTo(1));
        }

        [TestCase("missing_source")]
        [TestCase("missing_component")]
        public void EnabledProfile_RejectsIncompleteSpawnerCompositionBeforeInstantiation(string reason)
        {
            var enemySpawner = NewObject("EnemySpawner").AddComponent<EnemySpawner>();
            if (reason == "missing_component")
            {
                SetField(enemySpawner, "_chestDropSource", _source);
                Object.DestroyImmediate(_enemy.GetComponent<EnemyChestDrop>());
            }
            var request = new EnemySpawnRequest(_enemy, _enemyDefinition, _health.transform,
                _health, _registry, Vector3.zero, Quaternion.identity);
            Assert.Throws<InvalidOperationException>(() => enemySpawner.Spawn(in request));
            Assert.That(_registry.ActiveCount, Is.EqualTo(1));
        }

        [Test]
        public void EnemySpawner_InjectsDropSourceIntoNewInstance()
        {
            var enemySpawner = NewObject("EnemySpawner").AddComponent<EnemySpawner>();
            SetField(enemySpawner, "_chestDropSource", _source);
            _enemy.gameObject.AddComponent<EnemyAttackController>();
            var request = new EnemySpawnRequest(_enemy, _enemyDefinition, _health.transform,
                _health, _registry, new Vector3(4f, 0f, 0f), Quaternion.identity);
            var spawned = enemySpawner.Spawn(in request);
            _objects.Add(spawned.Instance);
            spawned.EnemyController.Health.ApplyDamage(new DamageInfo(1000f, null, Vector3.zero, Vector3.forward));
            Assert.That(_source.PendingChestCount, Is.EqualTo(1));
        }

        [Test]
        public void ReinitializationAndNonFinitePositions_AreRejected()
        {
            Assert.Throws<InvalidOperationException>(() => _source.Initialize(_health, _spawner, _placement, _random));
            Assert.Throws<InvalidOperationException>(() => _enemy.GetComponent<EnemyChestDrop>().Initialize(_source));
            Assert.Throws<ArgumentException>(() => _source.TryQueueDrop(_profile, Vector3.one * float.NaN, Vector3.forward));
            Assert.That(_source.PendingChestCount, Is.Zero);
            Assert.That(_random.Calls, Is.Zero);
        }

        [UnityTest]
        public IEnumerator AutomaticProcessing_DrainsQueuedDeaths()
        {
            Damage(1000f);
            yield return WaitFor(() => _source.PendingChestCount == 0);
            Assert.That(_source.SpawnedChestCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator SpawnFailure_KeepsSelectionAndDisablesUntilFixed()
        {
            Damage(1000f);
            SetField(_chestDefinition, "_rewardItemPool", null);
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("requires a reward item pool"));
            yield return WaitFor(() => !_source.enabled);
            Assert.That(_source.PendingChestCount, Is.EqualTo(1));
            SetField(_chestDefinition, "_rewardItemPool", _pool);
            _source.enabled = true;
            yield return WaitFor(() => _source.PendingChestCount == 0);
            Assert.That(_source.SpawnedChestCount, Is.EqualTo(1));
            Assert.That(_random.Calls, Is.EqualTo(1));
        }

        private static IEnumerator WaitFor(Func<bool> condition)
        {
            float deadline = Time.realtimeSinceStartup + 2f;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(condition(), Is.True, "Timed out waiting for source processing.");
        }
        private void Damage(float amount) => _enemy.Health.ApplyDamage(new DamageInfo(amount, null, Vector3.zero, Vector3.forward));
        private void InitializeSpawner() =>
            _spawner.Initialize(_build, _selection, new RewardOfferGenerator(new RewardCandidateFilter(), new UnityRandomSource()));
        private void CreateSource()
        {
            _source = NewObject("Enemy chest source").AddComponent<EnemyChestDropSource>();
            _placement = _source.GetComponent<ChestSpawnPlacement>();
            _source.Initialize(_health, _spawner, _placement, _random);
        }
        private GameObject NewObject(string name)
        {
            var item = new GameObject(name);
            _objects.Add(item);
            return item;
        }
        private T Asset<T>() where T : ScriptableObject
        {
            T item = ScriptableObject.CreateInstance<T>();
            _objects.Add(item);
            return item;
        }
        private sealed class CountingRandom : IRandomSource
        {
            public int Calls;
            public int Value;
            public int Next(int minInclusive, int maxExclusive) { Calls++; return Value; }
        }
        private static void SetField(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
