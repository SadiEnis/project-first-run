using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Waves;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectFirstRun.Tests.EditMode.Waves
{
    public sealed class EnemyWaveEntryTests
    {
        private GameObject _enemyObject;
        private EnemyController _enemyController;
        private EnemyDefinition _enemyDefinition;

        [SetUp]
        public void SetUp()
        {
            _enemyObject =
                new GameObject("WaveEntry_TestEnemy");

            _enemyObject.AddComponent<NavMeshAgent>();
            _enemyObject.AddComponent<HealthComponent>();
            _enemyObject.AddComponent<EnemyMotor>();

            _enemyController =
                _enemyObject.AddComponent<EnemyController>();

            _enemyDefinition =
                ScriptableObject.CreateInstance<EnemyDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_enemyObject != null)
            {
                Object.DestroyImmediate(
                    _enemyObject);
            }

            if (_enemyDefinition != null)
            {
                Object.DestroyImmediate(
                    _enemyDefinition);
            }
        }

        [Test]
        public void Constructor_WithValidValues_StoresValues()
        {
            EnemyWaveEntry entry =
                new EnemyWaveEntry(
                    _enemyController,
                    _enemyDefinition,
                    3);

            Assert.That(
                entry.EnemyPrefab,
                Is.SameAs(_enemyController));

            Assert.That(
                entry.EnemyDefinition,
                Is.SameAs(_enemyDefinition));

            Assert.That(
                entry.Count,
                Is.EqualTo(3));
        }

        [Test]
        public void Constructor_WithNullPrefab_Throws()
        {
            Assert.That(
                () =>
                {
                    _ = new EnemyWaveEntry(
                        null,
                        _enemyDefinition,
                        1);
                },
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNullDefinition_Throws()
        {
            Assert.That(
                () =>
                {
                    _ = new EnemyWaveEntry(
                        _enemyController,
                        null,
                        1);
                },
                Throws.ArgumentNullException);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_WithNonPositiveCount_Throws(
            int count)
        {
            Assert.That(
                () =>
                {
                    _ = new EnemyWaveEntry(
                        _enemyController,
                        _enemyDefinition,
                        count);
                },
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Validate_AfterPrefabDestroyed_Throws()
        {
            EnemyWaveEntry entry =
                new EnemyWaveEntry(
                    _enemyController,
                    _enemyDefinition,
                    1);

            Object.DestroyImmediate(
                _enemyObject);

            Assert.That(
                () => entry.Validate(),
                Throws.InvalidOperationException);
        }

        [Test]
        public void Validate_AfterDefinitionDestroyed_Throws()
        {
            EnemyWaveEntry entry =
                new EnemyWaveEntry(
                    _enemyController,
                    _enemyDefinition,
                    1);

            Object.DestroyImmediate(
                _enemyDefinition);

            Assert.That(
                () => entry.Validate(),
                Throws.InvalidOperationException);
        }
    }
}