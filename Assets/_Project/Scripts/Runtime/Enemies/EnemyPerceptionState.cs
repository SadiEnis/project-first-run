using System;
using UnityEngine;

namespace ProjectFirstRun.Enemies
{
    public enum EnemyAwareness { Idle, Visible, Investigating }

    // Contains no Transform references: damage renewal cannot follow an unseen target.
    public sealed class EnemyPerceptionState
    {
        private readonly float _duration;
        public EnemyAwareness Awareness { get; private set; }
        public bool IsAlerted => Awareness != EnemyAwareness.Idle;
        public Vector3 LastKnownPosition { get; private set; }
        public float MemoryRemaining { get; private set; }

        public EnemyPerceptionState(float duration)
        {
            if (!float.IsFinite(duration) || duration <= 0f)
                throw new ArgumentOutOfRangeException(nameof(duration));
            _duration = duration;
        }

        public void Tick(float deltaTime)
        {
            if (!float.IsFinite(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            MemoryRemaining = Mathf.Max(0f, MemoryRemaining - deltaTime);
            if (MemoryRemaining <= 0f) Reset();
        }

        public void Observe(bool visible, Vector3 position)
        {
            if (visible)
            {
                LastKnownPosition = position;
                MemoryRemaining = _duration;
                Awareness = EnemyAwareness.Visible;
            }
            else if (IsAlerted) Awareness = EnemyAwareness.Investigating;
        }

        public void Alarm(Vector3 position)
        {
            if (!IsAlerted)
            {
                LastKnownPosition = position;
                Awareness = EnemyAwareness.Investigating;
            }
            MemoryRemaining = _duration;
        }

        public void Reset()
        {
            Awareness = EnemyAwareness.Idle;
            MemoryRemaining = 0f;
            LastKnownPosition = Vector3.zero;
        }

        public static bool InView(Vector3 offset, Vector3 forward, float range, float coneDegrees)
        {
            if (offset.sqrMagnitude > range * range) return false;
            offset.y = 0f; forward.y = 0f;
            if (offset.sqrMagnitude < .000001f) return true;
            return Vector3.Dot(offset.normalized, forward.normalized) + .000001f >=
                Mathf.Cos(coneDegrees * .5f * Mathf.Deg2Rad);
        }
    }
}
