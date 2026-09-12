using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Waves;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectFirstRun.Tests.EditMode.Waves
{
    public sealed class EnemyWaveDefinitionTests
    {
        private readonly List<EnemyWaveDefinition> _createdWaves =
            new List<EnemyWaveDefinition>();

        private GameObject _enemyObject;
        private EnemyController _enemyController;
        private EnemyDefinition _enemyDefinition;

        [SetUp]
        public void SetUp()
        {
            _enemyObject =
                new GameObject("WaveDefinition_TestEnemy");

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
            foreach (EnemyWaveDefinition wave in _createdWaves)
            {
                if (wave != null)
                {
                    Object.DestroyImmediate(
                        wave);
                }
            }

            _createdWaves.Clear();

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
        public void Validate_WithValidDefinition_DoesNotThrow()
        {
            EnemyWaveDefinition wave =
                CreateWave(
                    "wave_01",
                    "Wave 1",
                    CreateEntry(2));

            Assert.That(
                wave.EntryCount,
                Is.EqualTo(1));

            Assert.DoesNotThrow(
                wave.Validate);
        }

        [Test]
        public void TotalEnemyCount_SumsAllEntries()
        {
            EnemyWaveDefinition wave =
                CreateWave(
                    "wave_01",
                    "Wave 1",
                    CreateEntry(2),
                    CreateEntry(3));

            Assert.That(
                wave.TotalEnemyCount,
                Is.EqualTo(5));
        }

        [Test]
        public void Validate_WithMissingStableId_Throws()
        {
            EnemyWaveDefinition wave =
                CreateWave(
                    "   ",
                    "Wave 1",
                    CreateEntry(1));

            Assert.That(
                () => wave.Validate(),
                Throws.InvalidOperationException);
        }

        [Test]
        public void Validate_WithMissingDisplayName_Throws()
        {
            EnemyWaveDefinition wave =
                CreateWave(
                    "wave_01",
                    "   ",
                    CreateEntry(1));

            Assert.That(
                () => wave.Validate(),
                Throws.InvalidOperationException);
        }

        [Test]
        public void Validate_WithNoEntries_Throws()
        {
            EnemyWaveDefinition wave =
                CreateWave(
                    "wave_01",
                    "Wave 1");

            Assert.That(
                () => wave.Validate(),
                Throws.InvalidOperationException);
        }

        [Test]
        public void Validate_WithNullEntry_ReportsEntryIndex()
        {
            EnemyWaveDefinition wave =
                CreateWave(
                    "wave_01",
                    "Wave 1",
                    CreateEntry(1),
                    null);

            System.InvalidOperationException exception =
                Assert.Throws<System.InvalidOperationException>(
                    wave.Validate);

            Assert.That(
                exception.Message,
                Does.Contain("index 1"));
        }

        [Test]
        public void Validate_WithInvalidEntry_WrapsEntryFailure()
        {
            EnemyWaveEntry invalidEntry =
                CreateEntry(1);

            WaveDefinitionTestFactory.SetPrivateField(
                invalidEntry,
                "_count",
                0);

            EnemyWaveDefinition wave =
                CreateWave(
                    "wave_01",
                    "Wave 1",
                    invalidEntry);

            System.InvalidOperationException exception =
                Assert.Throws<System.InvalidOperationException>(
                    wave.Validate);

            Assert.That(
                exception.Message,
                Does.Contain("index 0"));

            Assert.That(
                exception.InnerException,
                Is.Not.Null);
        }

        private EnemyWaveEntry CreateEntry(
            int count)
        {
            return new EnemyWaveEntry(
                _enemyController,
                _enemyDefinition,
                count);
        }

        private EnemyWaveDefinition CreateWave(
            string stableId,
            string displayName,
            params EnemyWaveEntry[] entries)
        {
            EnemyWaveDefinition wave =
                WaveDefinitionTestFactory.CreateWave(
                    stableId,
                    displayName,
                    entries);

            _createdWaves.Add(
                wave);

            return wave;
        }
    }
}