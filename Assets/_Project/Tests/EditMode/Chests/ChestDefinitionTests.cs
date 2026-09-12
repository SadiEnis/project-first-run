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

            Assert.That(
                _definition.WorldPrefab,
                Is.SameAs(_worldPrefab));
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
