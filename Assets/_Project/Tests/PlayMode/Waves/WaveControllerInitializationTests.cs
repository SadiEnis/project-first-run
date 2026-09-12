using NUnit.Framework;
using ProjectFirstRun.Waves;
using UnityEngine;

namespace ProjectFirstRun.Tests.PlayMode.Waves
{
    public sealed class WaveControllerInitializationTests :
        WaveControllerTestFixture
    {
        [Test]
        public void Initialize_WithValidDependencies_CreatesReadyState()
        {
            EnemyWaveDefinition wave =
                CreateValidWave(1);

            ArenaWaveDefinition arena =
                CreateArena(wave);

            InitializeController(arena);

            Assert.That(
                Controller.IsInitialized,
                Is.True);

            Assert.That(
                Controller.HasBegun,
                Is.False);

            Assert.That(
                Controller.CurrentWaveIndex,
                Is.EqualTo(-1));

            Assert.That(
                Controller.CurrentWaveDefinition,
                Is.Null);

            Assert.That(
                Controller.CurrentWaveProgress,
                Is.Null);

            Assert.That(
                Controller.TrackedEnemyCount,
                Is.EqualTo(0));

            Assert.That(
                Controller.SequenceState,
                Is.Not.Null);

            Assert.That(
                Controller.SequenceState.Status,
                Is.EqualTo(WaveSequenceStatus.Ready));

            Assert.That(
                Controller.SequenceState.TotalWaves,
                Is.EqualTo(1));
        }

        [Test]
        public void Initialize_WithNullArenaDefinition_Throws()
        {
            Assert.That(
                () =>
                    Controller.Initialize(
                        null,
                        WaveEnemySpawner),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Initialize_WithNullWaveEnemySpawner_Throws()
        {
            EnemyWaveDefinition wave =
                CreateValidWave(1);

            ArenaWaveDefinition arena =
                CreateArena(wave);

            Assert.That(
                () =>
                    Controller.Initialize(
                        arena,
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Begin_BeforeInitialize_Throws()
        {
            Assert.That(
                Controller.Begin,
                Throws.InvalidOperationException);

            Assert.That(
                Controller.HasBegun,
                Is.False);
        }

        [Test]
        public void Begin_Twice_Throws()
        {
            EnemyWaveDefinition wave =
                CreateValidWave(1);

            ArenaWaveDefinition arena =
                CreateArena(wave);

            InitializeController(arena);

            Controller.Begin();

            Assert.That(
                Controller.Begin,
                Throws.InvalidOperationException);

            Assert.That(
                Controller.HasBegun,
                Is.True);

            Assert.That(
                Controller.SequenceState.Status,
                Is.EqualTo(WaveSequenceStatus.Running));
        }

        [Test]
        public void Initialize_WhileEnemiesAreTracked_Throws()
        {
            EnemyWaveDefinition wave =
                CreateValidWave(1);

            ArenaWaveDefinition arena =
                CreateArena(wave);

            InitializeController(arena);
            Controller.Begin();

            Assert.That(
                Controller.TrackedEnemyCount,
                Is.EqualTo(1));

            Assert.That(
                () =>
                    Controller.Initialize(
                        arena,
                        WaveEnemySpawner),
                Throws.InvalidOperationException);

            Assert.That(
                Controller.SequenceState.Status,
                Is.EqualTo(WaveSequenceStatus.Running));

            Assert.That(
                Controller.TrackedEnemyCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Begin_AfterArenaDefinitionDestroyed_ThrowsWithoutStarting()
        {
            EnemyWaveDefinition wave =
                CreateValidWave(1);

            ArenaWaveDefinition arena =
                CreateArena(wave);

            InitializeController(arena);

            Object.DestroyImmediate(arena);

            Assert.That(
                Controller.Begin,
                Throws.InvalidOperationException);

            Assert.That(
                Controller.HasBegun,
                Is.False);

            Assert.That(
                Controller.SequenceState.Status,
                Is.EqualTo(WaveSequenceStatus.Ready));

            Assert.That(
                Controller.TrackedEnemyCount,
                Is.EqualTo(0));
        }
    }
}