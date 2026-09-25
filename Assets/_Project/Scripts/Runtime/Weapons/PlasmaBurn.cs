using System;
using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Weapons
{
    public sealed class PlasmaBurn : MonoBehaviour
    {
        private GameObject _source, _visual;
        private HealthComponent _sourceHealth, _targetHealth;
        private IDamageable _receiver;
        private Component _target;
        private Collider _collider;
        private Action<HitscanShotResult> _damageApplied;
        private readonly PlasmaBurnState _state = new PlasmaBurnState();
        private bool _ended;
        public PlasmaBurnState State => _state;

        public static void Apply(GameObject source, IDamageable receiver, Collider collider, float damage,
            GameObject visualPrefab, Action<HitscanShotResult> damageApplied)
        {
            if (!(receiver is Component target) || target == null || !target.gameObject.activeInHierarchy) return;
            var health = target.GetComponent<HealthComponent>();
            if (health != null && health.IsDead) return;
            PlasmaBurn burn = null;
            foreach (var candidate in target.GetComponents<PlasmaBurn>())
                if (!candidate._ended && candidate._source == source && ReferenceEquals(candidate._receiver, receiver))
                { burn = candidate; break; }
            if (burn == null)
            {
                burn = target.gameObject.AddComponent<PlasmaBurn>();
                burn._source = source; burn._sourceHealth = source.GetComponent<HealthComponent>();
                burn._receiver = receiver; burn._target = target; burn._targetHealth = health;
                if (visualPrefab != null)
                {
                    burn._visual = Instantiate(visualPrefab, target.transform);
                    burn._visual.transform.position = collider != null
                        ? new Vector3(collider.bounds.center.x, collider.bounds.max.y + .25f, collider.bounds.center.z)
                        : target.transform.position + Vector3.up;
                }
            }
            burn._collider = collider; burn._damageApplied = damageApplied;
            burn._state.Refresh(damage);
        }
        private void Update() => Advance(Time.deltaTime);
        public void Advance(float delta)
        {
            if (_ended) return;
            if (!Alive()) { End(); return; }
            int ticks = _state.Advance(delta);
            for (int i = 0; i < ticks && Alive() && !_ended; i++)
            {
                Vector3 point = _target.transform.position;
                var info = new DamageInfo(_state.Damage, _source, point, Vector3.up);
                var result = _receiver.ApplyDamage(in info);
                if (result.WasApplied)
                    _damageApplied?.Invoke(HitscanShotResult.HitDamageable(_collider, point, Vector3.up, point, result));
                if (result.WasLethal) { End(); return; }
            }
            if (_state.Remaining <= 0 || !Alive()) End();
        }
        private bool Alive() => _source != null && _target != null && _target.gameObject.activeInHierarchy &&
            (_sourceHealth == null || !_sourceHealth.IsDead) && (_targetHealth == null || !_targetHealth.IsDead);
        private void OnDisable() => End();
        private void End()
        {
            if (_ended) return;
            _ended = true;
            if (_visual != null) { _visual.SetActive(false); Destroy(_visual); }
            Destroy(this);
        }
    }
}
