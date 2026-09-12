using NUnit.Framework;
using System.Collections.Generic;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectFirstRun.Tests.PlayMode.Enemies
{
    public sealed class EnemyControllerTests
    {
        private GameObject _enemyObject;
        private GameObject _targetObject;
        private GameObject _registryObject;
        private NavMeshData _testNavMeshData;
        private NavMeshDataInstance _testNavMeshInstance;

        private EnemyDefinition _definition;
        private EnemyRegistry _registry;
        private HealthComponent _health;
        private EnemyMotor _motor;
        private EnemyController _controller;

        [SetUp]
        public void SetUp()
        {
            CreateTestNavMesh();
            
            _definition =
                ScriptableObject.CreateInstance<EnemyDefinition>();

            _targetObject =
                new GameObject("PlayerTarget");

            _registryObject =
                new GameObject("EnemyRegistry");

            _registry =
                _registryObject.AddComponent<EnemyRegistry>();

            _enemyObject =
                new GameObject("Enemy");

            _enemyObject.AddComponent<NavMeshAgent>();

            _health =
                _enemyObject.AddComponent<HealthComponent>();

            _motor =
                _enemyObject.AddComponent<EnemyMotor>();

            _controller =
                _enemyObject.AddComponent<EnemyController>();
        }
        
        private void CreateTestNavMesh()
        {
            NavMeshBuildSettings buildSettings =
                NavMesh.GetSettingsByID(0);

            List<NavMeshBuildSource> sources =
                new List<NavMeshBuildSource>
                {
                    new NavMeshBuildSource
                    {
                        shape = NavMeshBuildSourceShape.Box,
                        size = new Vector3(20f, 0.2f, 20f),

                        // Kutunun üst yüzeyi Y = 0 seviyesinde olur.
                        transform = Matrix4x4.TRS(
                            new Vector3(0f, -0.1f, 0f),
                            Quaternion.identity,
                            Vector3.one),

                        area = 0
                    }
                };

            Bounds buildBounds = new Bounds(
                Vector3.zero,
                new Vector3(20f, 5f, 20f));

            _testNavMeshData =
                NavMeshBuilder.BuildNavMeshData(
                    buildSettings,
                    sources,
                    buildBounds,
                    Vector3.zero,
                    Quaternion.identity);

            Assert.That(
                _testNavMeshData,
                Is.Not.Null,
                "The temporary test NavMesh could not be built.");

            _testNavMeshInstance =
                NavMesh.AddNavMeshData(_testNavMeshData);

            Assert.That(
                _testNavMeshInstance.valid,
                Is.True,
                "The temporary test NavMesh could not be registered.");
        }

        [TearDown]
        public void TearDown()
        {
            if (_enemyObject != null)
            {
                Object.DestroyImmediate(_enemyObject);
            }

            if (_targetObject != null)
            {
                Object.DestroyImmediate(_targetObject);
            }

            if (_registryObject != null)
            {
                Object.DestroyImmediate(_registryObject);
            }

            if (_definition != null)
            {
                Object.DestroyImmediate(_definition);
            }

            if (_testNavMeshInstance.valid)
            {
                _testNavMeshInstance.Remove();
            }

            if (_testNavMeshData != null)
            {
                Object.DestroyImmediate(_testNavMeshData);
            }
        }

        [Test]
        public void Initialize_WithValidDependencies_ConfiguresAndRegistersEnemy()
        {
            _controller.Initialize(
                _definition,
                _targetObject.transform,
                _registry);

            Assert.That(_controller.IsInitialized, Is.True);
            Assert.That(_controller.IsDead, Is.False);
            Assert.That(_controller.Definition, Is.SameAs(_definition));
            Assert.That(
                _controller.Target,
                Is.SameAs(_targetObject.transform));

            Assert.That(_health.MaximumHealth, Is.EqualTo(100f));
            Assert.That(_health.CurrentHealth, Is.EqualTo(100f));

            Assert.That(_motor.IsInitialized, Is.True);
            Assert.That(_motor.IsMovementEnabled, Is.True);
            Assert.That(
                _motor.Target,
                Is.SameAs(_targetObject.transform));

            Assert.That(_registry.ActiveCount, Is.EqualTo(1));
            Assert.That(
                _registry.ActiveEnemies,
                Does.Contain(_controller));
        }

        [Test]
        public void Initialize_WithNullDefinition_Throws()
        {
            Assert.That(
                () => _controller.Initialize(
                    null,
                    _targetObject.transform,
                    _registry),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Initialize_WithNullTarget_Throws()
        {
            Assert.That(
                () => _controller.Initialize(
                    _definition,
                    null,
                    _registry),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Initialize_WithNullRegistry_Throws()
        {
            Assert.That(
                () => _controller.Initialize(
                    _definition,
                    _targetObject.transform,
                    null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void LethalDamage_StopsMovementAndUnregistersOnlyOnce()
        {
            _controller.Initialize(
                _definition,
                _targetObject.transform,
                _registry);

            int deathEventCount = 0;

            _controller.Died += (
                enemy,
                damageInfo,
                damageResult) =>
            {
                deathEventCount++;
            };

            DamageInfo lethalDamage = new DamageInfo(
                amount: 100f,
                source: null,
                hitPoint: Vector3.zero,
                hitDirection: Vector3.forward);

            DamageResult firstResult =
                _health.ApplyDamage(in lethalDamage);

            DamageResult secondResult =
                _health.ApplyDamage(in lethalDamage);

            Assert.That(firstResult.WasLethal, Is.True);
            Assert.That(secondResult.WasApplied, Is.False);

            Assert.That(_controller.IsDead, Is.True);
            Assert.That(_motor.IsMovementEnabled, Is.False);
            Assert.That(_registry.ActiveCount, Is.EqualTo(0));
            Assert.That(deathEventCount, Is.EqualTo(1));
        }

        [Test]
        public void DisableAndEnable_WithLivingEnemy_UpdatesRegistryMembership()
        {
            _controller.Initialize(
                _definition,
                _targetObject.transform,
                _registry);

            Assert.That(_registry.ActiveCount, Is.EqualTo(1));

            _enemyObject.SetActive(false);

            Assert.That(_registry.ActiveCount, Is.EqualTo(0));
            Assert.That(_motor.IsMovementEnabled, Is.False);

            _enemyObject.SetActive(true);

            Assert.That(_registry.ActiveCount, Is.EqualTo(1));
            Assert.That(_motor.IsMovementEnabled, Is.True);
        }

        [Test]
        public void Initialize_AfterDeath_RestoresAndRegistersEnemy()
        {
            _controller.Initialize(
                _definition,
                _targetObject.transform,
                _registry);

            DamageInfo lethalDamage = new DamageInfo(
                amount: 100f,
                source: null,
                hitPoint: Vector3.zero,
                hitDirection: Vector3.forward);

            _health.ApplyDamage(in lethalDamage);

            Assert.That(_controller.IsDead, Is.True);
            Assert.That(_registry.ActiveCount, Is.EqualTo(0));

            _controller.Initialize(
                _definition,
                _targetObject.transform,
                _registry);

            Assert.That(_controller.IsDead, Is.False);
            Assert.That(_health.IsDead, Is.False);
            Assert.That(_health.CurrentHealth, Is.EqualTo(100f));
            Assert.That(_motor.IsMovementEnabled, Is.True);
            Assert.That(_registry.ActiveCount, Is.EqualTo(1));
        }

        [Test]
        public void Initialize_WhileInactive_RegistersOnlyAfterEnable()
        {
            _enemyObject.SetActive(false);

            _controller.Initialize(
                _definition,
                _targetObject.transform,
                _registry);

            Assert.That(_controller.IsInitialized, Is.True);
            Assert.That(_registry.ActiveCount, Is.EqualTo(0));

            _enemyObject.SetActive(true);

            Assert.That(_registry.ActiveCount, Is.EqualTo(1));
            Assert.That(_motor.IsMovementEnabled, Is.True);
        }
    }
}