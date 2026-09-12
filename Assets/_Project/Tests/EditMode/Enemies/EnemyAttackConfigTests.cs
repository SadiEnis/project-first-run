using System;
using NUnit.Framework;
using ProjectFirstRun.Enemies;

namespace ProjectFirstRun.Tests.EditMode.Enemies
{
    public sealed class EnemyAttackConfigTests
    {
        [Test]
        public void Constructor_WithValidValues_CreatesConfiguration()
        {
            EnemyAttackConfig config = new EnemyAttackConfig(
                damage: 10f,
                range: 1.75f,
                cooldownDuration: 1f);

            Assert.That(config.Damage, Is.EqualTo(10f));
            Assert.That(config.Range, Is.EqualTo(1.75f));
            Assert.That(
                config.RangeSquared,
                Is.EqualTo(3.0625f).Within(0.0001f));

            Assert.That(
                config.CooldownDuration,
                Is.EqualTo(1f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Constructor_WithInvalidDamage_Throws(
            float damage)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new EnemyAttackConfig(
                    damage,
                    1.75f,
                    1f);
            });
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Constructor_WithInvalidRange_Throws(
            float range)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new EnemyAttackConfig(
                    10f,
                    range,
                    1f);
            });
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Constructor_WithInvalidCooldown_Throws(
            float cooldown)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new EnemyAttackConfig(
                    10f,
                    1.75f,
                    cooldown);
            });
        }
    }
}