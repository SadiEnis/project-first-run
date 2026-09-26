using System;
using UnityEngine;

namespace ProjectFirstRun.Combat
{
    /// <summary>Shared half-second burn clock. Refresh preserves the pending tick.</summary>
    public class TimedBurnState
    {
        public const float Interval = .5f;
        public float Remaining { get; private set; }
        public float UntilTick { get; private set; } = Interval;
        public float Damage { get; private set; }
        public void Refresh(float damage, float duration)
        {
            if (!float.IsFinite(damage) || damage <= 0) throw new ArgumentOutOfRangeException(nameof(damage));
            if (!float.IsFinite(duration) || duration <= 0 || duration > 3) throw new ArgumentOutOfRangeException(nameof(duration));
            if (Remaining <= 0) UntilTick = Interval;
            Damage = damage; Remaining = duration;
        }
        public int Advance(float delta)
        {
            if (!float.IsFinite(delta) || delta < 0) throw new ArgumentOutOfRangeException(nameof(delta));
            if (Remaining <= 0 || delta == 0) return 0;
            float elapsed = Mathf.Min(delta, Remaining);
            Remaining = Mathf.Max(0, Remaining - elapsed); UntilTick -= elapsed;
            int ticks = 0;
            while (UntilTick <= .000001f && ticks < 6) { ticks++; UntilTick += Interval; }
            return ticks;
        }
    }
}
