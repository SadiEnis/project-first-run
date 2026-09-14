#if UNITY_EDITOR
using System;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Items;
using ProjectFirstRun.Stats;
using ProjectFirstRun.Upgrades;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Upgrades
{
    public sealed class UpgradeLevelEffectsTests
    {
        private GameObject _player;
        private UpgradeDefinition _definition;
        private PlayerUpgradeController _controller;
        private PlayerBuild _build;
        private PlayerStatCollection _stats;

        [SetUp]
        public void SetUp()
        {
            _player = new GameObject("Upgrade level test");
            var build = _player.AddComponent<PlayerBuildController>();
            _stats = _player.AddComponent<PlayerStatsController>().Stats;
            _controller = _player.AddComponent<PlayerUpgradeController>();
            build.Initialize(new PlayerBuildCapacity(1, 1, 1));
            _build = build.Build;
            _definition = Object.Instantiate(AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(
                "Assets/_Project/Data/Upgrade/Dev/UD_DevelopmentDamageBoost.asset"));
        }

        [TearDown]
        public void TearDown() { Object.DestroyImmediate(_player); Object.DestroyImmediate(_definition); }

        [Test]
        public void LevelsReplaceEffectsWithoutStackingOrAdditionalSlots()
        {
            int acquisitions = 0;
            _controller.UpgradeAcquired += _ => acquisitions++;
            Assert.That(_controller.TryAcquire(_definition), Is.EqualTo(UpgradeAcquireResult.Acquired));
            int changes = 0;
            _stats.Changed += () =>
            {
                changes++;
                Assert.That(_stats.ModifierCount, Is.EqualTo(2));
                Assert.That(_build.GetLevel(ItemCategory.Upgrade, _definition.StableId), Is.EqualTo(changes + 1));
            };
            for (int level = 2; level <= 3; level++)
            {
                Assert.That(_controller.TryLevelUp(_definition), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
                Assert.That(_stats.Evaluate(PlayerStatType.WeaponDamage, 100), Is.EqualTo(110 + level * 10).Within(.001));
                Assert.That(_stats.Evaluate(PlayerStatType.AbilityDamage, 100), Is.EqualTo(110 + level * 10).Within(.001));
            }
            Assert.That(_controller.TryLevelUp(_definition), Is.EqualTo(ItemLevelUpResult.MaximumLevelReached));
            Assert.That(_controller.TryAcquire(_definition), Is.EqualTo(UpgradeAcquireResult.AlreadyOwned));
            Assert.That(changes, Is.EqualTo(2));
            Assert.That(acquisitions, Is.EqualTo(1));
        }

        [Test]
        public void SameSourceUnrelatedModifierSurvivesAndSnapshotIgnoresAssetEdits()
        {
            _controller.TryAcquire(_definition);
            var other = new StatModifier(PlayerStatType.WeaponDamage, StatModifierOperation.Flat, 10, _definition.StableId);
            _stats.Add(other);
            Set(_definition, typeof(ItemDefinition), "_maximumLevel", 1);
            Set(_definition, typeof(UpgradeDefinition), "_additionalLevels", Array.Empty<UpgradeLevelData>());
            Assert.That(_controller.TryLevelUp(_definition), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
            Assert.That(_stats.Contains(other), Is.True);
            Assert.That(_stats.Evaluate(PlayerStatType.WeaponDamage, 100), Is.EqualTo(143).Within(.001));
        }

        [Test]
        public void MissingRuntimeAndRemovedEffectsFailBeforeLevelMutation()
        {
            Assert.That(_controller.TryLevelUp(_definition), Is.EqualTo(ItemLevelUpResult.NotOwned));
            _controller.TryAcquire(_definition);
            _stats.Remove(_stats.Modifiers[0]);
            Assert.Throws<InvalidOperationException>(() => _controller.TryLevelUp(_definition));
            Assert.That(_build.GetLevel(ItemCategory.Upgrade, _definition.StableId), Is.EqualTo(1));
        }

        [Test]
        public void DefinitionAliasAndBuildMismatchAreRejected()
        {
            _controller.TryAcquire(_definition);
            var alias = Object.Instantiate(_definition);
            try { Assert.Throws<InvalidOperationException>(() => _controller.TryLevelUp(alias)); }
            finally { Object.DestroyImmediate(alias); }
            _build.TryLevelUp(ItemCategory.Upgrade, _definition.StableId);
            Assert.Throws<InvalidOperationException>(() => _controller.TryLevelUp(_definition));
            Assert.That(_stats.Evaluate(PlayerStatType.WeaponDamage, 100), Is.EqualTo(120).Within(.001));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void MalformedFutureLevelsDoNotAcquire(int malformed)
        {
            Set(_definition, typeof(UpgradeDefinition), "_additionalLevels", malformed == 0 ? null :
                malformed == 1 ? Array.Empty<UpgradeLevelData>() : new UpgradeLevelData[2]);
            Assert.Throws<InvalidOperationException>(() => _controller.TryAcquire(_definition));
            Assert.That(_build.GetLevel(ItemCategory.Upgrade, _definition.StableId), Is.Zero);
            Assert.That(_stats.ModifierCount, Is.Zero);
        }

        [Test]
        public void ThrowingSubscriberSeesCommittedStateAndDoesNotCausePartialEffects()
        {
            _controller.TryAcquire(_definition);
            Action subscriber = () => throw new InvalidOperationException("subscriber");
            _stats.Changed += subscriber;
            Assert.Throws<InvalidOperationException>(() => _controller.TryLevelUp(_definition));
            _stats.Changed -= subscriber;
            Assert.That(_build.GetLevel(ItemCategory.Upgrade, _definition.StableId), Is.EqualTo(2));
            Assert.That(_stats.ModifierCount, Is.EqualTo(2));
            Assert.That(_stats.Evaluate(PlayerStatType.WeaponDamage, 100), Is.EqualTo(130).Within(.001));
            Assert.That(_controller.TryLevelUp(_definition), Is.EqualTo(ItemLevelUpResult.LevelIncreased));
        }

        private static void Set(object target, Type owner, string field, object value) =>
            owner.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
#endif
