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
    public sealed class BloodPactUpgradeTests
    {
        private GameObject _player;
        private HealthComponent _health;
        private PlayerStatCollection _stats;
        private PlayerUpgradeController _upgrades;
        private PlayerLastStandController _lastStand;

        [SetUp]
        public void SetUp()
        {
            _player = new GameObject("Blood Pact test");
            _health = _player.AddComponent<HealthComponent>();
            _stats = _player.AddComponent<PlayerStatsController>().Stats;
            _player.AddComponent<PlayerBuildController>().Initialize(new PlayerBuildCapacity(1, 1, 3));
            _upgrades = _player.AddComponent<PlayerUpgradeController>();
            _player.AddComponent<PlayerSurvivalController>();
            _lastStand = _player.AddComponent<PlayerLastStandController>();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_player);
        private static UpgradeDefinition Load(string name) =>
            AssetDatabase.LoadAssetAtPath<UpgradeDefinition>($"Assets/_Project/Data/Upgrade/UD_{name}.asset");
        private void Hit(float amount) =>
            _health.ApplyDamage(new DamageInfo(amount, null, Vector3.zero, Vector3.zero));
        private void AssertDamage(float expected)
        {
            Assert.That(_stats.Evaluate(PlayerStatType.WeaponDamage, 100), Is.EqualTo(expected).Within(.001f));
            Assert.That(_stats.Evaluate(PlayerStatType.AbilityDamage, 100), Is.EqualTo(expected).Within(.001f));
        }

        [TestCase(100f, 80f)]
        [TestCase(60f, 60f)]
        public void AcquisitionClampsOnlyExcessAndLevelsDoNotRepeatCost(float before, float after)
        {
            if (before < 100) Hit(100 - before);
            int damaged = 0, died = 0, changed = 0;
            _health.Damaged += (_, __) => damaged++;
            _health.Died += (_, __) => died++;
            _health.HealthChanged += (_, __) => changed++;
            var pact = Load("BloodPact");
            Assert.That(pact.MaximumLevel, Is.EqualTo(3));
            Assert.That(_upgrades.TryAcquire(pact), Is.EqualTo(UpgradeAcquireResult.Acquired));
            for (int level = 1; level <= 3; level++)
            {
                if (level > 1)
                    Assert.That(_upgrades.TryLevelUp(pact), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
                Assert.That(_health.MaximumHealth, Is.EqualTo(80).Within(.001f));
                Assert.That(_health.CurrentHealth, Is.EqualTo(after).Within(.001f));
                AssertDamage(105 + level * 10);
                Assert.That(_stats.ModifierCount, Is.EqualTo(3));
            }
            Assert.That(changed, Is.EqualTo(1));
            Assert.That(damaged, Is.Zero);
            Assert.That(died, Is.Zero);
            Assert.That(_upgrades.TryLevelUp(pact), Is.EqualTo(ItemLevelUpResult.MaximumLevelReached));
            Assert.That(_upgrades.TryAcquire(pact), Is.EqualTo(UpgradeAcquireResult.AlreadyOwned));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SecondHeartAndWrathSealStackAdditivelyInEitherAcquisitionOrder(bool pactFirst)
        {
            var pact = Load("BloodPact");
            var heart = Load("SecondHeart");
            if (pactFirst) _upgrades.TryAcquire(pact);
            _upgrades.TryAcquire(heart);
            for (int i = 1; i < 5; i++) _upgrades.TryLevelUp(heart);
            if (!pactFirst) _upgrades.TryAcquire(pact);
            _upgrades.TryAcquire(Load("WrathSeal"));
            Assert.That(_health.MaximumHealth, Is.EqualTo(130).Within(.001f));
            AssertDamage(125);
            _upgrades.TryLevelUp(pact);
            AssertDamage(135);
            Assert.That(_health.MaximumHealth, Is.EqualTo(130).Within(.001f));
        }

        [Test]
        public void ReducedMaximumReevaluatesLastStandWithoutRecursiveHealthChanges()
        {
            Hit(70); // 30/100: Last Stand is active.
            _upgrades.TryAcquire(Load("LastStand"));
            Assert.That(_lastStand.IsBonusActive, Is.True);
            AssertDamage(120);
            _upgrades.TryAcquire(Load("BloodPact")); // 30/80: now above 30%.
            Assert.That(_health.CurrentHealth, Is.EqualTo(30));
            Assert.That(_lastStand.IsBonusActive, Is.False);
            AssertDamage(115);
            Hit(6); // 24/80: exactly 30%.
            Assert.That(_lastStand.IsBonusActive, Is.True);
            AssertDamage(135);
            _upgrades.TryLevelUp(Load("BloodPact"));
            AssertDamage(145);
            Assert.That(_health.CurrentHealth, Is.EqualTo(24));
        }

        [Test]
        public void DeadPlayerCannotBeRevivedByAcquisitionOrLeveling()
        {
            Hit(100);
            var pact = Load("BloodPact");
            _upgrades.TryAcquire(pact);
            _upgrades.TryLevelUp(pact);
            Assert.That(_health.IsDead, Is.True);
            Assert.That(_health.CurrentHealth, Is.Zero);
            Assert.That(_health.MaximumHealth, Is.EqualTo(80).Within(.001f));
            Assert.That(_lastStand.IsBonusActive, Is.False);
        }
    }
}
#endif
