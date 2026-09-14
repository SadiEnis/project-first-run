using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Rewards;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Chests
{
    public sealed class ChestDefinitionTests
    {
        private ChestDefinition _definition;
        private RewardItemPool _rewardItemPool;
        private GameObject _worldPrefab;

        [SetUp]
        public void SetUp()
        {
            _definition =
                ScriptableObject
                    .CreateInstance<ChestDefinition>();

            _rewardItemPool =
                ScriptableObject
                    .CreateInstance<RewardItemPool>();

            _worldPrefab =
                new GameObject("ChestPrefab");

            _worldPrefab.AddComponent<
                ChestController>();

            ConfigureDefinition(
                "chest.foundation",
                _rewardItemPool,
                3,
                _worldPrefab);
        }

        [TearDown]
        public void TearDown()
        {
            if (_definition != null)
            {
                Object.DestroyImmediate(
                    _definition);
            }

            if (_rewardItemPool != null)
            {
                Object.DestroyImmediate(
                    _rewardItemPool);
            }

            if (_worldPrefab != null)
            {
                Object.DestroyImmediate(
                    _worldPrefab);
            }
        }

        [Test]
        public void Validate_WithValidConfiguration_PreservesValues()
        {
            Assert.That(
                _definition.Validate,
                Throws.Nothing);

            Assert.That(
                _definition.StableId,
                Is.EqualTo("chest.foundation"));

            Assert.That(
                _definition.RewardItemPool,
                Is.SameAs(_rewardItemPool));

            Assert.That(
                _definition.RequestedChoiceCount,
                Is.EqualTo(3));

            Assert.That(_definition.MaxSelections, Is.EqualTo(1));

            Assert.That(
                _definition.WorldPrefab,
                Is.SameAs(_worldPrefab));
        }

        [Test]
        public void DefaultRarity_PreservesExistingCommonChestConfiguration()
        {
            Assert.That(_definition.Rarity, Is.EqualTo(ChestRarity.Common));
            Assert.That(_definition.RequestedChoiceCount, Is.EqualTo(3));
            Assert.That(_definition.MaxSelections, Is.EqualTo(1));
        }

        [Test]
        public void MultiClaimConfiguration_AllowsSelectionsUpToRequestedChoices()
        {
            SetField("_requestedChoiceCount", 5);
            SetField("_maxSelections", 2);

            Assert.DoesNotThrow(() => _definition.Validate());
            Assert.That(_definition.RequestedChoiceCount, Is.EqualTo(5));
            Assert.That(_definition.MaxSelections, Is.EqualTo(2));
        }

        [Test]
        public void Validate_WithMoreSelectionsThanChoices_Throws()
        {
            SetField("_requestedChoiceCount", 3);
            SetField("_maxSelections", 4);

            Assert.Throws<System.InvalidOperationException>(() => _definition.Validate());
        }

        [TestCase(ChestRarity.Common, 0)]
        [TestCase(ChestRarity.Uncommon, 1)]
        [TestCase(ChestRarity.Rare, 2)]
        [TestCase(ChestRarity.Legendary, 3)]
        public void Rarity_HasStableSerializedValueWithoutChangingRewardPolicy(ChestRarity rarity, int value)
        {
            SetField("_rarity", rarity);
            Assert.DoesNotThrow(() => _definition.Validate());
            Assert.That((int)_definition.Rarity, Is.EqualTo(value));
            Assert.That(_definition.RewardItemPool, Is.SameAs(_rewardItemPool));
            Assert.That(_definition.RequestedChoiceCount, Is.EqualTo(3));
        }

        [TestCase(-1)]
        [TestCase(4)]
        [TestCase(int.MaxValue)]
        public void UnknownRarity_IsRejected(int value)
        {
            SetField("_rarity", (ChestRarity)value);
            Assert.Throws<System.InvalidOperationException>(() => _definition.Validate());
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Validate_WithMissingStableId_Throws(
            string stableId)
        {
            SetField(
                "_stableId",
                stableId);

            Assert.That(
                _definition.Validate,
                Throws.InvalidOperationException);
        }

        [Test]
        public void Validate_WithUntrimmedStableId_Throws()
        {
            SetField(
                "_stableId",
                " chest.foundation ");

            Assert.That(
                _definition.Validate,
                Throws.InvalidOperationException);
        }

        [Test]
        public void OnValidate_TrimsStableId()
        {
            SetField(
                "_stableId",
                " chest.foundation ");

            InvokeOnValidate();

            Assert.That(
                _definition.StableId,
                Is.EqualTo("chest.foundation"));
        }

        [Test]
        public void Validate_WithMissingRewardItemPool_Throws()
        {
            SetField(
                "_rewardItemPool",
                null);

            Assert.That(
                _definition.Validate,
                Throws.InvalidOperationException);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Validate_WithNonPositiveChoiceCount_Throws(
            int choiceCount)
        {
            SetField(
                "_requestedChoiceCount",
                choiceCount);

            Assert.That(
                _definition.Validate,
                Throws.InvalidOperationException);
        }

        [Test]
        public void Validate_WithMissingWorldPrefab_Throws()
        {
            SetField(
                "_worldPrefab",
                null);

            Assert.That(
                _definition.Validate,
                Throws.InvalidOperationException);
        }

        [Test]
        public void Validate_WithWorldPrefabMissingController_Throws()
        {
            Object.DestroyImmediate(
                _worldPrefab.GetComponent<
                    ChestController>());

            Assert.That(
                _definition.Validate,
                Throws.InvalidOperationException);
        }

        private void ConfigureDefinition(
            string stableId,
            RewardItemPool rewardItemPool,
            int requestedChoiceCount,
            GameObject worldPrefab)
        {
            SetField(
                "_stableId",
                stableId);

            SetField(
                "_rewardItemPool",
                rewardItemPool);

            SetField(
                "_requestedChoiceCount",
                requestedChoiceCount);

            SetField(
                "_worldPrefab",
                worldPrefab);
        }

        private void SetField(
            string fieldName,
            object value)
        {
            FieldInfo field =
                typeof(ChestDefinition)
                    .GetField(
                        fieldName,
                        BindingFlags.Instance |
                        BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null);

            field.SetValue(
                _definition,
                value);
        }

        private void InvokeOnValidate()
        {
            MethodInfo method =
                typeof(ChestDefinition)
                    .GetMethod(
                        "OnValidate",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic);

            Assert.That(
                method,
                Is.Not.Null);

            method.Invoke(
                _definition,
                null);
        }
    }
}
