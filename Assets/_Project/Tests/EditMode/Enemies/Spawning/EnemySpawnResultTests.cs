using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectFirstRun.Tests.EditMode.Enemies.Spawning
{
    public sealed class EnemySpawnResultTests
    {
        private GameObject _firstEnemyObject;
        private GameObject _secondEnemyObject;

        private EnemyController _firstEnemyController;
        private EnemyAttackController _firstAttackController;

        private EnemyController _secondEnemyController;
        private EnemyAttackController _secondAttackController;

        [SetUp]
        public void SetUp()
        {
            CreateEnemyComponents(
                "FirstEnemy",
                out _firstEnemyObject,
                out _firstEnemyController,
                out _firstAttackController);

            CreateEnemyComponents(
                "SecondEnemy",
                out _secondEnemyObject,
                out _secondEnemyController,
                out _secondAttackController);
        }

        [TearDown]
        public void TearDown()
        {
            if (_firstEnemyObject != null)
            {
                Object.DestroyImmediate(
                    _firstEnemyObject);
            }

            if (_secondEnemyObject != null)
            {
                Object.DestroyImmediate(
                    _secondEnemyObject);
            }
        }

        [Test]
        public void Constructor_WithMatchingControllers_StoresValues()
        {
            EnemySpawnResult result =
                new EnemySpawnResult(
                    _firstEnemyController,
                    _firstAttackController);

            Assert.That(
                result.EnemyController,
                Is.SameAs(_firstEnemyController));

            Assert.That(
                result.AttackController,
                Is.SameAs(_firstAttackController));

            Assert.That(
                result.Instance,
                Is.SameAs(_firstEnemyObject));
        }

        [Test]
        public void Constructor_WithNullEnemyController_Throws()
        {
            Assert.That(
                () =>
                {
                    _ = new EnemySpawnResult(
                        null,
                        _firstAttackController);
                },
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNullAttackController_Throws()
        {
            Assert.That(
                () =>
                {
                    _ = new EnemySpawnResult(
                        _firstEnemyController,
                        null);
                },
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithControllersFromDifferentObjects_Throws()
        {
            Assert.That(
                () =>
                {
                    _ = new EnemySpawnResult(
                        _firstEnemyController,
                        _secondAttackController);
                },
                Throws.ArgumentException);
        }

        private static void CreateEnemyComponents(
            string objectName,
            out GameObject enemyObject,
            out EnemyController enemyController,
            out EnemyAttackController attackController)
        {
            enemyObject =
                new GameObject(objectName);

            enemyObject.AddComponent<NavMeshAgent>();
            enemyObject.AddComponent<HealthComponent>();
            enemyObject.AddComponent<EnemyMotor>();

            enemyController =
                enemyObject.AddComponent<EnemyController>();

            attackController =
                enemyObject.AddComponent<EnemyAttackController>();
        }
    }
}