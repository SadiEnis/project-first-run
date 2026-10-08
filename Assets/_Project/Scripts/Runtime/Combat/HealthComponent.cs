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
        private readonly System.Collections.Generic.HashSet<object> _damageBlocks = new();
        public void SetDamageBlocked(object owner, bool blocked)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (blocked) _damageBlocks.Add(owner);
            else _damageBlocks.Remove(owner);
        }
        public float BaseMaximumHealth => _maximumHealth;
        public float DamageReduction { get; private set; }
        public float IncomingDamageMultiplier { get; private set; } = 1f;

        public void SetIncomingDamageMultiplier(float value)
        {
            if (!float.IsFinite(value) || value <= 0f) throw new ArgumentOutOfRangeException(nameof(value));
            IncomingDamageMultiplier = value;
        }

        public void SetDamageReduction(float value)
        {
            if (!float.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
            DamageReduction = Mathf.Clamp(value, 0f, .75f);
        }

        public void SetMaximumHealth(float value)
        {
            EnsureInitialized();
            if (_healthState.SetMaximumHealth(value))
                HealthChanged?.Invoke(CurrentHealth, MaximumHealth);
        }

        public float Heal(float amount)
        {
            EnsureInitialized();
            float restored = _healthState.Heal(amount);
            if (restored > 0f) HealthChanged?.Invoke(CurrentHealth, MaximumHealth);
            return restored;
        }

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
            DamageReduction = 0f;
            IncomingDamageMultiplier = 1f;
        }

        public DamageResult ApplyDamage(
            in DamageInfo damageInfo)
        {
            EnsureInitialized();
            if (_damageBlocks.Count > 0) return DamageResult.Rejected(damageInfo.Amount, CurrentHealth);

            DamageResult result =
                _healthState.ApplyDamage(damageInfo.Amount, DamageReduction, IncomingDamageMultiplier);

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
