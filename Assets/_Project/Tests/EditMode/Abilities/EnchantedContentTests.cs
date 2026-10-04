using NUnit.Framework;
using ProjectFirstRun.Abilities.EnchantedStaff;
using UnityEditor;
namespace ProjectFirstRun.Tests.EditMode.Abilities
{
    public sealed class EnchantedContentTests
    {
        [Test] public void AuthoredLevelsMatchDesign()
        {
            var asset = AssetDatabase.LoadAssetAtPath<EnchantedDefinition>("Assets/_Project/Data/Abilities/AD_EnchantedStaff.asset");
            Assert.That(asset, Is.Not.Null);
            var levels = asset.CreateLevelConfigs(); Assert.That(levels.Length, Is.EqualTo(8));
            for (int i = 0; i < 8; i++)
            {
                Assert.That(levels[i].Damage, Is.EqualTo(i == 0 ? 20 : i < 5 ? 30 : 40));
                Assert.That(levels[i].Lifetime, Is.EqualTo(i < 2 ? 2 : i < 6 ? 3 : 4));
                Assert.That(levels[i].Ability.Cooldown, Is.EqualTo(i < 3 ? 5 : i < 7 ? 4 : 3));
                Assert.That(levels[i].Count, Is.EqualTo(i < 4 ? 1 : 2));
            }
        }
        [Test] public void InvalidConfigurationIsRejected()
        {
            Assert.Throws<System.ArgumentException>(() => new EnchantedConfig(0, 2, 5, 1));
            Assert.Throws<System.ArgumentException>(() => new EnchantedConfig(20, 0, 5, 1));
            Assert.Throws<System.ArgumentException>(() => new EnchantedConfig(20, 2, 5, 3));
        }
    }
}
