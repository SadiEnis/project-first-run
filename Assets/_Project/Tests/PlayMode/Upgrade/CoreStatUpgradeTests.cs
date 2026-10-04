#if UNITY_EDITOR
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
using ProjectFirstRun.Player;
using ProjectFirstRun.Stats;
using ProjectFirstRun.Upgrades;
using UnityEditor;
using UnityEngine;

namespace ProjectFirstRun.Tests.PlayMode.Upgrades
{
    public sealed class CoreStatUpgradeTests
    {
        [TestCase("WrathSeal", PlayerStatType.WeaponDamage, .1f)]
        [TestCase("WrathSeal", PlayerStatType.AbilityDamage, .1f)]
        [TestCase("Windweave", PlayerStatType.MoveSpeed, .05f)]
        [TestCase("FireRhythm", PlayerStatType.WeaponFireRate, .1f)]
        [TestCase("AttractionCore", PlayerStatType.PickupRadius, .2f)]
        [TestCase("MemoryCrystal", PlayerStatType.ExperienceGain, .1f)]
        [TestCase("BrokenHourglass", PlayerStatType.AbilityCooldown, -.05f)]
        public void FiveLevelsReplaceOwnBonusAndKeepOtherSources(string name, PlayerStatType stat, float step)
        {
            var player = new GameObject("Core upgrade test");
            try
            {
                var build = player.AddComponent<PlayerBuildController>();
                var stats = player.AddComponent<PlayerStatsController>().Stats;
                var upgrades = player.AddComponent<PlayerUpgradeController>();
                build.Initialize(new PlayerBuildCapacity(1, 1, 3));
                var definition = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(
                    $"Assets/_Project/Data/Upgrade/UD_{name}.asset");
                Assert.That(definition, Is.Not.Null);
                Assert.That(definition.MaximumLevel, Is.EqualTo(5));
                var external = new StatModifier(stat, StatModifierOperation.AdditivePercent, .15f, "other");
                stats.Add(external);
                Assert.That(upgrades.TryAcquire(definition), Is.EqualTo(UpgradeAcquireResult.Acquired));
                int modifiers = stats.ModifierCount;
                for (int level = 1; level <= 5; level++)
                {
                    if (level > 1)
                        Assert.That(upgrades.TryLevelUp(definition), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
                    Assert.That(stats.Evaluate(stat, 100), Is.EqualTo(100 * (1.15f + level * step)).Within(.001f));
                    Assert.That(stats.ModifierCount, Is.EqualTo(modifiers));
                    Assert.That(stats.Contains(external), Is.True);
                    Assert.That(build.Build.GetLevel(ItemCategory.Upgrade, definition.StableId), Is.EqualTo(level));
                }
                Assert.That(upgrades.TryLevelUp(definition), Is.EqualTo(ItemLevelUpResult.MaximumLevelReached));
                Assert.That(upgrades.TryAcquire(definition), Is.EqualTo(UpgradeAcquireResult.AlreadyOwned));
            }
            finally { Object.DestroyImmediate(player); }
        }

        [Test]
        public void MovementUsesOriginalWalkAndSprintSpeedsWithoutCompounding()
        {
            var player = new GameObject("Movement upgrade test");
            try
            {
                var stats = player.AddComponent<PlayerStatsController>().Stats;
                var motor = player.AddComponent<PlayerMotor>();
                float walk = motor.EvaluateMovementSpeed(false);
                float sprint = motor.EvaluateMovementSpeed(true);
                var serialized = new SerializedObject(motor);
                float gravity = serialized.FindProperty("_gravity").floatValue;
                stats.Add(new StatModifier(PlayerStatType.MoveSpeed, StatModifierOperation.AdditivePercent, .25f, "wind"));
                for (int i = 0; i < 5; i++)
                {
                    Assert.That(motor.EvaluateMovementSpeed(false), Is.EqualTo(walk * 1.25f).Within(.001f));
                    Assert.That(motor.EvaluateMovementSpeed(true), Is.EqualTo(sprint * 1.25f).Within(.001f));
                }
                serialized.Update();
                Assert.That(serialized.FindProperty("_gravity").floatValue, Is.EqualTo(gravity));
                stats.RemoveBySource("wind");
                Assert.That(motor.EvaluateMovementSpeed(false), Is.EqualTo(walk));
                Assert.That(motor.EvaluateMovementSpeed(true), Is.EqualTo(sprint));
            }
            finally { Object.DestroyImmediate(player); }
        }
    }
}
#endif
