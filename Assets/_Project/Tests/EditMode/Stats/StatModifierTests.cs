using NUnit.Framework;
using ProjectFirstRun.Stats;

namespace ProjectFirstRun.Tests.EditMode.Stats
{
    public sealed class StatModifierTests
    {
        [Test]
        public void Constructor_WithValidValues_StoresValues()
        {
            StatModifier modifier =
                new StatModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.AdditivePercent,
                    0.25f,
                    "upgrade.test");

            Assert.That(
                modifier.StatType,
                Is.EqualTo(
                    PlayerStatType.WeaponDamage));

            Assert.That(
                modifier.Operation,
                Is.EqualTo(
                    StatModifierOperation.AdditivePercent));

            Assert.That(
                modifier.Value,
                Is.EqualTo(0.25f));

            Assert.That(
                modifier.SourceId,
                Is.EqualTo("upgrade.test"));
        }

        [Test]
        public void Constructor_WithNaNValue_Throws()
        {
            Assert.That(
                () =>
                    new StatModifier(
                        PlayerStatType.WeaponDamage,
                        StatModifierOperation.Flat,
                        float.NaN,
                        "upgrade.test"),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_WithInfiniteValue_Throws()
        {
            Assert.That(
                () =>
                    new StatModifier(
                        PlayerStatType.WeaponDamage,
                        StatModifierOperation.Flat,
                        float.PositiveInfinity,
                        "upgrade.test"),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Constructor_WithMissingSourceId_Throws(
            string sourceId)
        {
            Assert.That(
                () =>
                    new StatModifier(
                        PlayerStatType.WeaponDamage,
                        StatModifierOperation.Flat,
                        10f,
                        sourceId),
                Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WithWhitespaceAroundSourceId_Throws()
        {
            Assert.That(
                () =>
                    new StatModifier(
                        PlayerStatType.WeaponDamage,
                        StatModifierOperation.Flat,
                        10f,
                        " upgrade.test "),
                Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WithUnsupportedStatType_Throws()
        {
            PlayerStatType invalid =
                (PlayerStatType)999;

            Assert.That(
                () =>
                    new StatModifier(
                        invalid,
                        StatModifierOperation.Flat,
                        10f,
                        "upgrade.test"),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_WithUnsupportedOperation_Throws()
        {
            StatModifierOperation invalid =
                (StatModifierOperation)999;

            Assert.That(
                () =>
                    new StatModifier(
                        PlayerStatType.WeaponDamage,
                        invalid,
                        10f,
                        "upgrade.test"),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());
        }
    }
}