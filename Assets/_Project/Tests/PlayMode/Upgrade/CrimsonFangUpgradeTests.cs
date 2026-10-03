#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Abilities.Fireball;
using ProjectFirstRun.Abilities.Shuriken;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Items;
using ProjectFirstRun.Player;
using ProjectFirstRun.Stats;
using ProjectFirstRun.Upgrades;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectFirstRun.Tests.PlayMode.Upgrades
{
    public sealed class CrimsonFangUpgradeTests
    {
        private readonly List<Object> _objects = new List<Object>();
        private GameObject _player;
        private HealthComponent _health;
        private PlayerUpgradeController _upgrades;
        private EnemyRegistry _registry;
        private EnemyDefinition _enemyDefinition;
        private UpgradeDefinition _fang;
        private GameObject Make(string name)
        {
            var go = new GameObject(name); _objects.Add(go); return go;
        }
        [SetUp]
        public void SetUp()
        {
            _player = Make("Kill healing player");
            _health = _player.AddComponent<HealthComponent>();
            _player.AddComponent<PlayerStatsController>();
            _player.AddComponent<PlayerBuildController>().Initialize(new PlayerBuildCapacity(1, 1, 3));
            _upgrades = _player.AddComponent<PlayerUpgradeController>();
            _player.AddComponent<PlayerKillHealingController>();
            _registry = Make("Registry").AddComponent<EnemyRegistry>();
            _enemyDefinition = ScriptableObject.CreateInstance<EnemyDefinition>();
            _objects.Add(_enemyDefinition);
            _fang = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>("Assets/_Project/Data/Upgrade/UD_CrimsonFang.asset");
        }
        [TearDown]
        public void TearDown()
        {
            for (int i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
            _objects.Clear();
        }
        private EnemyController Enemy()
        {
            var go = Make("Victim");
            go.SetActive(false);
            go.AddComponent<NavMeshAgent>().enabled = false; // Stationary health fixture, no baked map needed.
            var enemy = go.AddComponent<EnemyController>();
            go.AddComponent<BoxCollider>();
            go.SetActive(true);
            enemy.Initialize(_enemyDefinition, _player.transform, _registry);
            return enemy;
        }
        private static DamageResult Hit(HealthComponent target, float amount, GameObject source) =>
            target.ApplyDamage(new DamageInfo(amount, source, Vector3.zero, Vector3.forward));
        private void Kill(EnemyController enemy, GameObject source) => Hit(enemy.Health, 10000, source);

        [Test]
        public void ThreeLevelsHealTwoFiveSevenAndDeathsOnlyCountOnce()
        {
            Hit(_health, 50, null);
            Assert.That(_fang.MaximumLevel, Is.EqualTo(3));
            Assert.That(_upgrades.TryAcquire(_fang), Is.EqualTo(UpgradeAcquireResult.Acquired));
            float expected = 50;
            var values = new[] { 2, 5, 7 };
            for (int i = 0; i < 3; i++)
            {
                if (i > 0) Assert.That(_upgrades.TryLevelUp(_fang), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
                var enemy = Enemy();
                Kill(enemy, _player); Kill(enemy, _player);
                expected += values[i];
                Assert.That(_health.CurrentHealth, Is.EqualTo(expected));
            }
            Assert.That(_upgrades.TryLevelUp(_fang), Is.EqualTo(ItemLevelUpResult.MaximumLevelReached));
            Assert.That(_upgrades.TryAcquire(_fang), Is.EqualTo(UpgradeAcquireResult.AlreadyOwned));
        }

        [Test]
        public void UnrelatedOrMissingLethalSourceAndDespawnDoNotHeal()
        {
            _upgrades.TryAcquire(_fang); Hit(_health, 50, null);
            var enemy = Enemy();
            Hit(enemy.Health, 1, _player);
            Kill(enemy, Make("Unrelated source"));
            Kill(Enemy(), null);
            var despawn = Enemy(); despawn.gameObject.SetActive(false);
            Assert.That(_health.CurrentHealth, Is.EqualTo(50));
        }

        [Test]
        public void BurnAndBleedUseCurrentLevelAtKill()
        {
            _upgrades.TryAcquire(_fang); Hit(_health, 50, null);
            var burnTarget = Enemy();
            FireballBurn.Apply(_player, burnTarget.Health, burnTarget.GetComponent<Collider>(), 10000, 1);
            _upgrades.TryLevelUp(_fang);
            burnTarget.GetComponent<FireballBurn>().Advance(.5f);
            Assert.That(_health.CurrentHealth, Is.EqualTo(55));
            var bleedTarget = Enemy();
            ShurikenBleed.Apply(_player, bleedTarget, 10000);
            _upgrades.TryLevelUp(_fang);
            bleedTarget.GetComponent<ShurikenBleed>().Advance(.5f);
            Assert.That(_health.CurrentHealth, Is.EqualTo(62));
        }

        [Test]
        public void PooledReuseAndNewRegistryNeedNoRebinding()
        {
            _upgrades.TryAcquire(_fang); Hit(_health, 50, null);
            var enemy = Enemy();
            Kill(enemy, _player);
            enemy.gameObject.SetActive(false);
            _registry = Make("Next map registry").AddComponent<EnemyRegistry>();
            enemy.Initialize(_enemyDefinition, _player.transform, _registry);
            enemy.gameObject.SetActive(true);
            var child = Make("Player-owned damage source");
            child.transform.SetParent(_player.transform);
            Kill(enemy, child);
            Kill(Enemy(), _player);
            Assert.That(_health.CurrentHealth, Is.EqualTo(56));
        }

        [Test]
        public void FullHealthDoesNotBankAndDeadPlayerCannotReviveFromBleed()
        {
            _upgrades.TryAcquire(_fang);
            Kill(Enemy(), _player);
            Hit(_health, 1, null);
            Assert.That(_health.CurrentHealth, Is.EqualTo(99));
            Kill(Enemy(), _player);
            Assert.That(_health.CurrentHealth, Is.EqualTo(100));
            var victim = Enemy();
            ShurikenBleed.Apply(_player, victim, 10000);
            Hit(_health, 100, null);
            victim.GetComponent<ShurikenBleed>().Advance(.5f);
            Assert.That(victim.IsDead, Is.True);
            Assert.That(_health.IsDead, Is.True);
        }

        [Test]
        public void HealingCanTurnOffLastStand()
        {
            _player.AddComponent<PlayerLastStandController>();
            var stand = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>("Assets/_Project/Data/Upgrade/UD_LastStand.asset");
            _upgrades.TryAcquire(stand); _upgrades.TryAcquire(_fang);
            Hit(_health, 70, null);
            Assert.That(_player.GetComponent<PlayerLastStandController>().IsBonusActive, Is.True);
            Kill(Enemy(), _player);
            Assert.That(_health.CurrentHealth, Is.EqualTo(32));
            Assert.That(_player.GetComponent<PlayerLastStandController>().IsBonusActive, Is.False);
        }
    }
}
#endif
