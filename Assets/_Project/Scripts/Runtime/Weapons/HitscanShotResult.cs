using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Weapons
{
    /// <summary>
    /// Contains the Unity world result of a resolved hitscan shot.
    /// </summary>
    public readonly struct HitscanShotResult
    {
        public bool HitSomething { get; }
        public Collider HitCollider { get; }
        public Vector3 ShotOrigin { get; }
        public Vector3 ShotDirection { get; }
        public Vector3 HitPoint { get; }
        public bool FoundDamageable { get; }
        public DamageResult DamageResult { get; }

        public bool DamageWasApplied =>
            FoundDamageable &&
            DamageResult.WasApplied;

        private HitscanShotResult(
            bool hitSomething,
            Collider hitCollider,
            Vector3 shotOrigin,
            Vector3 shotDirection,
            Vector3 hitPoint,
            bool foundDamageable,
            DamageResult damageResult)
        {
            HitSomething = hitSomething;
            HitCollider = hitCollider;
            ShotOrigin = shotOrigin;
            ShotDirection = shotDirection;
            HitPoint = hitPoint;
            FoundDamageable = foundDamageable;
            DamageResult = damageResult;
        }

        public static HitscanShotResult Miss(
            Vector3 shotOrigin,
            Vector3 shotDirection,
            Vector3 endPoint)
        {
            return new HitscanShotResult(
                false,
                null,
                shotOrigin,
                shotDirection,
                endPoint,
                false,
                default);
        }

        public static HitscanShotResult HitWithoutDamageable(
            Collider hitCollider,
            Vector3 shotOrigin,
            Vector3 shotDirection,
            Vector3 hitPoint)
        {
            return new HitscanShotResult(
                true,
                hitCollider,
                shotOrigin,
                shotDirection,
                hitPoint,
                false,
                default);
        }

        public static HitscanShotResult HitDamageable(
            Collider hitCollider,
            Vector3 shotOrigin,
            Vector3 shotDirection,
            Vector3 hitPoint,
            DamageResult damageResult)
        {
            return new HitscanShotResult(
                true,
                hitCollider,
                shotOrigin,
                shotDirection,
                hitPoint,
                true,
                damageResult);
        }
    }
}