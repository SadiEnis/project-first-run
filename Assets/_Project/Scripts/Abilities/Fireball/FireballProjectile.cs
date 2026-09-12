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
            if (!_isInitialized ||
                _hasResolvedHit)
            {
                return;
            }

            float deltaTime =
                Time.deltaTime;

            transform.position +=
                _direction *
                _speed *
                deltaTime;

            _lifetimeRemaining -=
                deltaTime;

            if (_lifetimeRemaining <= 0f)
            {
                Destroy(
                    gameObject);
            }
        }

        public void Initialize(
            Vector3 direction,
            float damage,
            float speed,
            float lifetime,
            GameObject damageSource)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(FireballProjectile)} " +
                    "has already been initialized.");
            }

            ValidateDirection(
                direction);

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

            _isInitialized = true;
        }

        private void OnTriggerEnter(
            Collider other)
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
    }
}