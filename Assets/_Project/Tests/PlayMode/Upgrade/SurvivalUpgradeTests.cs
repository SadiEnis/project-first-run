#if UNITY_EDITOR
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Items;
using ProjectFirstRun.Player;
using ProjectFirstRun.Stats;
using ProjectFirstRun.Upgrades;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Tests.PlayMode.Upgrades
{
    public sealed class SurvivalUpgradeTests
    {
        private GameObject _player;
        private HealthComponent _health;
        private PlayerStatCollection _stats;
        private PlayerUpgradeController _upgrades;
        private PlayerSurvivalController _survival;

        [SetUp]
        public void SetUp()
        {
            _player = new GameObject("Survival test player");
            _health = _player.AddComponent<HealthComponent>();
            _stats = _player.AddComponent<PlayerStatsController>().Stats;
            _player.AddComponent<PlayerBuildController>().Initialize(new PlayerBuildCapacity(1, 1, 3));
            _upgrades = _player.AddComponent<PlayerUpgradeController>();
            _survival = _player.AddComponent<PlayerSurvivalController>();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_player);

        private static UpgradeDefinition Load(string name) =>
            AssetDatabase.LoadAssetAtPath<UpgradeDefinition>($"Assets/_Project/Data/Upgrade/UD_{name}.asset");
        private DamageResult Hit(float damage) =>
            _health.ApplyDamage(new DamageInfo(damage, null, Vector3.zero, Vector3.zero));

        [Test]
        public void AllFiveLevelsWorkTogetherWithoutCompounding()
        {
            Hit(40);
            var heart = Load("SecondHeart");
            var armor = Load("Ironhide");
            var regen = Load("Lifesprout");
            foreach (var item in new[] { heart, armor, regen })
            {
                Assert.That(item.MaximumLevel, Is.EqualTo(5));
                Assert.That(_upgrades.TryAcquire(item), Is.EqualTo(UpgradeAcquireResult.Acquired));
            }
            for (int level = 1; level <= 5; level++)
            {
                if (level > 1)
                    foreach (var item in new[] { heart, armor, regen })
                        Assert.That(_upgrades.TryLevelUp(item), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
                Assert.That(_health.MaximumHealth, Is.EqualTo(100 + level * 10).Within(.001f));
                Assert.That(_health.CurrentHealth, Is.EqualTo(60 + level * 10).Within(.001f));
                Assert.That(_health.DamageReduction, Is.EqualTo(level * .05f).Within(.001f));
                Assert.That(_survival.RegenerationPerSecond, Is.EqualTo(level * .5f));
                Assert.That(_stats.ModifierCount, Is.EqualTo(3));
            }
            foreach (var item in new[] { heart, armor, regen })
                Assert.That(_upgrades.TryLevelUp(item), Is.EqualTo(ItemLevelUpResult.MaximumLevelReached));
            _stats.Add(new StatModifier(PlayerStatType.DamageReduction, StatModifierOperation.Flat, .6f, "other"));
            Assert.That(_health.DamageReduction, Is.EqualTo(.75f));
            Assert.That(Hit(20).AppliedDamage, Is.EqualTo(5));
            Assert.That(_health.CurrentHealth, Is.EqualTo(105).Within(.001f));
        }

        [Test]
        public void RegenerationDoesNotBankAndPausesAndCannotRevive()
        {
            var regen = Load("Lifesprout");
            _upgrades.TryAcquire(regen);
            _survival.TickRegeneration(20, true); // Full health must not bank ticks.
            Hit(10);
            _survival.TickRegeneration(.5f, true);
            Assert.That(_health.CurrentHealth, Is.EqualTo(90));
            _upgrades.TryLevelUp(regen);
            Assert.That(_health.CurrentHealth, Is.EqualTo(90));
            _survival.TickRegeneration(.5f, true);
            Assert.That(_health.CurrentHealth, Is.EqualTo(91)); // Current level, one tick.
            _survival.TickRegeneration(100, false);
            _survival.TickRegeneration(.5f, true);
            Assert.That(_health.CurrentHealth, Is.EqualTo(91));
            _survival.TickRegeneration(.5f, true);
            Assert.That(_health.CurrentHealth, Is.EqualTo(92));
            Hit(1000);
            _survival.TickRegeneration(100, true);
            _upgrades.TryAcquire(Load("SecondHeart"));
            Assert.That(_health.IsDead, Is.True);
            Assert.That(_health.CurrentHealth, Is.Zero);
        }

        [Test]
        public void ReenableDoesNotGrantHealthOrDuplicateSubscriptionsAndEnemiesStayUnarmored()
        {
            _upgrades.TryAcquire(Load("SecondHeart"));
            Hit(30);
            float injured = _health.CurrentHealth;
            for (int i = 0; i < 3; i++) { _survival.enabled = false; _survival.enabled = true; }
            Assert.That(_health.CurrentHealth, Is.EqualTo(injured));
            int events = 0;
            _health.HealthChanged += (_, __) => events++;
            _upgrades.TryLevelUp(Load("SecondHeart"));
            Assert.That(events, Is.EqualTo(1));
            _upgrades.TryAcquire(Load("Ironhide"));
            var enemy = new GameObject("Unarmored enemy");
            try
            {
                var health = enemy.AddComponent<HealthComponent>();
                Assert.That(health.ApplyDamage(new DamageInfo(20, null, Vector3.zero, Vector3.zero)).AppliedDamage,
                    Is.EqualTo(20));
            }
            finally { Object.DestroyImmediate(enemy); }
        }

        [Test]
        public void SceneMovePreservesStatsAndInjuryWhileFreshPlayerStartsClean()
        {
            var source = _player.scene;
            var target = SceneManager.CreateScene("Survival upgrade destination");
            try
            {
                _upgrades.TryAcquire(Load("SecondHeart"));
                _upgrades.TryAcquire(Load("Lifesprout"));
                Hit(20);
                SceneManager.MoveGameObjectToScene(_player, target);
                Assert.That(_health.MaximumHealth, Is.EqualTo(110).Within(.001f));
                Assert.That(_health.CurrentHealth, Is.EqualTo(90).Within(.001f));
                _survival.TickRegeneration(1, true);
                Assert.That(_health.CurrentHealth, Is.EqualTo(90.5f).Within(.001f));
                var fresh = new GameObject("Fresh run player");
                try
                {
                    var health = fresh.AddComponent<HealthComponent>();
                    fresh.AddComponent<PlayerStatsController>();
                    var survival = fresh.AddComponent<PlayerSurvivalController>();
                    Assert.That(health.MaximumHealth, Is.EqualTo(100));
                    Assert.That(health.DamageReduction, Is.Zero);
                    Assert.That(survival.RegenerationPerSecond, Is.Zero);
                }
                finally { Object.DestroyImmediate(fresh); }
            }
            finally
            {
                SceneManager.MoveGameObjectToScene(_player, source);
                SceneManager.UnloadSceneAsync(target);
            }
        }
    }
}
#endif
