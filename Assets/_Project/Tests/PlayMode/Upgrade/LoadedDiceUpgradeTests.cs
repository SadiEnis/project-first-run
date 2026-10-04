#if UNITY_EDITOR
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
using ProjectFirstRun.Stats;
using ProjectFirstRun.Upgrades;
using UnityEditor;
using UnityEngine;

namespace ProjectFirstRun.Tests.PlayMode.Upgrades
{
    public sealed class LoadedDiceUpgradeTests
    {
        [Test]
        public void ThreeLevelsReplaceLuckAndFreshPlayerDoesNotInheritIt()
        {
            var player = new GameObject("Loaded Dice owner");
            var fresh = new GameObject("Fresh run owner");
            try
            {
                var stats = player.AddComponent<PlayerStatsController>();
                var build = player.AddComponent<PlayerBuildController>();
                build.Initialize(new PlayerBuildCapacity(1, 1, 3));
                var upgrades = player.AddComponent<PlayerUpgradeController>();
                var item = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>("Assets/_Project/Data/Upgrade/UD_LoadedDice.asset");
                Assert.That(item, Is.Not.Null);
                Assert.That(item.MaximumLevel, Is.EqualTo(3));
                Assert.That(stats.Evaluate(PlayerStatType.Luck, 1f), Is.EqualTo(1f));
                Assert.That(upgrades.TryAcquire(item), Is.EqualTo(UpgradeAcquireResult.Acquired));
                Assert.That(stats.Evaluate(PlayerStatType.Luck, 1f), Is.EqualTo(1.2f).Within(0.00001f));
                Assert.That(upgrades.TryAcquire(item), Is.EqualTo(UpgradeAcquireResult.AlreadyOwned));
                foreach (float expected in new[] { 1.4f, 1.6f })
                {
                    Assert.That(upgrades.TryLevelUp(item), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
                    Assert.That(stats.Evaluate(PlayerStatType.Luck, 1f), Is.EqualTo(expected).Within(0.00001f));
                    Assert.That(stats.Stats.ModifierCount, Is.EqualTo(1));
                }
                Assert.That(upgrades.TryLevelUp(item), Is.EqualTo(ItemLevelUpResult.MaximumLevelReached));
                Assert.That(fresh.AddComponent<PlayerStatsController>().Evaluate(PlayerStatType.Luck, 1f), Is.EqualTo(1f));
            }
            finally { Object.DestroyImmediate(player); Object.DestroyImmediate(fresh); }
        }
    }
}
#endif
