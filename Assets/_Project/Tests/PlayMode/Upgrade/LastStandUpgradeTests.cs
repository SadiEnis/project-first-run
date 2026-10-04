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

namespace ProjectFirstRun.Tests.PlayMode.Upgrades
{
    public sealed class LastStandUpgradeTests
    {
        private GameObject _player;
        private HealthComponent _health;
        private PlayerStatCollection _stats;
        private PlayerUpgradeController _upgrades;
        private PlayerLastStandController _lastStand;
        private UpgradeDefinition _definition;

        [SetUp]
        public void SetUp()
        {
            _player = new GameObject("Last Stand test");
            _health = _player.AddComponent<HealthComponent>();
            _stats = _player.AddComponent<PlayerStatsController>().Stats;
            _player.AddComponent<PlayerBuildController>().Initialize(new PlayerBuildCapacity(1, 1, 3));
            _upgrades = _player.AddComponent<PlayerUpgradeController>();
            _player.AddComponent<PlayerSurvivalController>();
            _lastStand = _player.AddComponent<PlayerLastStandController>();
            _definition = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(
                "Assets/_Project/Data/Upgrade/UD_LastStand.asset");
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_player);

        private void Hit(float amount) =>
            _health.ApplyDamage(new DamageInfo(amount, null, Vector3.zero, Vector3.zero));

        private void AssertDamage(float expected)
        {
            Assert.That(_stats.Evaluate(PlayerStatType.WeaponDamage, 100), Is.EqualTo(expected).Within(.001f));
            Assert.That(_stats.Evaluate(PlayerStatType.AbilityDamage, 100), Is.EqualTo(expected).Within(.001f));
        }

        [Test]
        public void BoundaryHealingDeathAndResetReevaluateImmediately()
        {
            _upgrades.TryAcquire(_definition);
            AssertDamage(100);
            Hit(69);
            Assert.That(_lastStand.IsBonusActive, Is.False);
            Hit(1);
            Assert.That(_health.CurrentHealth, Is.EqualTo(30));
            Assert.That(_lastStand.IsBonusActive, Is.True);
            AssertDamage(120);
            _health.Heal(.1f);
            AssertDamage(100);
            Hit(1);
            AssertDamage(120);
            Hit(100);
            Assert.That(_lastStand.IsBonusActive, Is.False);
            AssertDamage(100);
            _health.ResetHealth();
            AssertDamage(100);
        }

        [Test]
        public void AcquireWhileInjuredAndThreeLevelsReplaceOnlyOwnEffects()
        {
            Hit(80);
            var weapon = new StatModifier(PlayerStatType.WeaponDamage,
                StatModifierOperation.AdditivePercent, .1f, "other");
            var ability = new StatModifier(PlayerStatType.AbilityDamage,
                StatModifierOperation.AdditivePercent, .1f, "other");
            _stats.Add(weapon); _stats.Add(ability);
            Assert.That(_upgrades.TryAcquire(_definition), Is.EqualTo(UpgradeAcquireResult.Acquired));
            AssertDamage(130);
            Assert.That(_definition.MaximumLevel, Is.EqualTo(3));
            for (int level = 2; level <= 3; level++)
            {
                Assert.That(_upgrades.TryLevelUp(_definition), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
                AssertDamage(110 + (level + 1) * 10);
                Assert.That(_stats.ModifierCount, Is.EqualTo(5));
            }
            Assert.That(_upgrades.TryLevelUp(_definition), Is.EqualTo(ItemLevelUpResult.MaximumLevelReached));
            Assert.That(_upgrades.TryAcquire(_definition), Is.EqualTo(UpgradeAcquireResult.AlreadyOwned));
            _health.Heal(100);
            AssertDamage(110);
            Assert.That(_stats.Contains(weapon) && _stats.Contains(ability), Is.True);
        }

        [Test]
        public void MaximumHealthChangesCanToggleThresholdWithoutDamage()
        {
            _upgrades.TryAcquire(_definition);
            Hit(70);
            AssertDamage(120);
            // Simulates Second Heart's capacity increase: 30/100 -> 80/150.
            var maximum = new StatModifier(PlayerStatType.MaxHealth,
                StatModifierOperation.AdditivePercent, .5f, "max");
            _stats.Add(maximum);
            Assert.That(_health.CurrentHealth, Is.EqualTo(80).Within(.001f));
            AssertDamage(100);
            Hit(40); // 40/150 is below the threshold.
            AssertDamage(120);
            _stats.Remove(maximum); // 40/100 is above it; no damage or heal.
            Assert.That(_health.CurrentHealth, Is.EqualTo(40).Within(.001f));
            AssertDamage(100);
        }

        [Test]
        public void InactiveLevelingReenableAndRepeatedNotificationsDoNotDuplicateBonuses()
        {
            _upgrades.TryAcquire(_definition);
            _upgrades.TryLevelUp(_definition);
            AssertDamage(100);
            Hit(80);
            AssertDamage(130);
            for (int i = 0; i < 3; i++)
            {
                _lastStand.enabled = false;
                AssertDamage(100);
                _lastStand.enabled = true;
                AssertDamage(130);
                _health.SetMaximumHealth(100);
                Assert.That(_stats.ModifierCount, Is.EqualTo(3));
            }
            _stats.RemoveBySource(_definition.StableId);
            AssertDamage(100);
            Assert.That(_stats.ModifierCount, Is.Zero);
        }
    }
}
#endif
