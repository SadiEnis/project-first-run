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
        public int SpawnVersion { get; private set; }
        public bool RequiresPerception { get; private set; }
        public EnemyPerceptionController Perception { get; private set; }

        public void ConfigurePerception(EnemyPerceptionProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            profile.Validate();
            RequiresPerception = true;
            Perception = GetComponent<EnemyPerceptionController>();
            if (Perception == null) Perception = gameObject.AddComponent<EnemyPerceptionController>();
            Perception.Configure(profile, this);
            EnsureReferences();
            _enemyMotor.Stop();
        }

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
                Perception?.Reactivate();
                Register();
                _enemyMotor.Resume();
            }
        }

        private void OnDisable()
        {
            Perception?.Suspend();
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

            definition.ValidateBehavior();
            SpawnVersion++;
            EnsureReferences();
            Unregister();

            _definition = definition;
            _target = target;
            _registry = registry;

            _isDead = false;
            Perception?.Reactivate();

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
            Perception?.Suspend();

            _enemyMotor.Stop();
            _enemyMotor.ClearStun();
            try
            {
                if (damageResult.WasLethal && damageInfo.Source != null)
                    damageInfo.Source.GetComponentInParent<ProjectFirstRun.Player.PlayerKillHealingController>()
                        ?.OnCreditedKill();
            }
            finally
            {
                // Healing observers must not prevent registry/death cleanup.
                Unregister();
                Died?.Invoke(this, damageInfo, damageResult);
            }
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
