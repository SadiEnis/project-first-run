using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Items;
using ProjectFirstRun.Rewards.Claims;
using ProjectFirstRun.Weapons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Rewards.Claims
{
    public sealed class RewardClaimHandlerRegistryTests
    {
        private readonly List<WeaponDefinition>
            _createdDefinitions =
                new List<WeaponDefinition>();

        private WeaponDefinition _definition;

        [SetUp]
        public void SetUp()
        {
            _definition =
                CreateWeapon(
                    "weapon.test");
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
        }

        [Test]
        public void NewRegistry_HasNoHandlers()
        {
            RewardClaimHandlerRegistry registry =
                new RewardClaimHandlerRegistry();

            Assert.That(
                registry.HandlerCount,
                Is.EqualTo(0));
        }

        [Test]
        public void Register_WithHandler_AddsHandler()
        {
            RewardClaimHandlerRegistry registry =
                new RewardClaimHandlerRegistry();

            TestRewardClaimHandler handler =
                new TestRewardClaimHandler(
                    _definition,
                    true);

            registry.Register(
                handler);

            Assert.That(
                registry.HandlerCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Register_WithNullHandler_Throws()
        {
            RewardClaimHandlerRegistry registry =
                new RewardClaimHandlerRegistry();

            Assert.That(
                () =>
                    registry.Register(
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Register_SameHandlerInstanceTwice_Throws()
        {
            RewardClaimHandlerRegistry registry =
                new RewardClaimHandlerRegistry();

            TestRewardClaimHandler handler =
                new TestRewardClaimHandler(
                    _definition,
                    true);

            registry.Register(
                handler);

            Assert.That(
                () =>
                    registry.Register(
                        handler),
                Throws.InvalidOperationException);

            Assert.That(
                registry.HandlerCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WithMatchingHandler_ReturnsHandler()
        {
            RewardClaimHandlerRegistry registry =
                new RewardClaimHandlerRegistry();

            TestRewardClaimHandler unsupported =
                new TestRewardClaimHandler(
                    _definition,
                    false);

            TestRewardClaimHandler supported =
                new TestRewardClaimHandler(
                    _definition,
                    true);

            registry.Register(
                unsupported);

            registry.Register(
                supported);

            IRewardClaimHandler result =
                registry.Resolve(
                    _definition);

            Assert.That(
                result,
                Is.SameAs(supported));
        }

        [Test]
        public void Resolve_WithNoRegisteredHandlers_Throws()
        {
            RewardClaimHandlerRegistry registry =
                new RewardClaimHandlerRegistry();

            Assert.That(
                () =>
                    registry.Resolve(
                        _definition),
                Throws.InvalidOperationException);
        }

        [Test]
        public void Resolve_WhenNoHandlerSupportsDefinition_Throws()
        {
            RewardClaimHandlerRegistry registry =
                new RewardClaimHandlerRegistry();

            TestRewardClaimHandler handler =
                new TestRewardClaimHandler(
                    _definition,
                    false);

            registry.Register(
                handler);

            Assert.That(
                () =>
                    registry.Resolve(
                        _definition),
                Throws.InvalidOperationException);
        }

        [Test]
        public void Resolve_WhenMultipleHandlersSupportDefinition_Throws()
        {
            RewardClaimHandlerRegistry registry =
                new RewardClaimHandlerRegistry();

            TestRewardClaimHandler first =
                new TestRewardClaimHandler(
                    _definition,
                    true);

            TestRewardClaimHandler second =
                new TestRewardClaimHandler(
                    _definition,
                    true);

            registry.Register(
                first);

            registry.Register(
                second);

            Assert.That(
                () =>
                    registry.Resolve(
                        _definition),
                Throws.InvalidOperationException);
        }

        [Test]
        public void Resolve_WithNullDefinition_Throws()
        {
            RewardClaimHandlerRegistry registry =
                new RewardClaimHandlerRegistry();

            Assert.That(
                () =>
                    registry.Resolve(
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Resolve_DoesNotCallTryClaim()
        {
            RewardClaimHandlerRegistry registry =
                new RewardClaimHandlerRegistry();

            TestRewardClaimHandler handler =
                new TestRewardClaimHandler(
                    _definition,
                    true);

            registry.Register(
                handler);

            registry.Resolve(
                _definition);

            Assert.That(
                handler.TryClaimCallCount,
                Is.EqualTo(0));
        }

        private WeaponDefinition CreateWeapon(
            string stableId)
        {
            WeaponDefinition definition =
                ScriptableObject
                    .CreateInstance<WeaponDefinition>();

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

            private readonly bool _supports;

            public int TryClaimCallCount
            {
                get;
                private set;
            }

            public TestRewardClaimHandler(
                ItemDefinition supportedDefinition,
                bool supports)
            {
                _supportedDefinition =
                    supportedDefinition;

                _supports =
                    supports;
            }

            public bool Supports(
                ItemDefinition definition)
            {
                return _supports &&
                       ReferenceEquals(
                           definition,
                           _supportedDefinition);
            }

            public RewardClaimResult TryClaim(
                ItemDefinition definition)
            {
                TryClaimCallCount++;

                return RewardClaimResult.Claimed;
            }
        }
    }
}