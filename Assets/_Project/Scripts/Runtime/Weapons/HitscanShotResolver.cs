using System;
using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Weapons
{
    /// <summary>
    /// Resolves a two-stage camera-to-muzzle hitscan shot.
    /// </summary>
    public sealed class HitscanShotResolver
    {
        private static readonly Vector3 ViewportCentre =
            new Vector3(0.5f, 0.5f, 0f);

        private readonly Camera _aimCamera;
        private readonly Transform _muzzle;
        private readonly GameObject _damageSource;

        public HitscanShotResolver(
            Camera aimCamera,
            Transform muzzle,
            GameObject damageSource)
        {
            _aimCamera = aimCamera != null
                ? aimCamera
                : throw new ArgumentNullException(nameof(aimCamera));

            _muzzle = muzzle != null
                ? muzzle
                : throw new ArgumentNullException(nameof(muzzle));

            _damageSource = damageSource != null
                ? damageSource
                : throw new ArgumentNullException(nameof(damageSource));
        }

        public HitscanShotResult Resolve(
            float damage,
            float range,
            int damageMask)
        {
            ValidatePositiveFiniteValue(
                damage,
                nameof(damage));

            ValidatePositiveFiniteValue(
                range,
                nameof(range));

            Ray aimRay =
                _aimCamera.ViewportPointToRay(ViewportCentre);

            Vector3 aimPoint =
                aimRay.origin + aimRay.direction * range;

            if (Physics.Raycast(
                    aimRay,
                    out RaycastHit aimHit,
                    range,
                    damageMask,
                    QueryTriggerInteraction.Ignore))
            {
                aimPoint = aimHit.point;
            }

            Vector3 shotOrigin = _muzzle.position;
            Vector3 vectorToAimPoint = aimPoint - shotOrigin;

            float shotDistance = vectorToAimPoint.magnitude;

            Vector3 shotDirection =
                shotDistance > Mathf.Epsilon
                    ? vectorToAimPoint / shotDistance
                    : aimRay.direction;

            if (shotDistance <= Mathf.Epsilon)
            {
                shotDistance = range;
            }

            if (!Physics.Raycast(
                    shotOrigin,
                    shotDirection,
                    out RaycastHit shotHit,
                    shotDistance,
                    damageMask,
                    QueryTriggerInteraction.Ignore))
            {
                Vector3 endPoint =
                    shotOrigin +
                    shotDirection * shotDistance;

                return HitscanShotResult.Miss(
                    shotOrigin,
                    shotDirection,
                    endPoint);
            }

            IDamageable damageable =
                FindDamageable(shotHit.collider);

            if (damageable == null)
            {
                return HitscanShotResult.HitWithoutDamageable(
                    shotHit.collider,
                    shotOrigin,
                    shotDirection,
                    shotHit.point);
            }

            DamageInfo damageInfo = new DamageInfo(
                damage,
                _damageSource,
                shotHit.point,
                shotDirection);

            DamageResult damageResult =
                damageable.ApplyDamage(in damageInfo);

            return HitscanShotResult.HitDamageable(
                shotHit.collider,
                shotOrigin,
                shotDirection,
                shotHit.point,
                damageResult);
        }

        private static IDamageable FindDamageable(
            Collider hitCollider)
        {
            MonoBehaviour[] behaviours =
                hitCollider.GetComponentsInParent<MonoBehaviour>(true);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IDamageable damageable)
                {
                    return damageable;
                }
            }

            return null;
        }

        private static void ValidatePositiveFiniteValue(
            float value,
            string parameterName)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value) ||
                value <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Value must be finite and greater than zero.");
            }
        }
    }
}