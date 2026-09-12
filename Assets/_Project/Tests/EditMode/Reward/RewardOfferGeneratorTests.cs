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
    public sealed class RewardOfferGeneratorTests
    {
        private readonly List<ItemDefinition>
            _createdDefinitions =
                new List<ItemDefinition>();

        private RewardItemPool _pool;
        private PlayerBuild _build;
        private RewardCandidateFilter _filter;

        [SetUp]
        public void SetUp()
        {
            _pool =
                ScriptableObject
                    .CreateInstance<RewardItemPool>();

            _build =
                new PlayerBuild(
                    PlayerBuildCapacity.CreateDefault());

            _filter =
                new RewardCandidateFilter();
        }

        [TearDown]
        public void TearDown()
        {
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
        public void Constructor_WithNullFilter_Throws()
        {
            Assert.That(
                () =>
                    new RewardOfferGenerator(
                        null,
                        new SequenceRandomSource(0)),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNullRandomSource_Throws()
        {
            Assert.That(
                () =>
                    new RewardOfferGenerator(
                        _filter,
                        null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Generate_WithNullPool_Throws()
        {
            RewardOfferGenerator generator =
                CreateGenerator(0);

            Assert.That(
                () =>
                    generator.Generate(
                        null,
                        _build,
                        1),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Generate_WithNullBuild_Throws()
        {
            RewardOfferGenerator generator =
                CreateGenerator(0);

            Assert.That(
                () =>
                    generator.Generate(
                        _pool,
                        null,
                        1),
                Throws.ArgumentNullException);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Generate_WithInvalidChoiceCount_Throws(
            int choiceCount)
        {
            RewardOfferGenerator generator =
                CreateGenerator(0);

            Assert.That(
                () =>
                    generator.Generate(
                        _pool,
                        _build,
                        choiceCount),
                Throws.TypeOf<
                    System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Generate_WithNoEligibleCandidates_ReturnsEmptyOffer()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.test");

            SetPoolItems(
                weapon);

            _build.TryAdd(
                ItemCategory.Weapon,
                "weapon.owned");

            RewardOfferGenerator generator =
                CreateGenerator(0);

            RewardOffer offer =
                generator.Generate(
                    _pool,
                    _build,
                    3);

            Assert.That(
                offer.IsEmpty,
                Is.True);

            Assert.That(
                offer.ChoiceCount,
                Is.EqualTo(0));
        }

        [Test]
        public void Generate_SelectsRequestedNumberOfChoices()
        {
            UpgradeDefinition first =
                CreateUpgrade(
                    "upgrade.first");

            UpgradeDefinition second =
                CreateUpgrade(
                    "upgrade.second");

            UpgradeDefinition third =
                CreateUpgrade(
                    "upgrade.third");

            SetPoolItems(
                first,
                second,
                third);

            RewardOfferGenerator generator =
                CreateGenerator(
                    0,
                    0);

            RewardOffer offer =
                generator.Generate(
                    _pool,
                    _build,
                    2);

            Assert.That(
                offer.ChoiceCount,
                Is.EqualTo(2));
        }

        [Test]
        public void Generate_UsesRandomSourceForSelectionOrder()
        {
            UpgradeDefinition first =
                CreateUpgrade(
                    "upgrade.first");

            UpgradeDefinition second =
                CreateUpgrade(
                    "upgrade.second");

            UpgradeDefinition third =
                CreateUpgrade(
                    "upgrade.third");

            SetPoolItems(
                first,
                second,
                third);

            /*
             * First:
             * [first, second, third]
             * index 2 → third
             *
             * Remaining:
             * [first, second]
             * index 0 → first
             */
            RewardOfferGenerator generator =
                CreateGenerator(
                    2,
                    0);

            RewardOffer offer =
                generator.Generate(
                    _pool,
                    _build,
                    2);

            Assert.That(
                offer.Choices[0],
                Is.SameAs(third));

            Assert.That(
                offer.Choices[1],
                Is.SameAs(first));
        }

        [Test]
        public void Generate_SelectsWithoutReplacement()
        {
            UpgradeDefinition first =
                CreateUpgrade(
                    "upgrade.first");

            UpgradeDefinition second =
                CreateUpgrade(
                    "upgrade.second");

            UpgradeDefinition third =
                CreateUpgrade(
                    "upgrade.third");

            SetPoolItems(
                first,
                second,
                third);

            RewardOfferGenerator generator =
                CreateGenerator(
                    0,
                    0,
                    0);

            RewardOffer offer =
                generator.Generate(
                    _pool,
                    _build,
                    3);

            Assert.That(
                offer.Choices[0],
                Is.SameAs(first));

            Assert.That(
                offer.Choices[1],
                Is.SameAs(second));

            Assert.That(
                offer.Choices[2],
                Is.SameAs(third));
        }

        [Test]
        public void Generate_WhenEligibleCountIsBelowRequested_ReturnsAvailableItems()
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

            RewardOfferGenerator generator =
                CreateGenerator(
                    0,
                    0);

            RewardOffer offer =
                generator.Generate(
                    _pool,
                    _build,
                    5);

            Assert.That(
                offer.ChoiceCount,
                Is.EqualTo(2));
        }

        [Test]
        public void Generate_FiltersAlreadyOwnedItemsBeforeSelection()
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

            _build.TryAdd(
                ItemCategory.Upgrade,
                owned.StableId);

            RewardOfferGenerator generator =
                CreateGenerator(0);

            RewardOffer offer =
                generator.Generate(
                    _pool,
                    _build,
                    2);

            Assert.That(
                offer.ChoiceCount,
                Is.EqualTo(1));

            Assert.That(
                offer.Choices[0],
                Is.SameAs(available));
        }

        [Test]
        public void Generate_FiltersItemsFromFullCategory()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.candidate");

            UpgradeDefinition upgrade =
                CreateUpgrade(
                    "upgrade.available");

            SetPoolItems(
                weapon,
                upgrade);

            _build.TryAdd(
                ItemCategory.Weapon,
                "weapon.owned");

            RewardOfferGenerator generator =
                CreateGenerator(0);

            RewardOffer offer =
                generator.Generate(
                    _pool,
                    _build,
                    2);

            Assert.That(
                offer.ChoiceCount,
                Is.EqualTo(1));

            Assert.That(
                offer.Choices[0],
                Is.SameAs(upgrade));
        }

        [Test]
        public void Generate_WithInvalidPoolContent_ThrowsBeforeSelection()
        {
            SetPoolItems(
                (ItemDefinition)null);

            SequenceRandomSource randomSource =
                new SequenceRandomSource(0);

            RewardOfferGenerator generator =
                new RewardOfferGenerator(
                    _filter,
                    randomSource);

            Assert.That(
                () =>
                    generator.Generate(
                        _pool,
                        _build,
                        1),
                Throws.InvalidOperationException);

            Assert.That(
                randomSource.CallCount,
                Is.EqualTo(0));
        }

        [Test]
        public void Generate_WhenRandomSourceReturnsOutOfRange_Throws()
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

            /*
             * Valid indexes are 0 and 1.
             * Returning 2 violates IRandomSource contract.
             */
            RewardOfferGenerator generator =
                CreateGenerator(2);

            Assert.That(
                () =>
                    generator.Generate(
                        _pool,
                        _build,
                        1),
                Throws.InvalidOperationException);
        }

        private RewardOfferGenerator CreateGenerator(
            params int[] values)
        {
            return new RewardOfferGenerator(
                _filter,
                new SequenceRandomSource(
                    values));
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

        private void SetPoolItems(
            params ItemDefinition[] items)
        {
            FieldInfo field =
                typeof(RewardItemPool)
                    .GetField(
                        "_items",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null);

            field.SetValue(
                _pool,
                new List<ItemDefinition>(
                    items));
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

        private sealed class SequenceRandomSource :
            IRandomSource
        {
            private readonly int[] _values;
            private int _nextIndex;

            public int CallCount
            {
                get;
                private set;
            }

            public SequenceRandomSource(
                params int[] values)
            {
                _values =
                    values ??
                    System.Array.Empty<int>();
            }

            public int Next(
                int minInclusive,
                int maxExclusive)
            {
                CallCount++;

                if (_nextIndex >=
                    _values.Length)
                {
                    throw new System.InvalidOperationException(
                        "No deterministic random value remains.");
                }

                int result =
                    _values[_nextIndex];

                _nextIndex++;

                return result;
            }
        }
    }
}