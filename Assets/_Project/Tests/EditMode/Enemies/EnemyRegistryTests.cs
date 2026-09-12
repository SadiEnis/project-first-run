using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectFirstRun.Tests.EditMode.Enemies
{
    public sealed class EnemyRegistryTests
    {
        private GameObject _registryObject;
        private GameObject _enemyObject;

        private EnemyRegistry _registry;
        private EnemyController _enemy;

        [SetUp]
        public void SetUp()
        {
            _registryObject =
                new GameObject("EnemyRegistryTest");

            _registry =
                _registryObject.AddComponent<EnemyRegistry>();

            _enemyObject =
                new GameObject("EnemyTest");

            _enemyObject.AddComponent<NavMeshAgent>();
            _enemyObject.AddComponent<HealthComponent>();
            _enemyObject.AddComponent<EnemyMotor>();

            _enemy =
                _enemyObject.AddComponent<EnemyController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_enemyObject != null)
            {
                Object.DestroyImmediate(_enemyObject);
            }

            if (_registryObject != null)
            {
                Object.DestroyImmediate(_registryObject);
            }
        }

        [Test]
        public void Register_WithNewEnemy_AddsEnemyAndRaisesEvents()
        {
            int registeredEventCount = 0;
            int reportedActiveCount = -1;

            _registry.EnemyRegistered += enemy =>
            {
                Assert.That(enemy, Is.SameAs(_enemy));
                registeredEventCount++;
            };

            _registry.ActiveCountChanged += activeCount =>
            {
                reportedActiveCount = activeCount;
            };

            bool registered =
                _registry.Register(_enemy);

            Assert.That(registered, Is.True);
            Assert.That(_registry.ActiveCount, Is.EqualTo(1));
            Assert.That(_registry.ActiveEnemies, Does.Contain(_enemy));
            Assert.That(registeredEventCount, Is.EqualTo(1));
            Assert.That(reportedActiveCount, Is.EqualTo(1));
        }

        [Test]
        public void Register_WithSameEnemyTwice_RejectsDuplicate()
        {
            int registeredEventCount = 0;

            _registry.EnemyRegistered += enemy =>
            {
                registeredEventCount++;
            };

            bool firstResult =
                _registry.Register(_enemy);

            bool secondResult =
                _registry.Register(_enemy);

            Assert.That(firstResult, Is.True);
            Assert.That(secondResult, Is.False);
            Assert.That(_registry.ActiveCount, Is.EqualTo(1));
            Assert.That(registeredEventCount, Is.EqualTo(1));
        }

        [Test]
        public void Unregister_WithRegisteredEnemy_RemovesEnemyAndRaisesEvents()
        {
            _registry.Register(_enemy);

            int unregisteredEventCount = 0;
            int reportedActiveCount = -1;

            _registry.EnemyUnregistered += enemy =>
            {
                Assert.That(enemy, Is.SameAs(_enemy));
                unregisteredEventCount++;
            };

            _registry.ActiveCountChanged += activeCount =>
            {
                reportedActiveCount = activeCount;
            };

            bool unregistered =
                _registry.Unregister(_enemy);

            Assert.That(unregistered, Is.True);
            Assert.That(_registry.ActiveCount, Is.EqualTo(0));
            Assert.That(_registry.ActiveEnemies, Has.No.Member(_enemy));
            Assert.That(unregisteredEventCount, Is.EqualTo(1));
            Assert.That(reportedActiveCount, Is.EqualTo(0));
        }

        [Test]
        public void Register_WithNullEnemy_Throws()
        {
            Assert.That(
                () => _registry.Register(null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Unregister_WithNullEnemy_ReturnsFalse()
        {
            bool result =
                _registry.Unregister(null);

            Assert.That(result, Is.False);
            Assert.That(_registry.ActiveCount, Is.EqualTo(0));
        }
    }
}