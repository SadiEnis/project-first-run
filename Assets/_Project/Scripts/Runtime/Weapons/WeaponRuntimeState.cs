using System;

namespace ProjectFirstRun.Weapons
{
    public enum WeaponFireResult
    {
        Fired = 0,
        BlockedByReload = 1,
        BlockedByCooldown = 2,
        EmptyMagazine = 3
    }

    public enum WeaponReloadResult
    {
        Started = 0,
        AlreadyReloading = 1,
        MagazineFull = 2,
        NoReserveAmmo = 3
    }

    /// <summary>
    /// Pure C# model containing ammunition, fire-rate, and reload rules.
    /// </summary>
    public sealed class WeaponRuntimeState
    {
        private readonly WeaponRuntimeConfig _config;

        public int MagazineCapacity =>
            _config.MagazineCapacity;

        public int MagazineAmmo { get; private set; }
        public int ReserveAmmo { get; private set; }

        public float FireCooldownRemaining { get; private set; }
        public float ReloadTimeRemaining { get; private set; }

        public bool IsReloading { get; private set; }

        public WeaponRuntimeState(
            in WeaponRuntimeConfig config)
        {
            _config = config;

            Reset();
        }

        /// <summary>
        /// Advances weapon timers.
        /// Returns true only on the frame in which reload completes.
        /// </summary>
        public bool Tick(float deltaTime)
        {
            ValidateDeltaTime(deltaTime);

            FireCooldownRemaining = Math.Max(
                0f,
                FireCooldownRemaining - deltaTime);

            if (!IsReloading)
            {
                return false;
            }

            ReloadTimeRemaining = Math.Max(
                0f,
                ReloadTimeRemaining - deltaTime);

            if (ReloadTimeRemaining > 0f)
            {
                return false;
            }

            CompleteReload();
            return true;
        }

        public WeaponFireResult TryFire()
        {
            if (IsReloading)
            {
                return WeaponFireResult.BlockedByReload;
            }

            if (FireCooldownRemaining > 0f)
            {
                return WeaponFireResult.BlockedByCooldown;
            }

            if (MagazineAmmo <= 0)
            {
                return WeaponFireResult.EmptyMagazine;
            }

            MagazineAmmo--;
            FireCooldownRemaining = _config.FireInterval;

            return WeaponFireResult.Fired;
        }

        public WeaponReloadResult TryStartReload()
        {
            if (IsReloading)
            {
                return WeaponReloadResult.AlreadyReloading;
            }

            if (MagazineAmmo >= _config.MagazineCapacity)
            {
                return WeaponReloadResult.MagazineFull;
            }

            if (ReserveAmmo <= 0)
            {
                return WeaponReloadResult.NoReserveAmmo;
            }

            IsReloading = true;
            ReloadTimeRemaining = _config.ReloadDuration;

            return WeaponReloadResult.Started;
        }

        public void Reset()
        {
            MagazineAmmo = _config.MagazineCapacity;
            ReserveAmmo = _config.StartingReserveAmmo;

            FireCooldownRemaining = 0f;
            ReloadTimeRemaining = 0f;
            IsReloading = false;
        }

        private void CompleteReload()
        {
            int missingAmmo =
                _config.MagazineCapacity - MagazineAmmo;

            int transferredAmmo =
                Math.Min(missingAmmo, ReserveAmmo);

            MagazineAmmo += transferredAmmo;
            ReserveAmmo -= transferredAmmo;

            ReloadTimeRemaining = 0f;
            IsReloading = false;
        }

        private static void ValidateDeltaTime(float deltaTime)
        {
            if (float.IsNaN(deltaTime) ||
                float.IsInfinity(deltaTime) ||
                deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(deltaTime),
                    deltaTime,
                    "Delta time must be a finite, non-negative value.");
            }
        }
    }
}