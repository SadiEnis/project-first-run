using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Enemies
{
    [DisallowMultipleComponent]
    public sealed class EnemyPerceptionController : MonoBehaviour
    {
        [SerializeField] private EnemyPerceptionProfile _profile;
        [SerializeField] private EnemyAwareness _awareness;
        [SerializeField] private Vector3 _lastKnownPosition;
        [SerializeField] private float _memoryRemaining;
        private EnemyController _enemy;
        private HealthComponent _health;
        private float _sampleTimer;
        public EnemyPerceptionState State { get; private set; }
        public EnemyPerceptionProfile Profile => _profile;
        public bool CanAct => isActiveAndEnabled && _enemy != null && _enemy.isActiveAndEnabled &&
            _enemy.IsInitialized && !_enemy.IsDead && !_enemy.Health.IsDead &&
            _enemy.Target != null && _enemy.Target.gameObject.activeInHierarchy;
        public bool HasSight => CanAct && State != null && State.Awareness == EnemyAwareness.Visible;

        public void Configure(EnemyPerceptionProfile profile, EnemyController enemy)
        {
            profile.Validate();
            Unsubscribe();
            _profile = profile; _enemy = enemy;
            State = new EnemyPerceptionState(profile.MemorySeconds);
            _health = enemy.GetComponent<HealthComponent>();
            if (isActiveAndEnabled) _health.Damaged += OnDamaged;
            ResetAwareness();
        }

        private void OnEnable()
        {
            ResetAwareness();
            if (_health != null) { _health.Damaged -= OnDamaged; _health.Damaged += OnDamaged; }
        }

        private void OnDisable()
        {
            Unsubscribe(); ResetAwareness();
            if (_enemy != null) _enemy.GetComponent<EnemyMotor>().Stop();
        }

        private void Unsubscribe() { if (_health != null) _health.Damaged -= OnDamaged; }

        public void Suspend()
        {
            Unsubscribe(); ResetAwareness();
        }

        public void Reactivate()
        {
            Unsubscribe(); ResetAwareness();
            if (isActiveAndEnabled && _health != null) _health.Damaged += OnDamaged;
        }

        public void ResetAwareness()
        {
            State?.Reset(); _sampleTimer = 0f; Publish();
        }

        // The attack owner advances perception before making any movement/attack decision.
        public void Tick(float deltaTime, bool targetAlive)
        {
            if (!CanAct || !targetAlive) { ResetAwareness(); return; }
            if (deltaTime <= 0f || State == null) return;
            State.Tick(deltaTime);
            _sampleTimer -= deltaTime;
            if (_sampleTimer <= 0f)
            {
                _sampleTimer = _profile.SampleInterval;
                bool visible = QuerySight();
                State.Observe(visible, visible ? _enemy.Target.position : State.LastKnownPosition);
            }
            Publish();
        }

        public bool QuerySight()
        {
            if (!CanAct || State == null) return false;
            Vector3 offset = _enemy.Target.position - transform.position;
            float range = State.IsAlerted ? _profile.TrackingRange : _profile.AcquisitionRange;
            float cone = State.IsAlerted ? 360f : _profile.ConeDegrees;
            if (!EnemyPerceptionState.InView(offset, transform.forward, range, cone)) return false;
            Vector3 origin = transform.position + _profile.EyeOffset;
            Vector3 end = _enemy.Target.position + _profile.TargetOffset;
            foreach (var hit in Physics.RaycastAll(origin, end - origin, Vector3.Distance(origin, end),
                         _profile.ObstructionMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.IsChildOf(transform) || hit.transform.IsChildOf(_enemy.Target)) continue;
                return false;
            }
            return true;
        }

        // Refresh at the actual attack boundary as well as on the cheaper sensing clock.
        public bool ConfirmAttackSight()
        {
            if (!HasSight) return false;
            if (QuerySight()) return true;
            State.Observe(false, State.LastKnownPosition); Publish();
            return false;
        }

        private void OnDamaged(DamageInfo info, DamageResult result)
        {
            if (!CanAct || Time.timeScale <= 0f || State == null || !result.WasApplied ||
                result.WasLethal || info.Source == null ||
                !info.Source.transform.IsChildOf(_enemy.Target)) return;
            var targetHealth = _enemy.Target.GetComponentInParent<HealthComponent>();
            if (targetHealth != null && targetHealth.IsDead) return;
            State.Alarm(_enemy.Target.position); Publish();
        }

        public void AlarmAt(Vector3 lastKnownPosition)
        {
            if (!CanAct || State == null || Time.timeScale <= 0f) return;
            State.Alarm(lastKnownPosition);
            Publish();
        }

        private void Publish()
        {
            _awareness = State?.Awareness ?? EnemyAwareness.Idle;
            _lastKnownPosition = State?.LastKnownPosition ?? Vector3.zero;
            _memoryRemaining = State?.MemoryRemaining ?? 0f;
        }

        private void OnDrawGizmosSelected()
        {
            if (_profile == null) return;
            Vector3 origin = transform.position + _profile.EyeOffset;
            Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(origin, _profile.AcquisitionRange);
            Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(origin, _profile.TrackingRange);
            foreach (float sign in new[] { -1f, 1f })
                Gizmos.DrawRay(origin, Quaternion.AngleAxis(sign * _profile.ConeDegrees * .5f, Vector3.up) *
                    transform.forward * _profile.AcquisitionRange);
            if (State == null || !State.IsAlerted) return;
            Gizmos.color = Color.red; Gizmos.DrawWireSphere(_lastKnownPosition, .4f);
            Gizmos.DrawLine(transform.position, _lastKnownPosition);
        }
    }
}
