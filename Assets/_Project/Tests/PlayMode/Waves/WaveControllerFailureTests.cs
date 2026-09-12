using System.Collections;
using NUnit.Framework;
using ProjectFirstRun.Waves;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Waves
{
    public sealed class WaveControllerFailureTests :
        WaveControllerTestFixture
    {
        [Test]
        public void Begin_WhenSpawnFails_MarksSequenceAsFailed()
        {
            EnemyWaveDefinition wave =
                CreatePartiallyFailingWave();

            ArenaWaveDefinition arena =
                CreateArena(wave);

            InitializeController(arena);

            Assert.That(
                Controller.Begin,
                Throws.InvalidOperationException);

            Assert.That(
                Controller.HasBegun,
                Is.True);

            Assert.That(
                Controller.SequenceState.Status,
                Is.EqualTo(WaveSequenceStatus.Failed));

            Assert.That(
                Controller.SequenceState.IsFailed,
                Is.True);

            Assert.That(
                Controller.SequenceState.IsCompleted,
                Is.False);

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
                Registry.ActiveCount,
                Is.EqualTo(0));
        }

        [Test]
        public void SpawnFailure_DoesNotPublishSuccessfulProgressEvents()
        {
            EnemyWaveDefinition wave =
                CreatePartiallyFailingWave();

            ArenaWaveDefinition arena =
                CreateArena(wave);

            int waveStartedCount = 0;
            int enemySpawnedCount = 0;
            int enemyDefeatedCount = 0;
            int waveCompletedCount = 0;
            int sequenceCompletedCount = 0;

            Controller.WaveStarted +=
                (_, _) => waveStartedCount++;

            Controller.EnemySpawned +=
                (_, _) => enemySpawnedCount++;

            Controller.EnemyDefeated +=
                (_, _) => enemyDefeatedCount++;

            Controller.WaveCompleted +=
                (_, _) => waveCompletedCount++;

            Controller.SequenceCompleted +=
                () => sequenceCompletedCount++;

            InitializeController(arena);

            Assert.That(
                Controller.Begin,
                Throws.InvalidOperationException);

            // WaveStarted is published before the spawn operation begins.
            Assert.That(
                waveStartedCount,
                Is.EqualTo(1));

            // EnemySpawned is only published after the entire wave
            // has been spawned successfully.
            Assert.That(
                enemySpawnedCount,
                Is.EqualTo(0));

            Assert.That(
                enemyDefeatedCount,
                Is.EqualTo(0));

            Assert.That(
                waveCompletedCount,
                Is.EqualTo(0));

            Assert.That(
                sequenceCompletedCount,
                Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator SpawnFailure_CleansPartialWaveInstances()
        {
            EnemyWaveDefinition wave =
                CreatePartiallyFailingWave();

            ArenaWaveDefinition arena =
                CreateArena(wave);

            InitializeController(arena);

            Assert.That(
                Controller.Begin,
                Throws.InvalidOperationException);

            // Instances are disabled immediately, which unregisters
            // successfully spawned enemies before deferred destruction.
            Assert.That(
                Registry.ActiveCount,
                Is.EqualTo(0));

            Assert.That(
                Controller.TrackedEnemyCount,
                Is.EqualTo(0));

            // Destroy is deferred until the end of the frame in Play Mode.
            yield return null;

            Assert.That(
                CountNamedInstances(
                    $"{ValidPrefab.name}_Instance"),
                Is.EqualTo(0));

            Assert.That(
                CountNamedInstances(
                    $"{MissingAttackPrefab.name}_Instance"),
                Is.EqualTo(0));
        }

        [Test]
        public void Begin_AfterSequenceFailed_DoesNotRestartSequence()
        {
            EnemyWaveDefinition wave =
                CreatePartiallyFailingWave();

            ArenaWaveDefinition arena =
                CreateArena(wave);

            int waveStartedCount = 0;

            Controller.WaveStarted +=
                (_, _) => waveStartedCount++;

            InitializeController(arena);

            Assert.That(
                Controller.Begin,
                Throws.InvalidOperationException);

            Assert.That(
                Controller.SequenceState.IsFailed,
                Is.True);

            Assert.That(
                Controller.Begin,
                Throws.InvalidOperationException);

            Assert.That(
                waveStartedCount,
                Is.EqualTo(1));

            Assert.That(
                Controller.SequenceState.Status,
                Is.EqualTo(WaveSequenceStatus.Failed));

            Assert.That(
                Controller.TrackedEnemyCount,
                Is.EqualTo(0));
        }

        [Test]
        public void Begin_AfterTargetDamageableDestroyed_MarksSequenceAsFailed()
        {
            EnemyWaveDefinition wave =
                CreateValidWave(1);

            ArenaWaveDefinition arena =
                CreateArena(wave);

            InitializeController(arena);

            Object.DestroyImmediate(
                TargetHealth);

            Assert.That(
                Controller.Begin,
                Throws.InvalidOperationException);

            Assert.That(
                Controller.HasBegun,
                Is.True);

            Assert.That(
                Controller.SequenceState.Status,
                Is.EqualTo(WaveSequenceStatus.Failed));

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
                Registry.ActiveCount,
                Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Initialize_AfterFailure_AllowsFreshSequence()
        {
            EnemyWaveDefinition failingWave =
                CreatePartiallyFailingWave();

            ArenaWaveDefinition failingArena =
                CreateArena(failingWave);

            InitializeController(failingArena);

            Assert.That(
                Controller.Begin,
                Throws.InvalidOperationException);

            Assert.That(
                Controller.SequenceState.IsFailed,
                Is.True);

            // Wait for deferred destruction of the failed wave instances.
            yield return null;

            EnemyWaveDefinition validWave =
                CreateValidWave(1);

            ArenaWaveDefinition validArena =
                CreateArena(validWave);

            Assert.DoesNotThrow(
                () => InitializeController(validArena));

            Assert.That(
                Controller.IsInitialized,
                Is.True);

            Assert.That(
                Controller.HasBegun,
                Is.False);

            Assert.That(
                Controller.SequenceState.Status,
                Is.EqualTo(WaveSequenceStatus.Ready));

            Assert.That(
                Controller.CurrentWaveIndex,
                Is.EqualTo(-1));

            Assert.DoesNotThrow(
                Controller.Begin);

            Assert.That(
                Controller.HasBegun,
                Is.True);

            Assert.That(
                Controller.SequenceState.Status,
                Is.EqualTo(WaveSequenceStatus.Running));

            Assert.That(
                Controller.CurrentWaveIndex,
                Is.EqualTo(0));

            Assert.That(
                Controller.TrackedEnemyCount,
                Is.EqualTo(1));

            Assert.That(
                Registry.ActiveCount,
                Is.EqualTo(1));
        }

        private EnemyWaveDefinition CreatePartiallyFailingWave()
        {
            return CreateWave(
                CreateEntry(
                    ValidPrefab,
                    FirstDefinition,
                    1),

                CreateEntry(
                    MissingAttackPrefab,
                    SecondDefinition,
                    1));
        }
    }
}