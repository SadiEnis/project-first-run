using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using ProjectFirstRun.Waves;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Waves
{
    public sealed class WaveEnemySpawnerTests
    {
        private readonly List<GameObject> _spawnedInstances =
            new List<GameObject>();

        private readonly List<GameObject> _spawnPointObjects =
            new List<GameObject>();

        private readonly List<EnemyWaveDefinition> _waveDefinitions =
            new List<EnemyWaveDefinition>();

        private GameObject _enemySpawnerObject;
        private GameObject _registryObject;
        private GameObject _targetObject;

        private GameObject _firstPrefabObject;
        private GameObject _secondPrefabObject;
        private GameObject _missingAttackPrefabObject;

        private EnemySpawner _enemySpawner;
        private EnemyRegistry _enemyRegistry;
        private HealthComponent _targetHealth;

        private EnemyController _firstPrefab;
        private EnemyController _secondPrefab;
        private EnemyController _missingAttackPrefab;

        private EnemyDefinition _firstDefinition;
        private EnemyDefinition _secondDefinition;

        private Transform _firstSpawnPoint;
        private Transform _secondSpawnPoint;

        private RoundRobinSpawnPointSelector _spawnPointSelector;
        private WaveEnemySpawner _waveEnemySpawner;

        [SetUp]
        public void SetUp()
        {
            _enemySpawnerObject =
                new GameObject("WaveEnemySpawner_Service");

            _enemySpawner =
                _enemySpawnerObject.AddComponent<EnemySpawner>();

            _registryObject =
                new GameObject("WaveEnemySpawner_Registry");

            _enemyRegistry =
                _registryObject.AddComponent<EnemyRegistry>();

            _targetObject =
                new GameObject("WaveEnemySpawner_Target");

            // Prevents spawned enemies from attacking during these tests.
            _targetObject.transform.position =
                new Vector3(1000f, 0f, 1000f);

            _targetHealth =
                _targetObject.AddComponent<HealthComponent>();

            _targetHealth.Initialize(100f);

            _firstDefinition =
                ScriptableObject.CreateInstance<EnemyDefinition>();

            _secondDefinition =
                ScriptableObject.CreateInstance<EnemyDefinition>();

            _firstPrefab =
                CreateEnemyPrefab(
                    "FirstEnemyPrefab",
                    includeAttackController: true,
                    out _firstPrefabObject);

            _secondPrefab =
                CreateEnemyPrefab(
                    "SecondEnemyPrefab",
                    includeAttackController: true,
                    out _secondPrefabObject);

            _missingAttackPrefab =
                CreateEnemyPrefab(
                    "MissingAttackPrefab",
                    includeAttackController: false,
                    out _missingAttackPrefabObject);

            _firstSpawnPoint =
                CreateSpawnPoint(
                    "SpawnPoint_0",
                    new Vector3(2f, 0f, 1f),
                    Quaternion.Euler(0f, 45f, 0f));

            _secondSpawnPoint =
                CreateSpawnPoint(
                    "SpawnPoint_1",
                    new Vector3(-3f, 0f, 4f),
                    Quaternion.Euler(0f, 135f, 0f));

            _spawnPointSelector =
                new RoundRobinSpawnPointSelector(
                    new[]
                    {
                        _firstSpawnPoint,
                        _secondSpawnPoint
                    });

            _waveEnemySpawner =
                new WaveEnemySpawner(
                    _enemySpawner,
                    _enemyRegistry,
                    _targetObject.transform,
                    _targetHealth,
                    _spawnPointSelector);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject instance in _spawnedInstances)
            {
                DestroyImmediateIfExists(instance);
            }

            _spawnedInstances.Clear();

            DestroyNamedInstances(
                $"{_firstPrefabObject.name}_Instance");

            DestroyNamedInstances(
                $"{_secondPrefabObject.name}_Instance");

            DestroyNamedInstances(
                $"{_missingAttackPrefabObject.name}_Instance");

            foreach (EnemyWaveDefinition wave in _waveDefinitions)
            {
                if (wave != null)
                {
                    Object.DestroyImmediate(wave);
                }
            }

            _waveDefinitions.Clear();

            foreach (GameObject spawnPointObject in _spawnPointObjects)
            {
                DestroyImmediateIfExists(spawnPointObject);
            }

            _spawnPointObjects.Clear();

            DestroyImmediateIfExists(_firstPrefabObject);
            DestroyImmediateIfExists(_secondPrefabObject);
            DestroyImmediateIfExists(_missingAttackPrefabObject);
            DestroyImmediateIfExists(_targetObject);
            DestroyImmediateIfExists(_registryObject);
            DestroyImmediateIfExists(_enemySpawnerObject);

            if (_firstDefinition != null)
            {
                Object.DestroyImmediate(_firstDefinition);
            }

            if (_secondDefinition != null)
            {
                Object.DestroyImmediate(_secondDefinition);
            }
        }

        [Test]
        public void Constructor_WithNullEnemySpawner_Throws()
        {
            Assert.That(
                () =>
                    new WaveEnemySpawner(
                        null,
                        _enemyRegistry,
                        _targetObject.transform,
                        _targetHealth,
                        _spawnPointSelector),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNonUnityDamageable_Throws()
        {
            NonUnityDamageable damageable =
                new NonUnityDamageable();

            Assert.That(
                () =>
                    new WaveEnemySpawner(
                        _enemySpawner,
                        _enemyRegistry,
                        _targetObject.transform,
                        damageable,
                        _spawnPointSelector),
                Throws.ArgumentException);
        }

        [Test]
        public void SpawnWave_WithSingleEntry_SpawnsRequestedCount()
        {
            EnemyWaveDefinition wave =
                CreateWave(
                    new EnemyWaveEntry(
                        _firstPrefab,
                        _firstDefinition,
                        3));

            IReadOnlyList<EnemySpawnResult> results =
                SpawnAndTrack(wave);

            Assert.That(
                results.Count,
                Is.EqualTo(3));

            Assert.That(
                _enemyRegistry.ActiveCount,
                Is.EqualTo(3));
        }

        [Test]
        public void SpawnWave_WithMultipleEntries_PreservesEntryOrder()
        {
            EnemyWaveDefinition wave =
                CreateWave(
                    new EnemyWaveEntry(
                        _firstPrefab,
                        _firstDefinition,
                        2),

                    new EnemyWaveEntry(
                        _secondPrefab,
                        _secondDefinition,
                        1));

            IReadOnlyList<EnemySpawnResult> results =
                SpawnAndTrack(wave);

            Assert.That(
                results.Count,
                Is.EqualTo(3));

            Assert.That(
                results[0].EnemyController.Definition,
                Is.SameAs(_firstDefinition));

            Assert.That(
                results[1].EnemyController.Definition,
                Is.SameAs(_firstDefinition));

            Assert.That(
                results[2].EnemyController.Definition,
                Is.SameAs(_secondDefinition));
        }

        [Test]
        public void SpawnWave_UsesSpawnPointsInRoundRobinOrder()
        {
            EnemyWaveDefinition wave =
                CreateWave(
                    new EnemyWaveEntry(
                        _firstPrefab,
                        _firstDefinition,
                        3));

            IReadOnlyList<EnemySpawnResult> results =
                SpawnAndTrack(wave);

            Assert.That(
                results[0].Instance.transform.position,
                Is.EqualTo(_firstSpawnPoint.position));

            Assert.That(
                results[1].Instance.transform.position,
                Is.EqualTo(_secondSpawnPoint.position));

            Assert.That(
                results[2].Instance.transform.position,
                Is.EqualTo(_firstSpawnPoint.position));

            Assert.That(
                Quaternion.Angle(
                    results[0].Instance.transform.rotation,
                    _firstSpawnPoint.rotation),
                Is.LessThan(0.01f));

            Assert.That(
                Quaternion.Angle(
                    results[1].Instance.transform.rotation,
                    _secondSpawnPoint.rotation),
                Is.LessThan(0.01f));
        }

        [Test]
        public void SpawnWave_AcrossCalls_ContinuesRoundRobinSequence()
        {
            EnemyWaveDefinition firstWave =
                CreateWave(
                    new EnemyWaveEntry(
                        _firstPrefab,
                        _firstDefinition,
                        1));

            EnemyWaveDefinition secondWave =
                CreateWave(
                    new EnemyWaveEntry(
                        _firstPrefab,
                        _firstDefinition,
                        1));

            IReadOnlyList<EnemySpawnResult> firstResults =
                SpawnAndTrack(firstWave);

            IReadOnlyList<EnemySpawnResult> secondResults =
                SpawnAndTrack(secondWave);

            Assert.That(
                firstResults[0].Instance.transform.position,
                Is.EqualTo(_firstSpawnPoint.position));

            Assert.That(
                secondResults[0].Instance.transform.position,
                Is.EqualTo(_secondSpawnPoint.position));
        }

        [Test]
        public void SpawnWave_ReturnsInitializedAndRegisteredEnemies()
        {
            EnemyWaveDefinition wave =
                CreateWave(
                    new EnemyWaveEntry(
                        _firstPrefab,
                        _firstDefinition,
                        2));

            IReadOnlyList<EnemySpawnResult> results =
                SpawnAndTrack(wave);

            foreach (EnemySpawnResult result in results)
            {
                Assert.That(
                    result.EnemyController.IsInitialized,
                    Is.True);

                Assert.That(
                    result.AttackController.IsInitialized,
                    Is.True);

                Assert.That(
                    result.EnemyController.Target,
                    Is.SameAs(_targetObject.transform));

                Assert.That(
                    result.AttackController.Target,
                    Is.SameAs(_targetObject.transform));

                Assert.That(
                    _enemyRegistry.ActiveEnemies,
                    Does.Contain(result.EnemyController));
            }
        }

        [Test]
        public void SpawnWave_WithNullDefinition_Throws()
        {
            Assert.That(
                () => _waveEnemySpawner.SpawnWave(null),
                Throws.ArgumentNullException);

            Assert.That(
                _enemyRegistry.ActiveCount,
                Is.EqualTo(0));
        }

        [Test]
        public void SpawnWave_WithInvalidDefinition_ThrowsBeforeInstantiation()
        {
            EnemyWaveDefinition invalidWave =
                CreateWave();

            int enemyCountBeforeSpawn =
                CountEnemyControllers();

            Assert.That(
                () => _waveEnemySpawner.SpawnWave(invalidWave),
                Throws.InvalidOperationException);

            Assert.That(
                CountEnemyControllers(),
                Is.EqualTo(enemyCountBeforeSpawn));

            Assert.That(
                _enemyRegistry.ActiveCount,
                Is.EqualTo(0));
        }

        [Test]
        public void SpawnWave_AfterTargetDamageableDestroyed_ThrowsBeforeInstantiation()
        {
            EnemyWaveDefinition wave =
                CreateWave(
                    new EnemyWaveEntry(
                        _firstPrefab,
                        _firstDefinition,
                        1));

            int enemyCountBeforeSpawn =
                CountEnemyControllers();

            Object.DestroyImmediate(_targetHealth);

            Assert.That(
                () => _waveEnemySpawner.SpawnWave(wave),
                Throws.InvalidOperationException);

            Assert.That(
                CountEnemyControllers(),
                Is.EqualTo(enemyCountBeforeSpawn));

            Assert.That(
                _enemyRegistry.ActiveCount,
                Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator SpawnWave_WhenLaterEntryFails_CleansPartialWave()
        {
            EnemyWaveDefinition wave =
                CreateWave(
                    new EnemyWaveEntry(
                        _firstPrefab,
                        _firstDefinition,
                        1),

                    new EnemyWaveEntry(
                        _missingAttackPrefab,
                        _secondDefinition,
                        1));

            Assert.That(
                () => _waveEnemySpawner.SpawnWave(wave),
                Throws.InvalidOperationException);

            Assert.That(
                _enemyRegistry.ActiveCount,
                Is.EqualTo(0));

            // Destroy is deferred until the end of the frame.
            yield return null;

            Assert.That(
                CountNamedInstances(
                    $"{_firstPrefab.name}_Instance"),
                Is.EqualTo(0));

            Assert.That(
                CountNamedInstances(
                    $"{_missingAttackPrefab.name}_Instance"),
                Is.EqualTo(0));
        }

        private IReadOnlyList<EnemySpawnResult> SpawnAndTrack(
            EnemyWaveDefinition wave)
        {
            IReadOnlyList<EnemySpawnResult> results =
                _waveEnemySpawner.SpawnWave(wave);

            foreach (EnemySpawnResult result in results)
            {
                _spawnedInstances.Add(result.Instance);
            }

            return results;
        }

        private EnemyWaveDefinition CreateWave(
            params EnemyWaveEntry[] entries)
        {
            EnemyWaveDefinition wave =
                ScriptableObject.CreateInstance<EnemyWaveDefinition>();

            wave.name =
                $"TestWave_{_waveDefinitions.Count}";

            SetPrivateField(
                wave,
                "_stableId",
                $"test_wave_{_waveDefinitions.Count}");

            SetPrivateField(
                wave,
                "_displayName",
                $"Test Wave {_waveDefinitions.Count}");

            SetPrivateField(
                wave,
                "_entries",
                new List<EnemyWaveEntry>(
                    entries ?? Array.Empty<EnemyWaveEntry>()));

            _waveDefinitions.Add(wave);

            return wave;
        }

        private Transform CreateSpawnPoint(
            string objectName,
            Vector3 position,
            Quaternion rotation)
        {
            GameObject spawnPointObject =
                new GameObject(objectName);

            spawnPointObject.transform.SetPositionAndRotation(
                position,
                rotation);

            _spawnPointObjects.Add(spawnPointObject);

            return spawnPointObject.transform;
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

        private static void SetPrivateField(
            object target,
            string fieldName,
            object value)
        {
            FieldInfo field =
                target.GetType().GetField(
                    fieldName,
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null,
                $"Field '{fieldName}' could not be found.");

            field.SetValue(target, value);
        }

        private static int CountEnemyControllers()
        {
            return Object
                .FindObjectsByType<EnemyController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Length;
        }

        private static int CountNamedInstances(
            string instanceName)
        {
            EnemyController[] enemies =
                Object.FindObjectsByType<EnemyController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            int count = 0;

            foreach (EnemyController enemy in enemies)
            {
                if (enemy != null &&
                    enemy.gameObject.name == instanceName)
                {
                    count++;
                }
            }

            return count;
        }

        private static void DestroyNamedInstances(
            string instanceName)
        {
            EnemyController[] enemies =
                Object.FindObjectsByType<EnemyController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            foreach (EnemyController enemy in enemies)
            {
                if (enemy != null &&
                    enemy.gameObject.name == instanceName)
                {
                    Object.DestroyImmediate(
                        enemy.gameObject);
                }
            }
        }

        private static void DestroyImmediateIfExists(
            GameObject gameObject)
        {
            if (gameObject != null)
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        private sealed class NonUnityDamageable : IDamageable
        {
            public DamageResult ApplyDamage(
                in DamageInfo damageInfo)
            {
                return DamageResult.Rejected(
                    damageInfo.Amount,
                    100f);
            }
        }
    }
}