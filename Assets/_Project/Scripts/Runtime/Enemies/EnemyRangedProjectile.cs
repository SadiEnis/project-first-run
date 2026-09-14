using System;
using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    public sealed class EnemyRangedProjectile : MonoBehaviour
    {
        private Vector3 _direction;
        private float _damage;
        private float _speed;
        private float _lifetime;
        private float _radius;
        private GameObject _source;
        private bool _initialized;
        private bool _resolved;
        private SphereCollider _collider;

        public bool IsInitialized => _initialized;
        public Vector3 Direction => _direction;
        public float LifetimeRemaining => _lifetime;

        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
            _collider.isTrigger = true;
        }

        private void Update()
        {
            if (!_initialized || _resolved || Time.deltaTime <= 0f) return;
            float step = _speed * Time.deltaTime;
            if (TryHit(step)) return;
            transform.position += _direction * step;
            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0f) Resolve();
        }

        public void Initialize(Vector3 direction, float damage, float speed, float lifetime,
            float radius, GameObject source)
        {
            if (_initialized) throw new InvalidOperationException("Projectile is already initialized.");
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (!Finite(direction) || direction.sqrMagnitude <= Mathf.Epsilon) throw new ArgumentException("Direction must be finite and non-zero.", nameof(direction));
            ValidatePositive(damage, nameof(damage)); ValidatePositive(speed, nameof(speed));
            ValidatePositive(lifetime, nameof(lifetime)); ValidatePositive(radius, nameof(radius));
            _direction = direction.normalized; _damage = damage; _speed = speed; _lifetime = lifetime; _radius = radius; _source = source; _collider.radius = radius; _initialized = true;
        }

        private bool TryHit(float distance)
        {
            Vector3 start = transform.position;
            RaycastHit[] hits = Physics.SphereCastAll(start, _radius, _direction, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null || IsIgnored(hit.collider)) continue;
                HealthComponent health = hit.collider.GetComponentInParent<HealthComponent>();
                if (health != null && !health.IsDead)
                {
                    DamageInfo info = new DamageInfo(_damage, _source, hit.point, _direction);
                    health.ApplyDamage(in info);
                }
                Resolve();
                return true;
            }
            return false;
        }

        private bool IsIgnored(Collider collider)
        {
            Transform hit = collider.transform;
            if (_source != null && (hit == _source.transform || hit.IsChildOf(_source.transform))) return true;
            return hit.GetComponentInParent<EnemyController>() != null || collider.isTrigger;
        }

        private void Resolve()
        {
            if (_resolved) return;
            _resolved = true;
            if (_collider != null) _collider.enabled = false;
            Destroy(gameObject);
        }

        private static void ValidatePositive(float value, string name)
        { if (!Finite(value) || value <= 0f) throw new ArgumentOutOfRangeException(name); }
        private static bool Finite(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z);
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
