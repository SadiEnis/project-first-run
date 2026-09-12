using System;
using NUnit.Framework;
using ProjectFirstRun.Weapons;

namespace ProjectFirstRun.Tests.EditMode.Weapons
{
    public sealed class WeaponRuntimeStateTests
    {
        private const int MagazineCapacity = 12;
        private const int StartingReserveAmmo = 48;
        private const float ShotsPerSecond = 5f;
        private const float FireInterval = 0.2f;
        private const float ReloadDuration = 1.5f;

        private WeaponRuntimeConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = new WeaponRuntimeConfig(
                MagazineCapacity,
                StartingReserveAmmo,
                ShotsPerSecond,
                ReloadDuration);
        }

        [Test]
        public void Constructor_StartsWithConfiguredAmmunition()
        {
            WeaponRuntimeState state =
                new WeaponRuntimeState(in _config);

            Assert.That(state.MagazineCapacity, Is.EqualTo(12));
            Assert.That(state.MagazineAmmo, Is.EqualTo(12));
            Assert.That(state.ReserveAmmo, Is.EqualTo(48));
            Assert.That(state.IsReloading, Is.False);
            Assert.That(state.FireCooldownRemaining, Is.EqualTo(0f));
            Assert.That(state.ReloadTimeRemaining, Is.EqualTo(0f));
        }

        [Test]
        public void TryFire_WhenReady_ConsumesOneRoundAndStartsCooldown()
        {
            WeaponRuntimeState state =
                new WeaponRuntimeState(in _config);

            WeaponFireResult result = state.TryFire();

            Assert.That(result, Is.EqualTo(WeaponFireResult.Fired));
            Assert.That(state.MagazineAmmo, Is.EqualTo(11));
            Assert.That(
                state.FireCooldownRemaining,
                Is.EqualTo(FireInterval).Within(0.0001f));
        }

        [Test]
        public void TryFire_DuringCooldown_IsRejectedWithoutConsumingAmmo()
        {
            WeaponRuntimeState state =
                new WeaponRuntimeState(in _config);

            state.TryFire();
            WeaponFireResult secondResult = state.TryFire();

            Assert.That(
                secondResult,
                Is.EqualTo(WeaponFireResult.BlockedByCooldown));

            Assert.That(state.MagazineAmmo, Is.EqualTo(11));
        }

        [Test]
        public void Tick_WhenCooldownCompletes_AllowsAnotherShot()
        {
            WeaponRuntimeState state =
                new WeaponRuntimeState(in _config);

            state.TryFire();
            state.Tick(FireInterval);

            WeaponFireResult result = state.TryFire();

            Assert.That(result, Is.EqualTo(WeaponFireResult.Fired));
            Assert.That(state.MagazineAmmo, Is.EqualTo(10));
        }

        [Test]
        public void TryFire_WithEmptyMagazine_DoesNotProduceNegativeAmmo()
        {
            WeaponRuntimeConfig config = new WeaponRuntimeConfig(
                magazineCapacity: 2,
                startingReserveAmmo: 0,
                shotsPerSecond: 5f,
                reloadDuration: 1f);

            WeaponRuntimeState state =
                new WeaponRuntimeState(in config);

            Assert.That(state.TryFire(), Is.EqualTo(WeaponFireResult.Fired));

            state.Tick(config.FireInterval);

            Assert.That(state.TryFire(), Is.EqualTo(WeaponFireResult.Fired));

            state.Tick(config.FireInterval);

            WeaponFireResult emptyResult = state.TryFire();

            Assert.That(
                emptyResult,
                Is.EqualTo(WeaponFireResult.EmptyMagazine));

            Assert.That(state.MagazineAmmo, Is.EqualTo(0));
        }

        [Test]
        public void TryStartReload_WithFullMagazine_IsRejected()
        {
            WeaponRuntimeState state =
                new WeaponRuntimeState(in _config);

            WeaponReloadResult result = state.TryStartReload();

            Assert.That(
                result,
                Is.EqualTo(WeaponReloadResult.MagazineFull));

            Assert.That(state.IsReloading, Is.False);
        }

        [Test]
        public void TryStartReload_WithNoReserveAmmo_IsRejected()
        {
            WeaponRuntimeConfig config = new WeaponRuntimeConfig(
                magazineCapacity: 2,
                startingReserveAmmo: 0,
                shotsPerSecond: 5f,
                reloadDuration: 1f);

            WeaponRuntimeState state =
                new WeaponRuntimeState(in config);

            state.TryFire();

            WeaponReloadResult result = state.TryStartReload();

            Assert.That(
                result,
                Is.EqualTo(WeaponReloadResult.NoReserveAmmo));

            Assert.That(state.IsReloading, Is.False);
        }

