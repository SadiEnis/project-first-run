using System;
using UnityEngine;

namespace ProjectFirstRun.Weapons
{
    public readonly struct WeaponShotConfig
    {
        public int PelletCount { get; }
        public float HalfAngle { get; }
        public float PushDistance { get; }

        public WeaponShotConfig(int pelletCount, float halfAngle, float pushDistance)
        {
            if (pelletCount < 1 || pelletCount > 64) throw new ArgumentOutOfRangeException(nameof(pelletCount));
            if (!float.IsFinite(halfAngle) || halfAngle < 0 || halfAngle >= 90) throw new ArgumentOutOfRangeException(nameof(halfAngle));
            if (!float.IsFinite(pushDistance) || pushDistance < 0) throw new ArgumentOutOfRangeException(nameof(pushDistance));
            PelletCount = pelletCount;
            HalfAngle = halfAngle;
            PushDistance = pushDistance;
        }
    }

    public static class PelletSpread
    {
        // Uniform solid-angle sampling; random values are supplied explicitly, never UnityEngine.Random.
        public static Vector3 Direction(Vector3 forward, float halfAngle, double radial, double azimuth)
        {
            if (!float.IsFinite(forward.x) || !float.IsFinite(forward.y) || !float.IsFinite(forward.z) || forward.sqrMagnitude < .000001f)
                throw new ArgumentOutOfRangeException(nameof(forward));
            _ = new WeaponShotConfig(1, halfAngle, 0);
            if (double.IsNaN(radial) || radial < 0 || radial > 1 || double.IsNaN(azimuth) || azimuth < 0 || azimuth > 1)
                throw new ArgumentOutOfRangeException(nameof(radial));
            float cosine = Mathf.Lerp(1, Mathf.Cos(halfAngle * Mathf.Deg2Rad), (float)radial);
            float sine = Mathf.Sqrt(Mathf.Max(0, 1 - cosine * cosine));
            float phi = (float)azimuth * Mathf.PI * 2;
            return (Quaternion.FromToRotation(Vector3.forward, forward.normalized) *
                new Vector3(sine * Mathf.Cos(phi), sine * Mathf.Sin(phi), cosine)).normalized;
        }
    }
}
