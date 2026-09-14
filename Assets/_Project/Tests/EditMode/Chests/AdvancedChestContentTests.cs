#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using UnityEditor;

namespace ProjectFirstRun.Tests.EditMode.Chests
{
    public sealed class AdvancedChestContentTests
    {
        private static readonly string[] AdvancedNames =
        {
            "Green",
            "Purple",
            "Legendary",
            "Boss"
        };

        [TestCase("Green", ChestRarity.Uncommon, 3, 1)]
        [TestCase("Purple", ChestRarity.Rare, 4, 1)]
        [TestCase("Legendary", ChestRarity.Legendary, 5, 2)]
        [TestCase("Boss", ChestRarity.Legendary, 6, 2)]
        public void AdvancedDefinition_HasExplicitMixedRewardPolicy(
            string name,
            ChestRarity rarity,
            int requestedChoiceCount,
            int maxSelections)
        {
            ChestDefinition definition = Load(name);

            Assert.That(definition, Is.Not.Null);
            Assert.DoesNotThrow(() => definition.Validate());
            Assert.That(definition.Rarity, Is.EqualTo(rarity));
            Assert.That(definition.RewardCategory, Is.EqualTo(ChestRewardCategory.Mixed));
            Assert.That(definition.RequestedChoiceCount, Is.EqualTo(requestedChoiceCount));
            Assert.That(definition.MaxSelections, Is.EqualTo(maxSelections));
            Assert.That(definition.RewardItemPool.GetValidatedItems().Count, Is.EqualTo(4));
            Assert.That(definition.DisplayName, Is.EqualTo(name + " Chest"));
        }

        [Test]
        public void AdvancedDefinitions_AreUniqueAndDoNotReplaceCommonDefinitions()
        {
            IReadOnlyList<ChestDefinition> definitions =
                AdvancedNames.Select(Load).ToArray();

            Assert.That(definitions.Select(item => item.StableId).Distinct().Count(), Is.EqualTo(4));
            Assert.That(Load("Green").StableId, Is.Not.EqualTo("chest.common.weapon"));
            Assert.That(Load("Boss").Rarity, Is.EqualTo(ChestRarity.Legendary));
        }

        [Test]
        public void AdvancedDevelopmentTable_ContainsAllFourDefinitionsWithProvisionalEqualWeights()
        {
            ChestDropTable table = AssetDatabase.LoadAssetAtPath<ChestDropTable>(
                "Assets/_Project/Data/Chests/Dev/CDT_DevelopmentAdvanced.asset");

            Assert.That(table, Is.Not.Null);
            Assert.That(table.Validate(), Is.EqualTo(4));
            Assert.That(
                table.Entries.Select(entry => entry.Definition),
                Is.EqualTo(AdvancedNames.Select(Load).ToArray()));
            Assert.That(table.Entries.All(entry => entry.Weight == 1), Is.True);
        }

        private static ChestDefinition Load(string name) =>
            AssetDatabase.LoadAssetAtPath<ChestDefinition>(
                $"Assets/_Project/Data/Chests/Dev/CD_{name}Chest.asset");
    }
}
#endif
