#if UNITY_EDITOR

using System.Collections.Generic;
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
    public sealed class PlayerUpgradeControllerTests
    {
        private readonly List<UpgradeDefinition>
            _createdDefinitions =
                new List<UpgradeDefinition>();

        private GameObject _playerObject;

        private PlayerBuildController _buildController;
        private PlayerStatsController _statsController;
        private PlayerUpgradeController _upgradeController;

        [SetUp]
        public void SetUp()
        {
            _playerObject =
                new GameObject(
                    "PlayerUpgradeController_Test");

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
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObject != null)
            {
                Object.DestroyImmediate(
                    _playerObject);
            }

            foreach (UpgradeDefinition definition
                     in _createdDefinitions)
            {
                if (definition != null)
                {
                    Object.DestroyImmediate(
                        definition);
                }
            }

            _createdDefinitions.Clear();
        }

        [Test]
        public void TryAcquire_WithValidUpgrade_ReturnsAcquired()
        {
            UpgradeDefinition definition =
                CreateUpgrade(
                    "upgrade.test",
                    new UpgradeStatModifierData(
                        PlayerStatType.WeaponDamage,
                        StatModifierOperation.Flat,
                        10f));

            UpgradeAcquireResult result =
                _upgradeController.TryAcquire(
                    definition);

            Assert.That(
                result,
                Is.EqualTo(
                    UpgradeAcquireResult.Acquired));
        }

        [Test]
        public void TryAcquire_WithValidUpgrade_AddsBuildOwnership()
        {
            UpgradeDefinition definition =
                CreateUpgrade(
                    "upgrade.test",
                    new UpgradeStatModifierData(
                        PlayerStatType.WeaponDamage,
                        StatModifierOperation.Flat,
                        10f));

            _upgradeController.TryAcquire(
                definition);

            Assert.That(
                _buildController.Contains(
                    definition),
                Is.True);

            Assert.That(
                _buildController.Build.GetCount(
                    ItemCategory.Upgrade),
                Is.EqualTo(1));
        }

        [Test]
        public void TryAcquire_WithValidUpgrade_AppliesModifier()
        {
            UpgradeDefinition definition =
                CreateUpgrade(
                    "upgrade.damage",
                    new UpgradeStatModifierData(
                        PlayerStatType.WeaponDamage,
                        StatModifierOperation.AdditivePercent,
                        0.25f));

            _upgradeController.TryAcquire(
                definition);

            float result =
                _statsController.Evaluate(
                    PlayerStatType.WeaponDamage,
                    100f);

            Assert.That(
                result,
                Is.EqualTo(125f)
                    .Within(0.0001f));

            Assert.That(
                _statsController
                    .Stats
                    .ModifierCount,
                Is.EqualTo(1));
        }

        [Test]
        public void TryAcquire_WithMultipleModifiers_AppliesAll()
        {
            UpgradeDefinition definition =
                CreateUpgrade(
                    "upgrade.hybrid_damage",
                    new UpgradeStatModifierData(
                        PlayerStatType.WeaponDamage,
                        StatModifierOperation.AdditivePercent,
                        0.20f),
                    new UpgradeStatModifierData(
                        PlayerStatType.AbilityDamage,
                        StatModifierOperation.AdditivePercent,
                        0.30f));

            _upgradeController.TryAcquire(
                definition);

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
                Is.EqualTo(130f)
                    .Within(0.0001f));

            Assert.That(
                _statsController
                    .Stats
                    .ModifierCount,
                Is.EqualTo(2));
        }

        [Test]
        public void TryAcquire_UsesUpgradeStableIdAsModifierSource()
        {
            UpgradeDefinition definition =
                CreateUpgrade(
                    "upgrade.source_test",
                    new UpgradeStatModifierData(
                        PlayerStatType.WeaponDamage,
                        StatModifierOperation.Flat,
                        10f),
                    new UpgradeStatModifierData(
                        PlayerStatType.AbilityDamage,
                        StatModifierOperation.Flat,
                        20f));

            _upgradeController.TryAcquire(
                definition);

            Assert.That(
                _statsController
                    .Stats
                    .Modifiers[0]
                    .SourceId,
                Is.EqualTo(
                    "upgrade.source_test"));

            Assert.That(
                _statsController
                    .Stats
                    .Modifiers[1]
                    .SourceId,
                Is.EqualTo(
                    "upgrade.source_test"));
        }

        [Test]
        public void TryAcquire_WhenAlreadyOwned_ReturnsAlreadyOwned()
        {
            UpgradeDefinition definition =
                CreateUpgrade(
                    "upgrade.duplicate",
                    new UpgradeStatModifierData(
                        PlayerStatType.WeaponDamage,
                        StatModifierOperation.Flat,
                        10f));

            UpgradeAcquireResult firstResult =
                _upgradeController.TryAcquire(
                    definition);

            UpgradeAcquireResult secondResult =
                _upgradeController.TryAcquire(
                    definition);

            Assert.That(
                firstResult,
                Is.EqualTo(
                    UpgradeAcquireResult.Acquired));

            Assert.That(
                secondResult,
                Is.EqualTo(
                    UpgradeAcquireResult.AlreadyOwned));
        }

        [Test]
        public void TryAcquire_WhenAlreadyOwned_DoesNotStackModifiers()
        {
            UpgradeDefinition definition =
                CreateUpgrade(
                    "upgrade.duplicate",
                    new UpgradeStatModifierData(
                        PlayerStatType.WeaponDamage,
                        StatModifierOperation.Flat,
                        10f));

            _upgradeController.TryAcquire(
                definition);

            _upgradeController.TryAcquire(
                definition);

            Assert.That(
                _statsController
                    .Stats
                    .ModifierCount,
                Is.EqualTo(1));

            float damage =
                _statsController.Evaluate(
                    PlayerStatType.WeaponDamage,
                    100f);

            Assert.That(
                damage,
                Is.EqualTo(110f));
        }

        [Test]
        public void TryAcquire_WhenCapacityReached_DoesNotApplyModifiers()
        {
            FillUpgradeCapacity();

            int modifiersBefore =
                _statsController
                    .Stats
                    .ModifierCount;

            UpgradeDefinition extraUpgrade =
                CreateUpgrade(
                    "upgrade.extra",
                    new UpgradeStatModifierData(
                        PlayerStatType.WeaponDamage,
                        StatModifierOperation.Flat,
                        999f));

            UpgradeAcquireResult result =
                _upgradeController.TryAcquire(
                    extraUpgrade);

            Assert.That(
                result,
                Is.EqualTo(
                    UpgradeAcquireResult.CapacityReached));

            Assert.That(
                _buildController.Contains(
                    extraUpgrade),
                Is.False);

            Assert.That(
                _statsController
                    .Stats
                    .ModifierCount,
                Is.EqualTo(modifiersBefore));
        }

        [Test]
        public void TryAcquire_WithNullDefinition_Throws()
        {
            Assert.That(
                () =>
                    _upgradeController
                        .TryAcquire(null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void TryAcquire_BeforeBuildInitialization_Throws()
        {
            GameObject testObject =
                new GameObject(
                    "UninitializedBuild_Test");

            PlayerBuildController buildController =
                testObject.AddComponent<
                    PlayerBuildController>();

            testObject.AddComponent<
                PlayerStatsController>();

            PlayerUpgradeController controller =
                testObject.AddComponent<
                    PlayerUpgradeController>();

            UpgradeDefinition definition =
                CreateUpgrade(
                    "upgrade.test_uninitialized",
                    new UpgradeStatModifierData(
                        PlayerStatType.WeaponDamage,
                        StatModifierOperation.Flat,
                        10f));

            Assert.That(
                buildController.IsInitialized,
                Is.False);

            Assert.That(
                () =>
                    controller.TryAcquire(
                        definition),
                Throws.InvalidOperationException);

            Object.DestroyImmediate(
                testObject);
        }

        [Test]
        public void TryAcquire_PublishesUpgradeAcquiredOnce()
        {
            UpgradeDefinition definition =
                CreateUpgrade(
                    "upgrade.event",
                    new UpgradeStatModifierData(
                        PlayerStatType.WeaponDamage,
                        StatModifierOperation.Flat,
                        10f));

            int eventCount = 0;
            UpgradeDefinition receivedDefinition = null;

            _upgradeController.UpgradeAcquired +=
                acquiredDefinition =>
                {
                    eventCount++;
                    receivedDefinition =
                        acquiredDefinition;
                };

            _upgradeController.TryAcquire(
                definition);

            _upgradeController.TryAcquire(
                definition);

            Assert.That(
                eventCount,
                Is.EqualTo(1));

            Assert.That(
                receivedDefinition,
                Is.SameAs(definition));
        }

        [Test]
        public void PlayerPrefab_ContainsPlayerUpgradeController()
        {
            GameObject playerPrefab =
                FindPlayerPrefab();

            PlayerUpgradeController controller =
                playerPrefab.GetComponent<
                    PlayerUpgradeController>();

            Assert.That(
                controller,
                Is.Not.Null,
                "The Player prefab requires " +
                $"{nameof(PlayerUpgradeController)}.");
        }

        private void FillUpgradeCapacity()
        {
            int capacity =
                _buildController
                    .Build
                    .GetCapacity(
                        ItemCategory.Upgrade);

            for (int index = 0;
                 index < capacity;
                 index++)
            {
                UpgradeDefinition definition =
                    CreateUpgrade(
                        $"upgrade.capacity_{index}");

                UpgradeAcquireResult result =
                    _upgradeController.TryAcquire(
                        definition);

                Assert.That(
                    result,
                    Is.EqualTo(
                        UpgradeAcquireResult.Acquired));
            }
        }

        private UpgradeDefinition CreateUpgrade(
            string stableId,
            params UpgradeStatModifierData[] modifiers)
        {
            UpgradeDefinition definition =
                ScriptableObject
                    .CreateInstance<UpgradeDefinition>();

            _createdDefinitions.Add(
                definition);

            SetPrivateField(
                definition,
                typeof(ItemDefinition),
                "_stableId",
                stableId);

            SetPrivateField(
                definition,
                typeof(UpgradeDefinition),
                "_statModifiers",
                new List<UpgradeStatModifierData>(
                    modifiers));

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

        private static GameObject FindPlayerPrefab()
        {
            string[] prefabGuids =
                AssetDatabase.FindAssets(
                    "Player t:Prefab");

            List<GameObject> candidates =
                new List<GameObject>();

            foreach (string prefabGuid
                     in prefabGuids)
            {
                string assetPath =
                    AssetDatabase
                        .GUIDToAssetPath(
                            prefabGuid);

                GameObject prefab =
                    AssetDatabase
                        .LoadAssetAtPath<GameObject>(
                            assetPath);

                if (prefab == null)
                {
                    continue;
                }

                if (prefab.name == "Player")
                {
                    return prefab;
                }

                if (prefab.GetComponent<
                        PlayerUpgradeController>() != null)
                {
                    candidates.Add(
                        prefab);
                }
            }

            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            Assert.Fail(
                candidates.Count == 0
                    ? "A Player prefab could not be found."
                    : "Multiple Player prefab candidates were found.");

            return null;
        }
    }
}

#endif