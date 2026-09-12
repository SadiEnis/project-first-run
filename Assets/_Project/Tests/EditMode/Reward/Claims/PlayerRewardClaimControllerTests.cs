using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Items;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.Rewards.Claims;
using ProjectFirstRun.Weapons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Rewards.Claims
{
    public sealed class PlayerRewardClaimControllerTests
    {
        private GameObject _playerObject;

        private PlayerRewardClaimController
            _claimController;

        private readonly List<WeaponDefinition>
            _createdDefinitions =
                new List<WeaponDefinition>();

        [SetUp]
        public void SetUp()
        {
            _playerObject =
                new GameObject(
                    "PlayerRewardClaim_Test");

            /*
             * Keep inactive so Awake does not attempt
             * production component composition.
             */
            _playerObject.SetActive(
                false);

            _claimController =
                _playerObject.AddComponent<
                    PlayerRewardClaimController>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (WeaponDefinition definition
                     in _createdDefinitions)
            {
                if (definition != null)
                {
                    Object.DestroyImmediate(
                        definition);
                }
            }

            _createdDefinitions.Clear();

            if (_playerObject != null)
            {
                Object.DestroyImmediate(
                    _playerObject);
            }
        }

        [Test]
        public void NewController_IsNotInitialized()
        {
            Assert.That(
                _claimController.IsInitialized,
                Is.False);
        }

        [Test]
        public void Initialize_WithRegistry_InitializesController()
        {
            RewardClaimHandlerRegistry registry =
                new RewardClaimHandlerRegistry();

            _claimController.Initialize(
                registry);

            Assert.That(
                _claimController.IsInitialized,
                Is.True);
        }

        [Test]
        public void Initialize_WithNullRegistry_Throws()
        {
            Assert.That(
                () =>
                    _claimController.Initialize(
                        null),
                Throws.ArgumentNullException);

            Assert.That(
                _claimController.IsInitialized,
                Is.False);
        }

        [Test]
        public void Initialize_WhenAlreadyInitialized_Throws()
        {
            RewardClaimHandlerRegistry registry =
                new RewardClaimHandlerRegistry();

            _claimController.Initialize(
                registry);

            Assert.That(
                () =>
                    _claimController.Initialize(
                        new RewardClaimHandlerRegistry()),
                Throws.InvalidOperationException);
        }

        [Test]
        public void TryClaim_WhenHandlerClaims_ConsumesSession()
        {
            WeaponDefinition definition =
                CreateWeapon(
                    "weapon.reward");

            TestRewardClaimHandler handler =
                CreateControllerWithHandler(
                    definition,
                    RewardClaimResult.Claimed);

            RewardClaimSession session =
                CreateSession(
                    definition);

            RewardClaimResult result =
                _claimController.TryClaim(
                    session,
                    definition);

            Assert.That(
                result,
                Is.EqualTo(
                    RewardClaimResult.Claimed));

            Assert.That(
                session.IsClaimed,
                Is.True);

            Assert.That(
                session.ClaimedDefinition,
                Is.SameAs(definition));

            Assert.That(
                handler.TryClaimCallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void TryClaim_WhenSuccessful_PublishesEventOnce()
        {
            WeaponDefinition definition =
                CreateWeapon(
                    "weapon.reward");

            CreateControllerWithHandler(
                definition,
                RewardClaimResult.Claimed);

            RewardClaimSession session =
                CreateSession(
                    definition);

            int eventCount = 0;

            ItemDefinition acquiredDefinition =
                null;

            _claimController.RewardClaimed +=
                claimedDefinition =>
                {
                    eventCount++;
                    acquiredDefinition =
                        claimedDefinition;
                };

            _claimController.TryClaim(
                session,
                definition);

            Assert.That(
                eventCount,
                Is.EqualTo(1));

            Assert.That(
                acquiredDefinition,
                Is.SameAs(definition));
        }

        [Test]
        public void TryClaim_WhenAlreadyOwned_DoesNotConsumeSession()
        {
            WeaponDefinition definition =
                CreateWeapon(
                    "weapon.reward");

            CreateControllerWithHandler(
                definition,
                RewardClaimResult.AlreadyOwned);

            RewardClaimSession session =
                CreateSession(
                    definition);

            RewardClaimResult result =
                _claimController.TryClaim(
                    session,
                    definition);

            Assert.That(
                result,
                Is.EqualTo(
                    RewardClaimResult.AlreadyOwned));

            Assert.That(
                session.IsClaimed,
                Is.False);

            Assert.That(
                session.ClaimedDefinition,
                Is.Null);
        }

        [Test]
        public void TryClaim_WhenCapacityReached_DoesNotConsumeSession()
        {
            WeaponDefinition definition =
                CreateWeapon(
                    "weapon.reward");

            CreateControllerWithHandler(
                definition,
                RewardClaimResult.CapacityReached);

            RewardClaimSession session =
                CreateSession(
                    definition);

            RewardClaimResult result =
                _claimController.TryClaim(
                    session,
                    definition);

            Assert.That(
                result,
                Is.EqualTo(
                    RewardClaimResult.CapacityReached));

            Assert.That(
                session.IsClaimed,
                Is.False);

            Assert.That(
                session.ClaimedDefinition,
                Is.Null);
        }

        [Test]
        public void TryClaim_WhenSessionAlreadyClaimed_ReturnsAlreadyClaimedWithoutCallingHandlerAgain()
        {
            WeaponDefinition definition =
                CreateWeapon(
                    "weapon.reward");

            TestRewardClaimHandler handler =
                CreateControllerWithHandler(
                    definition,
                    RewardClaimResult.Claimed);

            RewardClaimSession session =
                CreateSession(
                    definition);

            RewardClaimResult firstResult =
                _claimController.TryClaim(
                    session,
                    definition);

            RewardClaimResult secondResult =
                _claimController.TryClaim(
                    session,
                    definition);

            Assert.That(
                firstResult,
                Is.EqualTo(
                    RewardClaimResult.Claimed));

            Assert.That(
                secondResult,
                Is.EqualTo(
                    RewardClaimResult.AlreadyClaimed));

            Assert.That(
                handler.TryClaimCallCount,
                Is.EqualTo(1));
        }

        [Test]
        public void TryClaim_WithDefinitionOutsideOffer_ThrowsWithoutCallingHandler()
        {
            WeaponDefinition offered =
                CreateWeapon(
                    "weapon.offered");

            WeaponDefinition outside =
                CreateWeapon(
                    "weapon.outside");

            TestRewardClaimHandler handler =
                CreateControllerWithHandler(
                    outside,
                    RewardClaimResult.Claimed);

            RewardClaimSession session =
                CreateSession(
                    offered);

            Assert.That(
                () =>
                    _claimController.TryClaim(
                        session,
                        outside),
                Throws.InvalidOperationException);

            Assert.That(
                handler.TryClaimCallCount,
                Is.EqualTo(0));

            Assert.That(
                session.IsClaimed,
                Is.False);
        }

        [Test]
        public void TryClaim_WhenNoHandlerSupportsDefinition_ThrowsWithoutConsumingSession()
        {
            WeaponDefinition definition =
                CreateWeapon(
                    "weapon.reward");

            RewardClaimHandlerRegistry registry =
                new RewardClaimHandlerRegistry();

            _claimController.Initialize(
                registry);

            RewardClaimSession session =
                CreateSession(
                    definition);

            Assert.That(
                () =>
                    _claimController.TryClaim(
                        session,
                        definition),
                Throws.InvalidOperationException);

            Assert.That(
                session.IsClaimed,
                Is.False);
        }

        [Test]
        public void TryClaim_WithNullSession_Throws()
        {
            WeaponDefinition definition =
                CreateWeapon(
                    "weapon.reward");

            CreateControllerWithHandler(
                definition,
                RewardClaimResult.Claimed);

            Assert.That(
                () =>
                    _claimController.TryClaim(
                        null,
                        definition),
                Throws.ArgumentNullException);
        }

        [Test]
        public void TryClaim_WithNullDefinition_Throws()
        {
            RewardClaimHandlerRegistry registry =
                new RewardClaimHandlerRegistry();

            _claimController.Initialize(
                registry);

            WeaponDefinition definition =
                CreateWeapon(
                    "weapon.reward");

            RewardClaimSession session =
                CreateSession(
                    definition);

            Assert.That(
                () =>
                    _claimController.TryClaim(
                        session,
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void TryClaim_WhenHandlerReturnsAlreadyClaimed_ThrowsWithoutConsumingSession()
        {
            WeaponDefinition definition =
                CreateWeapon(
                    "weapon.reward");

            CreateControllerWithHandler(
                definition,
                RewardClaimResult.AlreadyClaimed);

            RewardClaimSession session =
                CreateSession(
                    definition);

            Assert.That(
                () =>
                    _claimController.TryClaim(
                        session,
                        definition),
                Throws.InvalidOperationException);

            Assert.That(
                session.IsClaimed,
                Is.False);
        }

        private TestRewardClaimHandler
            CreateControllerWithHandler(
                ItemDefinition supportedDefinition,
                RewardClaimResult result)
        {
            TestRewardClaimHandler handler =
                new TestRewardClaimHandler(
                    supportedDefinition,
                    result);

            RewardClaimHandlerRegistry registry =
                new RewardClaimHandlerRegistry();

            registry.Register(
                handler);

            _claimController.Initialize(
                registry);

            return handler;
        }

        private static RewardClaimSession CreateSession(
            params ItemDefinition[] definitions)
        {
            RewardOffer offer =
                new RewardOffer(
                    definitions);

            return new RewardClaimSession(
                offer);
        }

        private WeaponDefinition CreateWeapon(
            string stableId)
        {
            WeaponDefinition definition =
                ScriptableObject.CreateInstance<
                    WeaponDefinition>();

            SetStableId(
                definition,
                stableId);

            _createdDefinitions.Add(
                definition);

            return definition;
        }

        private static void SetStableId(
            ItemDefinition definition,
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
                definition,
                stableId);
        }

        private sealed class TestRewardClaimHandler :
            IRewardClaimHandler
        {
            private readonly ItemDefinition
                _supportedDefinition;

            private readonly RewardClaimResult
                _result;

            public int TryClaimCallCount
            {
                get;
                private set;
            }

            public TestRewardClaimHandler(
                ItemDefinition supportedDefinition,
                RewardClaimResult result)
            {
                _supportedDefinition =
                    supportedDefinition;

                _result =
                    result;
            }

            public bool Supports(
                ItemDefinition definition)
            {
                return ReferenceEquals(
                    definition,
                    _supportedDefinition);
            }

            public RewardClaimResult TryClaim(
                ItemDefinition definition)
            {
                TryClaimCallCount++;

                return _result;
            }
        }
    }
}