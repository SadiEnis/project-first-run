using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;

namespace ProjectFirstRun.Tests.EditMode.Builds
{
    public sealed class PlayerBuildCapacityTests
    {
        [Test]
        public void CreateDefault_UsesDefaultCapacities()
        {
            PlayerBuildCapacity capacity =
                PlayerBuildCapacity.CreateDefault();

            Assert.That(
                capacity.WeaponSlots,
                Is.EqualTo(1));

            Assert.That(
                capacity.AbilitySlots,
                Is.EqualTo(3));

            Assert.That(
                capacity.UpgradeSlots,
                Is.EqualTo(5));
        }

        [Test]
        public void Constructor_WithCustomValidValues_StoresValues()
        {
            PlayerBuildCapacity capacity =
                new PlayerBuildCapacity(
                    2,
                    4,
                    7);

            Assert.That(
                capacity.WeaponSlots,
                Is.EqualTo(2));

            Assert.That(
                capacity.AbilitySlots,
                Is.EqualTo(4));

            Assert.That(
                capacity.UpgradeSlots,
                Is.EqualTo(7));
        }

        [Test]
        public void Constructor_WithMaximumValues_AllowsValues()
        {
            Assert.DoesNotThrow(
                () =>
                    new PlayerBuildCapacity(
                        PlayerBuildCapacity.MaximumWeaponSlots,
                        PlayerBuildCapacity.MaximumAbilitySlots,
                        PlayerBuildCapacity.MaximumUpgradeSlots));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_WithInvalidWeaponCapacity_Throws(
            int capacity)
        {
            Assert.That(
                () =>
                    new PlayerBuildCapacity(
                        capacity,
                        3,
                        5),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_WithWeaponCapacityAboveMaximum_Throws()
        {
            Assert.That(
                () =>
                    new PlayerBuildCapacity(
                        PlayerBuildCapacity.MaximumWeaponSlots + 1,
                        3,
                        5),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_WithInvalidAbilityCapacity_Throws(
            int capacity)
        {
            Assert.That(
                () =>
                    new PlayerBuildCapacity(
                        1,
                        capacity,
                        5),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_WithAbilityCapacityAboveMaximum_Throws()
        {
            Assert.That(
                () =>
                    new PlayerBuildCapacity(
                        1,
                        PlayerBuildCapacity.MaximumAbilitySlots + 1,
                        5),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_WithInvalidUpgradeCapacity_Throws(
            int capacity)
        {
            Assert.That(
                () =>
                    new PlayerBuildCapacity(
                        1,
                        3,
                        capacity),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_WithUpgradeCapacityAboveMaximum_Throws()
        {
            Assert.That(
                () =>
                    new PlayerBuildCapacity(
                        1,
                        3,
                        PlayerBuildCapacity.MaximumUpgradeSlots + 1),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void GetCapacity_ReturnsCorrectCategoryCapacity()
        {
            PlayerBuildCapacity capacity =
                new PlayerBuildCapacity(
                    2,
                    4,
                    7);

            Assert.That(
                capacity.GetCapacity(
                    ItemCategory.Weapon),
                Is.EqualTo(2));

            Assert.That(
                capacity.GetCapacity(
                    ItemCategory.Ability),
                Is.EqualTo(4));

            Assert.That(
                capacity.GetCapacity(
                    ItemCategory.Upgrade),
                Is.EqualTo(7));
        }

        [Test]
        public void GetCapacity_WithUnsupportedType_Throws()
        {
            PlayerBuildCapacity capacity =
                PlayerBuildCapacity.CreateDefault();

            ItemCategory invalidType =
                (ItemCategory)999;

            Assert.That(
                () => capacity.GetCapacity(invalidType),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }
    }
}