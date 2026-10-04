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
    public sealed class IronOathUpgradeTests
    {
        private GameObject _player;
        private HealthComponent _health;
        private PlayerMotor _motor;
        private PlayerStatCollection _stats;
        private PlayerUpgradeController _upgrades;
        private float _walk, _sprint;

        [SetUp]
        public void SetUp()
        {
            _player = new GameObject("Iron Oath test");
            _health = _player.AddComponent<HealthComponent>();
            _stats = _player.AddComponent<PlayerStatsController>().Stats;
            _player.AddComponent<PlayerBuildController>().Initialize(new PlayerBuildCapacity(1, 1, 3));
            _upgrades = _player.AddComponent<PlayerUpgradeController>();
            _player.AddComponent<PlayerSurvivalController>();
            _motor = _player.AddComponent<PlayerMotor>();
            _walk = _motor.EvaluateMovementSpeed(false);
            _sprint = _motor.EvaluateMovementSpeed(true);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_player);

        private static UpgradeDefinition Load(string name) =>
            AssetDatabase.LoadAssetAtPath<UpgradeDefinition>($"Assets/_Project/Data/Upgrade/UD_{name}.asset");

        private void AssertSpeed(float multiplier)
        {
            Assert.That(_motor.EvaluateMovementSpeed(false), Is.EqualTo(_walk * multiplier).Within(.001f));
            Assert.That(_motor.EvaluateMovementSpeed(true), Is.EqualTo(_sprint * multiplier).Within(.001f));
        }

        [Test]
        public void ThreeLevelsIncreaseArmorButKeepMovementCostFixed()
        {
            var item = Load("IronOath");
            Assert.That(item.MaximumLevel, Is.EqualTo(3));
            Assert.That(_upgrades.TryAcquire(item), Is.EqualTo(UpgradeAcquireResult.Acquired));
            for (int level = 1; level <= 3; level++)
            {
                if (level > 1)
                    Assert.That(_upgrades.TryLevelUp(item), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
                float reduction = .05f + level * .05f;
                Assert.That(_health.DamageReduction, Is.EqualTo(reduction).Within(.001f));
                AssertSpeed(.9f);
                Assert.That(_stats.ModifierCount, Is.EqualTo(2));
                var hit = _health.ApplyDamage(new DamageInfo(10, null, Vector3.zero, Vector3.zero));
                Assert.That(hit.RequestedDamage, Is.EqualTo(10));
                Assert.That(hit.AppliedDamage, Is.EqualTo(10 * (1 - reduction)).Within(.001f));
            }
            Assert.That(_upgrades.TryLevelUp(item), Is.EqualTo(ItemLevelUpResult.MaximumLevelReached));
            Assert.That(_upgrades.TryAcquire(item), Is.EqualTo(UpgradeAcquireResult.AlreadyOwned));
            AssertSpeed(.9f);
        }

        [Test]
        public void StacksWithIronhideAndWindweaveAndKeepsSharedCap()
        {
            var oath = Load("IronOath");
            var armor = Load("Ironhide");
            var speed = Load("Windweave");
            foreach (var item in new[] { oath, armor, speed })
            {
                _upgrades.TryAcquire(item);
                for (int level = 1; level < item.MaximumLevel; level++)
                    _upgrades.TryLevelUp(item);
            }
            Assert.That(_health.DamageReduction, Is.EqualTo(.45f).Within(.001f));
            AssertSpeed(1.15f);
            var extra = new StatModifier(PlayerStatType.DamageReduction, StatModifierOperation.Flat, .6f, "other");
            _stats.Add(extra);
            Assert.That(_health.DamageReduction, Is.EqualTo(.75f));
            _stats.Remove(extra);
            _stats.RemoveBySource(oath.StableId);
            Assert.That(_health.DamageReduction, Is.EqualTo(.25f).Within(.001f));
            AssertSpeed(1.25f);
        }

        [Test]
        public void AcquisitionDoesNotChangeGravityHealthOrBaseSpeeds()
        {
            var before = new SerializedObject(_motor);
            float gravity = before.FindProperty("_gravity").floatValue;
            _upgrades.TryAcquire(Load("IronOath"));
            for (int i = 0; i < 5; i++) AssertSpeed(.9f);
            before.Update();
            Assert.That(before.FindProperty("_gravity").floatValue, Is.EqualTo(gravity));
            Assert.That(before.FindProperty("_walkSpeed").floatValue, Is.EqualTo(_walk));
            Assert.That(before.FindProperty("_sprintSpeed").floatValue, Is.EqualTo(_sprint));
            Assert.That(_health.CurrentHealth, Is.EqualTo(100));
            Assert.That(_health.MaximumHealth, Is.EqualTo(100));
        }
    }
}
#endif
