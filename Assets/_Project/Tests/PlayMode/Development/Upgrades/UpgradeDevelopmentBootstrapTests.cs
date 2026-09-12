#if UNITY_EDITOR

using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Development.Upgrades;
using ProjectFirstRun.Items;
using ProjectFirstRun.Stats;
using ProjectFirstRun.Upgrades;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Development.Upgrades
{
    public sealed class UpgradeDevelopmentBootstrapTests
    {
        private GameObject _playerObject;
        private GameObject _bootstrapObject;

        private PlayerBuildController _buildController;
        private PlayerStatsController _statsController;
        private PlayerUpgradeController _upgradeController;

        private UpgradeDevelopmentBootstrap _bootstrap;
        private UpgradeDefinition _definition;

        [SetUp]
        public void SetUp()
        {
            _playerObject =
                new GameObject(
                    "DevelopmentUpgrade_Player");

            _buildController =
                _playerObject
                    .AddComponent<PlayerBuildController>();

            _statsController =
                _playerObject
                    .AddComponent<PlayerStatsController>();

            _upgradeController =
                _playerObject
                    .AddComponent<PlayerUpgradeController>();

            _buildController.Initialize(
                PlayerBuildCapacity.CreateDefault());

            _definition =
                CreateUpgrade();

            _bootstrapObject =
                new GameObject(
                    "DevelopmentUpgrade_Bootstrap");

            _bootstrap =
                _bootstrapObject
                    .AddComponent<
                        UpgradeDevelopmentBootstrap>();

            _bootstrap.enabled = false;

            SetPrivateField(
                _bootstrap,
                typeof(UpgradeDevelopmentBootstrap),
                "_upgradeDefinition",
                _definition);

            SetPrivateField(
                _bootstrap,
                typeof(UpgradeDevelopmentBootstrap),
                "_playerUpgradeController",
                _upgradeController);
        }

        [TearDown]
        public void TearDown()
        {
            if (_bootstrapObject != null)
            {
                Object.DestroyImmediate(
                    _bootstrapObject);
            }

            if (_playerObject != null)
            {
                Object.DestroyImmediate(
                    _playerObject);
            }

            if (_definition != null)
            {
                Object.DestroyImmediate(
                    _definition);
            }
        }

        [Test]
        public void Install_AcquiresUpgrade()
        {
            _bootstrap.Install();

            Assert.That(
                _bootstrap.IsInstalled,
                Is.True);

            Assert.That(
                _buildController.Contains(
                    _definition),
                Is.True);
        }

        [Test]
        public void Install_AppliesDevelopmentDamageModifiers()
        {
            _bootstrap.Install();

            float weaponDamage =
                _statsController.Evaluate(
                    PlayerStatType.WeaponDamage,
                    100f);

            float abilityDamage =
                _statsController.Evaluate(
                    PlayerStatType.AbilityDamage,
                    100f);

            Assert.That(
                weaponDamage,
                Is.EqualTo(120f)
                    .Within(0.0001f));

            Assert.That(
                abilityDamage,
                Is.EqualTo(120f)
                    .Within(0.0001f));
        }

        [Test]
        public void Install_CalledTwice_ThrowsWithoutStacking()
        {
            _bootstrap.Install();

            Assert.That(
                () => _bootstrap.Install(),
                Throws.InvalidOperationException);

            Assert.That(
                _statsController
                    .Stats
                    .ModifierCount,
                Is.EqualTo(2));

            Assert.That(
                _buildController
                    .Build
                    .GetCount(
                        ItemCategory.Upgrade),
                Is.EqualTo(1));
        }

        [Test]
        public void Install_WhenUpgradeAlreadyOwned_ThrowsWithoutStacking()
        {
            UpgradeAcquireResult firstResult =
                _upgradeController.TryAcquire(
                    _definition);

            Assert.That(
                firstResult,
                Is.EqualTo(
                    UpgradeAcquireResult.Acquired));

            Assert.That(
                () => _bootstrap.Install(),
                Throws.InvalidOperationException);

            Assert.That(
                _bootstrap.IsInstalled,
                Is.False);

            Assert.That(
                _statsController
                    .Stats
                    .ModifierCount,
                Is.EqualTo(2));
        }

        private static UpgradeDefinition CreateUpgrade()
        {
            UpgradeDefinition definition =
                ScriptableObject
                    .CreateInstance<UpgradeDefinition>();

            SetPrivateField(
                definition,
                typeof(ItemDefinition),
                "_stableId",
                "upgrade.development_damage_boost");

            SetPrivateField(
                definition,
                typeof(UpgradeDefinition),
                "_statModifiers",
                new List<UpgradeStatModifierData>
                {
                    new UpgradeStatModifierData(
                        PlayerStatType.WeaponDamage,
                        StatModifierOperation.AdditivePercent,
                        0.20f),

                    new UpgradeStatModifierData(
                        PlayerStatType.AbilityDamage,
                        StatModifierOperation.AdditivePercent,
                        0.20f)
                });

            return definition;
        }

        private static void SetPrivateField(
            object target,
            System.Type declaringType,
            string fieldName,
            object value)
        {
            FieldInfo field =
                declaringType.GetField(
                    fieldName,
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null);

            field.SetValue(
                target,
                value);
        }
    }
}

#endif