using System;
using NUnit.Framework;
using ProjectFirstRun.Weapons;

namespace ProjectFirstRun.Tests.EditMode.Weapons
{
    public sealed class FireRhythmTimingTests
    {
        [Test]
        public void RateChangePreservesExistingWaitAndReload()
        {
            var state = new WeaponRuntimeState(new WeaponRuntimeConfig(12, 48, 5, 2));
            Assert.That(state.TryFire(), Is.EqualTo(WeaponFireResult.Fired));
            state.Tick(.1f);
            Assert.That(state.TryFire(7.5f), Is.EqualTo(WeaponFireResult.BlockedByCooldown));
            Assert.That(state.FireCooldownRemaining, Is.EqualTo(.1f).Within(.0001f));
            Assert.That(state.MagazineAmmo, Is.EqualTo(11));
            state.Tick(.1f);
            Assert.That(state.TryFire(7.5f), Is.EqualTo(WeaponFireResult.Fired));
            Assert.That(state.FireCooldownRemaining, Is.EqualTo(1f / 7.5f).Within(.0001f));
            Assert.That(state.TryStartReload(), Is.EqualTo(WeaponReloadResult.Started));
            Assert.That(state.ReloadTimeRemaining, Is.EqualTo(2));
            Assert.That(state.TryFire(10), Is.EqualTo(WeaponFireResult.BlockedByReload));
            state.Tick(2);
            Assert.That(state.MagazineAmmo, Is.EqualTo(12));
            Assert.That(state.ReserveAmmo, Is.EqualTo(46));
            state.TryFire(5);
            Assert.That(state.FireCooldownRemaining, Is.EqualTo(.2f).Within(.0001f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidRateCannotConsumeAmmo(float rate)
        {
            var state = new WeaponRuntimeState(new WeaponRuntimeConfig(12, 48, 5, 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => state.TryFire(rate));
            Assert.That(state.MagazineAmmo, Is.EqualTo(12));
            Assert.That(state.FireCooldownRemaining, Is.Zero);
        }
    }
}
