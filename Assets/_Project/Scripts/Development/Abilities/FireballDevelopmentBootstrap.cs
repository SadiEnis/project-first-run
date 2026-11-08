using System;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.Fireball;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;

namespace ProjectFirstRun.Development.Abilities
{
    /// <summary>Legacy fixture adapter: optional Fireball grant, shared runtime installation.</summary>
    [DisallowMultipleComponent]
    public sealed class FireballDevelopmentBootstrap : MonoBehaviour
    {
        [SerializeField] private FireballDefinition _fireballDefinition;
        [SerializeField] private bool _grantStartingAbility = true;
        [SerializeField] private PlayerAbilityAcquisitionController _playerAbilityAcquisitionController;
        [SerializeField] private PlayerStatsController _playerStatsController;
        [SerializeField] private GameObject _damageSource;
        [SerializeField] private EnemyRegistry _enemyRegistry;
        private PlayerAbilityRuntimeInstaller _installer;
        private bool _started;
        public bool IsInstalled { get; private set; }

        private void Start()
        {
            _started = true;
            Begin();
        }

        private void OnEnable()
        {
            if (_started && !IsInstalled) Begin();
        }

        private void OnDisable()
        {
            if (_installer != null) _installer.CancelPendingInstallation();
        }

        private void OnDestroy()
        {
            if (_installer != null)
            {
                _installer.Installed -= CompleteInstallation;
                _installer.CancelPendingInstallation();
            }
        }

        private void Begin()
        {
            if (IsInstalled) return;
            EnsureInstaller();
            _installer.Begin();
        }

        public void Install()
        {
            if (IsInstalled)
                throw new InvalidOperationException("Development Fireball has already been installed.");
            EnsureInstaller();
            _installer.Install();
        }

        private void EnsureInstaller()
        {
            if (_grantStartingAbility &&
                (_fireballDefinition == null || _fireballDefinition.ProjectilePrefab == null))
                throw new InvalidOperationException("Starting Fireball requires a definition and projectile prefab.");
            if (_installer != null) return;
            if (GetComponent<PlayerAbilityRuntimeInstaller>() != null)
                throw new InvalidOperationException("Do not combine the legacy adapter with another ability installer.");
            _installer = gameObject.AddComponent<PlayerAbilityRuntimeInstaller>();
            // The adapter owns scheduling; the helper must not start independently.
            _installer.enabled = false;
            _installer.Configure(_playerAbilityAcquisitionController, _playerStatsController,
                _damageSource, _enemyRegistry);
            _installer.Installed += CompleteInstallation;
        }

        private void CompleteInstallation()
        {
            if (_grantStartingAbility)
            {
                var result = _playerAbilityAcquisitionController.TryAcquire(_fireballDefinition);
                if (result != AbilityAcquireResult.Acquired)
                    throw new InvalidOperationException($"Development Fireball could not be acquired: {result}.");
            }
            IsInstalled = true;
        }
    }
}
