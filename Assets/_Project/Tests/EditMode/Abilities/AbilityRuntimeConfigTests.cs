using NUnit.Framework;
using ProjectFirstRun.Abilities;

namespace ProjectFirstRun.Tests.EditMode.Abilities
{
    public sealed class AbilityRuntimeConfigTests
    {
        [Test]
        public void Constructor_WithValidCooldown_StoresValue()
        {
            AbilityRuntimeConfig config =
                new AbilityRuntimeConfig(
                    2.5f);

            Assert.That(
                config.Cooldown,
                Is.EqualTo(2.5f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void Constructor_WithNonPositiveCooldown_Throws(
            float cooldown)
        {
            Assert.That(
                () => new AbilityRuntimeConfig(cooldown),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_WithNaNCooldown_Throws()
        {
            Assert.That(
                () =>
                    new AbilityRuntimeConfig(
                        float.NaN),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_WithInfiniteCooldown_Throws()
        {
            Assert.That(
                () =>
                    new AbilityRuntimeConfig(
                        float.PositiveInfinity),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }
    }
}