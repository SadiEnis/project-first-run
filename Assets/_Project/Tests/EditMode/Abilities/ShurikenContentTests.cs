using System;
using NUnit.Framework;
using ProjectFirstRun.Abilities.Shuriken;
using UnityEditor;
namespace ProjectFirstRun.Tests.EditMode.Abilities
{
    public sealed class ShurikenContentTests
    {
        [Test] public void AuthoredProgressionMatchesDesign()
        {
            var asset = AssetDatabase.LoadAssetAtPath<ShurikenDefinition>("Assets/_Project/Data/Abilities/AD_Shuriken.asset");
            Assert.That(asset, Is.Not.Null);
            Assert.That(asset.StableId, Is.EqualTo("ability.shuriken"));
            var levels = asset.CreateLevelConfigs(); Assert.That(levels.Length, Is.EqualTo(8));
            for (int i = 0; i < 8; i++)
            {
                Assert.That(levels[i].Damage, Is.EqualTo(i == 0 ? 15 : i < 4 ? 25 : 35));
                Assert.That(levels[i].TurnSeconds, Is.EqualTo(i < 2 ? 1.5f : 1));
                Assert.That(levels[i].Cooldown, Is.EqualTo(i < 3 ? 3 : 2));
                Assert.That(levels[i].Radius, Is.EqualTo(i < 6 ? 2.5f : 3.5f));
                Assert.That(levels[i].Count, Is.EqualTo(i < 5 ? 1 : 2));
                Assert.That(levels[i].BleedDamage, Is.EqualTo(i == 7 ? 5 : 0));
            }
        }
        [Test] public void RejectsInvalidConfiguration()
        {
            Assert.Throws<ArgumentException>(() => new ShurikenConfig(0, 1, 2, 3, 1, 0));
            Assert.Throws<ArgumentException>(() => new ShurikenConfig(1, 0, 2, 3, 1, 0));
            Assert.Throws<ArgumentException>(() => new ShurikenConfig(1, 1, 2, 3, 3, 0));
            Assert.Throws<ArgumentException>(() => new ShurikenConfig(1, 1, 2, 3, 1, -1));
        }
    }
}
