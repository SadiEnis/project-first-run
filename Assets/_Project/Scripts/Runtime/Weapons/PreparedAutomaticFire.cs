using System;

namespace ProjectFirstRun.Weapons
{
    /// <summary>Advances the existing ammo/cooldown model chronologically, with bounded catch-up.</summary>
    public sealed class PreparedAutomaticFire
    {
        public const int MaximumShotsPerTick = 3;
        public float PreparationElapsed { get; private set; }
        public void Reset() => PreparationElapsed = 0;

        // Call only while held, controlled, loaded and not reloading. The callback consumes one shot.
        public void Tick(WeaponRuntimeState weapon, float deltaTime, float preparationDuration, Func<bool> fire)
        {
            if (weapon == null) throw new ArgumentNullException(nameof(weapon));
            if (fire == null) throw new ArgumentNullException(nameof(fire));
            if (!float.IsFinite(deltaTime) || deltaTime < 0) throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (!float.IsFinite(preparationDuration) || preparationDuration < 0)
                throw new ArgumentOutOfRangeException(nameof(preparationDuration));
            if (deltaTime == 0) return;
            float preparing = Math.Min(deltaTime, Math.Max(0, preparationDuration - PreparationElapsed));
            PreparationElapsed += preparing;
            weapon.Tick(preparing);
            float remaining = deltaTime - preparing;
            if (PreparationElapsed < preparationDuration) return;

            for (int i = 0; i < MaximumShotsPerTick; i++)
            {
                float wait = weapon.FireCooldownRemaining;
                if (wait > remaining)
                {
                    weapon.Tick(remaining);
                    return;
                }
                weapon.Tick(wait);
                remaining -= wait;
                if (!fire()) return;
                if (weapon.MagazineAmmo == 0 || weapon.IsReloading)
                {
                    Reset();
                    weapon.Tick(remaining);
                    return;
                }
            }
            // Drop excess stalled-frame time, retain the last shot's cooldown. No debt next frame.
        }
    }
}
