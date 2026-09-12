using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using ProjectFirstRun.Waves;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Waves
{
    public abstract class WaveControllerTestFixture
    {
        private readonly List<GameObject> _spawnedInstances = new();
        private readonly List<GameObject> _spawnPointObjects = new();
        private readonly List<GameObject> _additionalEnemyObjects = new();

        private readonly List<EnemyWaveDefinition> _waveDefinitions = new();
        private readonly List<ArenaWaveDefinition> _arenaDefinitions = new();

        private GameObject _controllerObject;
        private GameObject _enemySpawnerObject;
        private GameObject _registryObject;
        private GameObject _targetObject;

        private GameObject _validPrefabObject;
        private GameObject _missingAttackPrefabObject;

        protected WaveController Controller { get; private set; }
        protected WaveEnemySpawner WaveEnemySpawner { get; private set; }
        protected EnemyRegistry Registry { get; private set; }
        protected HealthComponent TargetHealth { get; private set; }

        protected EnemyController ValidPrefab { get; private set; }
        protected EnemyController MissingAttackPrefab { get; private set; }

        protected EnemyDefinition FirstDefinition { get; private set; }
        protected EnemyDefinition SecondDefinition { get; private set; }

        protected IReadOnlyList<GameObject> SpawnedInstances =>
            _spawnedInstances;

        protected Transform Target =>
            _targetObject.transform;

        [SetUp]
        public void SetUp()
        {
            CreateSceneDependencies();
            CreateEnemyDefinitions();
            CreateEnemyPrefabs();
            CreateWaveServices();
            CreateWaveController();
        }

        [TearDown]
        public void TearDown()
        {
            DestroyImmediateIfExists(_controllerObject);

            foreach (GameObject instance in _spawnedInstances)
            {
                DestroyImmediateIfExists(instance);
            }

            _spawnedInstances.Clear();

            DestroyNamedInstances($"{ValidPrefab.name}_Instance");
            DestroyNamedInstances($"{MissingAttackPrefab.name}_Instance");

            foreach (GameObject enemyObject in _additionalEnemyObjects)
            {
                DestroyImmediateIfExists(enemyObject);
            }

            _additionalEnemyObjects.Clear();

            foreach (ArenaWaveDefinition arena in _arenaDefinitions)
            {
                if (arena != null)
                {
                    Object.DestroyImmediate(arena);
                }
            }

            foreach (EnemyWaveDefinition wave in _waveDefinitions)
            {
                if (wave != null)
                {
                    Object.DestroyImmediate(wave);
                }
            }

            _arenaDefinitions.Clear();
            _waveDefinitions.Clear();

            foreach (GameObject spawnPointObject in _spawnPointObjects)
            {
                DestroyImmediateIfExists(spawnPointObject);
            }

            _spawnPointObjects.Clear();

            DestroyImmediateIfExists(_validPrefabObject);
            DestroyImmediateIfExists(_missingAttackPrefabObject);
            DestroyImmediateIfExists(_targetObject);
            DestroyImmediateIfExists(_registryObject);
            DestroyImmediateIfExists(_enemySpawnerObject);

            if (FirstDefinition != null)
            {
                Object.DestroyImmediate(FirstDefinition);
            }

            if (SecondDefinition != null)
            {
                Object.DestroyImmediate(SecondDefinition);
            }
        }

        protected EnemyWaveEntry CreateEntry(
            EnemyController prefab,
            EnemyDefinition definition,
            int count)
        {
            return new EnemyWaveEntry(
                prefab,
                definition,
                count);
        }

        protected EnemyWaveDefinition CreateWave(
            params EnemyWaveEntry[] entries)
        {
            int waveIndex = _waveDefinitions.Count;

            EnemyWaveDefinition wave =
                ScriptableObject.CreateInstance<EnemyWaveDefinition>();

            wave.name = $"TestWave_{waveIndex}";

            SetPrivateField(
                wave,
                "_stableId",
                $"test_wave_{waveIndex}");

            SetPrivateField(
                wave,
                "_displayName",
                $"Test Wave {waveIndex}");

            SetPrivateField(
                wave,
                "_entries",
                new List<EnemyWaveEntry>(
                    entries ?? Array.Empty<EnemyWaveEntry>()));

            _waveDefinitions.Add(wave);

            return wave;
        }

        protected EnemyWaveDefinition CreateValidWave(
            int enemyCount)
        {
            return CreateWave(
                CreateEntry(
                    ValidPrefab,
                    FirstDefinition,
                    enemyCount));
        }

        protected ArenaWaveDefinition CreateArena(
            params EnemyWaveDefinition[] waves)
        {
            int arenaIndex = _arenaDefinitions.Count;

            ArenaWaveDefinition arena =
                ScriptableObject.CreateInstance<ArenaWaveDefinition>();

            arena.name = $"TestArena_{arenaIndex}";

            SetPrivateField(
                arena,
                "_stableId",
                $"test_arena_{arenaIndex}");

            SetPrivateField(
                arena,
                "_waves",
                new List<EnemyWaveDefinition>(
                    waves ?? Array.Empty<EnemyWaveDefinition>()));

            _arenaDefinitions.Add(arena);

            return arena;
        }

        protected void InitializeController(
            ArenaWaveDefinition arena)
        {
            Controller.Initialize(
                arena,
                WaveEnemySpawner);
        }

        protected EnemyController CreateUnrelatedEnemy(
            string objectName = "UnrelatedEnemy")
        {
            GameObject enemyObject =
                new GameObject(objectName);

            _additionalEnemyObjects.Add(enemyObject);

            enemyObject.AddComponent<NavMeshAgent>();
            enemyObject.AddComponent<HealthComponent>();
            enemyObject.AddComponent<EnemyMotor>();

            EnemyController enemy =
                enemyObject.AddComponent<EnemyController>();

            enemy.Initialize(
                FirstDefinition,
                Target,
                Registry);

            return enemy;
        }

        protected static void KillEnemy(
            EnemyController enemy)
        {
            DamageInfo lethalDamage = new DamageInfo(
                enemy.Health.MaximumHealth,
                null,
                enemy.transform.position,
                Vector3.forward);

            enemy.Health.ApplyDamage(
                in lethalDamage);
        }

        protected static void ApplyDamage(
            EnemyController enemy,
            float amount)
        {
            DamageInfo damageInfo = new DamageInfo(
                amount,
                null,
                enemy.transform.position,
                Vector3.forward);

            enemy.Health.ApplyDamage(
                in damageInfo);
        }

        protected static int CountNamedInstances(
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

        private void CreateSceneDependencies()
        {
            _enemySpawnerObject =
                new GameObject("WaveController_EnemySpawner");

            _registryObject =
                new GameObject("WaveController_Registry");

            _targetObject =
                new GameObject("WaveController_Target");

            _targetObject.transform.position =
                new Vector3(1000f, 0f, 1000f);

            Registry =
                _registryObject.AddComponent<EnemyRegistry>();

            TargetHealth =
                _targetObject.AddComponent<HealthComponent>();

            TargetHealth.Initialize(100f);
        }

        private void CreateEnemyDefinitions()
        {
            FirstDefinition =
                ScriptableObject.CreateInstance<EnemyDefinition>();

            SecondDefinition =
                ScriptableObject.CreateInstance<EnemyDefinition>();
        }

        private void CreateEnemyPrefabs()
        {
            ValidPrefab = CreateEnemyPrefab(
                "WaveController_ValidPrefab",
                includeAttackController: true,
                out _validPrefabObject);

            MissingAttackPrefab = CreateEnemyPrefab(
                "WaveController_MissingAttackPrefab",
                includeAttackController: false,
                out _missingAttackPrefabObject);
        }

        private void CreateWaveServices()
        {
            EnemySpawner enemySpawner =
                _enemySpawnerObject.AddComponent<EnemySpawner>();

            Transform firstSpawnPoint = CreateSpawnPoint(
                "SpawnPoint_0",
                new Vector3(2f, 0f, 0f));

            Transform secondSpawnPoint = CreateSpawnPoint(
                "SpawnPoint_1",
                new Vector3(-2f, 0f, 0f));

            RoundRobinSpawnPointSelector selector =
                new RoundRobinSpawnPointSelector(
                    new[]
                    {
                        firstSpawnPoint,
                        secondSpawnPoint
                    });

            WaveEnemySpawner =
                new WaveEnemySpawner(
                    enemySpawner,
                    Registry,
                    Target,
                    TargetHealth,
                    selector);
        }

        private void CreateWaveController()
        {
            _controllerObject =
                new GameObject("WaveController_Test");

            Controller =
                _controllerObject.AddComponent<WaveController>();

            Controller.EnemySpawned +=
                HandleEnemySpawnedForCleanup;
        }

        private void HandleEnemySpawnedForCleanup(
            int waveIndex,
            EnemySpawnResult spawnResult)
        {
            _spawnedInstances.Add(
                spawnResult.Instance);
        }

        private Transform CreateSpawnPoint(
            string objectName,
            Vector3 position)
        {
            GameObject spawnPointObject =
                new GameObject(objectName);

            spawnPointObject.transform.position =
                position;

            _spawnPointObjects.Add(
                spawnPointObject);

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

            if (field == null)
            {
                throw new MissingFieldException(
                    target.GetType().FullName,
                    fieldName);
            }

            field.SetValue(
                target,
                value);
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
    }
}