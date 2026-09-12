using UnityEngine;

namespace ProjectFirstRun.Combat
{
    /// <summary>
    /// Describes a single damage request and its runtime context.
    /// </summary>
    public readonly struct DamageInfo
    {
        public float Amount { get; }
        public GameObject Source { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitDirection { get; }

        public DamageInfo(
            float amount,
            GameObject source,
            Vector3 hitPoint,
            Vector3 hitDirection)
        {
            Amount = amount;
            Source = source;
            HitPoint = hitPoint;
            HitDirection = hitDirection;
        }
    }
}