using System;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Weapons;
using UnityEngine;

namespace ProjectFirstRun.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(PlayerWeaponController))]
    [RequireComponent(typeof(PlayerAbilityController))]
    public sealed class PlayerDeathController :
        MonoBehaviour,
        IPlayerDeathSource
    {
        private HealthComponent _healthComponent;
        private PlayerController _playerController;
        private PlayerWeaponController _weaponController;
        private PlayerAbilityController _abilityController;

        private bool _isDead;
        private bool _hasStarted;

        public event Action<DamageInfo, DamageResult> PlayerDied;
        public event Action PlayerRevived;

        public bool IsDead =>
            _isDead;

        private void Awake()
        {
            _healthComponent =
                GetComponent<HealthComponent>();

            _playerController =
                GetComponent<PlayerController>();

            _weaponController =
                GetComponent<PlayerWeaponController>();

            _abilityController =
                GetComponent<PlayerAbilityController>();
        }

        private void OnEnable()
        {
            _healthComponent.Died +=
                HandleDied;

            _healthComponent.HealthReset +=
                HandleHealthReset;

            if (_hasStarted)
            {
                SynchronizeWithHealthState();
            }
        }

        private void Start()
        {
            _hasStarted = true;

            SynchronizeWithHealthState();
        }

        private void OnDisable()
        {
            if (_healthComponent == null)
            {
                return;
            }

            _healthComponent.Died -=
                HandleDied;

            _healthComponent.HealthReset -=
                HandleHealthReset;
        }

        private void HandleDied(
            DamageInfo damageInfo,
            DamageResult damageResult)
        {
            if (_isDead)
            {
                return;
            }

            ApplyDeadState();

            PlayerDied?.Invoke(
                damageInfo,
                damageResult);
        }

        private void HandleHealthReset()
        {
            if (!_isDead)
            {
                return;
            }

            ApplyAliveState();

            PlayerRevived?.Invoke();
        }

        private void SynchronizeWithHealthState()
        {
            if (_healthComponent.IsDead)
            {
                ApplyDeadState();
                return;
            }

            ApplyAliveState();
        }

        private void ApplyDeadState()
        {
            if (_isDead)
            {
                return;
            }

            _isDead = true;

            _playerController
                .SetControlEnabled(false);

            _weaponController
                .SetWeaponControlEnabled(false);

            _abilityController
                .SetAbilityControlEnabled(false);
        }

        private void ApplyAliveState()
        {
            _isDead = false;

            _playerController
                .SetControlEnabled(true);

            _weaponController
                .SetWeaponControlEnabled(true);

            _abilityController
                .SetAbilityControlEnabled(true);
        }
    }
}