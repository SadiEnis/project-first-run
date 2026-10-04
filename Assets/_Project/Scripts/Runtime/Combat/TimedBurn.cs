using System;
using UnityEngine;

namespace ProjectFirstRun.Combat
{
    /// <summary>Lifecycle shared by the independently keyed Plasma and Fireball effects.</summary>
    public abstract class TimedBurn : MonoBehaviour
    {
        GameObject _source, _visual;
        HealthComponent _sourceHealth, _targetHealth;
        IDamageable _receiver;
        Component _target;
        Collider _collider;
        Action<Collider, Vector3, DamageResult> _applied;
        bool _ended;
        public TimedBurnState State { get; } = new TimedBurnState();

        protected static void ApplyEffect<T>(GameObject source, IDamageable receiver, Collider collider,
            float damage, float duration, GameObject visual, Action<Collider, Vector3, DamageResult> applied) where T : TimedBurn
        {
            // Validate before creating any component or presentation object.
            new TimedBurnState().Refresh(damage, duration);
            if (source == null || !(receiver is Component target) || target == null || !target.gameObject.activeInHierarchy) return;
            var sourceHealth = source.GetComponent<HealthComponent>();
            var health = target.GetComponent<HealthComponent>();
            if ((health != null && health.IsDead) || (sourceHealth != null && sourceHealth.IsDead)) return;
            TimedBurn burn = null;
            foreach (var candidate in target.GetComponents<T>())
                if (!candidate._ended && candidate._source == source && ReferenceEquals(candidate._receiver, receiver))
                { burn = candidate; break; }
            if (burn == null)
            {
                burn = target.gameObject.AddComponent<T>();
                burn._source = source; burn._sourceHealth = sourceHealth;
                burn._receiver = receiver; burn._target = target; burn._targetHealth = health;
                if (visual != null)
                {
                    burn._visual = Instantiate(visual, target.transform);
                    burn._visual.transform.position = collider != null
                        ? new Vector3(collider.bounds.center.x, collider.bounds.max.y + .25f, collider.bounds.center.z)
                        : target.transform.position + Vector3.up;
                }
            }
            burn._collider = collider; burn._applied = applied; burn.State.Refresh(damage, duration);
        }
        protected void Update() => Advance(Time.deltaTime);
        public void Advance(float delta)
        {
            if (_ended) return;
            if (!Alive()) { End(); return; }
            int ticks = State.Advance(delta);
            for (int i = 0; i < ticks && Alive() && !_ended; i++)
            {
                Vector3 point = _target.transform.position;
                var info = new DamageInfo(State.Damage, _source, point, Vector3.up);
                var result = _receiver.ApplyDamage(in info);
                if (result.WasApplied) _applied?.Invoke(_collider, point, result);
                if (result.WasLethal) { End(); return; }
            }
            if (State.Remaining <= 0 || !Alive()) End();
        }
        bool Alive() => _source != null && _target != null && _target.gameObject.activeInHierarchy &&
            (_sourceHealth == null || !_sourceHealth.IsDead) && (_targetHealth == null || !_targetHealth.IsDead);
        protected void OnDisable() => End();
        void End()
        {
            if (_ended) return;
            _ended = true;
            if (_visual != null) { _visual.SetActive(false); Destroy(_visual); }
            Destroy(this);
        }
    }
}
