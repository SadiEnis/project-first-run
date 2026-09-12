#if UNITY_EDITOR

using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using UnityEngine;

namespace ProjectFirstRun.Development.Enemies
{
    [DisallowMultipleComponent]
    public sealed class EnemyFoundationTestBootstrap : MonoBehaviour
    {
        [Header("Spawning")]
        [SerializeField]
        private EnemySpawner _enemySpawner;

        [SerializeField]
        private Transform _spawnPoint;

        [Header("Enemy")]
        [SerializeField]
        private EnemyController _enemyPrefab;

        [SerializeField]
        private EnemyDefinition _enemyDefinition;

        [Header("Scene Dependencies")]
        [SerializeField]
        private Transform _target;

        [SerializeField]
        private HealthComponent _targetHealth;

        [SerializeField]
        private EnemyRegistry _enemyRegistry;

        public EnemyController SpawnedEnemy
        {
            get;
            private set;
        }

        public EnemyAttackController SpawnedEnemyAttack
        {
            get;
            private set;
        }

        private void Start()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            EnemySpawnRequest spawnRequest =
                new EnemySpawnRequest(
                    _enemyPrefab,
                    _enemyDefinition,
                    _target,
                    _targetHealth,
                    _enemyRegistry,
                    _spawnPoint.position,
                    _spawnPoint.rotation);

            EnemySpawnResult spawnResult =
                _enemySpawner.Spawn(
                    in spawnRequest);

            SpawnedEnemy =
                spawnResult.EnemyController;

            SpawnedEnemyAttack =
                spawnResult.AttackController;
        }

        private bool ValidateReferences()
        {
            bool isValid = true;

            if (_enemySpawner == null)
            {
                Debug.LogError(
                    $"{nameof(EnemyFoundationTestBootstrap)} " +
                    $"requires an {nameof(EnemySpawner)}.",
                    this);

                isValid = false;
            }

            if (_spawnPoint == null)
            {
                Debug.LogError(
                    $"{nameof(EnemyFoundationTestBootstrap)} " +
                    "requires a spawn point.",
                    this);

                isValid = false;
            }

            if (_enemyPrefab == null)
            {
                Debug.LogError(
                    $"{nameof(EnemyFoundationTestBootstrap)} " +
                    "requires an enemy prefab.",
                    this);

                isValid = false;
            }

            if (_enemyDefinition == null)
            {
                Debug.LogError(
                    $"{nameof(EnemyFoundationTestBootstrap)} " +
                    "requires an enemy definition.",
                    this);

                isValid = false;
            }

            if (_target == null)
            {
                Debug.LogError(
                    $"{nameof(EnemyFoundationTestBootstrap)} " +
                    "requires a target.",
                    this);

                isValid = false;
            }

            if (_targetHealth == null)
            {
                Debug.LogError(
                    $"{nameof(EnemyFoundationTestBootstrap)} " +
                    $"requires the target {nameof(HealthComponent)}.",
                    this);

                isValid = false;
            }

            if (_enemyRegistry == null)
            {
                Debug.LogError(
                    $"{nameof(EnemyFoundationTestBootstrap)} " +
                    $"requires an {nameof(EnemyRegistry)}.",
                    this);

                isValid = false;
            }

            return isValid;
        }
    }
}

#endif