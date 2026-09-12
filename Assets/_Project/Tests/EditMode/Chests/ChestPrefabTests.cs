#if UNITY_EDITOR

using NUnit.Framework;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Rewards;
using UnityEditor;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Chests
{
    public sealed class ChestPrefabTests
    {
        private const string PrefabPath =
            "Assets/_Project/Prefabs/Chests/RewardChest.prefab";

        private const string DefinitionPath =
            "Assets/_Project/Data/Chests/Dev/CD_DevelopmentChest.asset";

        private const string RewardPoolPath =
            "Assets/_Project/Data/Reward/Dev/RP_DevelopmentRewardPool.asset";

        [Test]
        public void Prefab_ContainsInteractivePresentation()
        {
            GameObject prefab =
                LoadPrefab();

            ChestController controller =
                prefab.GetComponent<ChestController>();

            ChestView view =
                prefab.GetComponent<ChestView>();

            BoxCollider interactionCollider =
                prefab.GetComponent<BoxCollider>();

            Assert.That(controller, Is.Not.Null);
            Assert.That(view, Is.Not.Null);
            Assert.That(interactionCollider, Is.Not.Null);
            Assert.That(interactionCollider.enabled, Is.True);
            Assert.That(view.VisualRoot, Is.Not.Null);
            Assert.That(view.VisualRoot.activeSelf, Is.True);
            Assert.That(view.InteractionCollider, Is.SameAs(interactionCollider));
            Assert.That(view.VisualRoot.transform.IsChildOf(prefab.transform), Is.True);
        }

        [Test]
        public void DevelopmentDefinition_UsesPrefabAndRewardPool()
        {
            ChestDefinition definition =
                AssetDatabase.LoadAssetAtPath<ChestDefinition>(
                    DefinitionPath);

            RewardItemPool rewardPool =
                AssetDatabase.LoadAssetAtPath<RewardItemPool>(
                    RewardPoolPath);

            Assert.That(definition, Is.Not.Null);
            Assert.That(rewardPool, Is.Not.Null);
            Assert.That(definition.StableId, Is.EqualTo("chest.development"));
            Assert.That(definition.RequestedChoiceCount, Is.EqualTo(3));
            Assert.That(definition.RewardItemPool, Is.SameAs(rewardPool));
            Assert.That(definition.WorldPrefab, Is.SameAs(LoadPrefab()));
            Assert.That(() => definition.Validate(), Throws.Nothing);
        }

        private static GameObject LoadPrefab()
        {
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    PrefabPath);

            Assert.That(
                prefab,
                Is.Not.Null,
                $"Reward chest prefab is required at {PrefabPath}.");

            return prefab;
        }
    }
}

#endif
