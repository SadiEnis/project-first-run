#if UNITY_EDITOR
using System;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Evolutions;
using ProjectFirstRun.Items;
using ProjectFirstRun.Weapons;
using ProjectFirstRun.Upgrades;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Evolutions
{
    public sealed class EvolutionControllerTests
    {
        private GameObject _player;
        private PlayerBuildController _buildController;
        private PlayerEvolutionController _evolution;
        private WeaponDefinition _source;
        private WeaponDefinition _result;
        private EvolutionDefinition _definition;

        [SetUp]
        public void SetUp()
        {
            _player = new GameObject("Evolution player");
            _buildController = _player.AddComponent<PlayerBuildController>();
            _evolution = _player.AddComponent<PlayerEvolutionController>();
            _buildController.Initialize(new PlayerBuildCapacity(2, 1, 1));
            _source = CreateWeapon("weapon.base", 2);
            _result = CreateWeapon("weapon.evolved", 1);
            _definition = ScriptableObject.CreateInstance<EvolutionDefinition>();
            Set(_definition, "_sourceItem", _source);
            Set(_definition, "_resultItem", _result);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_player);
            Object.DestroyImmediate(_source);
            Object.DestroyImmediate(_result);
            Object.DestroyImmediate(_definition);
        }

        [Test]
        public void RequiresMaximumAndReplacesWithoutNewSlot()
        {
            _buildController.TryAdd(_source);
            Assert.That(_evolution.TryEvolve(_definition), Is.EqualTo(EvolutionResult.SourceNotAtMaximum));
            _buildController.Build.TryLevelUp(ItemCategory.Weapon, _source.StableId);
            int events = 0;
            _evolution.EvolutionApplied += _ => events++;
            Assert.That(_evolution.TryEvolve(_definition), Is.EqualTo(EvolutionResult.Evolved));
            Assert.That(_buildController.Build.GetCount(ItemCategory.Weapon), Is.EqualTo(1));
            Assert.That(_buildController.Build.Contains(ItemCategory.Weapon, _result.StableId), Is.True);
            Assert.That(events, Is.EqualTo(1));
        }

        [Test]
        public void RequirementAndExistingResultBlockWithoutMutation()
        {
            _buildController.TryAdd(_source);
            _buildController.Build.TryLevelUp(ItemCategory.Weapon, _source.StableId);
            var requirement = new EvolutionRequirement(ItemCategory.Upgrade, "upgrade.required");
            Set(_definition, "_requirements", new[] { requirement });
            Assert.That(_evolution.TryEvolve(_definition), Is.EqualTo(EvolutionResult.RequirementNotMet));
            Assert.That(_buildController.Build.Contains(ItemCategory.Weapon, _source.StableId), Is.True);
            Set(_definition, "_requirements", Array.Empty<EvolutionRequirement>());
            _buildController.TryAdd(_result);
            Assert.That(_evolution.TryEvolve(_definition), Is.EqualTo(EvolutionResult.ResultAlreadyOwned));
        }

        [Test]
        public void InvalidCategoryDefinitionThrowsBeforeBuildMutation()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            try
            {
                Set(_definition, "_resultItem", upgrade);
                Assert.Throws<InvalidOperationException>(() => _evolution.TryEvolve(_definition));
                Assert.That(_buildController.Build.GetCount(ItemCategory.Weapon), Is.Zero);
            }
            finally { Object.DestroyImmediate(upgrade); }
        }

        private static WeaponDefinition CreateWeapon(string stableId, int maximum)
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            Set(weapon, typeof(ItemDefinition), "_stableId", stableId);
            Set(weapon, typeof(ItemDefinition), "_maximumLevel", maximum);
            Set(weapon, typeof(WeaponDefinition), "_baseDamage", 10f);
            Set(weapon, typeof(WeaponDefinition), "_magazineCapacity", 1);
            Set(weapon, typeof(WeaponDefinition), "_startingReserveAmmo", 1);
            Set(weapon, typeof(WeaponDefinition), "_shotsPerSecond", 1f);
            Set(weapon, typeof(WeaponDefinition), "_reloadDuration", 1f);
            Set(weapon, typeof(WeaponDefinition), "_additionalLevels", maximum == 1 ? Array.Empty<WeaponLevelData>() : new[] { new WeaponLevelData(10f, 1, 1f, 1f) });
            return weapon;
        }

        private static void Set(object target, string field, object value) => Set(target, target.GetType(), field, value);
        private static void Set(object target, Type owner, string field, object value) => owner.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
#endif
