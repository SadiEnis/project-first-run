#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Development.Chests;
using ProjectFirstRun.Development.Upgrades;
using ProjectFirstRun.Development.Weapons;
using ProjectFirstRun.Development.Abilities;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Items;
using ProjectFirstRun.Rewards;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Chests
{
    public sealed class CommonChestContentTests
    {
        [TestCase("Weapon", ChestRewardCategory.Weapon, ItemCategory.Weapon, 5)]
        [TestCase("Ability", ChestRewardCategory.Ability, ItemCategory.Ability, 1)]
        [TestCase("Upgrade", ChestRewardCategory.Upgrade, ItemCategory.Upgrade, 1)]
        public void CommonContent_HasMatchingPoolAndSingleClaimChoiceCount(string name,
            ChestRewardCategory category, ItemCategory itemCategory, int count)
        {
            ChestDefinition definition = Load(name);
            Assert.DoesNotThrow(() => definition.Validate());
            Assert.That(definition.RewardCategory, Is.EqualTo(category));
            Assert.That(definition.Rarity, Is.EqualTo(ChestRarity.Common));
            Assert.That(definition.DisplayName, Is.EqualTo(name + " Chest"));
            Assert.That(definition.RequestedChoiceCount, Is.EqualTo(3));
            Assert.That(definition.RewardItemPool.ItemCount, Is.EqualTo(count));
            Assert.That(definition.RewardItemPool.GetValidatedItems().All(item => item.Category == itemCategory), Is.True);
            Assert.That(definition.WorldPrefab.GetComponent<ChestView>(), Is.Not.Null);
        }

        [TestCase(ChestRewardCategory.Weapon, "Weapon", true)]
        [TestCase(ChestRewardCategory.Weapon, "Ability", false)]
        [TestCase(ChestRewardCategory.Weapon, "Upgrade", false)]
        [TestCase(ChestRewardCategory.Ability, "Weapon", false)]
        [TestCase(ChestRewardCategory.Ability, "Ability", true)]
        [TestCase(ChestRewardCategory.Ability, "Upgrade", false)]
        [TestCase(ChestRewardCategory.Upgrade, "Weapon", false)]
        [TestCase(ChestRewardCategory.Upgrade, "Ability", false)]
        [TestCase(ChestRewardCategory.Upgrade, "Upgrade", true)]
        [TestCase(ChestRewardCategory.Mixed, "Weapon", true)]
        [TestCase(ChestRewardCategory.Mixed, "Ability", true)]
        [TestCase(ChestRewardCategory.Mixed, "Upgrade", true)]
        public void CategoryBoundary_RejectsCrossCategoryPoolItems(ChestRewardCategory category, string itemSource, bool allowed)
        {
            var definition = Object.Instantiate(Load("Weapon"));
            var pool = ScriptableObject.CreateInstance<RewardItemPool>();
            try
            {
                ItemDefinition item = Load(itemSource).RewardItemPool.GetValidatedItems()[0];
                SetField(pool, "_items", new List<ItemDefinition> { item });
                SetField(definition, "_rewardItemPool", pool);
                SetField(definition, "_rewardCategory", category);
                Assert.That(definition.AllowsCategory(item.Category), Is.EqualTo(allowed));
                if (allowed) Assert.DoesNotThrow(() => definition.Validate());
                else Assert.Throws<InvalidOperationException>(() => definition.Validate());
            }
            finally { Object.DestroyImmediate(definition); Object.DestroyImmediate(pool); }
        }

        [TestCase(-1)]
        [TestCase(4)]
        public void UnknownSerializedCategory_IsRejected(int value)
        {
            var definition = Object.Instantiate(Load("Weapon"));
            try
            {
                SetField(definition, "_rewardCategory", (ChestRewardCategory)value);
                Assert.Throws<InvalidOperationException>(() => definition.Validate());
            }
            finally { Object.DestroyImmediate(definition); }
        }

        [Test]
        public void LegacyDevelopmentChest_RemainsMixedWithStableIdDisplayFallback()
        {
            var definition = AssetDatabase.LoadAssetAtPath<ChestDefinition>("Assets/_Project/Data/Chests/Dev/CD_DevelopmentChest.asset");
            Assert.That(definition.RewardCategory, Is.EqualTo(ChestRewardCategory.Mixed));
            Assert.That(definition.DisplayName, Is.EqualTo(definition.StableId));
            Assert.DoesNotThrow(() => definition.Validate());
        }

        [TestCase("LevelUp")]
        [TestCase("Normal")]
        [TestCase("Elite")]
        public void SourceTables_ContainAllThreeCommonDefinitionsWithEqualWeights(string name)
        {
            var table = AssetDatabase.LoadAssetAtPath<ChestDropTable>($"Assets/_Project/Data/Chests/Dev/CDT_Development{name}.asset");
            Assert.That(table.Validate(), Is.EqualTo(3));
            Assert.That(table.Entries.Select(entry => entry.Definition), Is.EqualTo(new[] { Load("Weapon"), Load("Ability"), Load("Upgrade") }));
            Assert.That(table.Entries.All(entry => entry.Weight == 1), Is.True);
        }

        [Test]
        public void TestWaves_ShowcasesThreeTypesWithoutAutomaticallyGrantingTheUpgrade()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Tests/Test_Waves.unity", OpenSceneMode.Additive);
            try
            {
                var bootstrap = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<ChestDevelopmentBootstrap>(true)).Single();
                var serialized = new SerializedObject(bootstrap);
                Assert.That(serialized.FindProperty("_chestDefinition").objectReferenceValue, Is.EqualTo(Load("Weapon")));
                var extra = serialized.FindProperty("_additionalChestDefinitions");
                Assert.That(extra.arraySize, Is.EqualTo(6));
                Assert.That(extra.GetArrayElementAtIndex(0).objectReferenceValue, Is.EqualTo(Load("Ability")));
                Assert.That(extra.GetArrayElementAtIndex(1).objectReferenceValue, Is.EqualTo(Load("Upgrade")));
                Assert.That(extra.GetArrayElementAtIndex(2).objectReferenceValue, Is.EqualTo(LoadAdvanced("Green")));
                Assert.That(extra.GetArrayElementAtIndex(3).objectReferenceValue, Is.EqualTo(LoadAdvanced("Purple")));
                Assert.That(extra.GetArrayElementAtIndex(4).objectReferenceValue, Is.EqualTo(LoadAdvanced("Legendary")));
                Assert.That(extra.GetArrayElementAtIndex(5).objectReferenceValue, Is.EqualTo(LoadAdvanced("Boss")));
                var upgrade = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<UpgradeDevelopmentBootstrap>(true)).Single();
                Assert.That(upgrade.enabled, Is.False);
                var switching = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<WeaponSwitchingDevelopmentBootstrap>(true)).Single();
                Assert.That(switching.isActiveAndEnabled, Is.True, "The showcase requires the scene's two-slot starting loadout.");
                Assert.That(switching.gameObject, Is.SameAs(
                    ((ProjectFirstRun.Builds.PlayerBuildController)serialized.FindProperty("_playerBuildController").objectReferenceValue).gameObject));
                var fireball = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<FireballDevelopmentBootstrap>(true)).Single();
                Assert.That(fireball.isActiveAndEnabled, Is.True, "Fireball runtime factories must still be installed.");
                var fireballData = new SerializedObject(fireball);
                Assert.That(fireballData.FindProperty("_grantStartingAbility").boolValue, Is.False);
                Assert.That(fireballData.FindProperty("_playerAbilityAcquisitionController").objectReferenceValue,
                    Is.EqualTo(switching.GetComponent<PlayerAbilityAcquisitionController>()));
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }

        private static ChestDefinition Load(string name) =>
            AssetDatabase.LoadAssetAtPath<ChestDefinition>($"Assets/_Project/Data/Chests/Dev/CD_{name}Chest.asset");

        private static ChestDefinition LoadAdvanced(string name) =>
            AssetDatabase.LoadAssetAtPath<ChestDefinition>($"Assets/_Project/Data/Chests/Dev/CD_{name}Chest.asset");
        private static void SetField(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
#endif
