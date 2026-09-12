using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Items;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.Upgrades;
using ProjectFirstRun.Weapons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Rewards
{
    public sealed class RewardOfferTests
    {
        private readonly List<ItemDefinition>
            _createdDefinitions =
                new List<ItemDefinition>();

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
        public void Constructor_WithEmptyChoices_CreatesEmptyOffer()
        {
            RewardOffer offer =
                new RewardOffer(
                    new List<ItemDefinition>());

            Assert.That(
                offer.IsEmpty,
                Is.True);

            Assert.That(
                offer.ChoiceCount,
                Is.EqualTo(0));
        }

        [Test]
        public void Constructor_PreservesChoiceOrder()
        {
            WeaponDefinition first =
                CreateWeapon(
                    "weapon.first");

            WeaponDefinition second =
                CreateWeapon(
                    "weapon.second");

            RewardOffer offer =
                new RewardOffer(
                    new ItemDefinition[]
                    {
                        first,
                        second
                    });

            Assert.That(
                offer.ChoiceCount,
                Is.EqualTo(2));

            Assert.That(
                offer.Choices[0],
                Is.SameAs(first));

            Assert.That(
                offer.Choices[1],
                Is.SameAs(second));
        }

        [Test]
        public void Constructor_CopiesInputCollection()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.test");

            List<ItemDefinition> source =
                new List<ItemDefinition>
                {
                    weapon
                };

            RewardOffer offer =
                new RewardOffer(
                    source);

            source.Clear();

            Assert.That(
                offer.ChoiceCount,
                Is.EqualTo(1));

            Assert.That(
                offer.Choices[0],
                Is.SameAs(weapon));
        }

        [Test]
        public void Choices_CannotBeMutatedExternally()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.test");

            RewardOffer offer =
                new RewardOffer(
                    new ItemDefinition[]
                    {
                        weapon
                    });

            ICollection<ItemDefinition> collection =
                offer.Choices as
                    ICollection<ItemDefinition>;

            Assert.That(
                collection,
                Is.Not.Null);

            Assert.That(
                collection.IsReadOnly,
                Is.True);

            Assert.That(
                () =>
                    collection.Add(
                        CreateWeapon(
                            "weapon.hacked")),
                Throws.TypeOf<
                    System.NotSupportedException>());

            Assert.That(
                offer.ChoiceCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Constructor_WithNullChoices_Throws()
        {
            Assert.That(
                () =>
                    new RewardOffer(null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNullChoice_Throws()
        {
            Assert.That(
                () =>
                    new RewardOffer(
                        new ItemDefinition[]
                        {
                            null
                        }),
                Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WithDuplicateIdentity_Throws()
        {
            WeaponDefinition first =
                CreateWeapon(
                    "weapon.same");

            WeaponDefinition second =
                CreateWeapon(
                    "weapon.same");

            Assert.That(
                () =>
                    new RewardOffer(
                        new ItemDefinition[]
                        {
                            first,
                            second
                        }),
                Throws.ArgumentException);
        }

        [Test]
        public void SameStableId_InDifferentCategories_IsAllowed()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "shared.test");

            UpgradeDefinition upgrade =
                CreateUpgrade(
                    "shared.test");

            RewardOffer offer =
                new RewardOffer(
                    new ItemDefinition[]
                    {
                        weapon,
                        upgrade
                    });

            Assert.That(
                offer.ChoiceCount,
                Is.EqualTo(2));
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