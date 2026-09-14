using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;

namespace ProjectFirstRun.Tests.EditMode.Evolutions
{
    public sealed class EvolutionFoundationTests
    {
        [Test]
        public void ReplaceKeepsCategorySlotAndCreatesFreshResultState()
        {
            var build = new PlayerBuild(new PlayerBuildCapacity(1, 1, 1));
            Assert.That(build.TryAdd(ItemCategory.Weapon, "weapon.base", 8), Is.EqualTo(PlayerBuildAddResult.Added));
            for (int i = 0; i < 7; i++) build.TryLevelUp(ItemCategory.Weapon, "weapon.base");
            Assert.That(build.TryReplace(ItemCategory.Weapon, "weapon.base", "weapon.evolved", 1), Is.EqualTo(PlayerBuildReplaceResult.Replaced));
            Assert.That(build.GetCount(ItemCategory.Weapon), Is.EqualTo(1));
            Assert.That(build.GetLevel(ItemCategory.Weapon, "weapon.base"), Is.Zero);
            Assert.That(build.GetLevel(ItemCategory.Weapon, "weapon.evolved"), Is.EqualTo(1));
        }

        [Test]
        public void ReplaceMissingOrOwnedResultDoesNotMutate()
        {
            var build = new PlayerBuild(new PlayerBuildCapacity(2, 1, 1));
            build.TryAdd(ItemCategory.Weapon, "weapon.base", 1);
            build.TryAdd(ItemCategory.Weapon, "weapon.other", 1);
            Assert.That(build.TryReplace(ItemCategory.Weapon, "weapon.missing", "weapon.new"), Is.EqualTo(PlayerBuildReplaceResult.SourceNotOwned));
            Assert.That(build.TryReplace(ItemCategory.Weapon, "weapon.base", "weapon.other"), Is.EqualTo(PlayerBuildReplaceResult.TargetAlreadyOwned));
            Assert.That(build.Weapons[0], Is.EqualTo("weapon.base"));
        }
    }
}
