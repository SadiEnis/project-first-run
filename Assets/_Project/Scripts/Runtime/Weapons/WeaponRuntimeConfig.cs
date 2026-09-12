using System;

namespace ProjectFirstRun.Weapons
{
    public readonly struct WeaponRuntimeConfig
    {
        public int MagazineCapacity { get; }
        public int StartingReserveAmmo { get; }
        public float ShotsPerSecond { get; }
        public float FireInterval { get; }
        public float ReloadDuration { get; }

        public WeaponRuntimeConfig(
            int magazineCapacity,
            int startingReserveAmmo,
            float shotsPerSecond,
            float reloadDuration)
        {
            if (magazineCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(magazineCapacity),
                    magazineCapacity,
                    "Magazine capacity must be greater than zero.");
            }

            if (startingReserveAmmo < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startingReserveAmmo),
                    startingReserveAmmo,
                    "Starting reserve ammunition cannot be negative.");
            }

            if (!IsFinite(shotsPerSecond) ||
                shotsPerSecond <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(shotsPerSecond),
                    shotsPerSecond,
                    "Shots per second must be a finite value greater than zero.");
            }

            if (!IsFinite(reloadDuration) ||
                reloadDuration <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(reloadDuration),
                    reloadDuration,
                    "Reload duration must be a finite value greater than zero.");
            }

            MagazineCapacity = magazineCapacity;
            StartingReserveAmmo = startingReserveAmmo;
            ShotsPerSecond = shotsPerSecond;
            FireInterval = 1f / shotsPerSecond;
            ReloadDuration = reloadDuration;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value);
        }
    }
}