using System;
using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(EnemyMotor))]
    public sealed class EnemyController : MonoBehaviour
    {
        private HealthComponent _healthComponent;
        private EnemyMotor _enemyMotor;

        private EnemyDefinition _definition;
        private Transform _target;
        private EnemyRegistry _registry;

        private bool _isInitialized;
        private bool _isDead;
        private bool _isRegistered;

        public event Action<
            EnemyController,
            DamageInfo,
            DamageResult> Died;

        public EnemyDefinition Definition =>
            _definition;

        public Transform Target =>
            _target;

        public HealthComponent Health =>
            _healthComponent;

        public bool IsInitialized =>
            _isInitialized;

        public bool IsDead =>
            _isDead;

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            EnsureReferences();

            _healthComponent.Died += HandleDied;

            if (_isInitialized && !_isDead)
            {
                Register();
                _enemyMotor.Resume();
            }
        }

        private void OnDisable()
        {
            if (_healthComponent != null)
            {
                _healthComponent.Died -= HandleDied;
            }

            if (_enemyMotor != null)
            {
                _enemyMotor.Stop();
            }

            Unregister();
        }

        public void Initialize(
            EnemyDefinition definition,
            Transform target,
            EnemyRegistry registry)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            if (target == null)
            {
                throw new ArgumentNullException(
                    nameof(target));
            }

            if (registry == null)
            {
                throw new ArgumentNullException(
                    nameof(registry));
            }

            EnsureReferences();
            Unregister();

            _definition = definition;
            _target = target;
            _registry = registry;

            _isDead = false;

            _healthComponent.Initialize(
                definition.MaximumHealth);

            _enemyMotor.Initialize(
                definition,
                target);

            _isInitialized = true;

            Register();
        }

        private void HandleDied(
            DamageInfo damageInfo,
            DamageResult damageResult)
        {
            if (!_isInitialized || _isDead)
            {
                return;
            }

            _isDead = true;

            _enemyMotor.Stop();
            Unregister();

            Died?.Invoke(
                this,
                damageInfo,
                damageResult);
        }

        private void Register()
        {
            if (!isActiveAndEnabled ||
                !_isInitialized ||
                _isDead ||
                _registry == null ||
                _isRegistered)
            {
                return;
            }

            _isRegistered =
                _registry.Register(this);
        }

        private void Unregister()
        {
            if (!_isRegistered)
            {
                return;
            }

            if (_registry != null)
            {
                _registry.Unregister(this);
            }

            _isRegistered = false;
        }

        private void EnsureReferences()
        {
            if (_healthComponent == null)
            {
                _healthComponent =
                    GetComponent<HealthComponent>();
            }

            if (_enemyMotor == null)
            {
                _enemyMotor =
                    GetComponent<EnemyMotor>();
            }
        }
    }
}