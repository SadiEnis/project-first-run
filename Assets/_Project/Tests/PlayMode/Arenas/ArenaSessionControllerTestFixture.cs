using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using ProjectFirstRun.Player;
using ProjectFirstRun.Waves;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public abstract class ArenaSessionControllerTestFixture
    {
        private readonly List<GameObject> _spawnedEnemies =
            new List<GameObject>();

        private GameObject _arenaSessionObject;
        private GameObject _waveControllerObject;
        private GameObject _enemySpawnerObject;
        private GameObject _registryObject;
        private GameObject _targetObject;
        private GameObject _spawnPointObject;
        private GameObject _enemyPrefabObject;

        private EnemyWaveDefinition _waveDefinition;
        private ArenaWaveDefinition _arenaDefinition;
        private EnemyDefinition _enemyDefinition;

        protected ArenaSessionController ArenaSession
        {
            get;
            private set;
        }

        protected WaveController WaveController
        {
            get;
            private set;
        }

        protected EnemyRegistry Registry
        {
            get;
            private set;
        }

        protected FakePlayerDeathSource PlayerDeathSource
        {
            get;
            private set;
        }

        protected EnemyController SpawnedEnemy
        {
            get
            {
                Assert.That(
                    _spawnedEnemies.Count,
                    Is.GreaterThan(0));

                GameObject spawnedObject =
                    _spawnedEnemies[0];

                Assert.That(
                    spawnedObject != null,
                    Is.True);

                EnemyController enemy =
                    spawnedObject.GetComponent<EnemyController>();

                Assert.That(
                    enemy,
                    Is.Not.Null);

                return enemy;
            }
        }

        [SetUp]
        public void SetUp()
        {
            CreateTarget();
            CreateEnemyServices();
            CreateArenaSession();

            PlayerDeathSource =
                new FakePlayerDeathSource();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject enemy in _spawnedEnemies)
            {
                DestroyIfExists(enemy);
            }

            _spawnedEnemies.Clear();

            DestroyIfExists(_arenaSessionObject);
            DestroyIfExists(_waveControllerObject);
            DestroyIfExists(_enemySpawnerObject);
            DestroyIfExists(_registryObject);
            DestroyIfExists(_targetObject);
            DestroyIfExists(_spawnPointObject);
            DestroyIfExists(_enemyPrefabObject);

            if (_waveDefinition != null)
            {
                Object.DestroyImmediate(
                    _waveDefinition);
            }

            if (_arenaDefinition != null)
            {
                Object.DestroyImmediate(
                    _arenaDefinition);
            }

            if (_enemyDefinition != null)
            {
                Object.DestroyImmediate(
                    _enemyDefinition);
            }
        }

        protected void InitializeArenaSession()
        {
            ArenaSession.Initialize(
                WaveController,
                PlayerDeathSource);
        }

        protected void KillSpawnedEnemy()
        {
            EnemyController enemy =
                SpawnedEnemy;

            DamageInfo damageInfo =
                new DamageInfo(
                    enemy.Health.MaximumHealth,
                    null,
                    enemy.transform.position,
                    Vector3.forward);

            enemy.Health.ApplyDamage(
                in damageInfo);
        }

        private void CreateTarget()
        {
            _targetObject =
                new GameObject(
                    "ArenaSession_Target");

            _targetObject.transform.position =
                new Vector3(
                    1000f,
                    0f,
                    1000f);

            HealthComponent targetHealth =
                _targetObject.AddComponent<HealthComponent>();

            targetHealth.Initialize(
                100f);
        }

        private void CreateEnemyServices()
        {
            EnemySpawner enemySpawner =
                CreateEnemySpawner();

            Registry =
                CreateEnemyRegistry();

            Transform spawnPoint =
                CreateSpawnPoint();

            EnemyController enemyPrefab =
                CreateEnemyPrefab();

            _enemyDefinition =
                ScriptableObject.CreateInstance<EnemyDefinition>();

            CreateWaveData(
                enemyPrefab);

            WaveEnemySpawner waveEnemySpawner =
                CreateWaveEnemySpawner(
                    enemySpawner,
                    spawnPoint);

            CreateWaveController(
                waveEnemySpawner);
        }

        private EnemySpawner CreateEnemySpawner()
        {
            _enemySpawnerObject =
                new GameObject(
                    "ArenaSession_EnemySpawner");

            return _enemySpawnerObject
                .AddComponent<EnemySpawner>();
        }

        private EnemyRegistry CreateEnemyRegistry()
        {
            _registryObject =
                new GameObject(
                    "ArenaSession_Registry");

            return _registryObject
                .AddComponent<EnemyRegistry>();
        }

        private Transform CreateSpawnPoint()
        {
            _spawnPointObject =
                new GameObject(
                    "ArenaSession_SpawnPoint");

            return _spawnPointObject.transform;
        }

        private EnemyController CreateEnemyPrefab()
        {
            _enemyPrefabObject =
                new GameObject(
                    "ArenaSession_EnemyPrefab");

            _enemyPrefabObject
                .AddComponent<NavMeshAgent>();

            _enemyPrefabObject
                .AddComponent<HealthComponent>();

            _enemyPrefabObject
                .AddComponent<EnemyMotor>();

            EnemyController enemyController =
                _enemyPrefabObject
                    .AddComponent<EnemyController>();

            _enemyPrefabObject
                .AddComponent<EnemyAttackController>();

            return enemyController;
        }

        private void CreateWaveData(
            EnemyController enemyPrefab)
        {
            EnemyWaveEntry entry =
                new EnemyWaveEntry(
                    enemyPrefab,
                    _enemyDefinition,
                    1);

            CreateEnemyWaveDefinition(
                entry);

            CreateArenaWaveDefinition();
        }

        private void CreateEnemyWaveDefinition(
            EnemyWaveEntry entry)
        {
            _waveDefinition =
                ScriptableObject
                    .CreateInstance<EnemyWaveDefinition>();

            SetPrivateField(
                _waveDefinition,
                "_stableId",
                "arena_session_test_wave");

            SetPrivateField(
                _waveDefinition,
                "_displayName",
                "Arena Session Test Wave");

            SetPrivateField(
                _waveDefinition,
                "_entries",
                new List<EnemyWaveEntry>
                {
                    entry
                });
        }

        private void CreateArenaWaveDefinition()
        {
            _arenaDefinition =
                ScriptableObject
                    .CreateInstance<ArenaWaveDefinition>();

            SetPrivateField(
                _arenaDefinition,
                "_stableId",
                "arena_session_test");

            SetPrivateField(
                _arenaDefinition,
                "_waves",
                new List<EnemyWaveDefinition>
                {
                    _waveDefinition
                });
        }

        private WaveEnemySpawner CreateWaveEnemySpawner(
            EnemySpawner enemySpawner,
            Transform spawnPoint)
        {
            RoundRobinSpawnPointSelector selector =
                new RoundRobinSpawnPointSelector(
                    new[]
                    {
                        spawnPoint
                    });

            HealthComponent targetHealth =
                _targetObject
                    .GetComponent<HealthComponent>();

            return new WaveEnemySpawner(
                enemySpawner,
                Registry,
                _targetObject.transform,
                targetHealth,
                selector);
        }

        private void CreateWaveController(
            WaveEnemySpawner waveEnemySpawner)
        {
            _waveControllerObject =
                new GameObject(
                    "ArenaSession_WaveController");

            WaveController =
                _waveControllerObject
                    .AddComponent<WaveController>();

            WaveController.EnemySpawned +=
                HandleEnemySpawned;

            WaveController.Initialize(
                _arenaDefinition,
                waveEnemySpawner);
        }

        private void CreateArenaSession()
        {
            _arenaSessionObject =
                new GameObject(
                    "ArenaSession_Controller");

            ArenaSession =
                _arenaSessionObject
                    .AddComponent<ArenaSessionController>();
        }

        private void HandleEnemySpawned(
            int waveIndex,
            EnemySpawnResult spawnResult)
        {
            _spawnedEnemies.Add(
                spawnResult.Instance);
        }

        private static void SetPrivateField(
            object target,
            string fieldName,
            object value)
        {
            FieldInfo field =
                target
                    .GetType()
                    .GetField(
                        fieldName,
                        BindingFlags.Instance |
                        BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null,
                $"Field '{fieldName}' could not be found.");

            field.SetValue(
                target,
                value);
        }

        private static void DestroyIfExists(
            GameObject target)
        {
            if (target != null)
            {
                Object.DestroyImmediate(
                    target);
            }
        }

        protected sealed class FakePlayerDeathSource :
            IPlayerDeathSource
        {
            public bool IsDead
            {
                get;
                private set;
            }

            public event Action<DamageInfo, DamageResult>
                PlayerDied;

            public void Die()
            {
                if (IsDead)
                {
                    return;
                }

                IsDead = true;

                DamageInfo damageInfo =
                    default;

                DamageResult damageResult =
                    default;

                PlayerDied?.Invoke(
                    damageInfo,
                    damageResult);
            }

            public void SetDeadBeforeSession()
            {
                IsDead = true;
            }
        }
    }
}