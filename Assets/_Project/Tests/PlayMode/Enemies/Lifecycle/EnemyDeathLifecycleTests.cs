using System.Collections;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Lifecycle;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Enemies.Lifecycle
{
    public sealed class EnemyDeathLifecycleTests
    {
        private GameObject _enemyObject;
        private GameObject _targetObject;
        private GameObject _registryObject;

        private EnemyController _enemyController;
        private EnemyDeathLifecycle _deathLifecycle;
        private HealthComponent _healthComponent;
        private BoxCollider _collider;

        private EnemyDefinition _enemyDefinition;
        private EnemyRegistry _enemyRegistry;

        [SetUp]
        public void SetUp()
        {
            _targetObject =
                new GameObject("Lifecycle_Target");

            _registryObject =
                new GameObject("Lifecycle_Registry");

            _enemyRegistry =
                _registryObject.AddComponent<EnemyRegistry>();

            _enemyDefinition =
                ScriptableObject.CreateInstance<EnemyDefinition>();

            CreateEnemy();
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

            if (_enemyDefinition != null)
            {
                Object.DestroyImmediate(_enemyDefinition);
            }
        }

        [Test]
        public void InitializedEnemy_HasNotStartedDeathSequence()
        {
            Assert.That(
                _deathLifecycle.IsDeathSequenceStarted,
                Is.False);

            Assert.That(
                _collider.enabled,
                Is.True);

            Assert.That(
                _enemyController.IsDead,
                Is.False);
        }

        [Test]
        public void NonLethalDamage_DoesNotStartDeathSequence()
        {
            ApplyDamage(1f);

            Assert.That(
                _deathLifecycle.IsDeathSequenceStarted,
                Is.False);

            Assert.That(
                _collider.enabled,
                Is.True);

            Assert.That(
                _enemyController.IsDead,
                Is.False);
        }

        [Test]
        public void LethalDamage_StartsDeathSequence()
        {
            KillEnemy();

            Assert.That(
                _enemyController.IsDead,
                Is.True);

            Assert.That(
                _deathLifecycle.IsDeathSequenceStarted,
                Is.True);
        }

        [Test]
        public void LethalDamage_DisablesConfiguredColliderImmediately()
        {
            KillEnemy();

            Assert.That(
                _collider.enabled,
                Is.False);
        }

        [UnityTest]
        public IEnumerator Death_WithDelay_KeepsInstanceUntilDelayExpires()
        {
            SetDestroyDelay(0.1f);

            KillEnemy();

            Assert.That(
                _enemyObject != null,
                Is.True);

            yield return new WaitForSeconds(0.03f);

            Assert.That(
                _enemyObject != null,
                Is.True);

            yield return new WaitForSeconds(0.1f);

            Assert.That(
                _enemyObject == null,
                Is.True);
        }

        [UnityTest]
        public IEnumerator Death_WithZeroDelay_DestroysInstance()
        {
            SetDestroyDelay(0f);

            KillEnemy();

            Assert.That(
                _deathLifecycle.IsDeathSequenceStarted,
                Is.True);

            Assert.That(
                _collider.enabled,
                Is.False);

            // Destroy() is deferred until the end of the frame.
            yield return null;

            Assert.That(
                _enemyObject == null,
                Is.True);
        }

        [UnityTest]
        public IEnumerator Death_RemovesEnemyFromRegistryBeforeInstanceIsDestroyed()
        {
            SetDestroyDelay(0.1f);

            Assert.That(
                _enemyRegistry.ActiveCount,
                Is.EqualTo(1));

            KillEnemy();

            Assert.That(
                _enemyRegistry.ActiveCount,
                Is.EqualTo(0));

            Assert.That(
                _enemyObject != null,
                Is.True);

            yield return new WaitForSeconds(0.12f);

            Assert.That(
                _enemyObject == null,
                Is.True);
        }

        [UnityTest]
        public IEnumerator AdditionalDamage_DoesNotRestartCleanupDelay()
        {
            SetDestroyDelay(0.1f);

            KillEnemy();

            yield return new WaitForSeconds(0.06f);

            ApplyDamage(25f);

            yield return new WaitForSeconds(0.06f);

            Assert.That(
                _enemyObject == null,
                Is.True);
        }

        private void CreateEnemy()
        {
            _enemyObject =
                new GameObject("LifecycleEnemy");

            // Configure everything before OnEnable performs
            // the runtime event subscriptions.
            _enemyObject.SetActive(false);

            _enemyObject.AddComponent<NavMeshAgent>();

            _healthComponent =
                _enemyObject.AddComponent<HealthComponent>();

            _enemyObject.AddComponent<EnemyMotor>();

            _enemyController =
                _enemyObject.AddComponent<EnemyController>();

            _collider =
                _enemyObject.AddComponent<BoxCollider>();

            _deathLifecycle =
                _enemyObject.AddComponent<EnemyDeathLifecycle>();

            SetPrivateField(
                _deathLifecycle,
                "_destroyDelay",
                1f);

            SetPrivateField(
                _deathLifecycle,
                "_collidersToDisable",
                new Collider[]
                {
                    _collider
                });

            _enemyController.Initialize(
                _enemyDefinition,
                _targetObject.transform,
                _enemyRegistry);

            _enemyObject.SetActive(true);
        }

        private void KillEnemy()
        {
            ApplyDamage(
                _healthComponent.MaximumHealth);
        }

        private void ApplyDamage(
            float amount)
        {
            DamageInfo damageInfo =
                new DamageInfo(
                    amount,
                    null,
                    _enemyObject.transform.position,
                    Vector3.forward);

            _healthComponent.ApplyDamage(
                in damageInfo);
        }

        private void SetDestroyDelay(
            float delay)
        {
            SetPrivateField(
                _deathLifecycle,
                "_destroyDelay",
                delay);
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
    }
}