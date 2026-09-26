using System;
using System.Collections;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.Fireball;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;

namespace ProjectFirstRun.Development.Abilities
{
    [DisallowMultipleComponent]
    public sealed class FireballDevelopmentBootstrap :
        MonoBehaviour
    {
        [Header("Content")]
        [SerializeField]
        private FireballDefinition _fireballDefinition;

        [SerializeField]
        private bool _grantStartingAbility = true;

        [Header("Player")]
        [SerializeField]
        private PlayerAbilityAcquisitionController
            _playerAbilityAcquisitionController;

        [SerializeField]
        private PlayerStatsController _playerStatsController;

        [SerializeField]
        private GameObject _damageSource;

        [Header("World")]
        [SerializeField]
        private EnemyRegistry _enemyRegistry;

        public bool IsInstalled
        {
            get;
            private set;
        }

        private IEnumerator Start()
        {
            /*
             * PlayerStartingLoadoutInitializer initializes
             * PlayerBuild during its Start.
             *
             * Development Fireball installation happens one
             * frame later so we do not depend on Start order.
             */
            yield return null;

            Install();
        }

        public void Install()
        {
            if (IsInstalled)
            {
                throw new InvalidOperationException(
                    "Development Fireball has already been installed.");
            }

            ValidateDependencies();

            if (!_playerStatsController.IsInitialized)
            {
                throw new InvalidOperationException(
                    "PlayerStatsController must be initialized " +
                    "before installing the development Fireball.");
            }

            if (_playerAbilityAcquisitionController.IsInitialized)
            {
                throw new InvalidOperationException(
                    "PlayerAbilityAcquisitionController has already " +
                    "been initialized by another composition source.");
            }

            FireballAbilityRuntimeFactory fireballFactory =
                new FireballAbilityRuntimeFactory(
                    _enemyRegistry,
                    _damageSource,
                    _playerStatsController.Stats);

            AbilityRuntimeFactoryRegistry factoryRegistry =
                new AbilityRuntimeFactoryRegistry();

            factoryRegistry.Register(
                fireballFactory);
            // This legacy bootstrap owns the development ability registry for saved fixtures.
            factoryRegistry.Register(new ProjectFirstRun.Abilities.ForceWave.ForceWaveRuntimeFactory(
                _enemyRegistry, _damageSource, _playerStatsController.Stats));

            _playerAbilityAcquisitionController.Initialize(
                factoryRegistry);

            if (_grantStartingAbility)
            {
                AbilityAcquireResult acquireResult =
                    _playerAbilityAcquisitionController.TryAcquire(
                        _fireballDefinition);

                if (acquireResult != AbilityAcquireResult.Acquired)
                {
                    throw new InvalidOperationException(
                        $"Development Fireball could not be acquired. " +
                        $"Result: {acquireResult}.");
                }
            }

            IsInstalled = true;
        }

        private void ValidateDependencies()
        {
            if (_fireballDefinition == null)
            {
                throw new InvalidOperationException(
                    "Fireball definition is required.");
            }

            if (_fireballDefinition.ProjectilePrefab == null)
            {
                throw new InvalidOperationException(
                    "Fireball definition requires a projectile prefab.");
            }

            if (_playerAbilityAcquisitionController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerAbilityAcquisitionController)} " +
                    "is required.");
            }

            if (_playerStatsController == null)
            {
                throw new InvalidOperationException(
                    "PlayerStatsController is required.");
            }

            if (_damageSource == null)
            {
                throw new InvalidOperationException(
                    "Damage source is required.");
            }

            if (_enemyRegistry == null)
            {
                throw new InvalidOperationException(
                    "EnemyRegistry is required.");
            }
        }
    }
}
