#if UNITY_EDITOR

using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Development.Rewards;
using ProjectFirstRun.Items;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.Upgrades;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Development.Rewards
{
    public sealed class RewardDevelopmentControllerTests
    {
        private GameObject _playerObject;
        private GameObject _controllerObject;

        private PlayerBuildController _buildController;
        private RewardDevelopmentController _controller;

        private RewardItemPool _pool;

        private readonly List<ItemDefinition>
            _createdDefinitions =
                new List<ItemDefinition>();

        [SetUp]
        public void SetUp()
        {
            _playerObject =
                new GameObject(
                    "RewardDevelopment_Player");

            _buildController =
                _playerObject
                    .AddComponent<PlayerBuildController>();

            _buildController.Initialize(
                PlayerBuildCapacity.CreateDefault());

            _pool =
                ScriptableObject
                    .CreateInstance<RewardItemPool>();

            _controllerObject =
                new GameObject(
                    "RewardDevelopment_Controller");

            _controller =
                _controllerObject
                    .AddComponent<
                        RewardDevelopmentController>();

            /*
             * Prevent Start() from automatically generating
             * an offer during these explicit tests.
             */
            _controller.enabled = false;

            SetPrivateField(
                _controller,
                typeof(RewardDevelopmentController),
                "_rewardItemPool",
                _pool);

            SetPrivateField(
                _controller,
                typeof(RewardDevelopmentController),
                "_playerBuildController",
                _buildController);

            SetPrivateField(
                _controller,
                typeof(RewardDevelopmentController),
                "_requestedChoiceCount",
                3);
        }

        [TearDown]
        public void TearDown()
        {
            if (_controllerObject != null)
            {
                Object.DestroyImmediate(
                    _controllerObject);
            }

            if (_playerObject != null)
            {
                Object.DestroyImmediate(
                    _playerObject);
            }

            if (_pool != null)
            {
                Object.DestroyImmediate(
                    _pool);
            }

            foreach (ItemDefinition definition
                     in _createdDefinitions)
            {
                if (definition != null)
                {
                    Object.DestroyImmediate(
                        definition);
                }
            }

            _createdDefinitions.Clear();
        }

        [Test]
        public void GenerateOffer_WithEligibleItems_CreatesOffer()
        {
            UpgradeDefinition first =
                CreateUpgrade(
                    "upgrade.first");

            UpgradeDefinition second =
                CreateUpgrade(
                    "upgrade.second");

            SetPoolItems(
                first,
                second);

            RewardOffer offer =
                _controller.GenerateOffer();

            Assert.That(
                offer,
                Is.Not.Null);

            Assert.That(
                _controller.HasGeneratedOffer,
                Is.True);

            Assert.That(
                _controller.CurrentOffer,
                Is.SameAs(offer));

            Assert.That(
                offer.ChoiceCount,
                Is.EqualTo(2));
        }

        [Test]
        public void GenerateOffer_UsesCurrentPlayerBuildEligibility()
        {
            UpgradeDefinition owned =
                CreateUpgrade(
                    "upgrade.owned");

            UpgradeDefinition available =
                CreateUpgrade(
                    "upgrade.available");

            SetPoolItems(
                owned,
                available);

            _buildController.TryAdd(
                owned);

            RewardOffer offer =
                _controller.GenerateOffer();

            Assert.That(
                offer.ChoiceCount,
                Is.EqualTo(1));

            Assert.That(
                offer.Choices[0],
                Is.SameAs(available));
        }

        [Test]
        public void GenerateOffer_WithNoEligibleItems_CreatesEmptyOffer()
        {
            UpgradeDefinition owned =
                CreateUpgrade(
                    "upgrade.owned");

            SetPoolItems(
                owned);

            _buildController.TryAdd(
                owned);

            RewardOffer offer =
                _controller.GenerateOffer();

            Assert.That(
                offer.IsEmpty,
                Is.True);

            Assert.That(
                _controller.HasGeneratedOffer,
                Is.True);
        }

        [Test]
        public void GenerateOffer_CanReplacePreviousOffer()
        {
            UpgradeDefinition first =
                CreateUpgrade(
                    "upgrade.first");

            UpgradeDefinition second =
                CreateUpgrade(
                    "upgrade.second");

            SetPoolItems(
                first,
                second);

            RewardOffer firstOffer =
                _controller.GenerateOffer();

            RewardOffer secondOffer =
                _controller.GenerateOffer();

            Assert.That(
                secondOffer,
                Is.Not.Null);

            Assert.That(
                secondOffer,
                Is.Not.SameAs(firstOffer));

            Assert.That(
                _controller.CurrentOffer,
                Is.SameAs(secondOffer));
        }

        private UpgradeDefinition CreateUpgrade(
            string stableId)
        {
            UpgradeDefinition definition =
                ScriptableObject
                    .CreateInstance<UpgradeDefinition>();

            SetPrivateField(
                definition,
                typeof(ItemDefinition),
                "_stableId",
                stableId);

            _createdDefinitions.Add(
                definition);

            return definition;
        }

        private void SetPoolItems(
            params ItemDefinition[] items)
        {
            SetPrivateField(
                _pool,
                typeof(RewardItemPool),
                "_items",
                new List<ItemDefinition>(
                    items));
        }

        private static void SetPrivateField(
            object target,
            System.Type declaringType,
            string fieldName,
            object value)
        {
            FieldInfo field =
                declaringType.GetField(
                    fieldName,
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null);

            field.SetValue(
                target,
                value);
        }
    }
}

#endif