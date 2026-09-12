using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Items;
using ProjectFirstRun.Stats;
using ProjectFirstRun.Upgrades;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Upgrades
{
    public sealed class UpgradeDefinitionTests
    {
        private UpgradeDefinition _definition;

        [SetUp]
        public void SetUp()
        {
            _definition =
                ScriptableObject
                    .CreateInstance<UpgradeDefinition>();

            SetStableId(
                "upgrade.test");
        }

        [TearDown]
        public void TearDown()
        {
            if (_definition != null)
            {
                Object.DestroyImmediate(
                    _definition);
            }
        }

        [Test]
        public void Category_IsUpgrade()
        {
            Assert.That(
                _definition.Category,
                Is.EqualTo(
                    ItemCategory.Upgrade));
        }

        [Test]
        public void NewDefinition_HasNoModifiers()
        {
            Assert.That(
                _definition.ModifierCount,
                Is.EqualTo(0));

            Assert.That(
                _definition.StatModifiers.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void CreateRuntimeModifiers_WithNoModifiers_ReturnsEmptyArray()
        {
            StatModifier[] modifiers =
                _definition
                    .CreateRuntimeModifiers();

            Assert.That(
                modifiers,
                Is.Not.Null);

            Assert.That(
                modifiers.Length,
                Is.EqualTo(0));
        }

        [Test]
        public void CreateRuntimeModifiers_UsesDefinitionStableIdAsSource()
        {
            SetModifierData(
                new UpgradeStatModifierData(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.AdditivePercent,
                    0.25f));

            StatModifier[] modifiers =
                _definition
                    .CreateRuntimeModifiers();

            Assert.That(
                modifiers.Length,
                Is.EqualTo(1));

            Assert.That(
                modifiers[0].SourceId,
                Is.EqualTo(
                    "upgrade.test"));

            Assert.That(
                modifiers[0].StatType,
                Is.EqualTo(
                    PlayerStatType.WeaponDamage));

            Assert.That(
                modifiers[0].Operation,
                Is.EqualTo(
                    StatModifierOperation.AdditivePercent));

            Assert.That(
                modifiers[0].Value,
                Is.EqualTo(0.25f));
        }

        [Test]
        public void CreateRuntimeModifiers_WithMultipleModifiers_CreatesAll()
        {
            SetModifierData(
                new UpgradeStatModifierData(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.AdditivePercent,
                    0.20f),
                new UpgradeStatModifierData(
                    PlayerStatType.AbilityDamage,
                    StatModifierOperation.AdditivePercent,
                    0.30f),
                new UpgradeStatModifierData(
                    PlayerStatType.MaxHealth,
                    StatModifierOperation.Flat,
                    25f));

            StatModifier[] modifiers =
                _definition
                    .CreateRuntimeModifiers();

            Assert.That(
                modifiers.Length,
                Is.EqualTo(3));

            Assert.That(
                modifiers[0].StatType,
                Is.EqualTo(
                    PlayerStatType.WeaponDamage));

            Assert.That(
                modifiers[1].StatType,
                Is.EqualTo(
                    PlayerStatType.AbilityDamage));

            Assert.That(
                modifiers[2].StatType,
                Is.EqualTo(
                    PlayerStatType.MaxHealth));

            Assert.That(
                modifiers[0].SourceId,
                Is.EqualTo(
                    "upgrade.test"));

            Assert.That(
                modifiers[1].SourceId,
                Is.EqualTo(
                    "upgrade.test"));

            Assert.That(
                modifiers[2].SourceId,
                Is.EqualTo(
                    "upgrade.test"));
        }

        [Test]
        public void CreateRuntimeModifiers_EachCallCreatesNewRuntimeInstances()
        {
            SetModifierData(
                new UpgradeStatModifierData(
                    PlayerStatType.WeaponDamage,
                    StatModifierOperation.Flat,
                    10f));

            StatModifier[] first =
                _definition
                    .CreateRuntimeModifiers();

            StatModifier[] second =
                _definition
                    .CreateRuntimeModifiers();

            Assert.That(
                first,
                Is.Not.SameAs(second));

            Assert.That(
                first[0],
                Is.Not.SameAs(second[0]));
        }

        [Test]
        public void CreateRuntimeModifiers_WithMissingStableId_Throws()
        {
            SetStableId(
                "");

            Assert.That(
                () =>
                    _definition
                        .CreateRuntimeModifiers(),
                Throws.InvalidOperationException);
        }

        [Test]
        public void CreateRuntimeModifiers_WithNullModifierData_Throws()
        {
            SetModifierData(
                (UpgradeStatModifierData)null);

            Assert.That(
                () =>
                    _definition
                        .CreateRuntimeModifiers(),
                Throws.InvalidOperationException);
        }

        private void SetStableId(
            string stableId)
        {
            FieldInfo field =
                typeof(ItemDefinition)
                    .GetField(
                        "_stableId",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null);

            field.SetValue(
                _definition,
                stableId);
        }

        private void SetModifierData(
            params UpgradeStatModifierData[] modifiers)
        {
            FieldInfo field =
                typeof(UpgradeDefinition)
                    .GetField(
                        "_statModifiers",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null);

            field.SetValue(
                _definition,
                new List<UpgradeStatModifierData>(
                    modifiers));
        }
    }
}