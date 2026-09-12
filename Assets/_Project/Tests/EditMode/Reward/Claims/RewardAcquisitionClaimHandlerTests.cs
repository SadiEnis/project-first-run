using System;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Items;
using ProjectFirstRun.Rewards.Claims;
using ProjectFirstRun.Upgrades;
using ProjectFirstRun.Weapons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Rewards.Claims
{
    public sealed class RewardAcquisitionClaimHandlerTests
    {
        private WeaponDefinition _weapon;
        private TestAbilityDefinition _ability;
        private UpgradeDefinition _upgrade;

        [SetUp]
        public void SetUp()
        {
            _weapon =
                ScriptableObject
                    .CreateInstance<WeaponDefinition>();

            _ability =
                ScriptableObject
                    .CreateInstance<TestAbilityDefinition>();

            _upgrade =
                ScriptableObject
                    .CreateInstance<UpgradeDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_weapon != null)
            {
                Object.DestroyImmediate(
                    _weapon);
            }

            if (_ability != null)
            {
                Object.DestroyImmediate(
                    _ability);
            }

            if (_upgrade != null)
            {
                Object.DestroyImmediate(
                    _upgrade);
            }
        }

        [Test]
        public void WeaponHandler_SupportsOnlyWeaponDefinition()
        {
            WeaponRewardClaimHandler handler =
                new WeaponRewardClaimHandler(
                    _ =>
                        WeaponAcquireResult.Acquired);

            Assert.That(
                handler.Supports(_weapon),
                Is.True);

            Assert.That(
                handler.Supports(_ability),
                Is.False);

            Assert.That(
                handler.Supports(_upgrade),
                Is.False);

            Assert.That(
                handler.Supports(null),
                Is.False);
        }

        [TestCase(
            WeaponAcquireResult.Acquired,
            RewardClaimResult.Claimed)]
        [TestCase(
            WeaponAcquireResult.AlreadyOwned,
            RewardClaimResult.AlreadyOwned)]
        [TestCase(
            WeaponAcquireResult.CapacityReached,
            RewardClaimResult.CapacityReached)]
        public void WeaponHandler_MapsAcquisitionResult(
            WeaponAcquireResult acquisitionResult,
            RewardClaimResult expectedResult)
        {
            WeaponRewardClaimHandler handler =
                new WeaponRewardClaimHandler(
                    _ => acquisitionResult);

            RewardClaimResult result =
                handler.TryClaim(
                    _weapon);

            Assert.That(
                result,
                Is.EqualTo(expectedResult));
        }

        [Test]
        public void WeaponHandler_WithUnsupportedDefinition_Throws()
        {
            WeaponRewardClaimHandler handler =
                new WeaponRewardClaimHandler(
                    _ =>
                        WeaponAcquireResult.Acquired);

            Assert.That(
                () =>
                    handler.TryClaim(
                        _ability),
                Throws.ArgumentException);
        }

        [Test]
        public void WeaponHandler_WithNullDefinition_Throws()
        {
            WeaponRewardClaimHandler handler =
                new WeaponRewardClaimHandler(
                    _ =>
                        WeaponAcquireResult.Acquired);

            Assert.That(
                () =>
                    handler.TryClaim(
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void WeaponHandler_WithNullAcquisition_Throws()
        {
            Assert.That(
                () =>
                    new WeaponRewardClaimHandler(
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void AbilityHandler_SupportsOnlyAbilityDefinition()
        {
            AbilityRewardClaimHandler handler =
                new AbilityRewardClaimHandler(
                    _ =>
                        AbilityAcquireResult.Acquired);

            Assert.That(
                handler.Supports(_ability),
                Is.True);

            Assert.That(
                handler.Supports(_weapon),
                Is.False);

            Assert.That(
                handler.Supports(_upgrade),
                Is.False);

            Assert.That(
                handler.Supports(null),
                Is.False);
        }

        [TestCase(
            AbilityAcquireResult.Acquired,
            RewardClaimResult.Claimed)]
        [TestCase(
            AbilityAcquireResult.AlreadyOwned,
            RewardClaimResult.AlreadyOwned)]
        [TestCase(
            AbilityAcquireResult.CapacityReached,
            RewardClaimResult.CapacityReached)]
        public void AbilityHandler_MapsAcquisitionResult(
            AbilityAcquireResult acquisitionResult,
            RewardClaimResult expectedResult)
        {
            AbilityRewardClaimHandler handler =
                new AbilityRewardClaimHandler(
                    _ => acquisitionResult);

            RewardClaimResult result =
                handler.TryClaim(
                    _ability);

            Assert.That(
                result,
                Is.EqualTo(expectedResult));
        }

        [Test]
        public void AbilityHandler_WithUnsupportedDefinition_Throws()
        {
            AbilityRewardClaimHandler handler =
                new AbilityRewardClaimHandler(
                    _ =>
                        AbilityAcquireResult.Acquired);

            Assert.That(
                () =>
                    handler.TryClaim(
                        _weapon),
                Throws.ArgumentException);
        }

        [Test]
        public void AbilityHandler_WithNullDefinition_Throws()
        {
            AbilityRewardClaimHandler handler =
                new AbilityRewardClaimHandler(
                    _ =>
                        AbilityAcquireResult.Acquired);

            Assert.That(
                () =>
                    handler.TryClaim(
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void AbilityHandler_WithNullAcquisition_Throws()
        {
            Assert.That(
                () =>
                    new AbilityRewardClaimHandler(
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void UpgradeHandler_SupportsOnlyUpgradeDefinition()
        {
            UpgradeRewardClaimHandler handler =
                new UpgradeRewardClaimHandler(
                    _ =>
                        UpgradeAcquireResult.Acquired);

            Assert.That(
                handler.Supports(_upgrade),
                Is.True);

            Assert.That(
                handler.Supports(_weapon),
                Is.False);

            Assert.That(
                handler.Supports(_ability),
                Is.False);

            Assert.That(
                handler.Supports(null),
                Is.False);
        }

        [TestCase(
            UpgradeAcquireResult.Acquired,
            RewardClaimResult.Claimed)]
        [TestCase(
            UpgradeAcquireResult.AlreadyOwned,
            RewardClaimResult.AlreadyOwned)]
        [TestCase(
            UpgradeAcquireResult.CapacityReached,
            RewardClaimResult.CapacityReached)]
        public void UpgradeHandler_MapsAcquisitionResult(
            UpgradeAcquireResult acquisitionResult,
            RewardClaimResult expectedResult)
        {
            UpgradeRewardClaimHandler handler =
                new UpgradeRewardClaimHandler(
                    _ => acquisitionResult);

            RewardClaimResult result =
                handler.TryClaim(
                    _upgrade);

            Assert.That(
                result,
                Is.EqualTo(expectedResult));
        }

        [Test]
        public void UpgradeHandler_WithUnsupportedDefinition_Throws()
        {
            UpgradeRewardClaimHandler handler =
                new UpgradeRewardClaimHandler(
                    _ =>
                        UpgradeAcquireResult.Acquired);

            Assert.That(
                () =>
                    handler.TryClaim(
                        _weapon),
                Throws.ArgumentException);
        }

        [Test]
        public void UpgradeHandler_WithNullDefinition_Throws()
        {
            UpgradeRewardClaimHandler handler =
                new UpgradeRewardClaimHandler(
                    _ =>
                        UpgradeAcquireResult.Acquired);

            Assert.That(
                () =>
                    handler.TryClaim(
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void UpgradeHandler_WithNullAcquisition_Throws()
        {
            Assert.That(
                () =>
                    new UpgradeRewardClaimHandler(
                        null),
                Throws.ArgumentNullException);
        }

        private sealed class TestAbilityDefinition :
            AbilityDefinition
        {
        }
    }
}