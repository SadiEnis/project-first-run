using System;
using NUnit.Framework;
using ProjectFirstRun.Waves;

namespace ProjectFirstRun.Tests.EditMode.Waves
{
    public sealed class WaveProgressStateTests
    {
        [Test]
        public void Constructor_WithPositiveTotal_InitializesState()
        {
            WaveProgressState state =
                new WaveProgressState(3);

            Assert.That(
                state.TotalPlannedEnemies,
                Is.EqualTo(3));

            Assert.That(
                state.SpawnedEnemies,
                Is.EqualTo(0));

            Assert.That(
                state.LivingEnemies,
                Is.EqualTo(0));

            Assert.That(
                state.DefeatedEnemies,
                Is.EqualTo(0));

            Assert.That(
                state.RemainingToSpawn,
                Is.EqualTo(3));

            Assert.That(
                state.IsSpawningCompleted,
                Is.False);

            Assert.That(
                state.IsCompleted,
                Is.False);
        }

        [Test]
        public void Constructor_WithZeroTotal_Throws()
        {
            Assert.That(
                () => new WaveProgressState(0),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_WithNegativeTotal_Throws()
        {
            Assert.That(
                () => new WaveProgressState(-1),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void RegisterSpawnedEnemy_UpdatesCounts()
        {
            WaveProgressState state =
                new WaveProgressState(3);

            state.RegisterSpawnedEnemy();

            Assert.That(
                state.SpawnedEnemies,
                Is.EqualTo(1));

            Assert.That(
                state.LivingEnemies,
                Is.EqualTo(1));

            Assert.That(
                state.DefeatedEnemies,
                Is.EqualTo(0));

            Assert.That(
                state.RemainingToSpawn,
                Is.EqualTo(2));

            Assert.That(
                state.IsCompleted,
                Is.False);
        }

        [Test]
        public void RegisterSpawnedEnemy_MultipleTimes_TracksRemainingEnemies()
        {
            WaveProgressState state =
                new WaveProgressState(3);

            state.RegisterSpawnedEnemy();
            state.RegisterSpawnedEnemy();
            state.RegisterSpawnedEnemy();

            Assert.That(
                state.SpawnedEnemies,
                Is.EqualTo(3));

            Assert.That(
                state.LivingEnemies,
                Is.EqualTo(3));

            Assert.That(
                state.RemainingToSpawn,
                Is.EqualTo(0));

            Assert.That(
                state.IsSpawningCompleted,
                Is.False);
        }

        [Test]
        public void RegisterSpawnedEnemy_BeyondPlannedTotal_Throws()
        {
            WaveProgressState state =
                new WaveProgressState(1);

            state.RegisterSpawnedEnemy();

            Assert.That(
                state.RegisterSpawnedEnemy,
                Throws.InvalidOperationException);
        }

        [Test]
        public void RegisterDefeatedEnemy_UpdatesCounts()
        {
            WaveProgressState state =
                new WaveProgressState(2);

            state.RegisterSpawnedEnemy();
            state.RegisterSpawnedEnemy();
            state.RegisterDefeatedEnemy();

            Assert.That(
                state.SpawnedEnemies,
                Is.EqualTo(2));

            Assert.That(
                state.LivingEnemies,
                Is.EqualTo(1));

            Assert.That(
                state.DefeatedEnemies,
                Is.EqualTo(1));
        }

        [Test]
        public void RegisterDefeatedEnemy_WithoutLivingEnemies_Throws()
        {
            WaveProgressState state =
                new WaveProgressState(1);

            Assert.That(
                state.RegisterDefeatedEnemy,
                Throws.InvalidOperationException);
        }

        [Test]
        public void MarkSpawningCompleted_BeforeAllEnemiesSpawned_Throws()
        {
            WaveProgressState state =
                new WaveProgressState(2);

            state.RegisterSpawnedEnemy();

            Assert.That(
                state.MarkSpawningCompleted,
                Throws.InvalidOperationException);

            Assert.That(
                state.IsSpawningCompleted,
                Is.False);
        }

        [Test]
        public void MarkSpawningCompleted_AfterAllEnemiesSpawned_SetsFlag()
        {
            WaveProgressState state =
                new WaveProgressState(2);

            state.RegisterSpawnedEnemy();
            state.RegisterSpawnedEnemy();
            state.MarkSpawningCompleted();

            Assert.That(
                state.IsSpawningCompleted,
                Is.True);

            Assert.That(
                state.LivingEnemies,
                Is.EqualTo(2));

            Assert.That(
                state.IsCompleted,
                Is.False);
        }

        [Test]
        public void MarkSpawningCompleted_Twice_Throws()
        {
            WaveProgressState state =
                new WaveProgressState(1);

            state.RegisterSpawnedEnemy();
            state.MarkSpawningCompleted();

            Assert.That(
                state.MarkSpawningCompleted,
                Throws.InvalidOperationException);
        }

        [Test]
        public void RegisterSpawnedEnemy_AfterSpawningCompleted_Throws()
        {
            WaveProgressState state =
                new WaveProgressState(1);

            state.RegisterSpawnedEnemy();
            state.MarkSpawningCompleted();

            Assert.That(
                state.RegisterSpawnedEnemy,
                Throws.InvalidOperationException);
        }

        [Test]
        public void IsCompleted_BecomesTrueOnlyAfterAllLivingEnemiesAreDefeated()
        {
            WaveProgressState state =
                new WaveProgressState(2);

            state.RegisterSpawnedEnemy();
            state.RegisterSpawnedEnemy();
            state.MarkSpawningCompleted();

            Assert.That(
                state.IsCompleted,
                Is.False);

            state.RegisterDefeatedEnemy();

            Assert.That(
                state.IsCompleted,
                Is.False);

            state.RegisterDefeatedEnemy();

            Assert.That(
                state.LivingEnemies,
                Is.EqualTo(0));

            Assert.That(
                state.DefeatedEnemies,
                Is.EqualTo(2));

            Assert.That(
                state.IsCompleted,
                Is.True);
        }

        [Test]
        public void DefeatBeforeSpawningCompletes_IsSupported()
        {
            WaveProgressState state =
                new WaveProgressState(2);

            state.RegisterSpawnedEnemy();
            state.RegisterDefeatedEnemy();

            Assert.That(
                state.SpawnedEnemies,
                Is.EqualTo(1));

            Assert.That(
                state.LivingEnemies,
                Is.EqualTo(0));

            Assert.That(
                state.DefeatedEnemies,
                Is.EqualTo(1));

            Assert.That(
                state.RemainingToSpawn,
                Is.EqualTo(1));

            Assert.That(
                state.IsCompleted,
                Is.False);

            state.RegisterSpawnedEnemy();
            state.MarkSpawningCompleted();

            Assert.That(
                state.IsCompleted,
                Is.False);

            state.RegisterDefeatedEnemy();

            Assert.That(
                state.SpawnedEnemies,
                Is.EqualTo(2));

            Assert.That(
                state.LivingEnemies,
                Is.EqualTo(0));

            Assert.That(
                state.DefeatedEnemies,
                Is.EqualTo(2));

            Assert.That(
                state.IsCompleted,
                Is.True);
        }
    }
}