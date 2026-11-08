using System;

namespace ProjectFirstRun.Arenas
{
    /// <summary>Unscaled presentation clock; no gameplay, damage or scene loading.</summary>
    public sealed class DemoEndingTimeline
    {
        public float RevealSeconds { get; }
        public float WindupSeconds { get; }
        public float StrikeSeconds { get; }
        public float AftermathSeconds { get; }
        public float FadeSeconds { get; }
        public float Elapsed { get; private set; }
        public float ImpactAt => RevealSeconds + WindupSeconds + StrikeSeconds;
        public float FadeAt => ImpactAt + AftermathSeconds;
        public float Duration => FadeAt + FadeSeconds;
        public bool HasImpacted => Elapsed >= ImpactAt;
        public bool IsComplete => Elapsed >= Duration;
        public float Fade => Math.Min(1f, Math.Max(0f, (Elapsed - FadeAt) / FadeSeconds));

        public DemoEndingTimeline(float reveal, float windup, float strike, float aftermath, float fade)
        {
            foreach (float duration in new[] { reveal, windup, strike, aftermath, fade })
                if (!float.IsFinite(duration) || duration <= 0 || duration > 60)
                    throw new ArgumentOutOfRangeException(nameof(duration), "Ending stage durations must be within (0, 60] seconds.");
            RevealSeconds = reveal; WindupSeconds = windup; StrikeSeconds = strike;
            AftermathSeconds = aftermath; FadeSeconds = fade;
        }

        // True only on the first tick crossing impact, including a large frame step.
        public bool Tick(float unscaledDeltaTime)
        {
            if (!float.IsFinite(unscaledDeltaTime) || unscaledDeltaTime < 0)
                throw new ArgumentOutOfRangeException(nameof(unscaledDeltaTime));
            bool hadImpact = HasImpacted;
            Elapsed = Math.Min(Duration, Elapsed + unscaledDeltaTime);
            return !hadImpact && HasImpacted;
        }
    }
}
