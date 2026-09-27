using NUnit.Framework;
using ProjectFirstRun.Abilities.SniperBomb;
using UnityEditor;
namespace ProjectFirstRun.Tests.EditMode.Abilities
{
    public sealed class SniperContentTests
    {
        [Test] public void AuthoredLevelsMatchDesign()
        {
            var asset = AssetDatabase.LoadAssetAtPath<SniperDefinition>("Assets/_Project/Data/Abilities/AD_SniperBomb.asset");
            Assert.That(asset, Is.Not.Null);
            var levels = asset.CreateLevelConfigs(); Assert.That(levels.Length, Is.EqualTo(8));
            for (int i = 0; i < 8; i++)
            {
                Assert.That(levels[i].Damage, Is.EqualTo(i == 0 ? 50 : i < 5 ? 70 : 100));
                Assert.That(levels[i].Radius, Is.EqualTo(i < 2 ? 2 : i < 7 ? 3 : 4));
                Assert.That(levels[i].Ability.Cooldown, Is.EqualTo(i < 3 ? 6 : 4));
                Assert.That(levels[i].Count, Is.EqualTo(i < 4 ? 1 : 2));
                Assert.That(levels[i].Retarget, Is.EqualTo(i >= 6));
            }
        }
        [Test] public void InvalidConfigurationRejected()
        {
            Assert.Throws<System.ArgumentException>(() => new SniperConfig(0, 2, 6, 1, false));
            Assert.Throws<System.ArgumentException>(() => new SniperConfig(50, 0, 6, 1, false));
            Assert.Throws<System.ArgumentException>(() => new SniperConfig(50, 2, 6, 3, false));
        }
    }
}
