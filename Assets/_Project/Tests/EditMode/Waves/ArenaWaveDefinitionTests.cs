using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Waves;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectFirstRun.Tests.EditMode.Waves
{
    public sealed class ArenaWaveDefinitionTests
    {
        private readonly List<EnemyWaveDefinition> _createdWaves =
            new List<EnemyWaveDefinition>();

        private readonly List<ArenaWaveDefinition> _createdArenas =
            new List<ArenaWaveDefinition>();

        private GameObject _enemyObject;
        private EnemyController _enemyController;
        private EnemyDefinition _enemyDefinition;

        [SetUp]
        public void SetUp()
        {
            _enemyObject =
                new GameObject("ArenaWave_TestEnemy");

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
            foreach (ArenaWaveDefinition arena in _createdArenas)
            {
                if (arena != null)
                {
                    Object.DestroyImmediate(
                        arena);
                }
            }

            foreach (EnemyWaveDefinition wave in _createdWaves)
            {
                if (wave != null)
                {
                    Object.DestroyImmediate(
                        wave);
                }
            }

            _createdArenas.Clear();
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
        public void Validate_WithValidWaves_DoesNotThrow()
        {
            EnemyWaveDefinition firstWave =
                CreateWave(
                    "wave_01",
                    "Wave 1",
                    2);

            EnemyWaveDefinition secondWave =
                CreateWave(
                    "wave_02",
                    "Wave 2",
                    3);

            ArenaWaveDefinition arena =
                CreateArena(
                    "arena_01",
                    firstWave,
                    secondWave);

            Assert.DoesNotThrow(
                arena.Validate);

            Assert.That(
                arena.WaveCount,
                Is.EqualTo(2));

            Assert.That(
                arena.TotalPlannedEnemyCount,
                Is.EqualTo(5));
        }

        [Test]
        public void Waves_PreserveDefinedOrder()
        {
            EnemyWaveDefinition firstWave =
                CreateWave(
                    "wave_01",
                    "Wave 1",
                    1);

            EnemyWaveDefinition secondWave =
                CreateWave(
                    "wave_02",
                    "Wave 2",
                    1);

            ArenaWaveDefinition arena =
                CreateArena(
                    "arena_01",
                    firstWave,
                    secondWave);

            Assert.That(
                arena.Waves[0],
                Is.SameAs(firstWave));

            Assert.That(
                arena.Waves[1],
                Is.SameAs(secondWave));
        }

        [Test]
        public void Validate_WithMissingStableId_Throws()
        {
            EnemyWaveDefinition wave =
                CreateWave(
                    "wave_01",
                    "Wave 1",
                    1);

            ArenaWaveDefinition arena =
                CreateArena(
                    "   ",
                    wave);

            Assert.That(
                () => arena.Validate(),
                Throws.InvalidOperationException);
        }

        [Test]
        public void Validate_WithNoWaves_Throws()
        {
            ArenaWaveDefinition arena =
                CreateArena(
                    "arena_01");

            Assert.That(
                () => arena.Validate(),
                Throws.InvalidOperationException);
        }

        [Test]
        public void Validate_WithNullWave_ReportsWaveIndex()
        {
            EnemyWaveDefinition validWave =
                CreateWave(
                    "wave_01",
                    "Wave 1",
                    1);

            ArenaWaveDefinition arena =
                CreateArena(
                    "arena_01",
                    validWave,
                    null);

            System.InvalidOperationException exception =
                Assert.Throws<System.InvalidOperationException>(
                    arena.Validate);

            Assert.That(
                exception.Message,
                Does.Contain("index 1"));
        }

        [Test]
        public void Validate_WithInvalidNestedWave_WrapsFailure()
        {
            EnemyWaveDefinition invalidWave =
                CreateWave(
                    "wave_01",
                    string.Empty,
                    1);

            ArenaWaveDefinition arena =
                CreateArena(
                    "arena_01",
                    invalidWave);

            System.InvalidOperationException exception =
                Assert.Throws<System.InvalidOperationException>(
                    arena.Validate);

            Assert.That(
                exception.Message,
                Does.Contain("index 0"));

            Assert.That(
                exception.InnerException,
                Is.Not.Null);
        }

        private EnemyWaveDefinition CreateWave(
            string stableId,
            string displayName,
            int enemyCount)
        {
            EnemyWaveEntry entry =
                new EnemyWaveEntry(
                    _enemyController,
                    _enemyDefinition,
                    enemyCount);

            EnemyWaveDefinition wave =
                WaveDefinitionTestFactory.CreateWave(
                    stableId,
                    displayName,
                    entry);

            _createdWaves.Add(
                wave);

            return wave;
        }

        private ArenaWaveDefinition CreateArena(
            string stableId,
            params EnemyWaveDefinition[] waves)
        {
            ArenaWaveDefinition arena =
                WaveDefinitionTestFactory.CreateArena(
                    stableId,
                    waves);

            _createdArenas.Add(
                arena);

            return arena;
        }
    }
}