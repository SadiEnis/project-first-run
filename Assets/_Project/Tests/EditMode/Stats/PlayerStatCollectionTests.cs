using NUnit.Framework;
using ProjectFirstRun.Stats;

namespace ProjectFirstRun.Tests.EditMode.Stats
{
    public sealed class PlayerStatCollectionTests
    {
        private PlayerStatCollection _stats;

        [SetUp]
        public void SetUp()
        {
            _stats =
                new PlayerStatCollection();
        }

        [Test]
        public void NewCollection_HasNoModifiers()
        {
            Assert.That(
                _stats.ModifierCount,
                Is.EqualTo(0));

            Assert.That(
                _stats.Modifiers.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void Add_WithValidModifier_AddsModifier()
        {
            StatModifier modifier =
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.Flat,
                    10f);

            _stats.Add(
                modifier);

            Assert.That(
                _stats.ModifierCount,
                Is.EqualTo(1));

            Assert.That(
                _stats.Contains(modifier),
                Is.True);

            Assert.That(
                _stats.Modifiers[0],
                Is.SameAs(modifier));
        }

        [Test]
        public void Add_WithNullModifier_Throws()
        {
            Assert.That(
                () => _stats.Add(null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Add_SameModifierInstanceTwice_Throws()
        {
            StatModifier modifier =
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.Flat,
                    10f);

            _stats.Add(
                modifier);

            Assert.That(
                () => _stats.Add(modifier),
                Throws.InvalidOperationException);

            Assert.That(
                _stats.ModifierCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Evaluate_WithNoModifiers_ReturnsBaseValue()
        {
            float result =
                _stats.Evaluate(
                    PlayerStatType.WeaponDamage,
                    100f);

            Assert.That(
                result,
                Is.EqualTo(100f));
        }

        [Test]
        public void Evaluate_WithFlatModifier_AddsValue()
        {
            _stats.Add(
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.Flat,
                    20f));

            float result =
                _stats.Evaluate(
                    PlayerStatType.WeaponDamage,
                    100f);

            Assert.That(
                result,
                Is.EqualTo(120f));
        }

        [Test]
        public void Evaluate_WithAdditivePercentModifier_MultipliesValue()
        {
            _stats.Add(
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.AdditivePercent,
                    0.25f));

            float result =
                _stats.Evaluate(
                    PlayerStatType.WeaponDamage,
                    100f);

            Assert.That(
                result,
                Is.EqualTo(125f));
        }

        [Test]
        public void Evaluate_WithMultipleAdditivePercentModifiers_SumsThem()
        {
            _stats.Add(
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.AdditivePercent,
                    0.20f,
                    "upgrade.first"));

            _stats.Add(
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.AdditivePercent,
                    0.30f,
                    "upgrade.second"));

            float result =
                _stats.Evaluate(
                    PlayerStatType.WeaponDamage,
                    100f);

            Assert.That(
                result,
                Is.EqualTo(150f)
                    .Within(0.0001f));
        }

        [Test]
        public void Evaluate_WithMultiplicativePercentModifier_MultipliesValue()
        {
            _stats.Add(
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.MultiplicativePercent,
                    0.10f));

            float result =
                _stats.Evaluate(
                    PlayerStatType.WeaponDamage,
                    100f);

            Assert.That(
                result,
                Is.EqualTo(110f)
                    .Within(0.0001f));
        }

        [Test]
        public void Evaluate_WithMultipleMultiplicativeModifiers_CompoundsThem()
        {
            _stats.Add(
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.MultiplicativePercent,
                    0.20f,
                    "upgrade.first"));

            _stats.Add(
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.MultiplicativePercent,
                    0.50f,
                    "upgrade.second"));

            float result =
                _stats.Evaluate(
                    PlayerStatType.WeaponDamage,
                    100f);

