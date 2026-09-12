using System;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectFirstRun.Tests.EditMode.Enemies.Spawning
{
    public sealed class EnemySpawnRequestTests
    {
        private GameObject _enemyPrefabObject;
        private GameObject _targetObject;
        private GameObject _registryObject;
        private GameObject _separateDamageableObject;

        private EnemyController _enemyPrefab;
        private EnemyDefinition _definition;
        private HealthComponent _targetHealth;
        private EnemyRegistry _registry;

        [SetUp]
        public void SetUp()
        {
            _enemyPrefabObject =
                new GameObject("EnemyPrefab");

            _enemyPrefabObject.AddComponent<NavMeshAgent>();
            _enemyPrefabObject.AddComponent<HealthComponent>();
            _enemyPrefabObject.AddComponent<EnemyMotor>();

            _enemyPrefab =
                _enemyPrefabObject.AddComponent<EnemyController>();

            _definition =
                ScriptableObject.CreateInstance<EnemyDefinition>();

            _targetObject =
                new GameObject("Target");

            _targetHealth =
                _targetObject.AddComponent<HealthComponent>();

            _targetHealth.Initialize(100f);

            _registryObject =
                new GameObject("EnemyRegistry");

            _registry =
                _registryObject.AddComponent<EnemyRegistry>();
        }

        [TearDown]
        public void TearDown()
        {
            DestroyImmediateIfExists(
                _enemyPrefabObject);

            DestroyImmediateIfExists(
                _targetObject);

            DestroyImmediateIfExists(
                _registryObject);

            DestroyImmediateIfExists(
                _separateDamageableObject);

            if (_definition != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    _definition);
            }
        }

        [Test]
        public void Constructor_WithValidDependencies_StoresValues()
        {
            Vector3 position =
                new Vector3(2f, 0f, 5f);

            Quaternion rotation =
                Quaternion.Euler(0f, 90f, 0f);

            EnemySpawnRequest request =
                CreateValidRequest(
                    position,
                    rotation);

            Assert.That(
                request.Prefab,
                Is.SameAs(_enemyPrefab));

            Assert.That(
                request.Definition,
                Is.SameAs(_definition));

            Assert.That(
                request.Target,
                Is.SameAs(_targetObject.transform));

            Assert.That(
                request.TargetDamageable,
                Is.SameAs(_targetHealth));

            Assert.That(
                request.Registry,
                Is.SameAs(_registry));

            Assert.That(
                request.Position,
                Is.EqualTo(position));

            Assert.That(
                request.Rotation,
                Is.EqualTo(rotation));
        }

        [Test]
        public void Constructor_AllowsTargetAndDamageableOnDifferentObjects()
        {
            _separateDamageableObject =
                new GameObject("SeparateDamageable");

            HealthComponent separateHealth =
                _separateDamageableObject
                    .AddComponent<HealthComponent>();

            separateHealth.Initialize(50f);

            EnemySpawnRequest request =
                new EnemySpawnRequest(
                    _enemyPrefab,
                    _definition,
                    _targetObject.transform,
                    separateHealth,
                    _registry,
                    Vector3.zero,
                    Quaternion.identity);

            Assert.That(
                request.Target,
                Is.SameAs(_targetObject.transform));

            Assert.That(
                request.TargetDamageable,
                Is.SameAs(separateHealth));
        }

        [Test]
        public void Constructor_WithNullPrefab_Throws()
        {
            Assert.That(
                () =>
                {
                    _ = new EnemySpawnRequest(
                        null,
                        _definition,
                        _targetObject.transform,
                        _targetHealth,
                        _registry,
                        Vector3.zero,
                        Quaternion.identity);
                },
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNullDefinition_Throws()
        {
            Assert.That(
                () =>
                {
                    _ = new EnemySpawnRequest(
                        _enemyPrefab,
                        null,
                        _targetObject.transform,
                        _targetHealth,
                        _registry,
                        Vector3.zero,
                        Quaternion.identity);
                },
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNullTarget_Throws()
        {
            Assert.That(
                () =>
                {
                    _ = new EnemySpawnRequest(
                        _enemyPrefab,
                        _definition,
                        null,
                        _targetHealth,
                        _registry,
                        Vector3.zero,
                        Quaternion.identity);
                },
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNullTargetDamageable_Throws()
        {
            Assert.That(
                () =>
                {
                    _ = new EnemySpawnRequest(
                        _enemyPrefab,
                        _definition,
                        _targetObject.transform,
                        null,
                        _registry,
                        Vector3.zero,
                        Quaternion.identity);
                },
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNonUnityDamageable_Throws()
        {
            NonUnityDamageable damageable =
                new NonUnityDamageable();

            Assert.That(
                () =>
                {
                    _ = new EnemySpawnRequest(
                        _enemyPrefab,
                        _definition,
                        _targetObject.transform,
                        damageable,
                        _registry,
                        Vector3.zero,
                        Quaternion.identity);
                },
                Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WithNonFinitePosition_Throws()
        {
            Vector3 invalidPosition =
                new Vector3(
                    float.NaN,
                    0f,
                    0f);

            Assert.That(
                () =>
                {
                    _ = CreateValidRequest(
                        invalidPosition,
                        Quaternion.identity);
                },
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_WithNonFiniteRotation_Throws()
        {
            Quaternion invalidRotation =
                new Quaternion(
                    0f,
                    float.PositiveInfinity,
                    0f,
                    1f);

            Assert.That(
                () =>
                {
                    _ = CreateValidRequest(
                        Vector3.zero,
                        invalidRotation);
                },
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_WithZeroQuaternion_Throws()
        {
            Quaternion zeroRotation =
                new Quaternion(
                    0f,
                    0f,
                    0f,
                    0f);

            Assert.That(
                () =>
                {
                    _ = CreateValidRequest(
                        Vector3.zero,
                        zeroRotation);
                },
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Validate_OnDefaultRequest_Throws()
        {
            EnemySpawnRequest request =
                default;

            Assert.That(
                () => request.Validate(),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Validate_AfterTargetDamageableDestroyed_Throws()
        {
            EnemySpawnRequest request =
                CreateValidRequest(
                    Vector3.zero,
                    Quaternion.identity);

            UnityEngine.Object.DestroyImmediate(
                _targetHealth);

            Assert.That(
                () => request.Validate(),
                Throws.ArgumentException);
        }

        [Test]
        public void Validate_AfterRegistryDestroyed_Throws()
        {
            EnemySpawnRequest request =
                CreateValidRequest(
                    Vector3.zero,
                    Quaternion.identity);

            UnityEngine.Object.DestroyImmediate(
                _registryObject);

            Assert.That(
                () => request.Validate(),
                Throws.ArgumentNullException);
        }

        private EnemySpawnRequest CreateValidRequest(
            Vector3 position,
            Quaternion rotation)
        {
            return new EnemySpawnRequest(
                _enemyPrefab,
                _definition,
                _targetObject.transform,
                _targetHealth,
                _registry,
                position,
                rotation);
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