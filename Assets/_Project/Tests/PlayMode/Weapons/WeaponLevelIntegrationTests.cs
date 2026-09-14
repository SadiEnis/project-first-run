#if UNITY_EDITOR
using System;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
using ProjectFirstRun.Stats;
using ProjectFirstRun.Upgrades;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Weapons
{
    public sealed class WeaponLevelIntegrationTests
    {
        private GameObject _player;
        private WeaponDefinition _definition;
        private WeaponDefinition _second;
        private PlayerWeaponAcquisitionController _acquisition;
        private PlayerWeaponController _controller;
        private PlayerBuild _build;

        [SetUp]
        public void SetUp()
        {
            _player = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Player/Player.prefab"));
            _player.GetComponent<PlayerStartingLoadoutInitializer>().Initialize(
                new PlayerBuildCapacity(2, 3, 5));
            _acquisition = _player.GetComponent<PlayerWeaponAcquisitionController>();
            _controller = _player.GetComponent<PlayerWeaponController>();
            _build = _player.GetComponent<PlayerBuildController>().Build;
            _definition = _controller.ActiveDefinition;
            _second = Object.Instantiate(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(
                "Assets/_Project/Data/Items/Weapons/WD_DevelopmentSecondaryWeapon.asset"));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_player);
            Object.DestroyImmediate(_second);
        }

        [Test]
        public void ActiveWeapon_LevelsAndPreservesAmmoTimersAndRuntimeIdentity()
        {
            var entry = _controller.ActiveEntry;
            var state = entry.RuntimeState;
            state.TryFire();
            state.TryStartReload();
            state.Tick(0.1f);
            float reload = state.ReloadTimeRemaining;
            float fire = state.FireCooldownRemaining;
            int acquired = 0;
            _acquisition.WeaponAcquired += _ => acquired++;
            for (int level = 2; level <= 8; level++)
            {
                Assert.That(_acquisition.TryLevelUp(_definition), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
                Assert.That(_build.GetLevel(ItemCategory.Weapon, _definition.StableId), Is.EqualTo(level));
                Assert.That(entry.Level, Is.EqualTo(level));
            }
            Assert.That(_controller.ActiveEntry, Is.SameAs(entry));
            Assert.That(entry.RuntimeState, Is.SameAs(state));
            Assert.That(state.MagazineAmmo, Is.EqualTo(11));
            Assert.That(state.ReserveAmmo, Is.EqualTo(48));
            Assert.That(state.MagazineCapacity, Is.EqualTo(20));
            Assert.That(state.ReloadTimeRemaining, Is.EqualTo(reload));
            Assert.That(state.FireCooldownRemaining, Is.EqualTo(fire));
            Assert.That(_controller.CurrentDamage, Is.EqualTo(40f));
            Assert.That(_build.Weapons.Count, Is.EqualTo(1));
            Assert.That(acquired, Is.Zero);
            Assert.That(_acquisition.TryLevelUp(_definition), Is.EqualTo(ItemLevelUpResult.MaximumLevelReached));
            state.Tick(reload);
            Assert.That(state.MagazineAmmo, Is.EqualTo(20));
            Assert.That(state.ReserveAmmo, Is.EqualTo(39));
            state.TryFire();
            Assert.That(state.FireCooldownRemaining, Is.EqualTo(1f / 6f).Within(0.0001f));
            state.TryStartReload();
            Assert.That(state.ReloadTimeRemaining, Is.EqualTo(1.2f));
        }

        [Test]
        public void InactiveWeapon_UpgradeDoesNotEquipAndSurvivesSwitching()
        {
            _acquisition.TryAcquire(_second);
            var entry = _acquisition.Loadout.GetEntry(_second);
            entry.RuntimeState.TryFire();
            Assert.That(_acquisition.TryLevelUp(_second), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
            Assert.That(_controller.ActiveDefinition, Is.SameAs(_definition));
            Assert.That(_controller.CurrentDamage, Is.EqualTo(25f));
            var switcher = _player.GetComponent<PlayerWeaponSwitcher>();
            Assert.That(switcher.TrySwitchNext(), Is.True);
            Assert.That(_controller.ActiveEntry, Is.SameAs(entry));
            Assert.That(_controller.CurrentDamage, Is.EqualTo(48f));
            Assert.That(_controller.MagazineAmmo, Is.EqualTo(5));
            Assert.That(_acquisition.TryAcquire(_second), Is.EqualTo(WeaponAcquireResult.AlreadyOwned));
            Assert.That(entry.Level, Is.EqualTo(2));
            switcher.TrySwitchNext();
            switcher.TrySwitchNext();
            Assert.That(_controller.ActiveEntry.Level, Is.EqualTo(2));
        }

        [Test]
        public void AssetMutationAfterAcquisition_DoesNotChangeSnapshottedProgression()
        {
            _acquisition.TryAcquire(_second);
            Set(_second, typeof(ItemDefinition), "_maximumLevel", 1);
            Set(_second, typeof(WeaponDefinition), "_additionalLevels", Array.Empty<WeaponLevelData>());
            Set(_second, typeof(WeaponDefinition), "_baseDamage", 999f);
            Assert.That(_acquisition.TryLevelUp(_second), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
            Assert.That(_acquisition.Loadout.GetEntry(_second).BaseDamage, Is.EqualTo(48f));
            Assert.That(_acquisition.Loadout.GetEntry(_second).MaximumLevel, Is.EqualTo(8));
        }

        [Test]
        public void MissingAndMismatchedOwnership_DoNotChangeRuntime()
        {
            Assert.That(_acquisition.TryLevelUp(_second), Is.EqualTo(ItemLevelUpResult.NotOwned));
            _build.TryAdd(ItemCategory.Weapon, _second.StableId, 8);
            Assert.Throws<InvalidOperationException>(() => _acquisition.TryLevelUp(_second));
            _build.TryLevelUp(ItemCategory.Weapon, _definition.StableId);
            Assert.Throws<InvalidOperationException>(() => _acquisition.TryLevelUp(_definition));
            Assert.That(_controller.ActiveEntry.Level, Is.EqualTo(1));
            Assert.That(_controller.CurrentDamage, Is.EqualTo(25f));
        }

        [Test]
        public void DifferentDefinitionWithSameIdentity_IsRejectedWithoutLeveling()
        {
            var alias = Object.Instantiate(_definition);
            try
            {
                Assert.Throws<InvalidOperationException>(() => _acquisition.TryLevelUp(alias));
                Assert.That(_build.GetLevel(ItemCategory.Weapon, _definition.StableId), Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(alias); }
        }

        [Test]
        public void DamageUpgrade_AppliesOnTopOfCurrentWeaponLevelWithoutStackingLevelBonuses()
        {
            _acquisition.TryLevelUp(_definition);
            var upgrade = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(
                "Assets/_Project/Data/Upgrade/Dev/UD_DevelopmentDamageBoost.asset");
            _player.GetComponent<PlayerUpgradeController>().TryAcquire(upgrade);
            Assert.That(_controller.CurrentDamage, Is.EqualTo(36f).Within(0.001f));
            _acquisition.TryLevelUp(_definition); // capacity only
            Assert.That(_controller.CurrentDamage, Is.EqualTo(36f).Within(0.001f));
            _acquisition.TryLevelUp(_definition); // rate only
            _acquisition.TryLevelUp(_definition); // 35 base damage
            Assert.That(_controller.CurrentDamage, Is.EqualTo(42f).Within(0.001f));
        }

        [Test]
        public void InvalidRuntimeTransition_FailsBeforeBuildLevelChanges()
        {
            _controller.ActiveEntry.RuntimeState.ApplyConfiguration(new WeaponRuntimeConfig(100, 48, 5f, 1.5f));
            Assert.Throws<InvalidOperationException>(() => _acquisition.TryLevelUp(_definition));
            Assert.That(_build.GetLevel(ItemCategory.Weapon, _definition.StableId), Is.EqualTo(1));
            Assert.That(_controller.ActiveEntry.Level, Is.EqualTo(1));
            Assert.That(_controller.CurrentDamage, Is.EqualTo(25f));
        }

        [Test]
        public void MalformedFutureLevel_PreventsAcquisitionBeforeOwnershipChanges()
        {
            Set(_second, typeof(WeaponDefinition), "_additionalLevels", Array.Empty<WeaponLevelData>());
            Assert.Throws<InvalidOperationException>(() => _acquisition.TryAcquire(_second));
            Assert.That(_build.Weapons.Count, Is.EqualTo(1));
            Assert.That(_acquisition.Loadout.WeaponCount, Is.EqualTo(1));
        }

        private static void Set(object target, Type type, string field, object value) =>
            type.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
#endif
