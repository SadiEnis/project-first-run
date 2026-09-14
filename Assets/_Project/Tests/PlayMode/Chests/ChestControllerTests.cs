#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Items;
using ProjectFirstRun.Player;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.Rewards.Claims;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Upgrades;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Chests
{
    public sealed class ChestControllerTests
    {
        private GameObject _playerObject;
        private GameObject _buildObject;
        private GameObject _claimObject;
        private GameObject _uiObject;
        private GameObject _chestObject;
        private GameObject _worldPrefab;

        private PlayerController _playerController;
        private PlayerDeathController _deathController;
        private HealthComponent _healthComponent;
        private PlayerBuildController _buildController;
        private PlayerRewardClaimController _claimController;
        private RewardSelectionController _selectionController;
        private RewardSelectionView _selectionView;
        private RewardChoiceView _choiceView;
        private ChestController _chestController;

        private RewardItemPool _rewardItemPool;
        private ChestDefinition _chestDefinition;
        private UpgradeDefinition _rewardDefinition;
        private UpgradeDefinition _secondRewardDefinition;
        private TestRewardClaimHandler _claimHandler;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Time.timeScale = 1f;

            CreatePlayer();

            yield return null;

            _healthComponent.Initialize(
                100f);

            _playerController.SetControlEnabled(
                true);

            CreateBuild();
            CreateRewardContent();
            CreateClaimController();
            CreateSelectionUI();
            CreateChest();
        }

        [TearDown]
        public void TearDown()
        {
            DestroyImmediateIfExists(
                _chestObject);

            DestroyImmediateIfExists(
                _uiObject);

            DestroyImmediateIfExists(
                _claimObject);

            DestroyImmediateIfExists(
                _buildObject);

            DestroyImmediateIfExists(
                _playerObject);

            DestroyImmediateIfExists(
                _worldPrefab);

            if (_chestDefinition != null)
            {
                Object.DestroyImmediate(
                    _chestDefinition);
            }

            if (_rewardItemPool != null)
            {
                Object.DestroyImmediate(
                    _rewardItemPool);
            }

            if (_rewardDefinition != null)
            {
                Object.DestroyImmediate(
                    _rewardDefinition);
            }

            if (_secondRewardDefinition != null)
            {
                Object.DestroyImmediate(
                    _secondRewardDefinition);
            }

            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        [Test]
        public void TryOpen_WithEligibleReward_OpensExactSession()
        {
            ChestOpenResult result =
                _chestController.TryOpen();

            Assert.That(
                result,
                Is.EqualTo(
                    ChestOpenResult.SelectionOpened));

            Assert.That(
                _chestController.Status,
                Is.EqualTo(ChestStatus.Selecting));

            Assert.That(
                _chestController.ActiveSession,
                Is.SameAs(
                    _selectionController.ActiveSession));

            Assert.That(
                _chestController.ActiveSession.Offer.Choices[0],
                Is.SameAs(_rewardDefinition));

            Assert.That(
                _selectionView.IsVisible,
                Is.True);
        }

        [Test]
        public void PoolChangedAfterInitialization_CannotLeakAnotherCategoryIntoAnOffer()
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            try
            {
                SetPrivateField(weapon, typeof(ItemDefinition), "_stableId", "weapon.cross-category-test");
                SetPrivateField(_chestDefinition, typeof(ChestDefinition), "_rewardCategory", ChestRewardCategory.Upgrade);
                SetPrivateField(_rewardItemPool, typeof(RewardItemPool), "_items", new List<ItemDefinition> { weapon });
                Assert.Throws<System.InvalidOperationException>(() => _chestController.TryOpen());
                Assert.That(_chestController.Status, Is.EqualTo(ChestStatus.Available));
                Assert.That(_selectionController.IsOpen, Is.False);
                Assert.That(_chestController.ActiveSession, Is.Null);
            }
            finally { Object.DestroyImmediate(weapon); }
        }

        [Test]
        public void SuccessfulSelection_OpensChestExactlyOnce()
        {
            int openedCount = 0;
            ChestController openedChest = null;

            _chestController.ChestOpened +=
                chest =>
                {
                    openedCount++;
                    openedChest = chest;
                };

            _chestController.TryOpen();

            _choiceView.Button.onClick.Invoke();

            Assert.That(
                _chestController.Status,
                Is.EqualTo(ChestStatus.Opened));

            Assert.That(
                _chestController.ActiveSession,
                Is.Null);

            Assert.That(
                openedCount,
                Is.EqualTo(1));

            Assert.That(
                openedChest,
                Is.SameAs(_chestController));

            Assert.That(
                _chestController.TryOpen(),
                Is.EqualTo(
                    ChestOpenResult.AlreadyOpened));

            Assert.That(
                openedCount,
                Is.EqualTo(1));
        }

        [Test]
        public void TryOpen_WhileSelecting_ReusesActiveSession()
        {
            _chestController.TryOpen();

            RewardClaimSession activeSession =
                _chestController.ActiveSession;

            ChestOpenResult result =
                _chestController.TryOpen();

            Assert.That(
                result,
                Is.EqualTo(
                    ChestOpenResult.AlreadySelecting));

            Assert.That(
                _chestController.ActiveSession,
                Is.SameAs(activeSession));
        }

        [Test]
        public void MultiClaimChest_RemainsSelectingUntilFinalClaim()
        {
            _secondRewardDefinition =
                ScriptableObject.CreateInstance<UpgradeDefinition>();

            SetPrivateField(
                _secondRewardDefinition,
                typeof(ItemDefinition),
                "_stableId",
                "upgrade.chest-controller-test-second");

            SetPrivateField(
                _secondRewardDefinition,
                typeof(ItemDefinition),
                "_displayName",
                "Second Chest Controller Test Upgrade");

            SetPrivateField(
                _rewardItemPool,
                typeof(RewardItemPool),
                "_items",
                new List<ItemDefinition>
                {
                    _rewardDefinition,
                    _secondRewardDefinition
                });

            SetPrivateField(
                _chestDefinition,
                typeof(ChestDefinition),
                "_requestedChoiceCount",
                2);

            SetPrivateField(
                _chestDefinition,
                typeof(ChestDefinition),
                "_maxSelections",
                2);

            _claimHandler.AddSupportedDefinition(
                _secondRewardDefinition);

            Assert.That(
                _chestController.TryOpen(),
                Is.EqualTo(ChestOpenResult.SelectionOpened));

            Assert.That(
                _chestController.ActiveSession.SelectionsRemaining,
                Is.EqualTo(2));

            Assert.That(
                _selectionController.Select(_rewardDefinition),
                Is.EqualTo(RewardClaimResult.Claimed));

            Assert.That(_chestController.Status, Is.EqualTo(ChestStatus.Selecting));
            Assert.That(_selectionController.IsOpen, Is.True);
            Assert.That(_chestController.ActiveSession.SelectionsRemaining, Is.EqualTo(1));

            Assert.That(
                _selectionController.Select(_secondRewardDefinition),
                Is.EqualTo(RewardClaimResult.Claimed));

            Assert.That(_chestController.Status, Is.EqualTo(ChestStatus.Opened));
            Assert.That(_selectionController.IsOpen, Is.False);
            Assert.That(_chestController.ActiveSession, Is.Null);
        }

        [Test]
        public void TryOpen_WithNoEligibleRewards_LeavesChestAvailable()
        {
            _buildController.TryAdd(
                _rewardDefinition);

            ChestOpenResult result =
                _chestController.TryOpen();

            Assert.That(
                result,
                Is.EqualTo(
                    ChestOpenResult.NoEligibleRewards));

            Assert.That(
                _chestController.Status,
                Is.EqualTo(ChestStatus.Available));

            Assert.That(
                _chestController.ActiveSession,
                Is.Null);

            Assert.That(
                _selectionController.IsOpen,
                Is.False);
        }

        [Test]
        public void TryOpen_WhileAnotherSelectionIsOpen_ReturnsBusy()
        {
            RewardClaimSession externalSession =
                new RewardClaimSession(
                    new RewardOffer(
                        new ItemDefinition[]
                        {
                            _rewardDefinition
                        }));

            _selectionController.Open(
                externalSession);

            ChestOpenResult result =
                _chestController.TryOpen();

            Assert.That(
                result,
                Is.EqualTo(
                    ChestOpenResult.SelectionBusy));

            Assert.That(
                _chestController.Status,
                Is.EqualTo(ChestStatus.Available));

            Assert.That(
                _chestController.ActiveSession,
                Is.Null);
        }

        [Test]
        public void RecoverableClaimFailure_KeepsChestSelecting()
        {
            _claimHandler.Result =
                RewardClaimResult.AlreadyOwned;

            _chestController.TryOpen();

            RewardClaimResult result =
                _selectionController.Select(
                    _rewardDefinition);

            Assert.That(
                result,
                Is.EqualTo(
                    RewardClaimResult.AlreadyOwned));

            Assert.That(
                _chestController.Status,
                Is.EqualTo(ChestStatus.Selecting));

            Assert.That(
                _chestController.ActiveSession.IsClaimed,
                Is.False);

            Assert.That(
                _selectionController.IsOpen,
                Is.True);
        }

        [Test]
        public void TryOpen_WhenSelectionControllerDisabled_RollsBackState()
        {
            _selectionController.enabled =
                false;

            Assert.That(
                _chestController.TryOpen,
                Throws.InvalidOperationException);

            Assert.That(
                _chestController.Status,
                Is.EqualTo(ChestStatus.Available));

            Assert.That(
                _chestController.ActiveSession,
                Is.Null);
        }

        private void CreatePlayer()
        {
            _playerObject =
                Object.Instantiate(
                    FindPlayerPrefab());

            _playerObject.name =
                "ChestController_Player";

            _playerController =
                GetRequiredPlayerComponent<
                    PlayerController>();

            _deathController =
                GetRequiredPlayerComponent<
                    PlayerDeathController>();

            _healthComponent =
                GetRequiredPlayerComponent<
                    HealthComponent>();

            DisableCursorLockForTests();
        }

        private void CreateBuild()
        {
            _buildObject =
                new GameObject(
                    "ChestController_Build");

            _buildController =
                _buildObject.AddComponent<
                    PlayerBuildController>();

            _buildController.Initialize(
                PlayerBuildCapacity.CreateDefault());
        }

        private void CreateRewardContent()
        {
            _rewardDefinition =
                ScriptableObject.CreateInstance<
                    UpgradeDefinition>();

            SetPrivateField(
                _rewardDefinition,
                typeof(ItemDefinition),
                "_stableId",
                "upgrade.chest-controller-test");

            SetPrivateField(
                _rewardDefinition,
                typeof(ItemDefinition),
                "_displayName",
                "Chest Controller Test Upgrade");

            _rewardItemPool =
                ScriptableObject.CreateInstance<
                    RewardItemPool>();

            SetPrivateField(
                _rewardItemPool,
                typeof(RewardItemPool),
                "_items",
                new List<ItemDefinition>
                {
                    _rewardDefinition
                });

            _worldPrefab =
                new GameObject(
                    "ChestController_WorldPrefab");

            _worldPrefab.AddComponent<
                ChestController>();

            _chestDefinition =
                ScriptableObject.CreateInstance<
                    ChestDefinition>();

            SetPrivateField(
                _chestDefinition,
                typeof(ChestDefinition),
                "_stableId",
                "chest.controller-test");

            SetPrivateField(
                _chestDefinition,
                typeof(ChestDefinition),
                "_rewardItemPool",
                _rewardItemPool);

            SetPrivateField(
                _chestDefinition,
                typeof(ChestDefinition),
                "_requestedChoiceCount",
                1);

            SetPrivateField(
                _chestDefinition,
                typeof(ChestDefinition),
                "_worldPrefab",
                _worldPrefab);
        }

        private void CreateClaimController()
        {
            _claimObject =
                new GameObject(
                    "ChestController_Claim");

            _claimObject.SetActive(false);

            _claimController =
                _claimObject.AddComponent<
                    PlayerRewardClaimController>();

            _claimHandler =
                new TestRewardClaimHandler(
                    _rewardDefinition);

            RewardClaimHandlerRegistry registry =
                new RewardClaimHandlerRegistry();

            registry.Register(
                _claimHandler);

            _claimController.Initialize(
                registry);
        }

        private void CreateSelectionUI()
        {
            _uiObject =
                new GameObject(
                    "ChestController_UI");

            _uiObject.SetActive(false);

            _selectionView =
                _uiObject.AddComponent<
                    RewardSelectionView>();

            _selectionController =
                _uiObject.AddComponent<
                    RewardSelectionController>();

            GameObject modalRoot =
                CreateChild(
                    _uiObject.transform,
                    "Modal Root");

            _choiceView =
                CreateChoiceView(
                    modalRoot.transform);

            RewardChoiceView secondChoiceView =
                CreateChoiceView(
                    modalRoot.transform);

            Text header =
                CreateText(
                    modalRoot.transform,
                    "Header");

            Text remaining =
                CreateText(
                    modalRoot.transform,
                    "Remaining");

            Text feedback =
                CreateText(
                    modalRoot.transform,
                    "Feedback");

            modalRoot.SetActive(false);

            _selectionView.Initialize(
                modalRoot,
                header,
                remaining,
                feedback,
                new[]
                {
                    _choiceView,
                    secondChoiceView
                });

            _selectionController.Initialize(
                _selectionView,
                _claimController,
                _playerController,
                _deathController);

            _uiObject.SetActive(true);
        }

        private void CreateChest()
        {
            _chestObject =
                new GameObject(
                    "ChestController_Chest");

            _chestController =
                _chestObject.AddComponent<
                    ChestController>();

            _chestController.Initialize(
                _chestDefinition,
                _buildController,
                _selectionController,
                new RewardOfferGenerator(
                    new RewardCandidateFilter(),
                    new FirstRandomSource()));
        }

        private RewardChoiceView CreateChoiceView(
            Transform parent)
        {
            GameObject root =
                CreateChild(
                    parent,
                    "Choice",
                    typeof(RectTransform),
                    typeof(Button));

            RewardChoiceView choiceView =
                root.AddComponent<
                    RewardChoiceView>();

            choiceView.Initialize(
                root.GetComponent<Button>(),
                CreateText(root.transform, "Name"),
                CreateText(root.transform, "Category"),
                CreateText(root.transform, "Effect"),
                CreateText(root.transform, "State"));

            return choiceView;
        }

        private static Text CreateText(
            Transform parent,
            string name)
        {
            GameObject root =
                CreateChild(
                    parent,
                    name,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Text));

            return root.GetComponent<Text>();
        }

        private static GameObject CreateChild(
            Transform parent,
            string name,
            params System.Type[] components)
        {
            GameObject child =
                new GameObject(
                    name,
                    components);

            child.transform.SetParent(
                parent,
                false);

            return child;
        }

        private T GetRequiredPlayerComponent<T>()
            where T : Component
        {
            T component =
                _playerObject.GetComponent<T>();

            Assert.That(
                component,
                Is.Not.Null,
                $"Player prefab requires {typeof(T).Name}.");

            return component;
        }

        private void DisableCursorLockForTests()
        {
            SerializedObject serializedController =
                new SerializedObject(
                    _playerController);

            SerializedProperty lockCursorProperty =
                serializedController.FindProperty(
                    "_lockCursorWhileControlled");

            Assert.That(
                lockCursorProperty,
                Is.Not.Null);

            lockCursorProperty.boolValue = false;

            serializedController
                .ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject FindPlayerPrefab()
        {
            string[] prefabGuids =
                AssetDatabase.FindAssets(
                    "Player t:Prefab");

            foreach (string prefabGuid in prefabGuids)
            {
                string assetPath =
                    AssetDatabase.GUIDToAssetPath(
                        prefabGuid);

                GameObject prefab =
                    AssetDatabase.LoadAssetAtPath<
                        GameObject>(
                            assetPath);

                if (prefab != null &&
                    prefab.name == "Player" &&
                    prefab.GetComponent<
                        PlayerDeathController>() != null)
                {
                    return prefab;
                }
            }

            Assert.Fail(
                "Player prefab with PlayerDeathController was not found.");

            return null;
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

        private static void DestroyImmediateIfExists(
            GameObject gameObject)
        {
            if (gameObject != null)
            {
                Object.DestroyImmediate(
                    gameObject);
            }
        }

        private sealed class FirstRandomSource :
            IRandomSource
        {
            public int Next(
                int minInclusive,
                int maxExclusive)
            {
                return minInclusive;
            }
        }

        private sealed class TestRewardClaimHandler :
            IRewardClaimHandler
        {
            private readonly ItemDefinition
                _supportedDefinition;
            private readonly List<ItemDefinition>
                _additionalSupportedDefinitions =
                new List<ItemDefinition>();

            public RewardClaimResult Result
            {
                get;
                set;
            } = RewardClaimResult.Claimed;

            public TestRewardClaimHandler(
                ItemDefinition supportedDefinition)
            {
                _supportedDefinition =
                    supportedDefinition;
            }

            public bool Supports(
                ItemDefinition definition)
            {
                return ReferenceEquals(definition, _supportedDefinition) ||
                       _additionalSupportedDefinitions.Contains(definition);
            }

            public void AddSupportedDefinition(ItemDefinition definition)
            {
                _additionalSupportedDefinitions.Add(definition);
            }

            public RewardClaimResult TryClaim(
                ItemDefinition definition)
            {
                return Result;
            }
        }
    }
}

#endif
