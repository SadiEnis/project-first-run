using System;
using UnityEngine;

namespace ProjectFirstRun.Enemies
{
    // Owns attack timing and one-hit commitment; navigation stays in EnemyMotor.
    public sealed class EnemyChargeState
    {
        public EnemyChargeConfig Config { get; }
        public EnemyChargePhase Phase { get; private set; }
        public Vector3 Direction { get; private set; }
        public float TimeRemaining { get; private set; }
        public float DistanceRemaining { get; private set; }
        public bool HasHit { get; private set; }

        public EnemyChargeState(EnemyChargeConfig config)
        {
            // Also reject a default/uninitialized struct.
            Config = new EnemyChargeConfig(config.ActivationRange, config.Windup, config.Speed,
                config.Distance, config.HitRadius, config.Recovery);
        }

        public bool TryBegin(Vector3 offset)
        {
            if (!float.IsFinite(offset.x) || !float.IsFinite(offset.y) || !float.IsFinite(offset.z))
                throw new ArgumentOutOfRangeException(nameof(offset));
            offset.y = 0;
            if (Phase != EnemyChargePhase.Pursuing || offset.sqrMagnitude < 0.0001f ||
                offset.magnitude > Config.ActivationRange) return false;
            Direction = offset.normalized;
            TimeRemaining = Config.Windup;
            DistanceRemaining = Config.Distance;
            HasHit = false;
            Phase = EnemyChargePhase.Windup;
            return true;
        }

        public void Tick(float deltaTime)
        {
            ValidateDeltaTime(deltaTime);
            if (deltaTime == 0) return;
            if (Phase != EnemyChargePhase.Windup && Phase != EnemyChargePhase.Recovery) return;
            TimeRemaining = Math.Max(0, TimeRemaining - deltaTime);
            if (TimeRemaining > 0) return;
            Phase = Phase == EnemyChargePhase.Windup ? EnemyChargePhase.Charging : EnemyChargePhase.Pursuing;
        }

        public float StepDistance(float deltaTime)
        {
            ValidateDeltaTime(deltaTime);
            return Phase == EnemyChargePhase.Charging ? Math.Min(DistanceRemaining, Config.Speed * deltaTime) : 0;
        }

        public void Advance(float actualDistance, bool blocked)
        {
            ValidateDeltaTime(actualDistance);
            if (Phase != EnemyChargePhase.Charging) return;
            DistanceRemaining = Math.Max(0, DistanceRemaining - actualDistance);
            if (!blocked && DistanceRemaining > 0.001f) return;
            Phase = EnemyChargePhase.Recovery;
            TimeRemaining = Config.Recovery;
        }

        public bool TryCommitHit()
        {
            if (Phase != EnemyChargePhase.Charging || HasHit) return false;
            HasHit = true;
            return true;
        }

        public void Cancel()
        {
            Phase = EnemyChargePhase.Pursuing;
            TimeRemaining = 0; DistanceRemaining = 0; HasHit = false; Direction = Vector3.zero;
        }

        private static void ValidateDeltaTime(float value)
        {
            if (!float.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
}
