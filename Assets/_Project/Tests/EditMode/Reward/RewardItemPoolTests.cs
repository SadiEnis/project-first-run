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
    public sealed class RewardItemPoolTests
    {
        private readonly List<ItemDefinition>
            _createdDefinitions =
                new List<ItemDefinition>();

        private RewardItemPool _pool;

        [SetUp]
        public void SetUp()
        {
            _pool =
                ScriptableObject
                    .CreateInstance<RewardItemPool>();
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
        public void NewPool_HasNoItems()
        {
            Assert.That(
                _pool.ItemCount,
                Is.EqualTo(0));

            IReadOnlyList<ItemDefinition> items =
                _pool.GetValidatedItems();

            Assert.That(
                items.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void GetValidatedItems_WithValidItems_PreservesOrder()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.test");

            UpgradeDefinition upgrade =
                CreateUpgrade(
                    "upgrade.test");

            SetPoolItems(
                weapon,
                upgrade);

            IReadOnlyList<ItemDefinition> items =
                _pool.GetValidatedItems();

            Assert.That(
                items.Count,
                Is.EqualTo(2));

            Assert.That(
                items[0],
                Is.SameAs(weapon));

            Assert.That(
                items[1],
                Is.SameAs(upgrade));
        }

        [Test]
        public void GetValidatedItems_ReturnsReadOnlyCollection()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.test");

            SetPoolItems(
                weapon);

            IReadOnlyList<ItemDefinition> items =
                _pool.GetValidatedItems();

            ICollection<ItemDefinition> collection =
                items as ICollection<ItemDefinition>;

            Assert.That(
                collection,
                Is.Not.Null);

            Assert.That(
                collection.IsReadOnly,
                Is.True);

            Assert.That(
                () =>
                    collection.Add(
                        CreateUpgrade(
                            "upgrade.hacked")),
                Throws.TypeOf<
                    System.NotSupportedException>());

            Assert.That(
                _pool.ItemCount,
                Is.EqualTo(1));
        }

        [Test]
        public void ReturnedCollection_IsIndependentFromSerializedList()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "weapon.test");

            UpgradeDefinition upgrade =
                CreateUpgrade(
                    "upgrade.test");

            SetPoolItems(
                weapon);

            IReadOnlyList<ItemDefinition> items =
                _pool.GetValidatedItems();

            SetPoolItems(
                weapon,
                upgrade);

            Assert.That(
                items.Count,
                Is.EqualTo(1));

            Assert.That(
                _pool.ItemCount,
                Is.EqualTo(2));
        }

        [Test]
        public void ValidateContents_WithNullEntry_Throws()
        {
            SetPoolItems(
                (ItemDefinition)null);

            Assert.That(
                () =>
                    _pool.ValidateContents(),
                Throws.InvalidOperationException);
        }

        [Test]
        public void ValidateContents_WithMissingStableId_Throws()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "");

            SetPoolItems(
                weapon);

            Assert.That(
                () =>
                    _pool.ValidateContents(),
                Throws.InvalidOperationException);
        }

        [Test]
        public void ValidateContents_WithUntrimmedStableId_Throws()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    " weapon.test ");

            SetPoolItems(
                weapon);

            Assert.That(
                () =>
                    _pool.ValidateContents(),
                Throws.InvalidOperationException);
        }

        [Test]
        public void ValidateContents_WithDuplicateIdentity_Throws()
        {
            WeaponDefinition first =
                CreateWeapon(
                    "weapon.same");

            WeaponDefinition second =
                CreateWeapon(
                    "weapon.same");

            SetPoolItems(
                first,
                second);

            Assert.That(
                () =>
                    _pool.ValidateContents(),
                Throws.InvalidOperationException);
        }

        [Test]
        public void ValidateContents_SameStableIdDifferentCategory_IsAllowed()
        {
            WeaponDefinition weapon =
                CreateWeapon(
                    "shared.test");

            UpgradeDefinition upgrade =
                CreateUpgrade(
                    "shared.test");

            SetPoolItems(
                weapon,
                upgrade);

            Assert.That(
                () =>
                    _pool.ValidateContents(),
                Throws.Nothing);
        }

        [Test]
        public void GetValidatedItems_WithDuplicateIdentity_Throws()
        {
            UpgradeDefinition first =
                CreateUpgrade(
                    "upgrade.same");

            UpgradeDefinition second =
                CreateUpgrade(
                    "upgrade.same");

            SetPoolItems(
                first,
                second);

            Assert.That(
                () =>
                    _pool.GetValidatedItems(),
                Throws.InvalidOperationException);
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
    }
}