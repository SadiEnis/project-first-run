using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Progression;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.UI.Rewards;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Chests.Spawning
{
    public sealed class LevelUpChestSourceTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private PlayerExperienceController _xp;
        private HealthComponent _health;
        private ChestSpawner _spawner;
        private PlayerBuildController _build;
        private RewardSelectionController _selection;
        private ChestDefinition _definition;
        private RewardItemPool _pool;
        private GameObject _prefab;
        private GameObject _ground;
        private LevelUpChestSource _source;
        private ChestSpawnPlacement _placement;
        private float _previousTimeScale;

        [SetUp]
        public void SetUp()
        {
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 1f;
            GameObject player = NewObject("Player");
            _xp = player.AddComponent<PlayerExperienceController>();
            _xp.Initialize(new ExperienceState(new ExperienceCurve(100, 50)));
            _health = player.AddComponent<HealthComponent>();
            _build = player.AddComponent<PlayerBuildController>();
            _build.Initialize(PlayerBuildCapacity.CreateDefault());
            _selection = NewObject("Selection").AddComponent<RewardSelectionController>();
            _spawner = NewObject("Spawner").AddComponent<ChestSpawner>();
            InitializeSpawner(_spawner);
            _pool = ScriptableObject.CreateInstance<RewardItemPool>();
            _prefab = NewObject("ChestTemplate");
            _prefab.transform.position = Vector3.one * 100f;
            _prefab.AddComponent<ChestController>();
            BoxCollider box = _prefab.AddComponent<BoxCollider>();
            box.size = new Vector3(1.6f, 1.8f, 1.1f);
            box.center = new Vector3(0f, 0.8f, 0f);
            _definition = ScriptableObject.CreateInstance<ChestDefinition>();
            SetField(_definition, "_stableId", "chest.level-test");
            SetField(_definition, "_rewardItemPool", _pool);
            SetField(_definition, "_worldPrefab", _prefab);
            _ground = Cube("Ground", new Vector3(0f, -0.1f, 0f), new Vector3(40f, 0.2f, 40f), 7);
            CreateSource();
            Physics.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            if (_source != null) Object.DestroyImmediate(_source.gameObject);
            foreach (ChestController chest in Object.FindObjectsByType<ChestController>(FindObjectsSortMode.None))
                if (chest.Definition == _definition) Object.DestroyImmediate(chest.gameObject);
            foreach (GameObject item in _objects)
                if (item != null) Object.DestroyImmediate(item);
            _objects.Clear();
            Object.DestroyImmediate(_definition);
            Object.DestroyImmediate(_pool);
            Time.timeScale = _previousTimeScale;
        }

        [Test]
        public void InitialAndSubthresholdXP_DoNotSpawn()
        {
            _xp.GainExperience(99);
            Assert.That(_source.PendingChestCount, Is.Zero);
            Assert.That(_source.TrySpawnPendingChest(out _), Is.False);
        }

        [Test]
        public void MultiLevelGain_QueuesThenSpawnsDistinctChestsWithoutOpeningUI()
        {
            _xp.GainExperience(500);
            Assert.That(_source.PendingChestCount, Is.EqualTo(3));
            Assert.That(_source.SpawnedChestCount, Is.Zero, "XP callbacks only enqueue.");
            var positions = new HashSet<Vector3>();
            for (int i = 0; i < 3; i++)
            {
                Assert.That(_source.TrySpawnPendingChest(out ChestController chest), Is.True);
                Assert.That(chest.IsInitialized, Is.True);
                Assert.That(chest.Status, Is.EqualTo(ChestStatus.Available));
                Assert.That(positions.Add(chest.transform.position), Is.True);
            }
            Assert.That(_source.PendingChestCount, Is.Zero);
            Assert.That(_source.SpawnedChestCount, Is.EqualTo(3));
            Assert.That(_source.TrySpawnPendingChest(out _), Is.False);
            Assert.That(_selection.IsOpen, Is.False);
        }

        [TestCase("paused")]
        [TestCase("dead")]
        [TestCase("source_disabled")]
        [TestCase("spawner_disabled")]
        public void IneligibleProcessing_PreservesEarnedChest(string reason)
        {
            _xp.GainExperience(100);
            if (reason == "paused") Time.timeScale = 0f;
            if (reason == "dead") _health.ApplyDamage(new DamageInfo(1000f, null, Vector3.zero, Vector3.up));
            if (reason == "source_disabled") _source.enabled = false;
            if (reason == "spawner_disabled") _spawner.enabled = false;
            Assert.That(_source.TrySpawnPendingChest(out _), Is.False);
            Assert.That(_source.PendingChestCount, Is.EqualTo(1));
            Time.timeScale = 1f;
            _health.ResetHealth();
            _source.enabled = true;
            _spawner.enabled = true;
            Assert.That(_source.TrySpawnPendingChest(out _), Is.True);
            Assert.That(_source.PendingChestCount, Is.Zero);
        }

        [Test]
        public void DisabledPeriod_ReconcilesLevelsOnceOnEnable()
        {
            _source.enabled = false;
            _xp.GainExperience(500);
            _source.enabled = true;
            Assert.That(_source.PendingChestCount, Is.EqualTo(3));
            _source.enabled = false;
            _source.enabled = true;
            Assert.That(_source.PendingChestCount, Is.EqualTo(3));
        }

        [Test]
        public void LateSpawnerReadiness_DoesNotLoseLevelUps()
        {
            Object.DestroyImmediate(_source.gameObject);
            _spawner = NewObject("Uninitialized spawner").AddComponent<ChestSpawner>();
            CreateSource();
            _xp.GainExperience(100);
            Assert.That(_source.TrySpawnPendingChest(out _), Is.False);
            InitializeSpawner(_spawner);
            Assert.That(_source.TrySpawnPendingChest(out _), Is.True);
        }

        [Test]
        public void BindingExistingRun_DoesNotRetroactivelyRewardOldLevels()
        {
            Object.DestroyImmediate(_source.gameObject);
            _xp.GainExperience(500);
            CreateSource();
            Assert.That(_source.PendingChestCount, Is.Zero);
            _xp.GainExperience(200);
            Assert.That(_source.PendingChestCount, Is.EqualTo(1));
        }

        [Test]
        public void Reinitialization_IsRejectedWithoutReplacingPendingState()
        {
            _xp.GainExperience(100);
            Assert.Throws<InvalidOperationException>(() =>
                _source.Initialize(_xp, _health, _spawner, _definition, _placement));
            Assert.That(_source.PendingChestCount, Is.EqualTo(1));
        }

        [Test]
        public void BindingBeforeXPInitialization_ObservesFirstLevelGain()
        {
            Object.DestroyImmediate(_source.gameObject);
            GameObject player = NewObject("Late XP player");
            _xp = player.AddComponent<PlayerExperienceController>();
            _health = player.AddComponent<HealthComponent>();
            CreateSource();
            Assert.That(_source.TrySpawnPendingChest(out _), Is.False);
            _xp.Initialize(new ExperienceState(new ExperienceCurve(100, 50)));
            _xp.GainExperience(100);
            Assert.That(_source.PendingChestCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator RuntimeBootstrap_ReadiesSpawnerWithoutDevelopmentFixture()
        {
            Object.DestroyImmediate(_source.gameObject);
            _spawner = NewObject("Runtime spawner").AddComponent<ChestSpawner>();
            CreateSource();
            GameObject root = NewObject("Runtime bootstrap");
            root.SetActive(false);
            ChestSpawnerBootstrap bootstrap = root.AddComponent<ChestSpawnerBootstrap>();
            SetField(bootstrap, "_spawner", _spawner);
            SetField(bootstrap, "_playerBuild", _build);
            SetField(bootstrap, "_selection", _selection);
            root.SetActive(true);
            _xp.GainExperience(100);
            float deadline = Time.realtimeSinceStartup + 2f;
            while (_source.PendingChestCount > 0 && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(_spawner.IsInitialized, Is.True);
            Assert.That(_source.SpawnedChestCount, Is.EqualTo(1));
        }

        [Test]
        public void NoGround_PreservesRewardAndRetriesWhenGroundBecomesAvailable()
        {
            _ground.SetActive(false);
            _xp.GainExperience(100);
            Assert.That(_source.TrySpawnPendingChest(out _), Is.False);
            Assert.That(_source.PendingChestCount, Is.EqualTo(1));
            _ground.SetActive(true);
            Physics.SyncTransforms();
            Assert.That(_source.TrySpawnPendingChest(out _), Is.True);
        }

        [Test]
        public void AllPositionsBlocked_RetainsRewardUntilPlayerMoves()
        {
            Cube("Blocked region", Vector3.up, new Vector3(15f, 2f, 15f), 0);
            Physics.SyncTransforms();
            _xp.GainExperience(100);
            Assert.That(_source.TrySpawnPendingChest(out _), Is.False);
            _xp.transform.position = Vector3.right * 12f;
            Physics.SyncTransforms();
            Assert.That(_source.TrySpawnPendingChest(out ChestController chest), Is.True);
            Assert.That(chest.transform.position.x, Is.GreaterThan(8f));
        }

        [Test]
        public void SpawnValidationFailure_DoesNotConsumeEntitlement()
        {
            _xp.GainExperience(100);
            SetField(_definition, "_rewardItemPool", null);
            Assert.Throws<InvalidOperationException>(() => _source.TrySpawnPendingChest(out _));
            Assert.That(_source.PendingChestCount, Is.EqualTo(1));
            SetField(_definition, "_rewardItemPool", _pool);
            Assert.That(_source.TrySpawnPendingChest(out _), Is.True);
        }

        [Test]
        public void Placement_RejectsUnsupportedFootprintAtGroundEdge()
        {
            _ground.transform.position = new Vector3(0f, -0.1f, 2.5f);
            _ground.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            Physics.SyncTransforms();
            Assert.That(_placement.TryFind(_xp.transform, _prefab, out _, out _), Is.False);
        }

        [Test]
        public void Placement_AvoidsWallOnDirectPathAndIgnoresTriggers()
        {
            GameObject wall = Cube("Wall", new Vector3(0f, 1f, 1.25f), new Vector3(1f, 2f, 0.4f), 0);
            Physics.SyncTransforms();
            Assert.That(_placement.TryFind(_xp.transform, _prefab, out Vector3 diverted, out _), Is.True);
            Assert.That(Mathf.Abs(diverted.x), Is.GreaterThan(0.1f));
            wall.GetComponent<BoxCollider>().isTrigger = true;
            Physics.SyncTransforms();
            Assert.That(_placement.TryFind(_xp.transform, _prefab, out Vector3 straight, out _), Is.True);
            Assert.That(straight.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(straight.z, Is.EqualTo(2.5f).Within(0.001f));
            Assert.That(straight.y, Is.EqualTo(0.12f).Within(0.001f));
        }

        [Test]
        public void Placement_RejectsMissingRootBoxCollider()
        {
            Object.DestroyImmediate(_prefab.GetComponent<BoxCollider>());
            Assert.Throws<InvalidOperationException>(() => _placement.ValidatePrefab(_prefab));
        }

        [UnityTest]
        public IEnumerator AutomaticProcessing_DrainsMultiLevelQueue()
        {
            _xp.GainExperience(500);
            float deadline = Time.realtimeSinceStartup + 2f;
            while (_source.PendingChestCount > 0 && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(_source.PendingChestCount, Is.Zero);
            Assert.That(_source.SpawnedChestCount, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator AutomaticSpawnFailure_DisablesProcessingWithoutLosingReward()
        {
            SetField(_definition, "_rewardItemPool", null);
            LogAssert.Expect(LogType.Exception,
                new System.Text.RegularExpressions.Regex("requires a reward item pool"));
            Assert.DoesNotThrow(() => _xp.GainExperience(100));
            float deadline = Time.realtimeSinceStartup + 2f;
            while (_source.enabled && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(_source.enabled, Is.False);
            Assert.That(_source.PendingChestCount, Is.EqualTo(1));
            SetField(_definition, "_rewardItemPool", _pool);
            _source.enabled = true;
            deadline = Time.realtimeSinceStartup + 2f;
            while (_source.PendingChestCount > 0 && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(_source.SpawnedChestCount, Is.EqualTo(1));
        }

        private void CreateSource()
        {
            _source = NewObject("Level-up source").AddComponent<LevelUpChestSource>();
            _placement = _source.GetComponent<ChestSpawnPlacement>();
            _source.Initialize(_xp, _health, _spawner, _definition, _placement);
        }

        private void InitializeSpawner(ChestSpawner spawner) =>
            spawner.Initialize(_build, _selection, new RewardOfferGenerator(new RewardCandidateFilter(), new UnityRandomSource()));

        private GameObject NewObject(string name)
        {
            var item = new GameObject(name);
            _objects.Add(item);
            return item;
        }

        private GameObject Cube(string name, Vector3 position, Vector3 scale, int layer)
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = name;
            item.layer = layer;
            item.transform.position = position;
            item.transform.localScale = scale;
            _objects.Add(item);
            return item;
        }

        private static void SetField(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
