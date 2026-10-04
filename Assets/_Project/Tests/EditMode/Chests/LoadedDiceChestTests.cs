#if UNITY_EDITOR
using System;
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Rewards;
using UnityEditor;

namespace ProjectFirstRun.Tests.EditMode.Chests
{
    public sealed class LoadedDiceChestTests
    {
        private const string Root = "Assets/_Project/Data/Chests/Dev/";
        private static ChestDropTable Table(string name) =>
            AssetDatabase.LoadAssetAtPath<ChestDropTable>(Root + "CDT_Development" + name + ".asset");

        [TestCase("RP_UpgradeChest")]
        [TestCase("RP_DevelopmentRewardPool")]
        public void RewardPoolsIncludeLoadedDiceExactlyOnce(string name)
        {
            var pool = AssetDatabase.LoadAssetAtPath<RewardItemPool>("Assets/_Project/Data/Reward/Dev/" + name + ".asset");
            Assert.That(pool.GetValidatedItems().Count(item => item.StableId == "upgrade.loaded_dice"), Is.EqualTo(1));
        }

        [TestCase("Normal", 65, 60, 30, 15)]
        [TestCase("Elite", 40, 90, 75, 15)]
        [TestCase("LevelUp", 30, 150, 45, 15)]
        public void AllLevels_SelectAcrossEveryEntryBoundaryWithoutChangingAssets(string source,
            int common, int green, int purple, int legendary)
        {
            var table = Table(source);
            var weights = new[] { common, common, common, green, purple, legendary };
            for (int i = 0; i < 6; i++)
                Assert.That(table.Entries[i].Definition.AffectedByLuck, Is.EqualTo(i >= 3));
            foreach (float multiplier in new[] { 1f, 1.2f, 1.4f, 1.6f })
            {
                var adjusted = weights.Select((w, i) => w * (i < 3 ? 1d : multiplier)).ToArray();
                double total = adjusted.Sum();
                int range = multiplier == 1f ? 300 : 1000000000;
                double start = 0;
                for (int i = 0; i < 6; i++)
                {
                    int first = (int)Math.Ceiling(start / total * range);
                    int last = (int)Math.Ceiling((start + adjusted[i]) / total * range) - 1;
                    Assert.That(table.Select(new FixedRandom(first, range), multiplier),
                        Is.SameAs(table.Entries[i].Definition), $"{source}/{multiplier}/{i}/first");
                    Assert.That(table.Select(new FixedRandom(last, range), multiplier),
                        Is.SameAs(table.Entries[i].Definition), $"{source}/{multiplier}/{i}/last");
                    start += adjusted[i];
                }
            }
            Assert.That(table.Entries.Select(e => e.Weight), Is.EqualTo(weights));
        }

        [Test]
        public void BossAndLegacyContent_DoNotOptIntoLuck()
        {
            foreach (string name in new[] { "Boss", "Development", "Weapon", "Ability", "Upgrade" })
                Assert.That(AssetDatabase.LoadAssetAtPath<ChestDefinition>(Root + "CD_" + name + "Chest.asset")
                    .AffectedByLuck, Is.False);
            // Advanced showcase has three eligible entries and one excluded Boss entry.
            // With multiplier 2, total is 7; Boss starts at 6/7, not 3/4.
            var table = Table("Advanced");
            Assert.That(table.Select(new FixedRandom(800000000, 1000000000), 2f),
                Is.SameAs(table.Entries[2].Definition));
            Assert.That(table.Select(new FixedRandom(900000000, 1000000000), 2f),
                Is.SameAs(table.Entries[3].Definition));
        }

        [TestCase("Normal", 2500)]
        [TestCase("Elite", 5000)]
        public void LuckDoesNotChangeDropChance(string name, int chance)
        {
            var profile = AssetDatabase.LoadAssetAtPath<EnemyChestDropProfile>(Root + "CDP_Development" + name + ".asset");
            foreach (float multiplier in new[] { 1f, 1.2f, 1.4f, 1.6f })
            {
                Assert.That(profile.TryRoll(new FixedRandom(chance, 10000), out var missing, multiplier), Is.False);
                Assert.That(missing, Is.Null);
                var random = new ChanceThenType(chance - 1, multiplier == 1f ? 300 : 1000000000);
                Assert.That(profile.TryRoll(random, out var chest, multiplier), Is.True);
                Assert.That(chest, Is.SameAs(profile.DropTable.Entries[0].Definition));
                Assert.That(random.Calls, Is.EqualTo(2));
            }
        }

        [Test]
        public void InvalidLuckOrRandom_IsRejected()
        {
            foreach (float invalid in new[] { 0f, -1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                Assert.Throws<ArgumentOutOfRangeException>(() => Table("Normal").Select(new FixedRandom(0, 300), invalid));
            Assert.Throws<ArgumentNullException>(() => Table("Normal").Select(null, 1.2f));
            foreach (int invalid in new[] { -1, 1000000000 })
                Assert.Throws<InvalidOperationException>(() => Table("Normal").Select(new FixedRandom(invalid, 1000000000), 1.2f));
        }

        private sealed class FixedRandom : IRandomSource
        {
            private readonly int _value, _range;
            public FixedRandom(int value, int range) { _value = value; _range = range; }
            public int Next(int minInclusive, int maxExclusive)
            {
                Assert.That(minInclusive, Is.Zero);
                Assert.That(maxExclusive, Is.EqualTo(_range));
                return _value;
            }
        }
        private sealed class ChanceThenType : IRandomSource
        {
            private readonly int _chanceRoll, _typeRange;
            public int Calls;
            public ChanceThenType(int chanceRoll, int typeRange) { _chanceRoll = chanceRoll; _typeRange = typeRange; }
            public int Next(int minInclusive, int maxExclusive)
            {
                Assert.That(minInclusive, Is.Zero);
                Assert.That(maxExclusive, Is.EqualTo(Calls == 0 ? 10000 : _typeRange));
                return Calls++ == 0 ? _chanceRoll : 0;
            }
        }
    }
}
#endif
