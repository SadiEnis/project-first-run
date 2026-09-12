using System;
using UnityEngine;

namespace ProjectFirstRun.Progression
{
    [DisallowMultipleComponent]
    public sealed class PlayerExperienceController : MonoBehaviour
    {
        private ExperienceState _state;
        private bool _isNotifying;

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

            ExperienceGainResult result = state.GainExperience(amount);
            if (amount == 0)
            {
                return result;
            }

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
