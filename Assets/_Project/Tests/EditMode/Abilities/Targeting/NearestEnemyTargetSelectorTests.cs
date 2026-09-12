using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Abilities.Targeting;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Abilities.Targeting
{
    public sealed class NearestEnemyTargetSelectorTests
    {
        private readonly List<GameObject> _enemyObjects =
            new List<GameObject>();

        private GameObject _registryObject;
        private GameObject _movementTargetObject;

        private EnemyRegistry _registry;
        private EnemyDefinition _enemyDefinition;

        [SetUp]
        public void SetUp()
        {
            _registryObject =
                new GameObject(
                    "AbilityTargeting_Registry");

            _registry =
                _registryObject
                    .AddComponent<EnemyRegistry>();

            _movementTargetObject =
                new GameObject(
                    "AbilityTargeting_MovementTarget");

            _enemyDefinition =
                ScriptableObject
                    .CreateInstance<EnemyDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject enemyObject
                     in _enemyObjects)
            {
                if (enemyObject != null)
                {
                    Object.DestroyImmediate(
                        enemyObject);
                }
            }

            _enemyObjects.Clear();

            if (_registryObject != null)
            {
                Object.DestroyImmediate(
                    _registryObject);
            }

            if (_movementTargetObject != null)
            {
                Object.DestroyImmediate(
                    _movementTargetObject);
            }

            if (_enemyDefinition != null)
            {
                Object.DestroyImmediate(
                    _enemyDefinition);
            }
        }

        [Test]
        public void Constructor_WithNullRegistry_Throws()
        {
            Assert.That(
                () =>
                    new NearestEnemyTargetSelector(
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void TrySelectTarget_WithNoEnemies_ReturnsFalse()
        {
            NearestEnemyTargetSelector selector =
                CreateSelector();

            bool found =
                selector.TrySelectTarget(
                    Vector3.zero,
                    out Transform target);

            Assert.That(
                found,
                Is.False);

            Assert.That(
                target,
                Is.Null);
        }

        [Test]
        public void TrySelectTarget_WithSingleEnemy_ReturnsEnemy()
        {
            EnemyController enemy =
                CreateEnemy(
                    "Enemy_0",
                    new Vector3(
                        5f,
                        0f,
                        0f));

            NearestEnemyTargetSelector selector =
                CreateSelector();

            bool found =
                selector.TrySelectTarget(
                    Vector3.zero,
                    out Transform target);

            Assert.That(
                found,
                Is.True);

            Assert.That(
                target,
                Is.SameAs(enemy.transform));
        }

        [Test]
        public void TrySelectTarget_WithMultipleEnemies_ReturnsNearest()
        {
            CreateEnemy(
                "FarEnemy",
                new Vector3(
                    10f,
                    0f,
                    0f));

            EnemyController nearestEnemy =
                CreateEnemy(
                    "NearestEnemy",
                    new Vector3(
                        2f,
                        0f,
                        0f));

            CreateEnemy(
                "MiddleEnemy",
                new Vector3(
                    5f,
                    0f,
                    0f));

            NearestEnemyTargetSelector selector =
                CreateSelector();

            bool found =
                selector.TrySelectTarget(
                    Vector3.zero,
                    out Transform target);

            Assert.That(
                found,
                Is.True);

            Assert.That(
                target,
                Is.SameAs(
                    nearestEnemy.transform));
        }

        [Test]
        public void TrySelectTarget_UsesProvidedOrigin()
        {
            EnemyController leftEnemy =
                CreateEnemy(
                    "LeftEnemy",
                    new Vector3(
                        -10f,
                        0f,
                        0f));

            EnemyController rightEnemy =
                CreateEnemy(
                    "RightEnemy",
                    new Vector3(
                        10f,
                        0f,
                        0f));

            NearestEnemyTargetSelector selector =
                CreateSelector();

            bool found =
                selector.TrySelectTarget(
                    new Vector3(
                        8f,
                        0f,
                        0f),
                    out Transform target);

            Assert.That(
                found,
                Is.True);

            Assert.That(
                target,
                Is.SameAs(
                    rightEnemy.transform));

            Assert.That(
                target,
                Is.Not.SameAs(
                    leftEnemy.transform));
        }

        [Test]
        public void TrySelectTarget_AfterNearestEnemyIsUnregistered_SelectsNextEnemy()
        {
            EnemyController nearestEnemy =
                CreateEnemy(
                    "NearestEnemy",
                    new Vector3(
                        2f,
                        0f,
                        0f));

            EnemyController remainingEnemy =
                CreateEnemy(
                    "RemainingEnemy",
                    new Vector3(
                        6f,
                        0f,
                        0f));

            _registry.Unregister(
                nearestEnemy);

            NearestEnemyTargetSelector selector =
                CreateSelector();

            bool found =
                selector.TrySelectTarget(
                    Vector3.zero,
                    out Transform target);

            Assert.That(
                found,
                Is.True);

            Assert.That(
                target,
                Is.SameAs(
                    remainingEnemy.transform));

            Assert.That(
                _registry.ActiveCount,
                Is.EqualTo(1));
        }

        [Test]
        public void TrySelectTarget_WhenOnlyEnemyIsUnregistered_ReturnsFalse()
        {
            EnemyController enemy =
                CreateEnemy(
                    "Enemy_0",
                    new Vector3(
                        2f,
                        0f,
                        0f));

            _registry.Unregister(
                enemy);

            NearestEnemyTargetSelector selector =
                CreateSelector();

            bool found =
                selector.TrySelectTarget(
                    Vector3.zero,
                    out Transform target);

            Assert.That(
                found,
                Is.False);

            Assert.That(
                target,
                Is.Null);

            Assert.That(
                _registry.ActiveCount,
                Is.EqualTo(0));
        }

        [Test]
        public void TrySelectTarget_WithEnemyAtOrigin_ReturnsEnemy()
        {
            EnemyController enemy =
                CreateEnemy(
                    "Enemy_AtOrigin",
                    Vector3.zero);

            NearestEnemyTargetSelector selector =
                CreateSelector();

            bool found =
                selector.TrySelectTarget(
                    Vector3.zero,
                    out Transform target);

            Assert.That(
                found,
                Is.True);

            Assert.That(
                target,
                Is.SameAs(enemy.transform));
        }

        [Test]
        public void TrySelectTarget_WithNaNOrigin_Throws()
        {
            NearestEnemyTargetSelector selector =
                CreateSelector();

            Vector3 invalidOrigin =
                new Vector3(
                    float.NaN,
                    0f,
                    0f);

            Assert.That(
                () =>
                    selector.TrySelectTarget(
                        invalidOrigin,
                        out _),
                Throws.ArgumentException);
        }

        [Test]
        public void TrySelectTarget_WithInfiniteOrigin_Throws()
        {
            NearestEnemyTargetSelector selector =
                CreateSelector();

            Vector3 invalidOrigin =
                new Vector3(
                    float.PositiveInfinity,
                    0f,
                    0f);

            Assert.That(
                () =>
                    selector.TrySelectTarget(
                        invalidOrigin,
                        out _),
                Throws.ArgumentException);
        }

        private NearestEnemyTargetSelector
            CreateSelector()
        {
            return new NearestEnemyTargetSelector(
                _registry);
        }

        private EnemyController CreateEnemy(
            string objectName,
            Vector3 position)
        {
            GameObject enemyObject =
                new GameObject(objectName);

            enemyObject.SetActive(false);
            enemyObject.transform.position =
                position;

            _enemyObjects.Add(
                enemyObject);

            NavMeshAgent navMeshAgent =
                enemyObject
                    .AddComponent<NavMeshAgent>();

            navMeshAgent.enabled = false;

            enemyObject
                .AddComponent<HealthComponent>();

            enemyObject
                .AddComponent<EnemyMotor>();

            EnemyController enemy =
                enemyObject
                    .AddComponent<EnemyController>();

            enemy.Initialize(
                _enemyDefinition,
                _movementTargetObject.transform,
                _registry);

            enemyObject.SetActive(true);

/*
 * EditMode tests do not rely on MonoBehaviour OnEnable
 * to register the enemy. Registry membership is explicit
 * because target selection, not enemy lifecycle, is the
 * responsibility under test.
 */
            _registry.Register(enemy);

            return enemy;
        }
    }
}