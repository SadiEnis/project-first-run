using System;
using ProjectFirstRun.Stats;
using UnityEngine;

namespace ProjectFirstRun.Progression
{
    [DisallowMultipleComponent]
    public sealed class PlayerExperienceController : MonoBehaviour
    {
        private ExperienceState _state;
        private bool _isNotifying;
        private PlayerStatsController _stats;
        public decimal FractionalExperience { get; private set; }

        public bool IsInitialized => _state != null;
        public int Level => GetState().Level;
        public long CurrentExperience => GetState().CurrentExperience;
        public long TotalExperience => GetState().TotalExperience;
        public long RequiredExperience => GetState().RequiredExperience;

        public event Action<ExperienceGainResult> ExperienceChanged;
        public event Action<ExperienceGainResult> LevelChanged;

        public void Initialize(ExperienceState state)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerExperienceController)} has already been initialized.");
            }

            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public ExperienceGainResult GainExperience(int amount)
        {
            ExperienceState state = GetState();

            if (_isNotifying)
            {
                throw new InvalidOperationException(
                    "Experience cannot be awarded during an experience notification.");
            }

            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount == 0) return state.GainExperience(0);
            if (_stats == null) _stats = GetComponent<PlayerStatsController>();
            float multiplier = _stats == null ? 1f : _stats.Evaluate(PlayerStatType.ExperienceGain, 1f);
            if (!float.IsFinite(multiplier) || multiplier <= 0f)
                throw new InvalidOperationException("XP multiplier must be finite and positive.");
            // Decimal conversion removes binary float noise at configured percentage precision.
            // Calculate everything before committing either the integer XP or the remainder.
            decimal scaled = checked(amount * (decimal)multiplier + FractionalExperience);
            int awarded = checked((int)decimal.Floor(scaled));
            decimal remainder = scaled - awarded;
            ExperienceGainResult result = state.GainExperience(awarded);
            FractionalExperience = remainder;

            _isNotifying = true;
            try
            {
                ExperienceChanged?.Invoke(result);
                if (result.LevelsGained > 0)
                {
                    LevelChanged?.Invoke(result);
                }
            }
            finally
            {
                _isNotifying = false;
            }

            return result;
        }

        private ExperienceState GetState()
        {
            return _state ?? throw new InvalidOperationException(
                $"{nameof(PlayerExperienceController)} must be initialized before use.");
        }
    }
}
