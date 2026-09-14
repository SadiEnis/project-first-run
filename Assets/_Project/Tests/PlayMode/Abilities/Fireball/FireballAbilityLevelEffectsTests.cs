#if UNITY_EDITOR
using System;
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
    public sealed class FireballAbilityLevelEffectsTests
    {
        private GameObject _player;
        private GameObject _registryObject;
        private GameObject _damageSource;
        private GameObject _projectilePrefabObject;
        private FireballDefinition _definition;
        private PlayerAbilityAcquisitionController _acquisition;
        private PlayerAbilityController _abilities;

        [SetUp]
        public void SetUp()
        {
            _player = new GameObject("Fireball level player");
            var build = _player.AddComponent<PlayerBuildController>();
            _abilities = _player.AddComponent<PlayerAbilityController>();
            PlayerStatsController stats = _player.AddComponent<PlayerStatsController>();
            _acquisition = _player.AddComponent<PlayerAbilityAcquisitionController>();
            build.Initialize(new PlayerBuildCapacity(1, 1, 1));
            _registryObject = new GameObject("Fireball level registry");
            EnemyRegistry registry = _registryObject.AddComponent<EnemyRegistry>();
            _damageSource = new GameObject("Fireball level source");
            _projectilePrefabObject = new GameObject("Fireball level projectile");
            _projectilePrefabObject.AddComponent<SphereCollider>();
            _projectilePrefabObject.AddComponent<Rigidbody>();
            FireballProjectile prefab = _projectilePrefabObject.AddComponent<FireballProjectile>();
            _definition = ScriptableObject.CreateInstance<FireballDefinition>();
            Set(typeof(ItemDefinition), _definition, "_stableId", "ability.fireball.level_test");
            Set(typeof(FireballDefinition), _definition, "_projectilePrefab", prefab);
            Set(typeof(FireballDefinition), _definition, "_damage", 25f);
            Set(typeof(FireballDefinition), _definition, "_projectileSpeed", 12f);
            Set(typeof(FireballDefinition), _definition, "_projectileLifetime", 5f);
            Set(typeof(AbilityDefinition), _definition, "_cooldown", 2f);
            Set(typeof(ItemDefinition), _definition, "_maximumLevel", 3);
            Set(typeof(FireballDefinition), _definition, "_additionalLevels", new[] {
                new FireballLevelData(2f, 30f, 12f), new FireballLevelData(1f, 40f, 14f) });
            var registryRuntime = new AbilityRuntimeFactoryRegistry();
            registryRuntime.Register(new FireballAbilityRuntimeFactory(registry, _damageSource, stats.Stats));
            _acquisition.Initialize(registryRuntime);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_player);
            Object.DestroyImmediate(_registryObject);
            Object.DestroyImmediate(_damageSource);
            Object.DestroyImmediate(_projectilePrefabObject);
            Object.DestroyImmediate(_definition);
        }

        [Test]
        public void LevelsReplaceCooldownAndKeepOneRuntimeEntry()
        {
            Assert.That(_acquisition.TryAcquire(_definition), Is.EqualTo(AbilityAcquireResult.Acquired));
            AbilityRuntimeEntry entry = _abilities.Entries[0];
            Assert.That(entry.Level, Is.EqualTo(1));
            Assert.That(entry.State.Cooldown, Is.EqualTo(2f));
            Assert.That(_acquisition.TryLevelUp(_definition), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
            Assert.That(entry.Level, Is.EqualTo(2));
            Assert.That(entry.State.Cooldown, Is.EqualTo(2f));
            Assert.That(_acquisition.TryLevelUp(_definition), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
            Assert.That(entry.Level, Is.EqualTo(3));
            Assert.That(entry.State.Cooldown, Is.EqualTo(1f));
            Assert.That(_acquisition.TryLevelUp(_definition), Is.EqualTo(ItemLevelUpResult.MaximumLevelReached));
            Assert.That(_abilities.AbilityCount, Is.EqualTo(1));
        }

        [Test]
        public void AcquiredProgressionIgnoresLaterDefinitionMutation()
        {
            Assert.That(_acquisition.TryAcquire(_definition), Is.EqualTo(AbilityAcquireResult.Acquired));
            Set(typeof(ItemDefinition), _definition, "_maximumLevel", 1);
            Set(typeof(FireballDefinition), _definition, "_additionalLevels", Array.Empty<FireballLevelData>());
            Assert.That(_acquisition.TryLevelUp(_definition), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
            Assert.That(_abilities.Entries[0].Level, Is.EqualTo(2));
        }

        [Test]
        public void MissingAndAliasDefinitionsDoNotMutateRuntime()
        {
            Assert.That(_acquisition.TryLevelUp(_definition), Is.EqualTo(ItemLevelUpResult.NotOwned));
            Assert.That(_acquisition.TryAcquire(_definition), Is.EqualTo(AbilityAcquireResult.Acquired));
            FireballDefinition alias = Object.Instantiate(_definition);
            try { Assert.Throws<InvalidOperationException>(() => _acquisition.TryLevelUp(alias)); }
            finally { Object.DestroyImmediate(alias); }
            Assert.That(_abilities.Entries[0].Level, Is.EqualTo(1));
        }

        private static void Set(Type owner, object target, string field, object value) =>
            owner.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
#endif
