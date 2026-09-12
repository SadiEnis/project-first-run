using NUnit.Framework;
using ProjectFirstRun.Stats;
using ProjectFirstRun.Upgrades;

namespace ProjectFirstRun.Tests.EditMode.Upgrades
{
    public sealed class UpgradeStatModifierDataTests
    {
        [Test]
        public void Constructor_WithValidValues_StoresValues()
        {
            UpgradeStatModifierData data =
                new UpgradeStatModifierData(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.AdditivePercent,
                    0.25f);

            Assert.That(
                data.StatType,
                Is.EqualTo(
                    PlayerStatType.WeaponDamage));

            Assert.That(
                data.Operation,
                Is.EqualTo(
                    StatModifierOperation.AdditivePercent));

            Assert.That(
                data.Value,
                Is.EqualTo(0.25f));
        }

        [Test]
        public void Constructor_WithInvalidStatType_Throws()
        {
            PlayerStatType invalid =
                (PlayerStatType)999;

            Assert.That(
                () =>
                    new UpgradeStatModifierData(
                        invalid,
                        StatModifierOperation.Flat,
                        10f),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_WithInvalidOperation_Throws()
        {
            StatModifierOperation invalid =
                (StatModifierOperation)999;

            Assert.That(
                () =>
                    new UpgradeStatModifierData(
                        PlayerStatType.WeaponDamage,
                        invalid,
                        10f),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_WithNaNValue_Throws()
        {
            Assert.That(
                () =>
                    new UpgradeStatModifierData(
                        PlayerStatType.WeaponDamage,
                        StatModifierOperation.Flat,
                        float.NaN),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_WithInfiniteValue_Throws()
        {
            Assert.That(
                () =>
                    new UpgradeStatModifierData(
                        PlayerStatType.WeaponDamage,
                        StatModifierOperation.Flat,
                        float.PositiveInfinity),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void CreateRuntimeModifier_UsesProvidedSourceId()
        {
            UpgradeStatModifierData data =
                new UpgradeStatModifierData(
                    PlayerStatType.AbilityDamage,
                    StatModifierOperation.AdditivePercent,
                    0.20f);

            StatModifier modifier =
                data.CreateRuntimeModifier(
                    "upgrade.test");

            Assert.That(
                modifier.StatType,
                Is.EqualTo(
                    PlayerStatType.AbilityDamage));

            Assert.That(
                modifier.Operation,
                Is.EqualTo(
                    StatModifierOperation.AdditivePercent));

            Assert.That(
                modifier.Value,
                Is.EqualTo(0.20f));

            Assert.That(
                modifier.SourceId,
                Is.EqualTo(
                    "upgrade.test"));
        }

        [Test]
        public void CreateRuntimeModifier_WithInvalidSourceId_Throws()
        {
            UpgradeStatModifierData data =
                new UpgradeStatModifierData(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.Flat,
                    10f);

            Assert.That(
                () =>
                    data.CreateRuntimeModifier(
                        ""),
                Throws.ArgumentException);
        }
    }
}