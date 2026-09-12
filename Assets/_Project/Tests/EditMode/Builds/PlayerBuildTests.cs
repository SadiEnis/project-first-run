using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;

namespace ProjectFirstRun.Tests.EditMode.Builds
{
    public sealed class PlayerBuildTests
    {
        private PlayerBuild _build;

        [SetUp]
        public void SetUp()
        {
            _build =
                new PlayerBuild(
                    PlayerBuildCapacity.CreateDefault());
        }

        [Test]
        public void Constructor_WithDefaultCapacity_CreatesEmptyBuild()
        {
            Assert.That(
                _build.Weapons.Count,
                Is.EqualTo(0));

            Assert.That(
                _build.Abilities.Count,
                Is.EqualTo(0));

            Assert.That(
                _build.Upgrades.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void Constructor_WithNullCapacity_Throws()
        {
            Assert.That(
                () => new PlayerBuild(null),
                Throws.ArgumentNullException);
        }

        [TestCase(
            ItemCategory.Weapon,
            "weapon.shotgun")]
        [TestCase(
            ItemCategory.Ability,
            "ability.fireball")]
        [TestCase(
            ItemCategory.Upgrade,
            "upgrade.glass_cannon")]
        public void TryAdd_WithAvailableSlot_AddsItem(
            ItemCategory slotType,
            string stableId)
        {
            PlayerBuildAddResult result =
                _build.TryAdd(
                    slotType,
                    stableId);

            Assert.That(
                result,
                Is.EqualTo(PlayerBuildAddResult.Added));

            Assert.That(
                _build.Contains(
                    slotType,
                    stableId),
                Is.True);

            Assert.That(
                _build.GetCount(slotType),
                Is.EqualTo(1));
        }

        [Test]
        public void TryAdd_WhenItemAlreadyOwned_ReturnsAlreadyOwned()
        {
            _build.TryAdd(
                ItemCategory.Ability,
                "ability.fireball");

            PlayerBuildAddResult result =
                _build.TryAdd(
                    ItemCategory.Ability,
                    "ability.fireball");

            Assert.That(
                result,
                Is.EqualTo(
                    PlayerBuildAddResult.AlreadyOwned));

            Assert.That(
                _build.GetCount(
                    ItemCategory.Ability),
                Is.EqualTo(1));
        }

        [Test]
        public void TryAdd_WhenWeaponCapacityReached_ReturnsCapacityReached()
        {
            _build.TryAdd(
                ItemCategory.Weapon,
                "weapon.shotgun");

            PlayerBuildAddResult result =
                _build.TryAdd(
                    ItemCategory.Weapon,
                    "weapon.minigun");

            Assert.That(
                result,
                Is.EqualTo(
                    PlayerBuildAddResult.CapacityReached));

            Assert.That(
                _build.GetCount(
                    ItemCategory.Weapon),
                Is.EqualTo(1));

            Assert.That(
                _build.Contains(
                    ItemCategory.Weapon,
                    "weapon.minigun"),
                Is.False);
        }

        [Test]
        public void TryAdd_WhenAbilityCapacityReached_ReturnsCapacityReached()
        {
            _build.TryAdd(
                ItemCategory.Ability,
                "ability.fireball");

            _build.TryAdd(
                ItemCategory.Ability,
                "ability.drone");

            _build.TryAdd(
                ItemCategory.Ability,
                "ability.lightning");

            PlayerBuildAddResult result =
                _build.TryAdd(
                    ItemCategory.Ability,
                    "ability.shuriken");

            Assert.That(
                result,
                Is.EqualTo(
                    PlayerBuildAddResult.CapacityReached));

            Assert.That(
                _build.GetCount(
                    ItemCategory.Ability),
                Is.EqualTo(3));
        }

        [Test]
        public void TryAdd_WhenUpgradeCapacityReached_ReturnsCapacityReached()
        {
            for (int index = 0;
                 index < 5;
                 index++)
            {
                PlayerBuildAddResult added =
                    _build.TryAdd(
                        ItemCategory.Upgrade,
                        $"upgrade.test_{index}");

                Assert.That(
                    added,
                    Is.EqualTo(PlayerBuildAddResult.Added));
            }

            PlayerBuildAddResult result =
                _build.TryAdd(
                    ItemCategory.Upgrade,
                    "upgrade.extra");

            Assert.That(
                result,
                Is.EqualTo(
                    PlayerBuildAddResult.CapacityReached));

            Assert.That(
                _build.GetCount(
                    ItemCategory.Upgrade),
                Is.EqualTo(5));
        }

        [Test]
        public void HasFreeSlot_WhenCategoryHasSpace_ReturnsTrue()
        {
            Assert.That(
                _build.HasFreeSlot(
                    ItemCategory.Weapon),
                Is.True);
        }

        [Test]
        public void IsFull_WhenCategoryReachesCapacity_ReturnsTrue()
        {
            _build.TryAdd(
                ItemCategory.Weapon,
                "weapon.shotgun");

            Assert.That(
                _build.IsFull(
                    ItemCategory.Weapon),
                Is.True);

            Assert.That(
                _build.HasFreeSlot(
                    ItemCategory.Weapon),
                Is.False);
        }

        [Test]
        public void GetCapacity_UsesConfiguredCapacity()
        {
            PlayerBuild build =
                new PlayerBuild(
                    new PlayerBuildCapacity(
                        2,
                        4,
                        7));

            Assert.That(
                build.GetCapacity(
                    ItemCategory.Weapon),
                Is.EqualTo(2));

            Assert.That(
                build.GetCapacity(
                    ItemCategory.Ability),
                Is.EqualTo(4));

            Assert.That(
                build.GetCapacity(
                    ItemCategory.Upgrade),
                Is.EqualTo(7));
        }

        [Test]
        public void GetItems_ReturnsItemsInAcquisitionOrder()
        {
            _build.TryAdd(
                ItemCategory.Ability,
                "ability.fireball");

            _build.TryAdd(
                ItemCategory.Ability,
                "ability.drone");

            IReadOnlyList<string> items =
                _build.GetItems(
                    ItemCategory.Ability);

            Assert.That(
                items,
                Is.EqualTo(
                    new[]
                    {
                        "ability.fireball",
                        "ability.drone"
                    }));
        }

        [Test]
        public void ExposedCollection_CannotBeMutatedExternally()
        {
            _build.TryAdd(
                ItemCategory.Ability,
                "ability.fireball");

            IReadOnlyList<string> items =
                _build.Abilities;

            Assert.That(
                items,
                Is.Not.InstanceOf<List<string>>());

            ICollection<string> collection =
                items as ICollection<string>;

            Assert.That(
                collection,
                Is.Not.Null);

            Assert.That(
                collection.IsReadOnly,
                Is.True);

            Assert.That(
                () => collection.Add("ability.hacked"),
                Throws.TypeOf<System.NotSupportedException>());

            Assert.That(
                _build.GetCount(
                    ItemCategory.Ability),
                Is.EqualTo(1));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void TryAdd_WithMissingStableId_Throws(
            string stableId)
        {
            Assert.That(
                () =>
                    _build.TryAdd(
                        ItemCategory.Ability,
                        stableId),
                Throws.ArgumentException);
        }

        [TestCase(" ability.fireball")]
        [TestCase("ability.fireball ")]
        public void TryAdd_WithUntrimmedStableId_Throws(
            string stableId)
        {
            Assert.That(
                () =>
                    _build.TryAdd(
                        ItemCategory.Ability,
                        stableId),
                Throws.ArgumentException);
        }

        [Test]
        public void Contains_IsCaseSensitive()
        {
            _build.TryAdd(
                ItemCategory.Ability,
                "ability.fireball");

            Assert.That(
                _build.Contains(
                    ItemCategory.Ability,
                    "ability.fireball"),
                Is.True);

            Assert.That(
                _build.Contains(
                    ItemCategory.Ability,
                    "Ability.Fireball"),
                Is.False);
        }

        [Test]
        public void SameStableId_InDifferentCategories_IsIndependent()
        {
            PlayerBuildAddResult weaponResult =
                _build.TryAdd(
                    ItemCategory.Weapon,
                    "shared.test");

            PlayerBuildAddResult abilityResult =
                _build.TryAdd(
                    ItemCategory.Ability,
                    "shared.test");

            Assert.That(
                weaponResult,
                Is.EqualTo(PlayerBuildAddResult.Added));

            Assert.That(
                abilityResult,
                Is.EqualTo(PlayerBuildAddResult.Added));

            Assert.That(
                _build.GetCount(
                    ItemCategory.Weapon),
                Is.EqualTo(1));

            Assert.That(
                _build.GetCount(
                    ItemCategory.Ability),
                Is.EqualTo(1));
        }

        [Test]
        public void UnsupportedSlotType_Throws()
        {
            ItemCategory invalidType =
                (ItemCategory)999;

            Assert.That(
                () =>
                    _build.GetItems(invalidType),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());

            Assert.That(
                () =>
                    _build.TryAdd(
                        invalidType,
                        "item.test"),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }
    }
}