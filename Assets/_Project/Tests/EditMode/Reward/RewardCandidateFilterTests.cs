using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.Upgrades;
using ProjectFirstRun.Weapons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Rewards
{
    public sealed class RewardCandidateFilterTests
    {
        private readonly List<ItemDefinition>
            _createdDefinitions =
                new List<ItemDefinition>();

        private PlayerBuild _build;
        private RewardCandidateFilter _filter;

        [SetUp]
        public void SetUp()
        {
            _build =
                new PlayerBuild(
                    PlayerBuildCapacity.CreateDefault());

            _filter =
                new RewardCandidateFilter();
        }

        [TearDown]
        public void TearDown()
        {
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
        public void IsEligible_WithUnownedItemAndFreeSlot_ReturnsTrue()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.test");

            bool result =
                _filter.IsEligible(
                    weapon,
                    _build);

            Assert.That(
                result,
                Is.True);
        }

        [Test]
        public void IsEligible_WhenAlreadyOwned_ReturnsFalse()
        {
            UpgradeDefinition upgrade =
                CreateUpgrade(
                    "upgrade.test");

            _build.TryAdd(
                ItemCategory.Upgrade,
                upgrade.StableId);

            bool result =
                _filter.IsEligible(
                    upgrade,
                    _build);

            Assert.That(
                result,
                Is.False);
        }

        [Test]
        public void IsEligible_WhenCategoryIsFull_ReturnsFalse()
        {
            WeaponDefinition candidate =
                CreateWeapon(
                    "weapon.candidate");

            _build.TryAdd(
                ItemCategory.Weapon,
                "weapon.owned");

            bool result =
                _filter.IsEligible(
                    candidate,
                    _build);

            Assert.That(
                result,
                Is.False);
        }

        [Test]
        public void FullWeaponCategory_DoesNotBlockUpgrade()
        {
            _build.TryAdd(
                ItemCategory.Weapon,
                "weapon.owned");

            UpgradeDefinition upgrade =
                CreateUpgrade(
                    "upgrade.test");

            bool result =
                _filter.IsEligible(
                    upgrade,
                    _build);

            Assert.That(
                result,
                Is.True);
        }

        [Test]
        public void GetEligibleCandidates_FiltersOwnedAndFullCategories()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.candidate");

            UpgradeDefinition ownedUpgrade =
                CreateUpgrade(
                    "upgrade.owned");

            UpgradeDefinition availableUpgrade =
                CreateUpgrade(
                    "upgrade.available");

            _build.TryAdd(
                ItemCategory.Weapon,
                "weapon.already_equipped");

            _build.TryAdd(
                ItemCategory.Upgrade,
                ownedUpgrade.StableId);

            IReadOnlyList<ItemDefinition> result =
                _filter.GetEligibleCandidates(
                    new ItemDefinition[]
                    {
                        weapon,
                        ownedUpgrade,
                        availableUpgrade
                    },
                    _build);

            Assert.That(
                result.Count,
                Is.EqualTo(1));

            Assert.That(
                result[0],
                Is.SameAs(
                    availableUpgrade));
        }

        [Test]
        public void GetEligibleCandidates_PreservesCandidateOrder()
        {
            UpgradeDefinition first =
                CreateUpgrade(
                    "upgrade.first");

            UpgradeDefinition second =
                CreateUpgrade(
                    "upgrade.second");

            IReadOnlyList<ItemDefinition> result =
                _filter.GetEligibleCandidates(
                    new ItemDefinition[]
                    {
                        first,
                        second
                    },
                    _build);

            Assert.That(
                result.Count,
                Is.EqualTo(2));

            Assert.That(
                result[0],
                Is.SameAs(first));

            Assert.That(
                result[1],
                Is.SameAs(second));
        }

        [Test]
        public void GetEligibleCandidates_ReturnsReadOnlyCollection()
        {
            UpgradeDefinition upgrade =
                CreateUpgrade(
                    "upgrade.test");

            IReadOnlyList<ItemDefinition> result =
                _filter.GetEligibleCandidates(
                    new ItemDefinition[]
                    {
                        upgrade
                    },
                    _build);

            ICollection<ItemDefinition> collection =
                result as
                    ICollection<ItemDefinition>;

            Assert.That(
                collection,
                Is.Not.Null);

            Assert.That(
                collection.IsReadOnly,
                Is.True);
        }

        [Test]
        public void GetEligibleCandidates_WhenNoneEligible_ReturnsEmpty()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.test");

            _build.TryAdd(
                ItemCategory.Weapon,
                "weapon.owned");

            IReadOnlyList<ItemDefinition> result =
                _filter.GetEligibleCandidates(
                    new ItemDefinition[]
                    {
                        weapon
                    },
                    _build);

            Assert.That(
                result.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void GetEligibleCandidates_WithNullCandidate_Throws()
        {
            Assert.That(
                () =>
                    _filter.GetEligibleCandidates(
                        new ItemDefinition[]
                        {
                            null
                        },
                        _build),
                Throws.ArgumentException);
        }

        [Test]
        public void GetEligibleCandidates_WithNullCollection_Throws()
        {
            Assert.That(
                () =>
                    _filter.GetEligibleCandidates(
                        null,
                        _build),
                Throws.ArgumentNullException);
        }

        [Test]
        public void GetEligibleCandidates_WithNullBuild_Throws()
        {
            UpgradeDefinition upgrade =
                CreateUpgrade(
                    "upgrade.test");

            Assert.That(
                () =>
                    _filter.GetEligibleCandidates(
                        new ItemDefinition[]
                        {
                            upgrade
                        },
                        null),
                Throws.ArgumentNullException);
        }

        private WeaponDefinition CreateWeapon(
            string stableId)
        {
            WeaponDefinition definition =
                ScriptableObject
                    .CreateInstance<WeaponDefinition>();

            RegisterDefinition(
                definition,
                stableId);

            return definition;
        }

        private UpgradeDefinition CreateUpgrade(
            string stableId)
        {
            UpgradeDefinition definition =
                ScriptableObject
                    .CreateInstance<UpgradeDefinition>();

            RegisterDefinition(
                definition,
                stableId);

            return definition;
        }

        private void RegisterDefinition(
            ItemDefinition definition,
            string stableId)
        {
            SetStableId(
                definition,
                stableId);

            _createdDefinitions.Add(
                definition);
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