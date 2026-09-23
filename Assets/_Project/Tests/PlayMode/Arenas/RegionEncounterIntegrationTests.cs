using NUnit.Framework;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class RegionEncounterIntegrationTests : ArenaSessionControllerTestFixture
    {
        [Test]
        public void Reentry_PreservesLiveEnemyIdentityHealthAndSpawnCount()
        {
            InitializeArenaSession();
            using var region = new RegionEncounterSession("main_room");
            int spawns = 0;
            WaveController.EnemySpawned += (_, __) => spawns++;
            region.Prepare(ArenaSession);
            Assert.That(Registry.ActiveCount, Is.Zero);
            region.Enter();
            var enemy = SpawnedEnemy;
            var damage = new DamageInfo(1f, null, enemy.transform.position, Vector3.forward);
            enemy.Health.ApplyDamage(in damage);
            float remaining = enemy.Health.CurrentHealth;
            region.Leave();
            Assert.That(region.Status, Is.EqualTo(RegionEncounterStatus.Active));
            region.Enter();
            region.Enter();
            Assert.That(SpawnedEnemy, Is.SameAs(enemy));
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(remaining));
            Assert.That(Registry.ActiveCount, Is.EqualTo(1));
            Assert.That(spawns, Is.EqualTo(1));
        }

        [Test]
        public void CompletionOutsideThenReentry_DoesNotRespawn()
        {
            InitializeArenaSession();
            using var region = new RegionEncounterSession("side_room");
            int completions = 0;
            int spawns = 0;
            region.Completed += () => completions++;
            WaveController.EnemySpawned += (_, __) => spawns++;
            region.Prepare(ArenaSession);
            region.Enter();
            region.Leave();
            KillSpawnedEnemy();
            Assert.That(region.Status, Is.EqualTo(RegionEncounterStatus.Completed));
            Assert.That(region.IsPlayerInside, Is.False);
            region.Enter();
            Assert.That(completions, Is.EqualTo(1));
            Assert.That(spawns, Is.EqualTo(1));
            Assert.That(Registry.ActiveCount, Is.Zero);
        }

        [Test]
        public void PlayerDeathOutside_FailsEncounterWithoutAllowingRestart()
        {
            InitializeArenaSession();
            using var region = new RegionEncounterSession("side_room");
            region.Prepare(ArenaSession);
            region.Enter();
            region.Leave();
            PlayerDeathSource.Die();
            Assert.That(region.Status, Is.EqualTo(RegionEncounterStatus.Failed));
            Assert.That(ArenaSession.Status, Is.EqualTo(ArenaSessionStatus.Defeat));
            Assert.Throws<System.InvalidOperationException>(region.Enter);
        }
    }
}
