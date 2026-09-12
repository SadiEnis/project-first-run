using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace ProjectFirstRun.Tests.PlayMode.Enemies
{
    public sealed class EnemyAttackControllerTests
    {
        private GameObject _enemyObject;
        private GameObject _targetObject;
        private GameObject _registryObject;

        private NavMeshData _testNavMeshData;
        private NavMeshDataInstance _testNavMeshInstance;

        private EnemyDefinition _definition;
        private EnemyRegistry _registry;

        private HealthComponent _enemyHealth;
        private HealthComponent _targetHealth;

        private EnemyController _enemyController;
        private EnemyAttackController _attackController;

        [SetUp]
        public void SetUp()
        {
            CreateTestNavMesh();

            _definition =
                ScriptableObject.CreateInstance<EnemyDefinition>();

            _registryObject =
                new GameObject("EnemyRegistry");

            _registry =
                _registryObject.AddComponent<EnemyRegistry>();

            _targetObject =
                new GameObject("PlayerTarget");

            _targetObject.transform.position =
                new Vector3(1f, 0f, 0f);

            _targetHealth =
                _targetObject.AddComponent<HealthComponent>();

            _targetHealth.Initialize(100f);

            _enemyObject =
                new GameObject("Enemy");

            _enemyObject.transform.position =
                Vector3.zero;

            _enemyObject.AddComponent<NavMeshAgent>();

            _enemyHealth =
                _enemyObject.AddComponent<HealthComponent>();

            _enemyObject.AddComponent<EnemyMotor>();

            _enemyController =
                _enemyObject.AddComponent<EnemyController>();

            _attackController =
                _enemyObject.AddComponent<EnemyAttackController>();
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
        public void Initialize_WithValidDependencies_ConfiguresAttackController()
        {
            InitializeEnemyAndAttack();

            Assert.That(
                _attackController.IsInitialized,
                Is.True);

            Assert.That(
                _attackController.IsAttackEnabled,
                Is.True);

            Assert.That(
                _attackController.Target,
                Is.SameAs(_targetObject.transform));

            Assert.That(
                _attackController.CooldownRemaining,
                Is.EqualTo(0f));
        }

        [Test]
        public void Initialize_WithNullDefinition_Throws()
        {
            InitializeEnemyController();

            Assert.That(
                () => _attackController.Initialize(
                    null,
                    _targetObject.transform,
                    _targetHealth),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Initialize_WithNullTarget_Throws()
        {
            InitializeEnemyController();

            Assert.That(
                () => _attackController.Initialize(
                    _definition,
                    null,
                    _targetHealth),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Initialize_WithNullDamageable_Throws()
        {
            InitializeEnemyController();

            Assert.That(
                () => _attackController.Initialize(
                    _definition,
                    _targetObject.transform,
                    null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Initialize_WithNonUnityDamageable_Throws()
        {
            InitializeEnemyController();

            NonUnityDamageable damageable =
                new NonUnityDamageable();

            Assert.That(
                () => _attackController.Initialize(
                    _definition,
                    _targetObject.transform,
                    damageable),
                Throws.ArgumentException);
        }
        [UnityTest]
        public IEnumerator Update_WhenTargetIsInRange_AppliesDamage()
        {
            InitializeEnemyAndAttack();

            int attackEventCount = 0;
            int damageEventCount = 0;

            _attackController.AttackPerformed +=
                (_, _) => attackEventCount++;

            _attackController.DamageApplied +=
                (_, _) => damageEventCount++;

            yield return null;

            Assert.That(
                _targetHealth.CurrentHealth,
                Is.EqualTo(90f));

            Assert.That(
                attackEventCount,
                Is.EqualTo(1));

            Assert.That(
                damageEventCount,
                Is.EqualTo(1));

            Assert.That(
                _attackController.CooldownRemaining,
                Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator Update_WhenTargetIsOutOfRange_DoesNotAttack()
        {
            _targetObject.transform.position =
                new Vector3(5f, 0f, 0f);

            InitializeEnemyAndAttack();

            yield return null;
            yield return null;

            Assert.That(
                _targetHealth.CurrentHealth,
                Is.EqualTo(100f));

            Assert.That(
                _attackController.CooldownRemaining,
                Is.EqualTo(0f));
        }

        [UnityTest]
        public IEnumerator Update_DuringCooldown_DoesNotApplySecondDamage()
        {
            InitializeEnemyAndAttack();

            yield return null;

            Assert.That(
                _targetHealth.CurrentHealth,
                Is.EqualTo(90f));

            yield return null;

            Assert.That(
                _targetHealth.CurrentHealth,
                Is.EqualTo(90f));

            Assert.That(
                _attackController.CooldownRemaining,
                Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator Update_AfterCooldown_PerformsAnotherAttack()
        {
            InitializeEnemyAndAttack();

            yield return null;

            Assert.That(
                _targetHealth.CurrentHealth,
                Is.EqualTo(90f));

            yield return new WaitForSeconds(1.05f);

            Assert.That(
                _targetHealth.CurrentHealth,
                Is.EqualTo(80f));
        }

        [UnityTest]
        public IEnumerator DisableAndEnable_StopsAndResumesAttacking()
        {
            InitializeEnemyAndAttack();

            _enemyObject.SetActive(false);

            yield return null;

            Assert.That(
                _targetHealth.CurrentHealth,
                Is.EqualTo(100f));

            Assert.That(
                _attackController.IsAttackEnabled,
                Is.False);

            _enemyObject.SetActive(true);

            yield return null;

            Assert.That(
                _attackController.IsAttackEnabled,
                Is.True);

            Assert.That(
                _targetHealth.CurrentHealth,
                Is.EqualTo(90f));
        }

        [UnityTest]
        public IEnumerator EnemyDeath_StopsFutureAttacks()
        {
            InitializeEnemyAndAttack();

            DamageInfo lethalDamage = new DamageInfo(
                amount: 100f,
                source: null,
                hitPoint: Vector3.zero,
                hitDirection: Vector3.forward);

            _enemyHealth.ApplyDamage(in lethalDamage);

            Assert.That(
                _enemyController.IsDead,
                Is.True);

            Assert.That(
                _attackController.IsAttackEnabled,
                Is.False);

            yield return new WaitForSeconds(1.1f);

            Assert.That(
                _targetHealth.CurrentHealth,
                Is.EqualTo(100f));
        }

        [UnityTest]
        public IEnumerator Reinitialize_AfterDeath_RestoresAttackBehaviour()
        {
            InitializeEnemyAndAttack();

            yield return null;

            Assert.That(
                _targetHealth.CurrentHealth,
                Is.EqualTo(90f));

            DamageInfo lethalDamage = new DamageInfo(
                amount: 100f,
                source: null,
                hitPoint: Vector3.zero,
                hitDirection: Vector3.forward);

            _enemyHealth.ApplyDamage(in lethalDamage);

            Assert.That(
                _enemyController.IsDead,
                Is.True);

            _enemyController.Initialize(
                _definition,
                _targetObject.transform,
                _registry);

            _attackController.Initialize(
                _definition,
                _targetObject.transform,
                _targetHealth);

            Assert.That(
                _enemyController.IsDead,
                Is.False);

            Assert.That(
                _attackController.IsAttackEnabled,
                Is.True);

            Assert.That(
                _attackController.CooldownRemaining,
                Is.EqualTo(0f));

            yield return null;

            Assert.That(
                _targetHealth.CurrentHealth,
                Is.EqualTo(80f));
        }

        private void InitializeEnemyController()
        {
            _enemyController.Initialize(
                _definition,
                _targetObject.transform,
                _registry);
        }

        private void InitializeEnemyAndAttack()
        {
            InitializeEnemyController();

            _attackController.Initialize(
                _definition,
                _targetObject.transform,
                _targetHealth);
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