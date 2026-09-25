using System;

namespace ProjectFirstRun.Weapons
{
    public readonly struct WeaponRecoilConfig
    {
        public float Kick { get; }
        public float Delay { get; }
        public float Duration { get; }
        public float MaximumOffset { get; }

        public WeaponRecoilConfig(float kick, float delay = .08f, float duration = .3f, float maximumOffset = 12)
        {
            if (!float.IsFinite(kick) || kick < 0) throw new ArgumentOutOfRangeException(nameof(kick));
            if (!float.IsFinite(delay) || delay < 0) throw new ArgumentOutOfRangeException(nameof(delay));
            if (!float.IsFinite(duration) || duration <= 0) throw new ArgumentOutOfRangeException(nameof(duration));
            if (!float.IsFinite(maximumOffset) || maximumOffset <= 0 || kick > maximumOffset)
                throw new ArgumentOutOfRangeException(nameof(maximumOffset));
            Kick = kick;
            Delay = delay;
            Duration = duration;
            MaximumOffset = maximumOffset;
        }
    }

    /// <summary>Temporary upward pitch offset, independent of player aim and camera transforms.</summary>
    public sealed class WeaponRecoilState
    {
        private float _startOffset, _duration, _elapsed;
        public float Offset { get; private set; }
        public float DelayRemaining { get; private set; }

        public void Kick(WeaponRecoilConfig config)
        {
            // Struct defaults bypass the validating constructor.
            _ = new WeaponRecoilConfig(config.Kick, config.Delay, config.Duration, config.MaximumOffset);
            if (config.Kick == 0) return;
            // A lower-cap weapon must not snap an existing larger offset downward.
            Offset += Math.Min(config.Kick, Math.Max(0, config.MaximumOffset - Offset));
            _startOffset = Offset;
            _elapsed = 0;
            _duration = config.Duration;
            DelayRemaining = config.Delay;
        }

        public void Tick(float deltaTime)
        {
            if (!float.IsFinite(deltaTime) || deltaTime < 0) throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (deltaTime == 0 || Offset == 0) return;
            float wait = Math.Min(DelayRemaining, deltaTime);
            DelayRemaining -= wait;
            float recovering = deltaTime - wait;
            if (recovering <= 0) return;
            _elapsed = Math.Min(_duration, _elapsed + recovering);
            float t = _elapsed / _duration;
            Offset = _startOffset * (1 - t * t * (3 - 2 * t));
            if (_elapsed >= _duration) Reset();
        }

        public void Constrain(float headroom)
        {
            if (!float.IsFinite(headroom) || headroom < 0) throw new ArgumentOutOfRangeException(nameof(headroom));
            if (Offset <= headroom) return;
            if (headroom == 0) { Reset(); return; }
            _startOffset *= headroom / Offset;
            Offset = headroom;
        }

        public void Reset()
        {
            Offset = DelayRemaining = _startOffset = _elapsed = _duration = 0;
        }
    }
}
