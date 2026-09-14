using System;
using UnityEngine;

namespace ProjectFirstRun.Enemies
{
    public enum EnemyRangedPhase { Approaching = 0, Holding = 1, Windup = 2, Cooldown = 3 }

    public readonly struct EnemyRangedConfig
    {
        public float PreferredMin { get; }
        public float PreferredMax { get; }
        public float FireRange { get; }
        public float Windup { get; }
        public float Cooldown { get; }
        public float ProjectileSpeed { get; }
        public float ProjectileLifetime { get; }
        public float ProjectileRadius { get; }

        public EnemyRangedConfig(float preferredMin, float preferredMax, float fireRange,
            float windup, float cooldown, float projectileSpeed, float projectileLifetime,
            float projectileRadius)
        {
            if (!FinitePositive(preferredMin) || !FinitePositive(preferredMax) ||
                preferredMax <= preferredMin || !FinitePositive(fireRange) ||
                fireRange < preferredMax || !FinitePositive(windup) || !FinitePositive(cooldown) ||
                !FinitePositive(projectileSpeed) || !FinitePositive(projectileLifetime) ||
                !FinitePositive(projectileRadius))
                throw new ArgumentOutOfRangeException(nameof(preferredMin), "Ranged settings are invalid.");
            PreferredMin = preferredMin; PreferredMax = preferredMax; FireRange = fireRange;
            Windup = windup; Cooldown = cooldown; ProjectileSpeed = projectileSpeed;
            ProjectileLifetime = projectileLifetime; ProjectileRadius = projectileRadius;
        }

        private static bool FinitePositive(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
    }

    public sealed class EnemyRangedState
    {
        public EnemyRangedConfig Config { get; }
        public EnemyRangedPhase Phase { get; private set; } = EnemyRangedPhase.Approaching;
        public float TimeRemaining { get; private set; }
        public Vector3 Direction { get; private set; }
        public bool IsReadyToRelease => Phase == EnemyRangedPhase.Windup && TimeRemaining <= 0f;
        public float CooldownRemaining => Phase == EnemyRangedPhase.Cooldown ? TimeRemaining : 0f;

        public EnemyRangedState(EnemyRangedConfig config) => Config = config;

        public void SetApproaching() { if (Phase != EnemyRangedPhase.Windup) Phase = EnemyRangedPhase.Approaching; }
        public void SetHolding() { if (Phase == EnemyRangedPhase.Approaching) Phase = EnemyRangedPhase.Holding; }

        public bool TryBeginWindup(Vector3 offset, bool hasLineOfSight)
        {
            offset.y = 0f;
            float distance = offset.magnitude;
            if (Phase != EnemyRangedPhase.Holding || distance < Config.PreferredMin ||
                distance > Config.PreferredMax || distance > Config.FireRange || !hasLineOfSight ||
                offset.sqrMagnitude <= Mathf.Epsilon) return false;
            Direction = offset.normalized;
            TimeRemaining = Config.Windup;
            Phase = EnemyRangedPhase.Windup;
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (deltaTime == 0f) return;
            if (Phase != EnemyRangedPhase.Windup && Phase != EnemyRangedPhase.Cooldown) return;
            TimeRemaining = Math.Max(0f, TimeRemaining - deltaTime);
            if (TimeRemaining > 0f) return;
            // Keep Windup as the release-ready phase; the controller commits the shot.
            if (Phase == EnemyRangedPhase.Cooldown) Phase = EnemyRangedPhase.Holding;
        }

        public void CancelWindup() { if (Phase == EnemyRangedPhase.Windup) { Phase = EnemyRangedPhase.Holding; TimeRemaining = 0f; } }
        public bool TryRelease() => IsReadyToRelease;
        public void CommitRelease() { if (!TryRelease()) return; Phase = EnemyRangedPhase.Cooldown; TimeRemaining = Config.Cooldown; }
        public void Cancel() { Phase = EnemyRangedPhase.Approaching; TimeRemaining = 0f; Direction = Vector3.zero; }
    }
}
