using System;
using NUnit.Framework;
using ProjectFirstRun.Weapons;

namespace ProjectFirstRun.Tests.EditMode.Weapons
{
    public sealed class WeaponRuntimeConfigTests
    {
        [Test]
        public void Constructor_WithValidValues_CreatesConfiguration()
        {
            WeaponRuntimeConfig config = new WeaponRuntimeConfig(
                magazineCapacity: 12,
                startingReserveAmmo: 48,
                shotsPerSecond: 5f,
                reloadDuration: 1.5f);

            Assert.That(config.MagazineCapacity, Is.EqualTo(12));
            Assert.That(config.StartingReserveAmmo, Is.EqualTo(48));
            Assert.That(config.ShotsPerSecond, Is.EqualTo(5f));
            Assert.That(config.FireInterval, Is.EqualTo(0.2f).Within(0.0001f));
            Assert.That(config.ReloadDuration, Is.EqualTo(1.5f));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_WithInvalidMagazineCapacity_Throws(
            int magazineCapacity)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new WeaponRuntimeConfig(
                    magazineCapacity,
                    48,
                    5f,
                    1.5f);
            });
        }

        [Test]
        public void Constructor_WithNegativeReserveAmmo_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new WeaponRuntimeConfig(
                    12,
                    -1,
                    5f,
                    1.5f);
            });
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Constructor_WithInvalidShotsPerSecond_Throws(
            float shotsPerSecond)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new WeaponRuntimeConfig(
                    12,
                    48,
                    shotsPerSecond,
                    1.5f);
            });
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Constructor_WithInvalidReloadDuration_Throws(
            float reloadDuration)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new WeaponRuntimeConfig(
                    12,
                    48,
                    5f,
                    reloadDuration);
            });
        }
    }
}