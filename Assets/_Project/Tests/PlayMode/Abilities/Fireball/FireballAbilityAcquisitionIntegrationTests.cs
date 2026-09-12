using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.Fireball;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Items;
using ProjectFirstRun.Stats;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Abilities.Fireball
{
    public sealed class
        FireballAbilityAcquisitionIntegrationTests
    {
        private GameObject _playerObject;
        private GameObject _registryObject;
        private GameObject _damageSource;

        private PlayerBuildController _buildController;
        private PlayerAbilityController _abilityController;
        private PlayerStatsController _statsController;
        private PlayerAbilityAcquisitionController
            _acquisitionController;

        private EnemyRegistry _enemyRegistry;
        private FireballDefinition _definition;

        [SetUp]
        public void SetUp()
        {
            _playerObject =
                new GameObject(
                    "FireballAcquisition_Player");

            _buildController =
                _playerObject
                    .AddComponent<PlayerBuildController>();

            _abilityController =
                _playerObject
                    .AddComponent<PlayerAbilityController>();

            _statsController =
                _playerObject
                    .AddComponent<PlayerStatsController>();

            _acquisitionController =
                _playerObject
                    .AddComponent<
                        PlayerAbilityAcquisitionController>();

            _buildController.Initialize(
                PlayerBuildCapacity.CreateDefault());

            _registryObject =
                new GameObject(
                    "FireballAcquisition_EnemyRegistry");

            _enemyRegistry =
                _registryObject
                    .AddComponent<EnemyRegistry>();

            _damageSource =
                new GameObject(
                    "FireballAcquisition_DamageSource");

            _definition =
                ScriptableObject
                    .CreateInstance<FireballDefinition>();

            SetStableId(
                _definition,
                "ability.fireball_test");

            FireballAbilityRuntimeFactory fireballFactory =
                new FireballAbilityRuntimeFactory(
                    _enemyRegistry,
                    _damageSource,
                    _statsController.Stats);

            AbilityRuntimeFactoryRegistry registry =
                new AbilityRuntimeFactoryRegistry();

            registry.Register(
                fireballFactory);

            _acquisitionController.Initialize(
                registry);
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObject != null)
            {
                Object.DestroyImmediate(
                    _playerObject);
            }

            if (_registryObject != null)
            {
                Object.DestroyImmediate(
                    _registryObject);
            }

            if (_damageSource != null)
            {
                Object.DestroyImmediate(
                    _damageSource);
            }

            if (_definition != null)
            {
                Object.DestroyImmediate(
                    _definition);
            }
        }

        [Test]
        public void TryAcquire_WithFireball_AddsOwnershipAndRuntime()
        {
            AbilityAcquireResult result =
                _acquisitionController.TryAcquire(
                    _definition);

            Assert.That(
                result,
                Is.EqualTo(
                    AbilityAcquireResult.Acquired));

            Assert.That(
                _buildController.Contains(
                    _definition),
                Is.True);

            Assert.That(
                _abilityController.AbilityCount,
                Is.EqualTo(1));

            Assert.That(
                _abilityController
                    .Entries[0]
                    .Definition,
                Is.SameAs(_definition));
        }

        [Test]
        public void AcquiredFireball_WithNoEnemies_ReturnsNoTarget()
        {
            _acquisitionController.TryAcquire(
                _definition);

            AbilityRuntimeEntry runtimeEntry =
                _abilityController.Entries[0];

            AbilityAutoCastResult result =
                runtimeEntry.TryAutoCast(
                    Vector3.zero);

            Assert.That(
                result,
                Is.EqualTo(
                    AbilityAutoCastResult.NoTarget));

            Assert.That(
                runtimeEntry.IsReady,
                Is.True);
        }

        private static void SetStableId(
            ItemDefinition definition,
            string stableId)
        {
            FieldInfo field =
                typeof(ItemDefinition)
                    .GetField(
                        "_stableId",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null);

            field.SetValue(
                definition,
                stableId);
        }
    }
}