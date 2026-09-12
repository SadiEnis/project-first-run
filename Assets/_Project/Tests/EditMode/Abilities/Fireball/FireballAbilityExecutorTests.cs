using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Abilities.Execution;
using ProjectFirstRun.Abilities.Fireball;
using ProjectFirstRun.Stats;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Abilities.Fireball
{
    public sealed class FireballAbilityExecutorTests
    {
        private readonly List<GameObject> _createdObjects =
            new List<GameObject>();

        private FireballDefinition _definition;
        private FireballProjectile _projectilePrefab;
        private GameObject _damageSource;
        private GameObject _targetObject;
        private PlayerStatCollection _stats;

        [SetUp]
        public void SetUp()
        {
            _definition =
                ScriptableObject
                    .CreateInstance<FireballDefinition>();

            _damageSource =
                CreateObject(
                    "FireballExecutor_Source");

            _targetObject =
                CreateObject(
                    "FireballExecutor_Target");

            _targetObject.transform.position =
                new Vector3(
                    0f,
                    0f,
                    10f);

            _projectilePrefab =
                CreateProjectilePrefab();

            _stats =
                new PlayerStatCollection();

            ConfigureDefinition();
        }

        [TearDown]
        public void TearDown()
        {
            FireballProjectile[] projectiles =
                Object.FindObjectsByType<FireballProjectile>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            foreach (FireballProjectile projectile
                     in projectiles)
            {
                if (projectile != null &&
                    projectile != _projectilePrefab)
                {
                    Object.DestroyImmediate(
                        projectile.gameObject);
                }
            }

            foreach (GameObject createdObject
                     in _createdObjects)
            {
                if (createdObject != null)
                {
                    Object.DestroyImmediate(
                        createdObject);
                }
            }

            _createdObjects.Clear();

            if (_definition != null)
            {
                Object.DestroyImmediate(
                    _definition);
            }
        }

        [Test]
        public void Constructor_WithNullDefinition_Throws()
        {
            Assert.That(
                () =>
                    new FireballAbilityExecutor(
                        null,
                        _damageSource,
                        _stats),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNullDamageSource_Throws()
        {
            Assert.That(
                () =>
                    new FireballAbilityExecutor(
                        _definition,
                        null,
                        _stats),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNullStats_Throws()
        {
            Assert.That(
                () =>
                    new FireballAbilityExecutor(
                        _definition,
                        _damageSource,
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void TryExecute_WithoutTarget_ReturnsFailed()
        {
            FireballAbilityExecutor executor =
                CreateExecutor();

            AbilityExecutionContext context =
                new AbilityExecutionContext(
                    Vector3.zero,
                    null);

            AbilityExecutionResult result =
                executor.TryExecute(
                    in context);

            Assert.That(
                result,
                Is.EqualTo(
                    AbilityExecutionResult.Failed));

            Assert.That(
                FindSpawnedProjectile(),
                Is.Null);
        }

        [Test]
        public void TryExecute_WithValidTarget_SpawnsInitializedProjectile()
        {
            FireballAbilityExecutor executor =
                CreateExecutor();

            Vector3 origin =
                new Vector3(
                    1f,
                    2f,
                    3f);

            AbilityExecutionContext context =
                new AbilityExecutionContext(
                    origin,
                    _targetObject.transform);

            AbilityExecutionResult result =
                executor.TryExecute(
                    in context);

            FireballProjectile projectile =
                FindSpawnedProjectile();

            Assert.That(
                result,
                Is.EqualTo(
                    AbilityExecutionResult.Performed));

            Assert.That(
                projectile,
                Is.Not.Null);

            Assert.That(
                projectile.IsInitialized,
                Is.True);

            Assert.That(
                projectile.transform.position,
                Is.EqualTo(origin));

            Assert.That(
                projectile.Damage,
                Is.EqualTo(30f));

            Assert.That(
                projectile.Speed,
                Is.EqualTo(12f));

            Assert.That(
                projectile.LifetimeRemaining,
                Is.EqualTo(4f));
        }

        [Test]
        public void TryExecute_WithAbilityDamageModifier_UsesEvaluatedDamage()
        {
            _stats.Add(
                new StatModifier(
                    PlayerStatType.AbilityDamage,
                    StatModifierOperation.AdditivePercent,
                    0.50f,
                    "test.fireball_damage"));

            FireballAbilityExecutor executor =
                CreateExecutor();

            AbilityExecutionContext context =
                new AbilityExecutionContext(
                    Vector3.zero,
                    _targetObject.transform);

            AbilityExecutionResult result =
                executor.TryExecute(
                    in context);

            FireballProjectile projectile =
                FindSpawnedProjectile();

            Assert.That(
                result,
                Is.EqualTo(
                    AbilityExecutionResult.Performed));

            Assert.That(
                projectile,
                Is.Not.Null);

            Assert.That(
                projectile.Damage,
                Is.EqualTo(45f)
                    .Within(0.0001f));
        }

        [Test]
        public void TryExecute_WithWeaponDamageModifier_DoesNotChangeDamage()
        {
            _stats.Add(
                new StatModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.AdditivePercent,
                    5f,
                    "test.weapon_damage"));

            FireballAbilityExecutor executor =
                CreateExecutor();

            AbilityExecutionContext context =
                new AbilityExecutionContext(
                    Vector3.zero,
                    _targetObject.transform);

            executor.TryExecute(
                in context);

            FireballProjectile projectile =
                FindSpawnedProjectile();

            Assert.That(
                projectile,
                Is.Not.Null);

            Assert.That(
                projectile.Damage,
                Is.EqualTo(30f)
                    .Within(0.0001f));
        }

        [Test]
        public void TryExecute_WhenEvaluatedDamageIsZero_ThrowsWithoutSpawning()
        {
            _stats.Add(
                new StatModifier(
                    PlayerStatType.AbilityDamage,
                    StatModifierOperation.Flat,
                    -30f,
                    "test.invalid_fireball_damage"));

            FireballAbilityExecutor executor =
                CreateExecutor();

            AbilityExecutionContext context =
                new AbilityExecutionContext(
                    Vector3.zero,
                    _targetObject.transform);

            Assert.That(
                () =>
                    executor.TryExecute(
                        in context),
                Throws.InvalidOperationException);

            Assert.That(
                FindSpawnedProjectile(),
                Is.Null);
        }

        [Test]
        public void TryExecute_ProjectileDirectionPointsTowardTarget()
        {
            FireballAbilityExecutor executor =
                CreateExecutor();

            Vector3 origin =
                Vector3.zero;

            _targetObject.transform.position =
                new Vector3(
                    3f,
                    0f,
                    4f);

            AbilityExecutionContext context =
                new AbilityExecutionContext(
                    origin,
                    _targetObject.transform);

            executor.TryExecute(
                in context);

            FireballProjectile projectile =
                FindSpawnedProjectile();

            Vector3 expectedDirection =
                (_targetObject.transform.position - origin)
                .normalized;

            Assert.That(
                projectile,
                Is.Not.Null);

            Assert.That(
                Vector3.Distance(
                    projectile.Direction,
                    expectedDirection),
                Is.LessThan(0.0001f));
        }

        [Test]
        public void TryExecute_WithoutProjectilePrefab_Throws()
        {
            SetPrivateField(
                _definition,
                "_projectilePrefab",
                null,
                typeof(FireballDefinition));

            FireballAbilityExecutor executor =
                CreateExecutor();

            AbilityExecutionContext context =
                new AbilityExecutionContext(
                    Vector3.zero,
                    _targetObject.transform);

            Assert.That(
                () =>
                    executor.TryExecute(
                        in context),
                Throws.InvalidOperationException);
        }

        [Test]
        public void TryExecute_WithInvalidOrigin_Throws()
        {
            FireballAbilityExecutor executor =
                CreateExecutor();

            AbilityExecutionContext context =
                new AbilityExecutionContext(
                    new Vector3(
                        float.NaN,
                        0f,
                        0f),
                    _targetObject.transform);

            Assert.That(
                () =>
                    executor.TryExecute(
                        in context),
                Throws.ArgumentException);
        }

        private FireballAbilityExecutor CreateExecutor()
        {
            return new FireballAbilityExecutor(
                _definition,
                _damageSource,
                _stats);
        }

        private void ConfigureDefinition()
        {
            SetPrivateField(
                _definition,
                "_damage",
                30f,
                typeof(FireballDefinition));

            SetPrivateField(
                _definition,
                "_projectileSpeed",
                12f,
                typeof(FireballDefinition));

            SetPrivateField(
                _definition,
                "_projectileLifetime",
                4f,
                typeof(FireballDefinition));

            SetPrivateField(
                _definition,
                "_projectilePrefab",
                _projectilePrefab,
                typeof(FireballDefinition));
        }

        private FireballProjectile CreateProjectilePrefab()
        {
            GameObject projectileObject =
                CreateObject(
                    "FireballExecutor_ProjectilePrefab");

            projectileObject
                .AddComponent<SphereCollider>();

            projectileObject
                .AddComponent<Rigidbody>();

            return projectileObject
                .AddComponent<FireballProjectile>();
        }

        private FireballProjectile FindSpawnedProjectile()
        {
            FireballProjectile[] projectiles =
                Object.FindObjectsByType<FireballProjectile>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            foreach (FireballProjectile projectile
                     in projectiles)
            {
                if (projectile != null &&
                    projectile != _projectilePrefab)
                {
                    return projectile;
                }
            }

            return null;
        }

        private GameObject CreateObject(
            string objectName)
        {
            GameObject createdObject =
                new GameObject(
                    objectName);

            _createdObjects.Add(
                createdObject);

            return createdObject;
        }

        private static void SetPrivateField(
            object target,
            string fieldName,
            object value,
            System.Type declaringType)
        {
            FieldInfo field =
                declaringType.GetField(
                    fieldName,
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null);

            field.SetValue(
                target,
                value);
        }
    }
}