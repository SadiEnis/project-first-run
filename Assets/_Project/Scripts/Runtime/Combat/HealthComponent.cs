using System;
using UnityEngine;

namespace ProjectFirstRun.Combat
{
    [DisallowMultipleComponent]
    public sealed class HealthComponent : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField, Min(0.01f)]
        private float _maximumHealth = 100f;

        private HealthState _healthState;

        public event Action<float, float> HealthChanged;
        public event Action<DamageInfo, DamageResult> Damaged;
        public event Action<DamageInfo, DamageResult> Died;
        public event Action HealthReset;

        public float MaximumHealth
        {
            get
            {
                EnsureInitialized();
                return _healthState.MaximumHealth;
            }
        }

        public float CurrentHealth
        {
            get
            {
                EnsureInitialized();
                return _healthState.CurrentHealth;
            }
        }

        public bool IsDead
        {
            get
            {
                EnsureInitialized();
                return _healthState.IsDead;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Recreates the runtime health state with the supplied maximum health.
        /// Intended for initial spawn configuration and pooled-object reuse.
        /// </summary>
        public void Initialize(float maximumHealth)
        {
            // Create the new state before changing the serialized fallback.
            // If validation fails, the existing state remains untouched.
            HealthState newHealthState =
                new HealthState(maximumHealth);

            _maximumHealth = maximumHealth;
            _healthState = newHealthState;
        }

        public DamageResult ApplyDamage(
            in DamageInfo damageInfo)
        {
            EnsureInitialized();

            DamageResult result =
                _healthState.ApplyDamage(damageInfo.Amount);

            if (!result.WasApplied)
            {
                return result;
            }

            Damaged?.Invoke(damageInfo, result);

            HealthChanged?.Invoke(
                _healthState.CurrentHealth,
                _healthState.MaximumHealth);

            if (result.WasLethal)
            {
                Died?.Invoke(damageInfo, result);
            }

            return result;
        }

        public void ResetHealth()
        {
            EnsureInitialized();

            _healthState.ResetToMaximum();

            HealthChanged?.Invoke(
                _healthState.CurrentHealth,
                _healthState.MaximumHealth);

            HealthReset?.Invoke();
        }

        private void EnsureInitialized()
        {
            if (_healthState != null)
            {
                return;
            }

            _healthState =
                new HealthState(_maximumHealth);
        }

        private void OnValidate()
        {
            if (float.IsNaN(_maximumHealth) ||
                float.IsInfinity(_maximumHealth))
            {
                _maximumHealth = 100f;
                return;
            }

            _maximumHealth = Mathf.Max(
                0.01f,
                _maximumHealth);
        }
    }
}