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