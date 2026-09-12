using System;
using NUnit.Framework;
using ProjectFirstRun.Combat;

namespace ProjectFirstRun.Tests.EditMode.Combat
{
    public sealed class HealthStateTests
    {
        [Test]
        public void Constructor_WithValidMaximumHealth_StartsAtMaximum()
        {
            HealthState healthState = new HealthState(100f);

            Assert.That(healthState.MaximumHealth, Is.EqualTo(100f));
            Assert.That(healthState.CurrentHealth, Is.EqualTo(100f));
            Assert.That(healthState.IsDead, Is.False);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void Constructor_WithNonPositiveMaximumHealth_Throws(
            float maximumHealth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new HealthState(maximumHealth));
        }

        [Test]
        public void Constructor_WithNaNMaximumHealth_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new HealthState(float.NaN));
        }

        [Test]
        public void Constructor_WithInfiniteMaximumHealth_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new HealthState(float.PositiveInfinity));
        }

        [Test]
        public void ApplyDamage_WithValidDamage_ReducesCurrentHealth()
        {
            HealthState healthState = new HealthState(100f);

            DamageResult result = healthState.ApplyDamage(25f);

            Assert.That(result.RequestedDamage, Is.EqualTo(25f));
            Assert.That(result.AppliedDamage, Is.EqualTo(25f));
            Assert.That(result.PreviousHealth, Is.EqualTo(100f));
            Assert.That(result.CurrentHealth, Is.EqualTo(75f));
            Assert.That(result.WasApplied, Is.True);
            Assert.That(result.WasLethal, Is.False);

            Assert.That(healthState.CurrentHealth, Is.EqualTo(75f));
            Assert.That(healthState.IsDead, Is.False);
        }

        [Test]
        public void ApplyDamage_WhenDamageExceedsHealth_ClampsAtZero()
        {
            HealthState healthState = new HealthState(30f);

            DamageResult result = healthState.ApplyDamage(50f);

            Assert.That(result.RequestedDamage, Is.EqualTo(50f));
            Assert.That(result.AppliedDamage, Is.EqualTo(30f));
            Assert.That(result.PreviousHealth, Is.EqualTo(30f));
            Assert.That(result.CurrentHealth, Is.EqualTo(0f));
            Assert.That(result.WasApplied, Is.True);
            Assert.That(result.WasLethal, Is.True);

            Assert.That(healthState.CurrentHealth, Is.EqualTo(0f));
            Assert.That(healthState.IsDead, Is.True);
        }

        [TestCase(0f)]
        [TestCase(-10f)]
        public void ApplyDamage_WithNonPositiveDamage_IsRejected(
            float requestedDamage)
        {
            HealthState healthState = new HealthState(100f);

            DamageResult result =
                healthState.ApplyDamage(requestedDamage);

            Assert.That(result.AppliedDamage, Is.EqualTo(0f));
            Assert.That(result.PreviousHealth, Is.EqualTo(100f));
            Assert.That(result.CurrentHealth, Is.EqualTo(100f));
            Assert.That(result.WasApplied, Is.False);
            Assert.That(result.WasLethal, Is.False);

            Assert.That(healthState.CurrentHealth, Is.EqualTo(100f));
        }

        [Test]
        public void ApplyDamage_WithNaNDamage_IsRejected()
        {
            HealthState healthState = new HealthState(100f);

            DamageResult result =
                healthState.ApplyDamage(float.NaN);

            Assert.That(result.WasApplied, Is.False);
            Assert.That(result.WasLethal, Is.False);
            Assert.That(healthState.CurrentHealth, Is.EqualTo(100f));
        }

        [Test]
        public void ApplyDamage_WhenAlreadyDead_IsRejected()
        {
            HealthState healthState = new HealthState(20f);

            DamageResult lethalResult =
                healthState.ApplyDamage(20f);

            DamageResult secondResult =
                healthState.ApplyDamage(10f);

            Assert.That(lethalResult.WasLethal, Is.True);

            Assert.That(secondResult.WasApplied, Is.False);
            Assert.That(secondResult.WasLethal, Is.False);
            Assert.That(secondResult.AppliedDamage, Is.EqualTo(0f));
            Assert.That(secondResult.PreviousHealth, Is.EqualTo(0f));
            Assert.That(secondResult.CurrentHealth, Is.EqualTo(0f));

            Assert.That(healthState.IsDead, Is.True);
        }

        [Test]
        public void ResetToMaximum_AfterDeath_RestoresHealth()
        {
            HealthState healthState = new HealthState(50f);

            healthState.ApplyDamage(50f);

            Assert.That(healthState.IsDead, Is.True);

            healthState.ResetToMaximum();

            Assert.That(healthState.CurrentHealth, Is.EqualTo(50f));
            Assert.That(healthState.IsDead, Is.False);

            DamageResult result =
                healthState.ApplyDamage(10f);

            Assert.That(result.WasApplied, Is.True);
            Assert.That(healthState.CurrentHealth, Is.EqualTo(40f));
        }
    }
}