        [Test]
        public void TryStartReload_WhileAlreadyReloading_IsRejected()
        {
            WeaponRuntimeState state =
                new WeaponRuntimeState(in _config);

            FireRounds(state, 1);

            Assert.That(
                state.TryStartReload(),
                Is.EqualTo(WeaponReloadResult.Started));

            Assert.That(
                state.TryStartReload(),
                Is.EqualTo(WeaponReloadResult.AlreadyReloading));
        }

        [Test]
        public void TryFire_DuringReload_IsRejected()
        {
            WeaponRuntimeState state =
                new WeaponRuntimeState(in _config);

            FireRounds(state, 1);
            state.TryStartReload();

            int ammunitionBeforeFire = state.MagazineAmmo;

            WeaponFireResult result = state.TryFire();

            Assert.That(
                result,
                Is.EqualTo(WeaponFireResult.BlockedByReload));

            Assert.That(
                state.MagazineAmmo,
                Is.EqualTo(ammunitionBeforeFire));
        }

        [Test]
        public void Tick_BeforeReloadDuration_DoesNotCompleteReload()
        {
            WeaponRuntimeState state =
                new WeaponRuntimeState(in _config);

            FireRounds(state, 5);
            state.TryStartReload();

            bool reloadCompleted = state.Tick(1f);

            Assert.That(reloadCompleted, Is.False);
            Assert.That(state.IsReloading, Is.True);
            Assert.That(state.MagazineAmmo, Is.EqualTo(7));
            Assert.That(state.ReserveAmmo, Is.EqualTo(48));
            Assert.That(
                state.ReloadTimeRemaining,
                Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void Tick_WhenReloadCompletes_TransfersRequiredAmmo()
        {
            WeaponRuntimeState state =
                new WeaponRuntimeState(in _config);

            FireRounds(state, 5);
            state.TryStartReload();

            bool reloadCompleted = state.Tick(ReloadDuration);

            Assert.That(reloadCompleted, Is.True);
            Assert.That(state.IsReloading, Is.False);
            Assert.That(state.MagazineAmmo, Is.EqualTo(12));
            Assert.That(state.ReserveAmmo, Is.EqualTo(43));
            Assert.That(state.ReloadTimeRemaining, Is.EqualTo(0f));
        }

        [Test]
        public void Reload_WithInsufficientReserveAmmo_TransfersAvailableAmmo()
        {
            WeaponRuntimeConfig config = new WeaponRuntimeConfig(
                magazineCapacity: 12,
                startingReserveAmmo: 3,
                shotsPerSecond: 5f,
                reloadDuration: 1f);

            WeaponRuntimeState state =
                new WeaponRuntimeState(in config);

            FireRounds(state, 5);
            state.TryStartReload();
            state.Tick(config.ReloadDuration);

            Assert.That(state.MagazineAmmo, Is.EqualTo(10));
            Assert.That(state.ReserveAmmo, Is.EqualTo(0));
            Assert.That(state.IsReloading, Is.False);
        }

        [Test]
        public void Reset_RestoresInitialState()
        {
            WeaponRuntimeState state =
                new WeaponRuntimeState(in _config);

            FireRounds(state, 4);
            state.TryStartReload();
            state.Tick(0.5f);

            state.Reset();

            Assert.That(state.MagazineAmmo, Is.EqualTo(12));
            Assert.That(state.ReserveAmmo, Is.EqualTo(48));
            Assert.That(state.FireCooldownRemaining, Is.EqualTo(0f));
            Assert.That(state.ReloadTimeRemaining, Is.EqualTo(0f));
            Assert.That(state.IsReloading, Is.False);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Tick_WithInvalidDeltaTime_Throws(float deltaTime)
        {
            WeaponRuntimeState state =
                new WeaponRuntimeState(in _config);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => state.Tick(deltaTime));
        }

        private static void FireRounds(
            WeaponRuntimeState state,
            int roundCount)
        {
            for (int index = 0; index < roundCount; index++)
            {
                WeaponFireResult result = state.TryFire();

                Assert.That(
                    result,
                    Is.EqualTo(WeaponFireResult.Fired));

                state.Tick(FireInterval);
            }
        }
    }
}