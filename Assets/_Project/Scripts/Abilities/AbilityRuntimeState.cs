using System;

namespace ProjectFirstRun.Abilities
{
    public sealed class AbilityRuntimeState
    {
        private readonly float _cooldown;

        public float Cooldown =>
            _cooldown;

        public float CooldownRemaining
        {
            get;
            private set;
        }

        public bool IsReady =>
            CooldownRemaining <= 0f;

        public AbilityRuntimeState(
            in AbilityRuntimeConfig config)
        {
            _cooldown = config.Cooldown;
            CooldownRemaining = 0f;
        }

        public void Tick(
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

            if (IsReady ||
                deltaTime <= 0f)
            {
                return;
            }

            CooldownRemaining =
                Math.Max(
                    0f,
                    CooldownRemaining - deltaTime);
        }

        public AbilityCastResult TryCommitCast()
        {
            if (!IsReady)
            {
                return AbilityCastResult.OnCooldown;
            }

            CooldownRemaining =
                _cooldown;

            return AbilityCastResult.Performed;
        }
    }
}