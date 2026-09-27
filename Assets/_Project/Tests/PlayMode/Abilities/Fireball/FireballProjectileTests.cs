using System.Collections;
using NUnit.Framework;
using ProjectFirstRun.Abilities.Fireball;
using ProjectFirstRun.Combat;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Abilities.Fireball
{
    public sealed class FireballProjectileTests
    {
        private GameObject _sourceObject;
        private GameObject _projectileObject;
        private GameObject _targetObject;

        private FireballProjectile _projectile;

        [SetUp]
        public void SetUp()
        {
            _sourceObject =
                new GameObject(
                    "Fireball_Source");

            CreateProjectile();
        }

        [TearDown]
        public void TearDown()
        {
            DestroyIfExists(
                _projectileObject);

            DestroyIfExists(
                _targetObject);

            DestroyIfExists(
                _sourceObject);
        }

        [Test]
        public void Initialize_WithValidValues_StoresRuntimeState()
        {
            _projectile.Initialize(
                Vector3.forward,
                25f,
                10f,
                3f,
                _sourceObject);

            Assert.That(
                _projectile.IsInitialized,
                Is.True);

            Assert.That(
                _projectile.Damage,
                Is.EqualTo(25f));

            Assert.That(
                _projectile.Speed,
                Is.EqualTo(10f));

            Assert.That(
                _projectile.LifetimeRemaining,
                Is.EqualTo(3f));

            Assert.That(
                _projectile.Direction,
                Is.EqualTo(
                    Vector3.forward));
        }

        [Test]
        public void Initialize_Twice_Throws()
        {
            _projectile.Initialize(
                Vector3.forward,
                25f,
                10f,
                3f,
                _sourceObject);

            Assert.That(
                () =>
                    _projectile.Initialize(
                        Vector3.forward,
                        25f,
                        10f,
                        3f,
                        _sourceObject),
                Throws.InvalidOperationException);
        }

        [Test]
        public void Initialize_WithZeroDirection_Throws()
        {
            Assert.That(
                () =>
                    _projectile.Initialize(
                        Vector3.zero,
                        25f,
                        10f,
                        3f,
                        _sourceObject),
                Throws.ArgumentException);
        }

        [UnityTest]
        public IEnumerator Projectile_MovesInConfiguredDirection()
        {
            _projectile.Initialize(
                Vector3.forward,
                25f,
                10f,
                3f,
                _sourceObject);

            Vector3 startPosition =
                _projectile.transform.position;

            yield return null;

            Assert.That(
                _projectile.transform.position.z,
                Is.GreaterThan(
                    startPosition.z));
        }

        [UnityTest]
        public IEnumerator Projectile_AfterLifetime_IsDestroyed()
        {
            _projectile.Initialize(
                Vector3.forward,
                25f,
                10f,
                0.05f,
                _sourceObject);

            yield return new WaitForSeconds(
                0.1f);

            Assert.That(
                _projectileObject == null,
                Is.True);
        }

        [Test]
        public void Sweep_HighSpeedHitResolvesOnlyOnce()
        {
            _targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _targetObject.transform.position = Vector3.forward * 5;
            var health = _targetObject.AddComponent<HealthComponent>(); health.Initialize(100);
            Physics.SyncTransforms();
            _projectile.Initialize(Vector3.forward, 25, 1000, 3, _sourceObject);
            _projectile.Advance(.02f); _projectile.Advance(.02f);
            Assert.That(health.CurrentHealth, Is.EqualTo(75));
            Assert.That(_projectile.IsResolved, Is.True);
            Assert.That(_projectile.transform.position.z, Is.LessThan(5));
        }

        [Test]
        public void Sweep_InitialOverlapResolvesWithoutTravel()
        {
            _targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var health = _targetObject.AddComponent<HealthComponent>(); health.Initialize(100);
            Physics.SyncTransforms();
            _projectile.Initialize(Vector3.forward, 25, 10, 3, _sourceObject);
            _projectile.Advance(.1f);
            Assert.That(health.CurrentHealth, Is.EqualTo(75));
            Assert.That(_projectile.transform.position, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Travel_IsClampedToRemainingRange()
        {
            _projectile.Initialize(Vector3.forward, 25, 100, 3, _sourceObject, range: 2);
            _projectile.Advance(1);
            Assert.That(_projectile.transform.position.z, Is.EqualTo(2));
            Assert.That(_projectile.IsResolved, Is.True);
        }

        [Test]
        public void SourceDestroyed_DisarmsProjectile()
        {
            _projectile.Initialize(Vector3.forward, 25, 10, 3, _sourceObject);
            Object.DestroyImmediate(_sourceObject); _projectile.Advance(.1f);
            Assert.That(_projectile.IsResolved, Is.True);
            Assert.That(_projectile.transform.position, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void ZeroDelta_FreezesProjectileAndLifetime()
        {
            _projectile.Initialize(Vector3.forward, 25, 10, 3, _sourceObject);
            _projectile.Advance(0);
            Assert.That(_projectile.transform.position, Is.EqualTo(Vector3.zero));
            Assert.That(_projectile.LifetimeRemaining, Is.EqualTo(3));
        }

        [Test]
        public void WorldWall_InterceptsBeforeEnemy()
        {
            _targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _targetObject.transform.position = Vector3.forward * 5;
            var health = _targetObject.AddComponent<HealthComponent>(); health.Initialize(100);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                wall.transform.position = Vector3.forward * 2; Physics.SyncTransforms();
                _projectile.Initialize(Vector3.forward, 25, 1000, 3, _sourceObject);
                _projectile.Advance(.02f);
                Assert.That(health.CurrentHealth, Is.EqualTo(100));
                Assert.That(_projectile.IsResolved, Is.True);
                Assert.That(_projectile.transform.position.z, Is.LessThan(2));
            }
            finally { Object.DestroyImmediate(wall); }
        }

        [Test]
        public void SourceColliderAndTrigger_DoNotIntercept()
        {
            _sourceObject.AddComponent<BoxCollider>();
            _targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _targetObject.GetComponent<Collider>().isTrigger = true;
            _targetObject.transform.position = Vector3.forward;
            Physics.SyncTransforms();
            _projectile.Initialize(Vector3.forward, 25, 10, 3, _sourceObject);
            _projectile.Advance(.2f);
            Assert.That(_projectile.IsResolved, Is.False);
            Assert.That(_projectile.transform.position.z, Is.EqualTo(2));
        }

        [Test]
        public void SourceDeath_DisarmsBeforeDamage()
        {
            var sourceHealth = _sourceObject.AddComponent<HealthComponent>(); sourceHealth.Initialize(100);
            _targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var health = _targetObject.AddComponent<HealthComponent>(); health.Initialize(100);
            _projectile.Initialize(Vector3.forward, 25, 10, 3, _sourceObject);
            var lethal = new DamageInfo(100, _sourceObject, Vector3.zero, Vector3.forward);
            sourceHealth.ApplyDamage(in lethal); Physics.SyncTransforms(); _projectile.Advance(.1f);
            Assert.That(health.CurrentHealth, Is.EqualTo(100));
            Assert.That(_projectile.IsResolved, Is.True);
        }

        private void CreateProjectile()
        {
            _projectileObject =
                new GameObject(
                    "FireballProjectile_Test");

            SphereCollider collider =
                _projectileObject
                    .AddComponent<SphereCollider>();

            collider.radius = 0.25f;
            collider.isTrigger = true;

            Rigidbody rigidbody =
                _projectileObject
                    .AddComponent<Rigidbody>();

            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;

            _projectile =
                _projectileObject
                    .AddComponent<FireballProjectile>();
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
    }
}
