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
    public sealed class RewardClaimSessionTests
    {
        private readonly List<WeaponDefinition>
            _createdDefinitions =
                new List<WeaponDefinition>();

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
        public void Constructor_WithOffer_CreatesUnclaimedSession()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.test");

            RewardOffer offer =
                CreateOffer(
                    weapon);

            RewardClaimSession session =
                new RewardClaimSession(
                    offer);

            Assert.That(
                session.Offer,
                Is.SameAs(offer));

            Assert.That(
                session.IsClaimed,
                Is.False);

            Assert.That(
                session.ClaimedDefinition,
                Is.Null);

            Assert.That(session.MaxSelections, Is.EqualTo(1));
            Assert.That(session.SelectionsRemaining, Is.EqualTo(1));
            Assert.That(session.ClaimedDefinitions, Is.Empty);
        }

        [Test]
        public void MultiSelectionSession_RemainsOpenUntilAllSelectionsAreCommitted()
        {
            WeaponDefinition first = CreateWeapon("weapon.first");
            WeaponDefinition second = CreateWeapon("weapon.second");
            WeaponDefinition third = CreateWeapon("weapon.third");

            RewardClaimSession session =
                new RewardClaimSession(
                    CreateOffer(first, second, third),
                    2);

            session.Commit(first);

            Assert.That(session.IsClaimed, Is.False);
            Assert.That(session.SelectionsRemaining, Is.EqualTo(1));
            Assert.That(session.ClaimedDefinition, Is.SameAs(first));
            Assert.That(session.ClaimedDefinitions, Is.EqualTo(new[] { first }));

            session.Commit(second);

            Assert.That(session.IsClaimed, Is.True);
            Assert.That(session.SelectionsRemaining, Is.EqualTo(0));
            Assert.That(session.ClaimedDefinitions, Is.EqualTo(new[] { first, second }));
        }

        [Test]
        public void MultiSelectionSession_RejectsDuplicateChoiceWithoutChangingRemainingSelections()
        {
            WeaponDefinition first = CreateWeapon("weapon.first");
            WeaponDefinition second = CreateWeapon("weapon.second");
            RewardClaimSession session =
                new RewardClaimSession(
                    CreateOffer(first, second),
                    2);

            session.Commit(first);

            Assert.That(() => session.Commit(first), Throws.InvalidOperationException);
            Assert.That(session.SelectionsRemaining, Is.EqualTo(1));
            Assert.That(session.ClaimedDefinitions, Is.EqualTo(new[] { first }));
        }

        [TestCase(0)]
        [TestCase(3)]
        public void Constructor_WithInvalidMaximumSelections_Throws(int maxSelections)
        {
            WeaponDefinition first = CreateWeapon("weapon.first");
            WeaponDefinition second = CreateWeapon("weapon.second");

            Assert.That(
                () => new RewardClaimSession(CreateOffer(first, second), maxSelections),
                Throws.Exception);
        }

        [Test]
        public void Constructor_WithNullOffer_Throws()
        {
            Assert.That(
                () =>
                    new RewardClaimSession(
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void ValidateSelection_WithExactOfferedInstance_DoesNotThrow()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.test");

            RewardClaimSession session =
                new RewardClaimSession(
                    CreateOffer(
                        weapon));

            Assert.That(
                () =>
                    session.ValidateSelection(
                        weapon),
                Throws.Nothing);
        }

        [Test]
        public void ValidateSelection_WithNonOfferedDefinition_Throws()
        {
            WeaponDefinition offered =
                CreateWeapon(
                    "weapon.offered");

            WeaponDefinition other =
                CreateWeapon(
                    "weapon.other");

            RewardClaimSession session =
                new RewardClaimSession(
                    CreateOffer(
                        offered));

            Assert.That(
                () =>
                    session.ValidateSelection(
                        other),
                Throws.InvalidOperationException);
        }

        [Test]
        public void ValidateSelection_WithDifferentInstanceHavingSameStableId_Throws()
        {
            WeaponDefinition offered =
                CreateWeapon(
                    "weapon.same");

            WeaponDefinition equivalent =
                CreateWeapon(
                    "weapon.same");

            RewardClaimSession session =
                new RewardClaimSession(
                    CreateOffer(
                        offered));

            Assert.That(
                () =>
                    session.ValidateSelection(
                        equivalent),
                Throws.InvalidOperationException);
        }

        [Test]
        public void ValidateSelection_WithNullDefinition_Throws()
        {
            WeaponDefinition offered =
                CreateWeapon(
                    "weapon.test");

            RewardClaimSession session =
                new RewardClaimSession(
                    CreateOffer(
                        offered));

            Assert.That(
                () =>
                    session.ValidateSelection(
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Commit_WithOfferedDefinition_ClaimsSession()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.test");

            RewardClaimSession session =
                new RewardClaimSession(
                    CreateOffer(
                        weapon));

            session.Commit(
                weapon);

            Assert.That(
                session.IsClaimed,
                Is.True);

            Assert.That(
                session.ClaimedDefinition,
                Is.SameAs(weapon));
        }

        [Test]
        public void Commit_WithNonOfferedDefinition_ThrowsWithoutClaimingSession()
        {
            WeaponDefinition offered =
                CreateWeapon(
                    "weapon.offered");

            WeaponDefinition other =
                CreateWeapon(
                    "weapon.other");

            RewardClaimSession session =
                new RewardClaimSession(
                    CreateOffer(
                        offered));

            Assert.That(
                () =>
                    session.Commit(
                        other),
                Throws.InvalidOperationException);

            Assert.That(
                session.IsClaimed,
                Is.False);

            Assert.That(
                session.ClaimedDefinition,
                Is.Null);
        }

        [Test]
        public void Commit_WhenAlreadyClaimed_ThrowsWithoutChangingClaim()
        {
            WeaponDefinition first =
                CreateWeapon(
                    "weapon.first");

            WeaponDefinition second =
                CreateWeapon(
                    "weapon.second");

            RewardClaimSession session =
                new RewardClaimSession(
                    CreateOffer(
                        first,
                        second));

            session.Commit(
                first);

            Assert.That(
                () =>
                    session.Commit(
                        second),
                Throws.InvalidOperationException);

            Assert.That(
                session.IsClaimed,
                Is.True);

            Assert.That(
                session.ClaimedDefinition,
                Is.SameAs(first));
        }

        [Test]
        public void Commit_WhenSameDefinitionIsCommittedTwice_Throws()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.test");

            RewardClaimSession session =
                new RewardClaimSession(
                    CreateOffer(
                        weapon));

            session.Commit(
                weapon);

            Assert.That(
                () =>
                    session.Commit(
                        weapon),
                Throws.InvalidOperationException);

            Assert.That(
                session.ClaimedDefinition,
                Is.SameAs(weapon));
        }

        private static RewardOffer CreateOffer(
            params ItemDefinition[] choices)
        {
            return new RewardOffer(
                choices);
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
    }
}
