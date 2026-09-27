using NUnit.Framework;
using ProjectFirstRun.Abilities.AcidBottle;
using ProjectFirstRun.Enemies;
using UnityEditor;
using UnityEngine;
namespace ProjectFirstRun.Tests.EditMode.Abilities
{
    public sealed class AcidBottleContentTests
    {
        [Test] public void AuthoredLevelsMatchApprovedProgression()
        {
            var asset = AssetDatabase.LoadAssetAtPath<AcidBottleDefinition>("Assets/_Project/Data/Abilities/AD_AcidBottle.asset");
            Assert.That(asset, Is.Not.Null); Assert.That(asset.StableId, Is.EqualTo("ability.acid-bottle"));
            var levels = asset.CreateLevelConfigs(); Assert.That(levels.Length, Is.EqualTo(8));
            float[] damage = {5,7,7,7,7,10,10,10}, duration = {3,3,4,4,4,4,5,5};
            for (int i = 0; i < 8; i++)
            {
                Assert.That(levels[i].Damage, Is.EqualTo(damage[i]));
                Assert.That(levels[i].Duration, Is.EqualTo(duration[i]));
                Assert.That(levels[i].Radius, Is.EqualTo(i < 3 ? 2 : 3));
                Assert.That(levels[i].Bottles, Is.EqualTo(i < 4 ? 1 : 2));
                Assert.That(levels[i].Slow, Is.EqualTo(i == 7 ? .5f : 0));
            }
            Assert.That(asset.Cooldown, Is.EqualTo(4)); Assert.That(asset.TargetRange, Is.EqualTo(15));
        }
        [Test] public void SlowContributionsDoNotMultiplyAndRemovingOnePreservesTheOther()
        {
            var go = new GameObject("acid motor test");
            go.SetActive(false);
            try
            {
                var motor = go.AddComponent<EnemyMotor>(); var a = new object(); var b = new object();
                motor.SetMovementModifier(a, .7f); motor.SetMovementModifier(b, .7f);
                Assert.That(motor.MovementMultiplier, Is.EqualTo(.7f));
                motor.RemoveMovementModifier(a); Assert.That(motor.MovementMultiplier, Is.EqualTo(.7f));
                motor.RemoveMovementModifier(b); Assert.That(motor.MovementMultiplier, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(go); }
        }
        [Test] public void InvalidAcidConfigurationIsRejected()
        {
            Assert.Throws<System.ArgumentException>(() => new AcidConfig(5, 3, 2, 1, 1));
            Assert.Throws<System.ArgumentException>(() => new AcidConfig(0, 3, 2, 1, 0));
        }
    }
}