            Assert.That(
                result,
                Is.EqualTo(180f)
                    .Within(0.0001f));
        }

        [Test]
        public void Evaluate_WithMixedModifiers_UsesDefinedOperationOrder()
        {
            _stats.Add(
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.Flat,
                    20f,
                    "upgrade.flat"));

            _stats.Add(
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.AdditivePercent,
                    0.25f,
                    "upgrade.additive"));

            _stats.Add(
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.MultiplicativePercent,
                    0.10f,
                    "upgrade.multiplicative"));

            float result =
                _stats.Evaluate(
                    PlayerStatType.WeaponDamage,
                    100f);

            Assert.That(
                result,
                Is.EqualTo(165f)
                    .Within(0.0001f));
        }

        [Test]
        public void Evaluate_IgnoresModifiersForOtherStatTypes()
        {
            _stats.Add(
                CreateModifier(
                    PlayerStatType.AbilityDamage,
                    StatModifierOperation.Flat,
                    1000f));

            float result =
                _stats.Evaluate(
                    PlayerStatType.WeaponDamage,
                    100f);

            Assert.That(
                result,
                Is.EqualTo(100f));
        }

        [Test]
        public void Evaluate_WithNegativeModifier_SupportsReduction()
        {
            _stats.Add(
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.AdditivePercent,
                    -0.25f));

            float result =
                _stats.Evaluate(
                    PlayerStatType.WeaponDamage,
                    100f);

            Assert.That(
                result,
                Is.EqualTo(75f));
        }

        [Test]
        public void Evaluate_WithInvalidBaseValue_Throws()
        {
            Assert.That(
                () =>
                    _stats.Evaluate(
                        PlayerStatType.WeaponDamage,
                        float.NaN),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());

            Assert.That(
                () =>
                    _stats.Evaluate(
                        PlayerStatType.WeaponDamage,
                        float.PositiveInfinity),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Evaluate_WithUnsupportedStatType_Throws()
        {
            PlayerStatType invalid =
                (PlayerStatType)999;

            Assert.That(
                () =>
                    _stats.Evaluate(
                        invalid,
                        100f),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Remove_WithExistingModifier_RemovesIt()
        {
            StatModifier modifier =
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.Flat,
                    10f);

            _stats.Add(
                modifier);

            bool removed =
                _stats.Remove(
                    modifier);

            Assert.That(
                removed,
                Is.True);

            Assert.That(
                _stats.ModifierCount,
                Is.EqualTo(0));

            Assert.That(
                _stats.Contains(modifier),
                Is.False);
        }

        [Test]
        public void Remove_WithMissingModifier_ReturnsFalse()
        {
            StatModifier modifier =
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.Flat,
                    10f);

            bool removed =
                _stats.Remove(
                    modifier);

            Assert.That(
                removed,
                Is.False);
        }

        [Test]
        public void Remove_WithNullModifier_ReturnsFalse()
        {
            Assert.That(
                _stats.Remove(null),
                Is.False);
        }

        [Test]
        public void RemoveBySource_RemovesAllModifiersFromSource()
        {
            _stats.Add(
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.AdditivePercent,
                    0.20f,
                    "upgrade.hybrid"));

            _stats.Add(
                CreateModifier(
                    PlayerStatType.AbilityDamage,
                    StatModifierOperation.AdditivePercent,
                    0.20f,
                    "upgrade.hybrid"));

            _stats.Add(
                CreateModifier(
                    PlayerStatType.MaxHealth,
                    StatModifierOperation.Flat,
                    25f,
                    "upgrade.other"));

            int removed =
                _stats.RemoveBySource(
                    "upgrade.hybrid");

            Assert.That(
                removed,
                Is.EqualTo(2));

            Assert.That(
                _stats.ModifierCount,
                Is.EqualTo(1));

            Assert.That(
                _stats.Modifiers[0].SourceId,
                Is.EqualTo("upgrade.other"));
        }

        [Test]
        public void RemoveBySource_WithUnknownSource_ReturnsZero()
        {
            _stats.Add(
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.Flat,
                    10f));

            int removed =
                _stats.RemoveBySource(
                    "upgrade.missing");

            Assert.That(
                removed,
                Is.EqualTo(0));

            Assert.That(
                _stats.ModifierCount,
                Is.EqualTo(1));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void RemoveBySource_WithMissingSourceId_Throws(
            string sourceId)
        {
            Assert.That(
                () =>
                    _stats.RemoveBySource(
                        sourceId),
                Throws.ArgumentException);
        }

        [Test]
        public void RemoveBySource_WithWhitespaceAroundSourceId_Throws()
        {
            Assert.That(
                () =>
                    _stats.RemoveBySource(
                        " upgrade.test "),
                Throws.ArgumentException);
        }

        [Test]
        public void SourceComparison_IsCaseSensitive()
        {
            _stats.Add(
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.Flat,
                    10f,
                    "upgrade.Test"));

            int removed =
                _stats.RemoveBySource(
                    "upgrade.test");

            Assert.That(
                removed,
                Is.EqualTo(0));

            Assert.That(
                _stats.ModifierCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Modifiers_CannotBeMutatedExternally()
        {
            StatModifier modifier =
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.Flat,
                    10f);

            _stats.Add(
                modifier);

            System.Collections.Generic.ICollection<
                StatModifier> collection =
                _stats.Modifiers as
                    System.Collections.Generic.ICollection<
                        StatModifier>;

            Assert.That(
                collection,
                Is.Not.Null);

            Assert.That(
                collection.IsReadOnly,
                Is.True);

            Assert.That(
                () =>
                    collection.Add(
                        CreateModifier(
                            PlayerStatType.AbilityDamage,
                            StatModifierOperation.Flat,
                            20f)),
                Throws.TypeOf<
                    System.NotSupportedException>());

            Assert.That(
                _stats.ModifierCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Evaluate_WhenCalculationOverflows_Throws()
        {
            _stats.Add(
                CreateModifier(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.MultiplicativePercent,
                    float.MaxValue));

            Assert.That(
                () =>
                    _stats.Evaluate(
                        PlayerStatType.WeaponDamage,
                        float.MaxValue),
                Throws.InvalidOperationException);
        }

        private static StatModifier CreateModifier(
            PlayerStatType statType,
            StatModifierOperation operation,
            float value,
            string sourceId = "upgrade.test")
        {
            return new StatModifier(
                statType,
                operation,
                value,
                sourceId);
        }
    }
}