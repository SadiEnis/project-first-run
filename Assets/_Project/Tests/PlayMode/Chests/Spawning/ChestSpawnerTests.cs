using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.UI.Rewards;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Chests.Spawning
{
    public sealed class ChestSpawnerTests
    {
        private readonly List<GameObject>
            _spawnedInstances =
                new List<GameObject>();

        private GameObject _spawnerObject;
        private GameObject _buildObject;
        private GameObject _selectionObject;
        private GameObject _worldPrefab;

        private ChestSpawner _spawner;
        private PlayerBuildController _buildController;
        private RewardSelectionController _selectionController;
        private RewardItemPool _rewardItemPool;
        private ChestDefinition _definition;

        [SetUp]
        public void SetUp()
        {
            _spawnerObject =
                new GameObject(
                    "ChestSpawner_Test");

            _spawner =
                _spawnerObject.AddComponent<
                    ChestSpawner>();

            _buildObject =
                new GameObject(
                    "ChestSpawner_Build");

            _buildController =
                _buildObject.AddComponent<
                    PlayerBuildController>();

            _buildController.Initialize(
                PlayerBuildCapacity.CreateDefault());

            _selectionObject =
                new GameObject(
                    "ChestSpawner_Selection");

            _selectionController =
                _selectionObject.AddComponent<
                    RewardSelectionController>();

            _rewardItemPool =
                ScriptableObject.CreateInstance<
                    RewardItemPool>();

            _worldPrefab =
                new GameObject(
                    "ChestSpawner_Prefab");

            _worldPrefab.AddComponent<
                ChestController>();

            _definition =
                CreateDefinition();

            _spawner.Initialize(
                _buildController,
                _selectionController,
                CreateOfferGenerator());
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject instance
                     in _spawnedInstances)
            {
                DestroyImmediateIfExists(
                    instance);
            }

            _spawnedInstances.Clear();

            DestroyImmediateIfExists(
                _worldPrefab);

            DestroyImmediateIfExists(
                _selectionObject);

            DestroyImmediateIfExists(
                _buildObject);

            DestroyImmediateIfExists(
                _spawnerObject);

            if (_definition != null)
            {
                Object.DestroyImmediate(
                    _definition);
            }

            if (_rewardItemPool != null)
            {
                Object.DestroyImmediate(
                    _rewardItemPool);
            }
        }

        [Test]
        public void Spawn_WithValidRequest_ReturnsInitializedChest()
        {
            ChestSpawnResult result =
                Spawn(
                    new Vector3(
                        3f,
                        1f,
                        -2f),
                    Quaternion.identity);

            Assert.That(
                result.Instance,
                Is.SameAs(
                    result.ChestController.gameObject));

            Assert.That(
                result.ChestController.IsInitialized,
                Is.True);

            Assert.That(
                result.ChestController.Definition,
                Is.SameAs(_definition));

            Assert.That(
                result.ChestController.Status,
                Is.EqualTo(ChestStatus.Available));
        }

        [Test]
        public void Spawn_UsesRequestedTransform()
        {
            Vector3 position =
                new Vector3(
                    -4f,
                    0.5f,
                    6f);

            Quaternion rotation =
                Quaternion.Euler(
                    0f,
                    135f,
                    0f);

            ChestSpawnResult result =
                Spawn(
                    position,
                    rotation);

            Assert.That(
                result.Instance.transform.position,
                Is.EqualTo(position));

            Assert.That(
                Quaternion.Angle(
                    result.Instance.transform.rotation,
                    rotation),
                Is.LessThan(0.01f));
        }

        [Test]
        public void Spawn_Twice_CreatesDistinctInstances()
        {
            ChestSpawnResult first =
                Spawn(
                    Vector3.left,
                    Quaternion.identity);

            ChestSpawnResult second =
                Spawn(
                    Vector3.right,
                    Quaternion.identity);

            Assert.That(
                first.Instance,
                Is.Not.SameAs(
                    second.Instance));

            Assert.That(
                first.ChestController,
                Is.Not.SameAs(
                    second.ChestController));
        }

        [Test]
        public void Spawn_BeforeSpawnerInitialized_Throws()
        {
            GameObject uninitializedObject =
                new GameObject(
                    "ChestSpawner_Uninitialized");

            try
            {
                ChestSpawner uninitializedSpawner =
                    uninitializedObject.AddComponent<
                        ChestSpawner>();

                ChestSpawnRequest request =
                    CreateRequest(
                        Vector3.zero,
                        Quaternion.identity);

                Assert.That(
                    () =>
                    {
                        _ = uninitializedSpawner.Spawn(
                            in request);
                    },
                    Throws.InvalidOperationException);
            }
            finally
            {
                DestroyImmediateIfExists(
                    uninitializedObject);
            }
        }

        [Test]
        public void Spawn_WithDefaultRequest_ThrowsBeforeInstantiation()
        {
            ChestSpawnRequest request =
                default;

            Assert.That(
                () =>
                {
                    _ = _spawner.Spawn(
                        in request);
                },
                Throws.ArgumentNullException);
        }

        private ChestSpawnResult Spawn(
            Vector3 position,
            Quaternion rotation)
        {
            ChestSpawnRequest request =
                CreateRequest(
                    position,
                    rotation);

            ChestSpawnResult result =
                _spawner.Spawn(
                    in request);

            _spawnedInstances.Add(
                result.Instance);

            return result;
        }

        private ChestSpawnRequest CreateRequest(
            Vector3 position,
            Quaternion rotation)
        {
            return new ChestSpawnRequest(
                _definition,
                position,
                rotation);
        }

        private ChestDefinition CreateDefinition()
        {
            ChestDefinition definition =
                ScriptableObject.CreateInstance<
                    ChestDefinition>();

            SetPrivateField(
                definition,
                "_stableId",
                "chest.spawner-test");

            SetPrivateField(
                definition,
                "_rewardItemPool",
                _rewardItemPool);

            SetPrivateField(
                definition,
                "_requestedChoiceCount",
                3);

            SetPrivateField(
                definition,
                "_worldPrefab",
                _worldPrefab);

            return definition;
        }

        private static RewardOfferGenerator
            CreateOfferGenerator()
        {
            return new RewardOfferGenerator(
                new RewardCandidateFilter(),
                new FirstRandomSource());
        }

        private static void SetPrivateField(
            ChestDefinition definition,
            string fieldName,
            object value)
        {
            FieldInfo field =
                typeof(ChestDefinition)
                    .GetField(
                        fieldName,
                        BindingFlags.Instance |
                        BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null);

            field.SetValue(
                definition,
                value);
        }

        private static void DestroyImmediateIfExists(
            GameObject gameObject)
        {
            if (gameObject != null)
            {
                Object.DestroyImmediate(
                    gameObject);
            }
        }

        private sealed class FirstRandomSource :
            IRandomSource
        {
            public int Next(
                int minInclusive,
                int maxExclusive)
            {
                return minInclusive;
            }
        }
    }
}
