using System;

namespace ProjectFirstRun.Arenas
{
    public enum KeyAmbushPhase
    {
        AwaitingKey, PreparingFirst, FightingFirst, Intermission,
        PreparingSecond, FightingSecond, Completed, Failed, Cancelled
    }

    /// <summary>Run-local ownership and sequencing; unrelated encounter events cannot complete this session.</summary>
    public sealed class KeyAmbushSession
    {
        private readonly float _intermissionSeconds;
        private float _remaining;
        public KeyAmbushPhase Phase { get; private set; } = KeyAmbushPhase.AwaitingKey;
        public bool HasKey { get; private set; }
        public bool CanEnterFinal => HasKey && Phase == KeyAmbushPhase.Completed;
        public bool EntranceClosed => HasKey;
        public bool ExitOpen => CanEnterFinal;
        public string Error { get; private set; }
        public int RequestedGroup => Phase == KeyAmbushPhase.PreparingFirst ? 0 :
            Phase == KeyAmbushPhase.PreparingSecond ? 1 : -1;

        public KeyAmbushSession(float intermissionSeconds = 1f)
        {
            if (!float.IsFinite(intermissionSeconds) || intermissionSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(intermissionSeconds));
            _intermissionSeconds = intermissionSeconds;
        }

        public bool TryCollect(bool playerAlive, bool interactionAllowed, bool entranceClear)
        {
            if (Phase != KeyAmbushPhase.AwaitingKey || !playerAlive || !interactionAllowed || !entranceClear)
                return false;
            HasKey = true;
            Phase = KeyAmbushPhase.PreparingFirst;
            return true;
        }

        public bool TryStartGroup(int index)
        {
            if (index < 0 || index != RequestedGroup) return false;
            Phase = index == 0 ? KeyAmbushPhase.FightingFirst : KeyAmbushPhase.FightingSecond;
            return true;
        }

        public void CompleteGroup(int index)
        {
            if (index == 0 && Phase == KeyAmbushPhase.FightingFirst)
            {
                Phase = KeyAmbushPhase.Intermission;
                _remaining = _intermissionSeconds;
            }
            else if (index == 1 && Phase == KeyAmbushPhase.FightingSecond)
                Phase = KeyAmbushPhase.Completed;
        }

        public void Tick(float scaledDeltaTime)
        {
            if (!float.IsFinite(scaledDeltaTime) || scaledDeltaTime < 0)
                throw new ArgumentOutOfRangeException(nameof(scaledDeltaTime));
            if (Phase != KeyAmbushPhase.Intermission || scaledDeltaTime == 0) return;
            _remaining -= scaledDeltaTime;
            if (_remaining <= 0) Phase = KeyAmbushPhase.PreparingSecond;
        }

        public void Fail(string message)
        {
            if (Phase == KeyAmbushPhase.Cancelled || Phase == KeyAmbushPhase.Completed || Phase == KeyAmbushPhase.Failed) return;
            Error = string.IsNullOrWhiteSpace(message) ? "Ambush failed." : message;
            Phase = KeyAmbushPhase.Failed;
        }

        public void Cancel()
        {
            HasKey = false;
            Phase = KeyAmbushPhase.Cancelled;
        }
    }
}
