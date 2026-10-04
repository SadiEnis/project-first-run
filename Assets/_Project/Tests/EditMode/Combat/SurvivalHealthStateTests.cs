using System;
using NUnit.Framework;
using ProjectFirstRun.Combat;

namespace ProjectFirstRun.Tests.EditMode.Combat
{
    public sealed class SurvivalHealthStateTests
    {
        [Test]
        public void MaximumChangesPreserveInjuryAndDoNotResurrect()
        {
            var health = new HealthState(100);
            health.ApplyDamage(40);
            Assert.That(health.SetMaximumHealth(110), Is.True);
            Assert.That(health.CurrentHealth, Is.EqualTo(70));
            Assert.That(health.SetMaximumHealth(110), Is.False);
            Assert.That(health.CurrentHealth, Is.EqualTo(70));
            health.SetMaximumHealth(80);
            Assert.That(health.CurrentHealth, Is.EqualTo(70));
            health.SetMaximumHealth(50);
            Assert.That(health.CurrentHealth, Is.EqualTo(50));
            health.ApplyDamage(50);
            health.SetMaximumHealth(150);
            Assert.That(health.Heal(100), Is.Zero);
            Assert.That(health.IsDead, Is.True);
        }

        [TestCase(0f, 20f)]
        [TestCase(.05f, 19f)]
        [TestCase(.25f, 15f)]
        [TestCase(1f, 5f)]
        [TestCase(-1f, 20f)]
        public void ReductionIsCappedAndOriginalRequestPreserved(float reduction, float expected)
        {
            var health = new HealthState(100);
            var result = health.ApplyDamage(20, reduction);
            Assert.That(result.RequestedDamage, Is.EqualTo(20));
            Assert.That(result.AppliedDamage, Is.EqualTo(expected).Within(.001f));
            Assert.That(health.CurrentHealth, Is.EqualTo(100 - expected).Within(.001f));
        }

        [Test]
        public void FractionalAndLethalDamageAndHealing()
        {
            var health = new HealthState(10);
            Assert.That(health.ApplyDamage(1, .75f).AppliedDamage, Is.EqualTo(.25f));
            Assert.That(health.Heal(100), Is.EqualTo(.25f));
            Assert.That(health.Heal(100), Is.Zero);
            Assert.That(health.ApplyDamage(100, .75f).WasLethal, Is.True);
            Assert.That(health.ApplyDamage(100).WasApplied, Is.False);
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(-1f)]
        public void InvalidHealingAndMaximumDoNotMutate(float value)
        {
            var health = new HealthState(100);
            Assert.Throws<ArgumentOutOfRangeException>(() => health.SetMaximumHealth(value));
            Assert.Throws<ArgumentOutOfRangeException>(() => health.Heal(value));
            Assert.That(health.CurrentHealth, Is.EqualTo(100));
            Assert.That(health.MaximumHealth, Is.EqualTo(100));
        }
    }
}
