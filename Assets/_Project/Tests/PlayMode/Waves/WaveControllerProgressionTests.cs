using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Waves;
using UnityEngine;

namespace ProjectFirstRun.Tests.PlayMode.Waves
{
    public sealed class WaveControllerProgressionTests :
        WaveControllerTestFixture
    {
        [Test]
        public void Begin_StartsFirstWaveAndTracksSpawnedEnemies()
        {
            EnemyWaveDefinition wave =
                CreateValidWave(2);

            ArenaWaveDefinition arena =
                CreateArena(wave);

            InitializeController(arena);
            Controller.Begin();

            Assert.That(
                Controller.HasBegun,
                Is.True);

            Assert.That(
                Controller.CurrentWaveIndex,
                Is.EqualTo(0));

            Assert.That(
                Controller.CurrentWaveDefinition,
                Is.SameAs(wave));

            Assert.That(
                Controller.SequenceState.Status,
                Is.EqualTo(WaveSequenceStatus.Running));

            Assert.That(
                Controller.CurrentWaveProgress,
                Is.Not.Null);

            Assert.That(
                Controller.CurrentWaveProgress.TotalPlannedEnemies,
                Is.EqualTo(2));

            Assert.That(
                Controller.CurrentWaveProgress.SpawnedEnemies,
                Is.EqualTo(2));

            Assert.That(
                Controller.CurrentWaveProgress.LivingEnemies,
                Is.EqualTo(2));

            Assert.That(
                Controller.CurrentWaveProgress.DefeatedEnemies,
                Is.EqualTo(0));

            Assert.That(
                Controller.CurrentWaveProgress.IsSpawningCompleted,
                Is.True);

            Assert.That(
                Controller.TrackedEnemyCount,
                Is.EqualTo(2));

            Assert.That(
                Registry.ActiveCount,
                Is.EqualTo(2));
        }

        [Test]
        public void Begin_PublishesWaveStartedAndEnemySpawnedEvents()
        {
            EnemyWaveDefinition wave =
                CreateValidWave(3);

            ArenaWaveDefinition arena =
                CreateArena(wave);

            int waveStartedCount = 0;
            int startedWaveIndex = -1;
            EnemyWaveDefinition startedWave = null;

            int enemySpawnedCount = 0;
            List<int> spawnedWaveIndices =
                new List<int>();

            Controller.WaveStarted +=
                (waveIndex, definition) =>
                {
                    waveStartedCount++;
                    startedWaveIndex = waveIndex;
                    startedWave = definition;
                };

            Controller.EnemySpawned +=
                (waveIndex, _) =>
                {
                    enemySpawnedCount++;
                    spawnedWaveIndices.Add(waveIndex);
                };

            InitializeController(arena);
            Controller.Begin();

            Assert.That(
                waveStartedCount,
                Is.EqualTo(1));

            Assert.That(
                startedWaveIndex,
                Is.EqualTo(0));

            Assert.That(
                startedWave,
                Is.SameAs(wave));

            Assert.That(
                enemySpawnedCount,
                Is.EqualTo(3));

            Assert.That(
                spawnedWaveIndices,
                Is.EqualTo(new[] { 0, 0, 0 }));
        }

        [Test]
        public void Begin_WithMultipleEntries_PreservesEntryOrder()
        {
            EnemyWaveDefinition wave =
                CreateWave(
                    CreateEntry(
                        ValidPrefab,
                        FirstDefinition,
                        2),

                    CreateEntry(
                        ValidPrefab,
                        SecondDefinition,
                        1));

            ArenaWaveDefinition arena =
                CreateArena(wave);

            InitializeController(arena);
            Controller.Begin();

            Assert.That(
                SpawnedInstances.Count,
                Is.EqualTo(3));

            Assert.That(
                GetSpawnedEnemy(0).Definition,
                Is.SameAs(FirstDefinition));

            Assert.That(
                GetSpawnedEnemy(1).Definition,
                Is.SameAs(FirstDefinition));

            Assert.That(
                GetSpawnedEnemy(2).Definition,
                Is.SameAs(SecondDefinition));
        }

        [Test]
        public void EnemyDeath_UpdatesProgressAndPublishesDefeatedEvent()
        {
            EnemyWaveDefinition wave =
                CreateValidWave(2);

            ArenaWaveDefinition arena =
                CreateArena(wave);

            int enemyDefeatedCount = 0;
            int defeatedWaveIndex = -1;
            EnemyController defeatedEnemy = null;

            Controller.EnemyDefeated +=
                (waveIndex, enemy) =>
                {
                    enemyDefeatedCount++;
                    defeatedWaveIndex = waveIndex;
                    defeatedEnemy = enemy;
                };

            InitializeController(arena);
            Controller.Begin();

            EnemyController firstEnemy =
                GetSpawnedEnemy(0);

            KillEnemy(firstEnemy);

            Assert.That(
                enemyDefeatedCount,
                Is.EqualTo(1));

            Assert.That(
                defeatedWaveIndex,
                Is.EqualTo(0));

            Assert.That(
                defeatedEnemy,
                Is.SameAs(firstEnemy));

            Assert.That(
                Controller.CurrentWaveProgress.LivingEnemies,
                Is.EqualTo(1));

            Assert.That(
                Controller.CurrentWaveProgress.DefeatedEnemies,
                Is.EqualTo(1));

            Assert.That(
                Controller.TrackedEnemyCount,
                Is.EqualTo(1));

            Assert.That(
                Registry.ActiveCount,
                Is.EqualTo(1));

            Assert.That(
                Controller.SequenceState.Status,
                Is.EqualTo(WaveSequenceStatus.Running));
        }

        [Test]
        public void UnrelatedEnemyDeath_DoesNotAffectWaveProgress()
        {
            EnemyWaveDefinition wave =
                CreateValidWave(1);

            ArenaWaveDefinition arena =
                CreateArena(wave);

            EnemyController unrelatedEnemy =
                CreateUnrelatedEnemy();

            int enemyDefeatedCount = 0;

            Controller.EnemyDefeated +=
                (_, _) => enemyDefeatedCount++;

            InitializeController(arena);
            Controller.Begin();

            Assert.That(
                Registry.ActiveCount,
                Is.EqualTo(2));

            KillEnemy(unrelatedEnemy);

            Assert.That(
                enemyDefeatedCount,
                Is.EqualTo(0));

            Assert.That(
                Controller.CurrentWaveProgress.LivingEnemies,
                Is.EqualTo(1));

            Assert.That(
                Controller.CurrentWaveProgress.DefeatedEnemies,
                Is.EqualTo(0));

            Assert.That(
                Controller.TrackedEnemyCount,
                Is.EqualTo(1));

            Assert.That(
                Controller.SequenceState.CompletedWaves,
                Is.EqualTo(0));

            Assert.That(
                Controller.SequenceState.Status,
                Is.EqualTo(WaveSequenceStatus.Running));
        }

        [Test]
        public void FinalEnemyDeath_CompletesWaveAndStartsNextWave()
        {
            EnemyWaveDefinition firstWave =
                CreateValidWave(2);

            EnemyWaveDefinition secondWave =
                CreateValidWave(1);

            ArenaWaveDefinition arena =
                CreateArena(
                    firstWave,
                    secondWave);

            int waveStartedCount = 0;
            int waveCompletedCount = 0;

            Controller.WaveStarted +=
                (_, _) => waveStartedCount++;

            Controller.WaveCompleted +=
                (_, _) => waveCompletedCount++;

            InitializeController(arena);
            Controller.Begin();

            EnemyController firstEnemy =
                GetSpawnedEnemy(0);

            EnemyController secondEnemy =
                GetSpawnedEnemy(1);

            KillEnemy(firstEnemy);
            KillEnemy(secondEnemy);

            Assert.That(
                waveStartedCount,
                Is.EqualTo(2));

            Assert.That(
                waveCompletedCount,
                Is.EqualTo(1));

            Assert.That(
                Controller.SequenceState.CompletedWaves,
                Is.EqualTo(1));

            Assert.That(
                Controller.SequenceState.Status,
                Is.EqualTo(WaveSequenceStatus.Running));

            Assert.That(
                Controller.CurrentWaveIndex,
                Is.EqualTo(1));

            Assert.That(
                Controller.CurrentWaveDefinition,
                Is.SameAs(secondWave));

            Assert.That(
                Controller.CurrentWaveProgress.TotalPlannedEnemies,
                Is.EqualTo(1));

            Assert.That(
                Controller.CurrentWaveProgress.LivingEnemies,
                Is.EqualTo(1));

            Assert.That(
                Controller.TrackedEnemyCount,
                Is.EqualTo(1));

            Assert.That(
                SpawnedInstances.Count,
                Is.EqualTo(3));

            Assert.That(
                Registry.ActiveCount,
                Is.EqualTo(1));
        }

        [Test]
        public void WaveCompleted_IsPublishedBeforeNextWaveStarts()
        {
            EnemyWaveDefinition firstWave =
                CreateValidWave(1);

            EnemyWaveDefinition secondWave =
                CreateValidWave(1);

            ArenaWaveDefinition arena =
                CreateArena(
                    firstWave,
                    secondWave);

            List<string> eventOrder =
                new List<string>();

            Controller.WaveStarted +=
                (waveIndex, _) =>
                    eventOrder.Add(
                        $"Started:{waveIndex}");

            Controller.WaveCompleted +=
                (waveIndex, _) =>
                    eventOrder.Add(
                        $"Completed:{waveIndex}");

            InitializeController(arena);
            Controller.Begin();

            KillEnemy(
                GetSpawnedEnemy(0));

            Assert.That(
                eventOrder,
                Is.EqualTo(
                    new[]
                    {
                        "Started:0",
                        "Completed:0",
                        "Started:1"
                    }));
        }

        [Test]
        public void FinalWaveCompletion_CompletesSequenceOnce()
        {
            EnemyWaveDefinition wave =
                CreateValidWave(1);

            ArenaWaveDefinition arena =
                CreateArena(wave);

            int waveCompletedCount = 0;
            int sequenceCompletedCount = 0;

            Controller.WaveCompleted +=
                (_, _) => waveCompletedCount++;

            Controller.SequenceCompleted +=
                () => sequenceCompletedCount++;

            InitializeController(arena);
            Controller.Begin();

            EnemyController enemy =
                GetSpawnedEnemy(0);

            KillEnemy(enemy);

            Assert.That(
                waveCompletedCount,
                Is.EqualTo(1));

            Assert.That(
                sequenceCompletedCount,
                Is.EqualTo(1));

            Assert.That(
                Controller.SequenceState.Status,
                Is.EqualTo(WaveSequenceStatus.Completed));

            Assert.That(
                Controller.SequenceState.CompletedWaves,
                Is.EqualTo(1));

            Assert.That(
                Controller.SequenceState.IsCompleted,
                Is.True);

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

            ApplyDamage(enemy, 25f);
            ApplyDamage(enemy, 25f);

            Assert.That(
                waveCompletedCount,
                Is.EqualTo(1));

            Assert.That(
                sequenceCompletedCount,
                Is.EqualTo(1));
        }

        private EnemyController GetSpawnedEnemy(
            int index)
        {
            Assert.That(
                SpawnedInstances.Count,
                Is.GreaterThan(index),
                $"No spawned instance exists at index {index}.");

            GameObject instance =
                SpawnedInstances[index];

            Assert.That(
                instance,
                Is.Not.Null);

            EnemyController enemy =
                instance.GetComponent<EnemyController>();

            Assert.That(
                enemy,
                Is.Not.Null);

            return enemy;
        }
    }
}