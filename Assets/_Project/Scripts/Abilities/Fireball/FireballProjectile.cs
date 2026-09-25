using System;
using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Abilities.Fireball
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class FireballProjectile :
        MonoBehaviour
    {
        private Collider _collider;
        private Rigidbody _rigidbody;

        private GameObject _damageSource;

        private Vector3 _direction;
        private float _damage;
        private float _speed;
        private float _lifetimeRemaining;

        private bool _isInitialized;
        private bool _hasResolvedHit;
        private float _radius, _remainingRange;
        private int _mask;
        private HealthComponent _sourceHealth;
        private Collider _launchBlocker;
        public bool IsResolved => _hasResolvedHit;
        public void SetLaunchBlocker(Collider blocker) => _launchBlocker = blocker;

        public bool IsInitialized =>
            _isInitialized;

        public float Damage =>
            _damage;

        public float Speed =>
            _speed;

        public float LifetimeRemaining =>
            _lifetimeRemaining;

        public Vector3 Direction =>
            _direction;

        private void Awake()
        {
            _collider =
                GetComponent<Collider>();

            _rigidbody =
                GetComponent<Rigidbody>();

            ConfigurePhysics();
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }

        public void Advance(float deltaTime)
        {
            if (!float.IsFinite(deltaTime) || deltaTime < 0) throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (!_isInitialized ||
                _hasResolvedHit)
            {
                return;
            }

            if (_damageSource == null || (_sourceHealth != null && _sourceHealth.IsDead)) { ResolveHit(); return; }
            if (deltaTime == 0) return;
            if (_launchBlocker != null) { Hit(_launchBlocker); return; }
            float elapsed = Mathf.Min(deltaTime, _lifetimeRemaining);
            float step = Mathf.Min(_remainingRange, _speed * elapsed);
            if (Sweep(transform.position, _radius, _direction, step, _mask, _damageSource, out var collider, out float travel))
            {
                transform.position += _direction * travel; Hit(collider); return;
            }
            transform.position += _direction * step;
            _lifetimeRemaining = Mathf.Max(0, _lifetimeRemaining - elapsed);
            _remainingRange = Mathf.Max(0, _remainingRange - step);
            if (_lifetimeRemaining <= 0 || _remainingRange <= 0) ResolveHit();
        }

        public void Initialize(
            Vector3 direction,
            float damage,
            float speed,
            float lifetime,
            GameObject damageSource, float collisionRadius = .12f, float range = 60, int collisionMask = 247)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(FireballProjectile)} " +
                    "has already been initialized.");
            }

            ValidateDirection(
                direction);
            ValidatePositive(collisionRadius, nameof(collisionRadius)); ValidatePositive(range, nameof(range));

            ValidatePositive(
                damage,
                nameof(damage));

            ValidatePositive(
                speed,
                nameof(speed));

            ValidatePositive(
                lifetime,
                nameof(lifetime));

            if (damageSource == null)
            {
                throw new ArgumentNullException(
                    nameof(damageSource));
            }

            _direction =
                direction.normalized;

            _damage =
                damage;

            _speed =
                speed;

            _lifetimeRemaining =
                lifetime;

            _damageSource =
                damageSource;
            _radius = collisionRadius; _remainingRange = range; _mask = collisionMask;
            _sourceHealth = damageSource.GetComponent<HealthComponent>();

            _isInitialized = true;
        }

        private void OnTriggerEnter(
            Collider other)
        {
            if (Ignored(other, _damageSource) || (_mask & (1 << other.gameObject.layer)) == 0 || Time.timeScale == 0) return;
            Hit(other);
        }

        private void Hit(Collider other)
        {
            if (!_isInitialized ||
                _hasResolvedHit ||
                other == null)
            {
                return;
            }

            if (BelongsToDamageSource(other))
            {
                return;
            }

            HealthComponent health =
                other.GetComponentInParent<HealthComponent>();
            if (_damageSource == null || (_sourceHealth != null && _sourceHealth.IsDead)) { ResolveHit(); return; }
            _hasResolvedHit = true;

            if (health != null &&
                !health.IsDead)
            {
                ApplyDamage(
                    health,
                    other);
            }

            ResolveHit();
        }

        private void ApplyDamage(
            HealthComponent health,
            Collider hitCollider)
        {
            Vector3 hitDirection =
                _direction.sqrMagnitude >
                Mathf.Epsilon
                    ? _direction
                    : transform.forward;

            DamageInfo damageInfo =
                new DamageInfo(
                    _damage,
                    _damageSource,
                    hitCollider.ClosestPoint(
                        transform.position),
                    hitDirection);

            health.ApplyDamage(
                in damageInfo);
        }

        private bool BelongsToDamageSource(
            Collider other)
        {
            if (_damageSource == null)
            {
                return false;
            }

            Transform sourceTransform =
                _damageSource.transform;

            Transform otherTransform =
                other.transform;

            return otherTransform == sourceTransform ||
                   otherTransform.IsChildOf(
                       sourceTransform);
        }

        private void ResolveHit()
        {
            _hasResolvedHit = true;

            if (_collider != null)
            {
                _collider.enabled = false;
            }

            Destroy(
                gameObject);
        }

        private void ConfigurePhysics()
        {
            if (_collider != null)
            {
                _collider.isTrigger = true;
                _collider.enabled = false; // Swept queries are authoritative; keep prefab compatibility.
            }

            if (_rigidbody == null)
            {
                return;
            }

            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
        }

        private static void ValidateDirection(
            Vector3 direction)
        {
            if (!IsFinite(direction.x) ||
                !IsFinite(direction.y) ||
                !IsFinite(direction.z) ||
                !float.IsFinite(direction.sqrMagnitude) ||
                direction.sqrMagnitude <= Mathf.Epsilon)
            {
                throw new ArgumentException(
                    "Projectile direction must be finite and non-zero.",
                    nameof(direction));
            }
        }

        private static void ValidatePositive(
            float value,
            string parameterName)
        {
            if (float.IsNaN(value) ||
                float.IsInfinity(value) ||
                value <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Value must be finite and greater than zero.");
            }
        }

        private static bool IsFinite(
            float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value);
        }

        private static bool Ignored(Collider collider, GameObject source) => collider == null || collider.isTrigger ||
            (source != null && collider.transform.IsChildOf(source.transform)) ||
            collider.GetComponentInParent<FireballProjectile>() != null;

        public static bool Sweep(Vector3 origin, float radius, Vector3 direction, float distance, int mask,
            GameObject source, out Collider collider, out float travel)
        {
            collider = null; travel = distance;
            foreach (var overlap in Physics.OverlapSphere(origin, radius, mask, QueryTriggerInteraction.Ignore))
            {
                if (Ignored(overlap, source)) continue;
                // World cover wins ambiguous initial overlaps with a damage receiver.
                if (collider == null || overlap.GetComponentInParent<HealthComponent>() == null)
                    collider = overlap;
                travel = 0;
            }
            if (collider != null) return true;
            if (distance <= 0) return false;
            foreach (var hit in Physics.SphereCastAll(origin, radius, direction, distance, mask, QueryTriggerInteraction.Ignore))
                if (!Ignored(hit.collider, source) && hit.distance <= travel) { collider = hit.collider; travel = hit.distance; }
            return collider != null;
        }
    }
}
