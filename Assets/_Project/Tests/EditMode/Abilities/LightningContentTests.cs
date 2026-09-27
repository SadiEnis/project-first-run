using System;
using NUnit.Framework;
using ProjectFirstRun.Abilities.Lightning;
using UnityEditor;
namespace ProjectFirstRun.Tests.EditMode.Abilities
{
    public sealed class LightningContentTests
    {
        [Test]
        public void AuthoredLevelsMatchDesign()
        {
            var asset = AssetDatabase.LoadAssetAtPath<LightningDefinition>("Assets/_Project/Data/Abilities/AD_LightningStaff.asset");
            Assert.That(asset, Is.Not.Null);
            Assert.That(asset.StableId, Is.EqualTo("ability.lightning-staff"));
            var levels = asset.CreateLevelConfigs();
            Assert.That(levels.Length, Is.EqualTo(8));
            for (int i = 0; i < 8; i++)
            {
                Assert.That(levels[i].Damage, Is.EqualTo(i == 0 ? 20 : i < 4 ? 30 : 40));
                Assert.That(levels[i].Ability.Cooldown, Is.EqualTo(i < 2 ? 4 : 3));
                Assert.That(levels[i].Strikes, Is.EqualTo(i < 3 ? 1 : i < 6 ? 2 : 3));
                Assert.That(levels[i].Stun, Is.EqualTo(i < 5 ? 0 : .5f));
                Assert.That(levels[i].Chains, Is.EqualTo(i == 7 ? 1 : 0));
            }
            Assert.That(asset.Range, Is.EqualTo(15));
            Assert.That(asset.Radius, Is.EqualTo(2));
            Assert.That(asset.ChainRange, Is.EqualTo(4));
        }
        [TestCase(0, 1, 0, 0)]
        [TestCase(20, 0, 0, 0)]
        [TestCase(20, 4, 0, 0)]
        [TestCase(20, 1, -1, 0)]
        [TestCase(20, 1, 0, 2)]
        public void RejectsInvalidConfiguration(float damage, int strikes, float stun, int chains)
        {
            Assert.Throws<ArgumentException>(() => new LightningConfig(damage, 4, strikes, stun, chains));
        }
    }
}
