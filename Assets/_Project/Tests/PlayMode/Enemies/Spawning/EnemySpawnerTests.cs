using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace ProjectFirstRun.Tests.PlayMode.Enemies.Spawning
{
    public sealed class EnemySpawnerTests
    {
        private readonly List<GameObject> _spawnedInstances =
            new List<GameObject>();

        private GameObject _spawnerObject;
        private GameObject _enemyPrefabObject;
        private GameObject _enemyWithoutAttackPrefabObject;
        private GameObject _targetObject;
        private GameObject _registryObject;

        private EnemySpawner _spawner;
        private EnemyController _enemyPrefab;
        private EnemyController _enemyWithoutAttackPrefab;
        private EnemyDefinition _definition;
        private HealthComponent _targetHealth;
        private EnemyRegistry _registry;

        [SetUp]
        public void SetUp()
        {
            _spawnerObject =
                new GameObject("EnemySpawner_Test");

            _spawner =
                _spawnerObject.AddComponent<EnemySpawner>();

            _targetObject =
                new GameObject("EnemySpawner_Target");

            _targetObject.transform.position =
                Vector3.zero;

            _targetHealth =
                _targetObject.AddComponent<HealthComponent>();

            _targetHealth.Initialize(100f);

            _registryObject =
                new GameObject("EnemySpawner_Registry");

            _registry =
                _registryObject.AddComponent<EnemyRegistry>();

            _definition =
                ScriptableObject.CreateInstance<EnemyDefinition>();

            _enemyPrefab =
                CreateEnemyPrefab(
                    "EnemySpawner_ValidPrefab",
                    includeAttackController: true,
                    out _enemyPrefabObject);

            _enemyWithoutAttackPrefab =
                CreateEnemyPrefab(
                    "EnemySpawner_MissingAttackPrefab",
                    includeAttackController: false,
                    out _enemyWithoutAttackPrefabObject);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject instance in _spawnedInstances)
            {
                DestroyImmediateIfExists(instance);
            }

            _spawnedInstances.Clear();

            DestroyRemainingInstance(
                $"{_enemyPrefabObject.name}_Instance");

            DestroyRemainingInstance(
                $"{_enemyWithoutAttackPrefabObject.name}_Instance");

            DestroyImmediateIfExists(_enemyPrefabObject);
            DestroyImmediateIfExists(_enemyWithoutAttackPrefabObject);
            DestroyImmediateIfExists(_targetObject);
            DestroyImmediateIfExists(_registryObject);
            DestroyImmediateIfExists(_spawnerObject);

            if (_definition != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    _definition);
            }
        }

        [Test]
        public void Spawn_WithValidRequest_ReturnsInitializedControllers()
        {
            EnemySpawnResult result =
                SpawnValidEnemy(
                    new Vector3(3f, 0f, 2f),
                    Quaternion.identity);

            Assert.That(
                result.Instance,
                Is.Not.Null);

            Assert.That(
                result.EnemyController,
                Is.Not.Null);

            Assert.That(
                result.AttackController,
                Is.Not.Null);

            Assert.That(
                result.Instance,
                Is.SameAs(result.EnemyController.gameObject));

            Assert.That(
                result.Instance,
                Is.SameAs(result.AttackController.gameObject));

            Assert.That(
                result.EnemyController.IsInitialized,
                Is.True);

            Assert.That(
                result.AttackController.IsInitialized,
                Is.True);

            Assert.That(
                result.EnemyController.Definition,
                Is.SameAs(_definition));

            Assert.That(
                result.EnemyController.Target,
                Is.SameAs(_targetObject.transform));

            Assert.That(
                result.AttackController.Target,
                Is.SameAs(_targetObject.transform));
        }

        [Test]
        public void Spawn_WithValidRequest_UsesRequestedPositionAndRotation()
        {
            Vector3 requestedPosition =
                new Vector3(4f, 0.5f, -3f);

            Quaternion requestedRotation =
                Quaternion.Euler(0f, 135f, 0f);

            EnemySpawnResult result =
                SpawnValidEnemy(
                    requestedPosition,
                    requestedRotation);

            Assert.That(
                result.Instance.transform.position,
                Is.EqualTo(requestedPosition));

            Assert.That(
                Quaternion.Angle(
                    result.Instance.transform.rotation,
                    requestedRotation),
                Is.LessThan(0.01f));
        }

        [Test]
        public void Spawn_WithValidRequest_RegistersEnemy()
        {
            EnemySpawnResult result =
                SpawnValidEnemy(
                    new Vector3(3f, 0f, 0f),
                    Quaternion.identity);

            Assert.That(
                _registry.ActiveCount,
                Is.EqualTo(1));

            Assert.That(
                _registry.ActiveEnemies,
                Does.Contain(result.EnemyController));
        }

        [Test]
        public void Spawn_Twice_CreatesDistinctRegisteredInstances()
        {
            EnemySpawnResult firstResult =
                SpawnValidEnemy(
                    new Vector3(3f, 0f, 0f),
                    Quaternion.identity);

            EnemySpawnResult secondResult =
                SpawnValidEnemy(
                    new Vector3(-3f, 0f, 0f),
                    Quaternion.identity);

            Assert.That(
                firstResult.Instance,
                Is.Not.SameAs(secondResult.Instance));

            Assert.That(
                firstResult.EnemyController,
                Is.Not.SameAs(secondResult.EnemyController));

            Assert.That(
                _registry.ActiveCount,
                Is.EqualTo(2));

            Assert.That(
                _registry.ActiveEnemies,
                Does.Contain(firstResult.EnemyController));

            Assert.That(
                _registry.ActiveEnemies,
                Does.Contain(secondResult.EnemyController));
        }

        [UnityTest]
        public IEnumerator SpawnedEnemy_WhenTargetIsInRange_DamagesTarget()
        {
            SpawnValidEnemy(
                new Vector3(1f, 0f, 0f),
                Quaternion.identity);

            Assert.That(
                _targetHealth.CurrentHealth,
                Is.EqualTo(100f));

            yield return null;

            Assert.That(
                _targetHealth.CurrentHealth,
                Is.EqualTo(90f));
        }

        [Test]
        public void SpawnedEnemy_WhenKilled_UnregistersFromRegistry()
        {
            EnemySpawnResult result =
                SpawnValidEnemy(
                    new Vector3(3f, 0f, 0f),
                    Quaternion.identity);

            Assert.That(
                _registry.ActiveCount,
                Is.EqualTo(1));

            DamageInfo lethalDamage =
                new DamageInfo(
                    result.EnemyController.Health.MaximumHealth,
                    source: null,
                    result.Instance.transform.position,
                    Vector3.forward);

            result.EnemyController.Health.ApplyDamage(
                in lethalDamage);

            Assert.That(
                result.EnemyController.IsDead,
                Is.True);

            Assert.That(
                _registry.ActiveCount,
                Is.EqualTo(0));
        }

        [Test]
        public void Spawn_WithDefaultRequest_ThrowsBeforeInstantiation()
        {
            int enemyCountBeforeSpawn =
                CountAllEnemyControllers();

            EnemySpawnRequest request =
                default;

            Assert.That(
                () =>
                {
                    _ = _spawner.Spawn(in request);
                },
                Throws.ArgumentNullException);

            Assert.That(
                CountAllEnemyControllers(),
                Is.EqualTo(enemyCountBeforeSpawn));
        }

        [Test]
        public void Spawn_WhenDamageableWasDestroyed_ThrowsBeforeInstantiation()
        {
            EnemySpawnRequest request =
                CreateRequest(
                    _enemyPrefab,
                    new Vector3(2f, 0f, 0f),
                    Quaternion.identity);

            int enemyCountBeforeSpawn =
                CountAllEnemyControllers();

            UnityEngine.Object.DestroyImmediate(
                _targetHealth);

            Assert.That(
                () =>
                {
                    _ = _spawner.Spawn(in request);
                },
                Throws.ArgumentException);

            Assert.That(
                CountAllEnemyControllers(),
                Is.EqualTo(enemyCountBeforeSpawn));
        }

        [UnityTest]
        public IEnumerator Spawn_WithMissingAttackController_CleansFailedInstance()
        {
            EnemySpawnRequest request =
                CreateRequest(
                    _enemyWithoutAttackPrefab,
                    new Vector3(2f, 0f, 0f),
                    Quaternion.identity);

            string expectedInstanceName =
                $"{_enemyWithoutAttackPrefab.name}_Instance";

            InvalidOperationException exception =
                Assert.Throws<InvalidOperationException>(
                    () =>
                    {
                        _ = _spawner.Spawn(in request);
                    });

            Assert.That(
                exception.Message,
                Does.Contain(nameof(EnemyAttackController)));

            Assert.That(
                _registry.ActiveCount,
                Is.EqualTo(0));

            // Destroy is deferred until the end of the frame in Play Mode.
            yield return null;

            Assert.That(
                CountNamedEnemyInstances(expectedInstanceName),
                Is.EqualTo(0));
        }

        private EnemySpawnResult SpawnValidEnemy(
            Vector3 position,
            Quaternion rotation)
        {
            EnemySpawnRequest request =
                CreateRequest(
                    _enemyPrefab,
                    position,
                    rotation);

            EnemySpawnResult result =
                _spawner.Spawn(in request);

            _spawnedInstances.Add(
                result.Instance);

            return result;
        }

        private EnemySpawnRequest CreateRequest(
            EnemyController prefab,
            Vector3 position,
            Quaternion rotation)
        {
            return new EnemySpawnRequest(
                prefab,
                _definition,
                _targetObject.transform,
                _targetHealth,
                _registry,
                position,
                rotation);
        }

        private static EnemyController CreateEnemyPrefab(
            string objectName,
            bool includeAttackController,
            out GameObject prefabObject)
        {
            prefabObject =
                new GameObject(objectName);

            prefabObject.AddComponent<NavMeshAgent>();
            prefabObject.AddComponent<HealthComponent>();
            prefabObject.AddComponent<EnemyMotor>();

            EnemyController enemyController =
                prefabObject.AddComponent<EnemyController>();

            if (includeAttackController)
            {
                prefabObject.AddComponent<EnemyAttackController>();
            }

            return enemyController;
        }

        private static int CountAllEnemyControllers()
        {
            EnemyController[] controllers =
                UnityEngine.Object.FindObjectsByType<EnemyController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            return controllers.Length;
        }

        private static int CountNamedEnemyInstances(
            string instanceName)
        {
            EnemyController[] controllers =
                UnityEngine.Object.FindObjectsByType<EnemyController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            int count = 0;

            foreach (EnemyController controller in controllers)
            {
                if (controller != null &&
                    controller.gameObject.name == instanceName)
                {
                    count++;
                }
            }

            return count;
        }

        private static void DestroyRemainingInstance(
            string instanceName)
        {
            EnemyController[] controllers =
                UnityEngine.Object.FindObjectsByType<EnemyController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            foreach (EnemyController controller in controllers)
            {
                if (controller != null &&
                    controller.gameObject.name == instanceName)
                {
                    UnityEngine.Object.DestroyImmediate(
                        controller.gameObject);
                }
            }
        }

        private static void DestroyImmediateIfExists(
            GameObject gameObject)
        {
            if (gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    gameObject);
            }
        }
    }
}