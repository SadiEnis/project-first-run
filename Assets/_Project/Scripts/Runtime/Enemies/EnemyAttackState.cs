using System;

namespace ProjectFirstRun.Enemies
{
    public enum EnemyAttackAttemptResult
    {
        Performed = 0,
        TargetOutOfRange = 1,
        BlockedByCooldown = 2
    }

    /// <summary>
    /// Pure C# model containing enemy attack cooldown rules.
    /// </summary>
    public sealed class EnemyAttackState
    {
        private readonly EnemyAttackConfig _config;

        public float Damage =>
            _config.Damage;

        public float Range =>
            _config.Range;

        public float RangeSquared =>
            _config.RangeSquared;

        public float CooldownDuration =>
            _config.CooldownDuration;

        public float CooldownRemaining
        {
            get;
            private set;
        }

        public bool IsReady =>
            CooldownRemaining <= 0f;

        public EnemyAttackState(
            in EnemyAttackConfig config)
        {
            _config = config;
            CooldownRemaining = 0f;
        }

        public void Tick(float deltaTime)
        {
            ValidateDeltaTime(deltaTime);

            CooldownRemaining = Math.Max(
                0f,
                CooldownRemaining - deltaTime);
        }

        public EnemyAttackAttemptResult TryCommitAttack(
            bool targetIsInRange)
        {
            if (!targetIsInRange)
            {
                return EnemyAttackAttemptResult.TargetOutOfRange;
            }

            if (!IsReady)
            {
                return EnemyAttackAttemptResult.BlockedByCooldown;
            }

            CooldownRemaining =
                _config.CooldownDuration;

            return EnemyAttackAttemptResult.Performed;
        }

        public void Reset()
        {
            CooldownRemaining = 0f;
        }

        private static void ValidateDeltaTime(
            float deltaTime)
        {
            if (float.IsNaN(deltaTime) ||
                float.IsInfinity(deltaTime) ||
                deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(deltaTime),
                    deltaTime,
                    "Delta time must be finite and non-negative.");
            }
        }
    }
}