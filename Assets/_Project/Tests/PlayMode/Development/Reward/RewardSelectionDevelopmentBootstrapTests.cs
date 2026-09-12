#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Development.Rewards;
using ProjectFirstRun.Items;
using ProjectFirstRun.Player;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.Rewards.Claims;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Upgrades;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Development.Rewards
{
    public sealed class RewardSelectionDevelopmentBootstrapTests
    {
        private const string PlayerPrefabPath =
            "Assets/_Project/Prefabs/Player/Player.prefab";

        private const string SelectionPrefabPath =
            "Assets/_Project/Prefabs/UI/RewardSelectionUI.prefab";

        private GameObject _playerObject;
        private GameObject _selectionObject;
        private GameObject _rewardObject;
        private GameObject _bootstrapObject;
        private RewardItemPool _pool;
        private UpgradeDefinition _definition;

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;

            DestroyImmediate(
                _bootstrapObject);

            DestroyImmediate(
                _rewardObject);

            DestroyImmediate(
                _selectionObject);

            DestroyImmediate(
                _playerObject);

            if (_pool != null)
            {
                Object.DestroyImmediate(
                    _pool);
            }

            if (_definition != null)
            {
                Object.DestroyImmediate(
                    _definition);
            }
        }

        [UnityTest]
        public IEnumerator Start_WhenDevelopmentOfferIsReady_OpensSelection()
        {
            CreatePlayer();

            yield return null;

            RewardDevelopmentController developmentController =
                CreateRewardDevelopmentController();

            RewardSelectionController selectionController =
                CreateRewardSelectionController();

            CreateBootstrap(
                developmentController,
                selectionController);

            _rewardObject.SetActive(true);
            _bootstrapObject.SetActive(true);

            for (int frame = 0;
                 frame < 3 && !selectionController.IsOpen;
                 frame++)
            {
                yield return null;
            }

            Assert.That(
                developmentController.HasClaimSession,
                Is.True);

            Assert.That(
                selectionController.IsOpen,
                Is.True);

            Assert.That(
                selectionController.ActiveSession,
                Is.SameAs(
                    developmentController.CurrentClaimSession));

            Assert.That(
                Time.timeScale,
                Is.EqualTo(0f));

            Assert.That(
                _playerObject
                    .GetComponent<PlayerController>()
                    .IsControlEnabled,
                Is.False);
        }

        private void CreatePlayer()
        {
            GameObject playerPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    PlayerPrefabPath);

            Assert.That(
                playerPrefab,
                Is.Not.Null);

            _playerObject =
                Object.Instantiate(
                    playerPrefab);

            PlayerController playerController =
                _playerObject.GetComponent<
                    PlayerController>();

            SerializedObject serializedPlayer =
                new SerializedObject(
                    playerController);

            serializedPlayer.FindProperty(
                    "_lockCursorWhileControlled")
                .boolValue = false;

            serializedPlayer
                .ApplyModifiedPropertiesWithoutUndo();
        }

        private RewardDevelopmentController
            CreateRewardDevelopmentController()
        {
            _pool =
                ScriptableObject.CreateInstance<
                    RewardItemPool>();

            _definition =
                ScriptableObject.CreateInstance<
                    UpgradeDefinition>();

            SetPrivateField(
                _definition,
                typeof(ItemDefinition),
                "_stableId",
                "upgrade.reward-selection-development-test");

            SetPrivateField(
                _pool,
                typeof(RewardItemPool),
                "_items",
                new List<ItemDefinition>
                {
                    _definition
                });

            _rewardObject =
                new GameObject(
                    "RewardSelectionDevelopment_Reward");

            _rewardObject.SetActive(false);

            RewardDevelopmentController controller =
                _rewardObject.AddComponent<
                    RewardDevelopmentController>();

            SetPrivateField(
                controller,
                typeof(RewardDevelopmentController),
                "_rewardItemPool",
                _pool);

            SetPrivateField(
                controller,
                typeof(RewardDevelopmentController),
                "_playerBuildController",
                _playerObject.GetComponent<
                    PlayerBuildController>());

            SetPrivateField(
                controller,
                typeof(RewardDevelopmentController),
                "_rewardClaimController",
                _playerObject.GetComponent<
                    PlayerRewardClaimController>());

            return controller;
        }

        private RewardSelectionController
            CreateRewardSelectionController()
        {
            GameObject selectionPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    SelectionPrefabPath);

            Assert.That(
                selectionPrefab,
                Is.Not.Null);

            _selectionObject =
                Object.Instantiate(
                    selectionPrefab);

            RewardSelectionController controller =
                _selectionObject.GetComponent<
                    RewardSelectionController>();

            controller.Initialize(
                _selectionObject.GetComponent<
                    RewardSelectionView>(),
                _playerObject.GetComponent<
                    PlayerRewardClaimController>(),
                _playerObject.GetComponent<
                    PlayerController>(),
                _playerObject.GetComponent<
                    PlayerDeathController>());

            return controller;
        }

        private void CreateBootstrap(
            RewardDevelopmentController developmentController,
            RewardSelectionController selectionController)
        {
            _bootstrapObject =
                new GameObject(
                    "RewardSelectionDevelopment_Bootstrap");

            _bootstrapObject.SetActive(false);

            RewardSelectionDevelopmentBootstrap bootstrap =
                _bootstrapObject.AddComponent<
                    RewardSelectionDevelopmentBootstrap>();

            bootstrap.Initialize(
                developmentController,
                selectionController);
        }

        private static void DestroyImmediate(
            GameObject target)
        {
            if (target != null)
            {
                Object.DestroyImmediate(
                    target);
            }
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
