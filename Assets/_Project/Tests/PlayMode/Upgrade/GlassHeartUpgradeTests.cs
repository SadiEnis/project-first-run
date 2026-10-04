#if UNITY_EDITOR
using System;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Items;
using ProjectFirstRun.Player;
using ProjectFirstRun.Stats;
using ProjectFirstRun.Upgrades;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Upgrades
{
    public sealed class GlassHeartUpgradeTests
    {
        private GameObject _player;
        private HealthComponent _health;
        private PlayerStatCollection _stats;
        private PlayerUpgradeController _upgrades;
        private UpgradeDefinition _item;

        [SetUp]
        public void SetUp()
        {
            _player = new GameObject("Glass Heart test");
            _health = _player.AddComponent<HealthComponent>();
            _stats = _player.AddComponent<PlayerStatsController>().Stats;
            _player.AddComponent<PlayerBuildController>().Initialize(new PlayerBuildCapacity(1, 1, 3));
            _upgrades = _player.AddComponent<PlayerUpgradeController>();
            _player.AddComponent<PlayerSurvivalController>();
            _item = Load("GlassHeart");
        }
        [TearDown] public void TearDown() => Object.DestroyImmediate(_player);
        private static UpgradeDefinition Load(string name) =>
            AssetDatabase.LoadAssetAtPath<UpgradeDefinition>($"Assets/_Project/Data/Upgrade/UD_{name}.asset");
        private DamageResult Hit(float amount) =>
            _health.ApplyDamage(new DamageInfo(amount, null, Vector3.zero, Vector3.forward));

        [Test]
        public void LevelsIncreaseBothDamageStatsWithoutIncreasingCost()
        {
            Assert.That(_item.MaximumLevel, Is.EqualTo(3));
            Assert.That(_upgrades.TryAcquire(_item), Is.EqualTo(UpgradeAcquireResult.Acquired));
            for (int level = 1; level <= 3; level++)
            {
                if (level > 1)
                    Assert.That(_upgrades.TryLevelUp(_item), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
                foreach (var stat in new[] { PlayerStatType.WeaponDamage, PlayerStatType.AbilityDamage })
                    Assert.That(_stats.Evaluate(stat, 100), Is.EqualTo(105 + level * 15).Within(.001f));
                Assert.That(_health.IncomingDamageMultiplier, Is.EqualTo(1.25f));
                Assert.That(Hit(10).AppliedDamage, Is.EqualTo(12.5f).Within(.001f));
                Assert.That(_stats.ModifierCount, Is.EqualTo(3));
                Assert.That(_health.MaximumHealth, Is.EqualTo(100));
            }
            Assert.That(_upgrades.TryLevelUp(_item), Is.EqualTo(ItemLevelUpResult.MaximumLevelReached));
            Assert.That(_upgrades.TryAcquire(_item), Is.EqualTo(UpgradeAcquireResult.AlreadyOwned));
        }

        [Test]
        public void ArmorAndEventsPreserveOriginalHitAndActualHealthLoss()
        {
            _upgrades.TryAcquire(_item);
            var oath = Load("IronOath");
            _upgrades.TryAcquire(oath);
            _upgrades.TryLevelUp(oath); _upgrades.TryLevelUp(oath);
            int events = 0;
            _health.Damaged += (info, result) =>
            {
                events++;
                Assert.That(info.Amount, Is.EqualTo(20));
                Assert.That(info.HitDirection, Is.EqualTo(Vector3.forward));
                Assert.That(result.RequestedDamage, Is.EqualTo(20));
                Assert.That(result.AppliedDamage, Is.EqualTo(20).Within(.001f));
            };
            Hit(20); // 20 * 1.25 * 0.80.
            Assert.That(_health.CurrentHealth, Is.EqualTo(80).Within(.001f));
            Assert.That(_health.Heal(10), Is.EqualTo(10).Within(.001f));
            _health.SetMaximumHealth(80);
            Assert.That(events, Is.EqualTo(1));
            Assert.That(_health.CurrentHealth, Is.EqualTo(80));
        }

        [Test]
        public void StackingAndRemovalPreserveOtherSources()
        {
            _upgrades.TryAcquire(_item);
            _upgrades.TryAcquire(Load("WrathSeal"));
            Assert.That(_stats.Evaluate(PlayerStatType.WeaponDamage, 100), Is.EqualTo(130).Within(.001f));
            _stats.Add(new StatModifier(PlayerStatType.DamageReduction, StatModifierOperation.Flat, 1f, "armor"));
            Assert.That(Hit(20).AppliedDamage, Is.EqualTo(6.25f));
            _stats.RemoveBySource(_item.StableId);
            Assert.That(_health.IncomingDamageMultiplier, Is.EqualTo(1));
            Assert.That(_stats.Evaluate(PlayerStatType.WeaponDamage, 100), Is.EqualTo(110).Within(.001f));
            Assert.That(_health.DamageReduction, Is.EqualTo(.75f));
        }

        [Test]
        public void EnemyDefaultsAndPoolInitializationAreUnaffected()
        {
            _upgrades.TryAcquire(_item);
            var enemy = new GameObject("Enemy health");
            try
            {
                var health = enemy.AddComponent<HealthComponent>();
                Assert.That(health.ApplyDamage(new DamageInfo(20, null, Vector3.zero, Vector3.zero)).AppliedDamage,
                    Is.EqualTo(20));
                health.SetIncomingDamageMultiplier(1.25f);
                health.Initialize(100);
                Assert.That(health.IncomingDamageMultiplier, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(enemy); }
        }

        [Test]
        public void LethalAmplifiedHitEmitsDeathOnce()
        {
            _upgrades.TryAcquire(_item);
            int deaths = 0;
            _health.Died += (_, __) => deaths++;
            var hit = Hit(80);
            Assert.That(hit.WasLethal, Is.True);
            Assert.That(hit.RequestedDamage, Is.EqualTo(80));
            Assert.That(hit.AppliedDamage, Is.EqualTo(100));
            Assert.That(Hit(80).WasApplied, Is.False);
            Assert.That(deaths, Is.EqualTo(1));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidMultiplierCannotMutateHealth(float value)
        {
            var state = new HealthState(100);
            Assert.Throws<ArgumentOutOfRangeException>(() => state.ApplyDamage(20, 0, value));
            Assert.That(state.CurrentHealth, Is.EqualTo(100));
            Assert.Throws<ArgumentOutOfRangeException>(() => _health.SetIncomingDamageMultiplier(value));
            Assert.That(_health.IncomingDamageMultiplier, Is.EqualTo(1));
        }
    }
}
#endif
