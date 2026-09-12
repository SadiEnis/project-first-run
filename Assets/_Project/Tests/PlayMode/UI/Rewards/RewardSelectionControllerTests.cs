#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Items;
using ProjectFirstRun.Player;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.Rewards.Claims;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.UI.Rewards
{
    public sealed class RewardSelectionControllerTests
    {
        private GameObject _playerObject;
        private GameObject _claimObject;
        private GameObject _uiObject;

        private PlayerController _playerController;
        private PlayerDeathController _deathController;
        private HealthComponent _healthComponent;
        private PlayerRewardClaimController _claimController;
        private RewardSelectionController _selectionController;
        private RewardSelectionView _selectionView;
        private RewardChoiceView _choiceView;
        private Text _feedbackText;
        private WeaponDefinition _definition;
        private TestRewardClaimHandler _claimHandler;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Time.timeScale = 1f;

            _playerObject =
                Object.Instantiate(
                    FindPlayerPrefab());

            _playerObject.name =
                "RewardSelection_Player";

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

            yield return null;

            _healthComponent.Initialize(100f);
            _playerController.SetControlEnabled(true);

            _definition =
                CreateWeaponDefinition();

            _claimHandler =
                new TestRewardClaimHandler(
                    _definition);

            CreateClaimController();
            CreateSelectionUI();
        }

        [TearDown]
        public void TearDown()
        {
            if (_uiObject != null)
            {
                Object.DestroyImmediate(
                    _uiObject);
            }

            if (_claimObject != null)
            {
                Object.DestroyImmediate(
                    _claimObject);
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

            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        [Test]
        public void Open_ShowsOfferPausesTimeAndDisablesPlayerControl()
        {
            RewardClaimSession session =
                CreateSession();

            int openedCount = 0;
            RewardClaimSession openedSession = null;
            _selectionController.SelectionOpened +=
                candidate =>
                {
                    openedCount++;
                    openedSession = candidate;
                };

            _selectionController.Open(
                session);

            Assert.That(
                _selectionController.IsOpen,
                Is.True);
            Assert.That(
                _selectionController.ActiveSession,
                Is.SameAs(session));
            Assert.That(
                _selectionView.IsVisible,
                Is.True);
            Assert.That(
                _choiceView.Definition,
                Is.SameAs(_definition));
            Assert.That(
                Time.timeScale,
                Is.EqualTo(0f));
            Assert.That(
                _playerController.IsControlEnabled,
                Is.False);
            Assert.That(
                openedCount,
                Is.EqualTo(1));
            Assert.That(
                openedSession,
                Is.SameAs(session));
        }

        [Test]
        public void SuccessfulButtonSelectionClaimsExactChoiceClosesAndRestores()
        {
            Time.timeScale = 0.4f;

            RewardClaimSession session =
                CreateSession();

            int closedCount = 0;
            RewardClaimResult closedResult =
                RewardClaimResult.CapacityReached;

            _selectionController.SelectionClosed +=
                result =>
                {
                    closedCount++;
                    closedResult = result;
                };

            _selectionController.Open(
                session);

            _choiceView.Button.onClick.Invoke();

            Assert.That(
                session.IsClaimed,
                Is.True);
            Assert.That(
                session.ClaimedDefinition,
                Is.SameAs(_definition));
            Assert.That(
                _claimHandler.LastDefinition,
                Is.SameAs(_definition));
            Assert.That(
                _selectionController.IsOpen,
                Is.False);
            Assert.That(
                _selectionView.IsVisible,
                Is.False);
            Assert.That(
                Time.timeScale,
                Is.EqualTo(0.4f));
            Assert.That(
                _playerController.IsControlEnabled,
                Is.True);
            Assert.That(
                closedCount,
                Is.EqualTo(1));
            Assert.That(
                closedResult,
                Is.EqualTo(
                    RewardClaimResult.Claimed));
        }

        [TestCase(
            RewardClaimResult.AlreadyOwned,
            "already owned")]
        [TestCase(
            RewardClaimResult.CapacityReached,
            "no available slot")]
        public void RecoverableClaimFailure_KeepsModalOpenAndShowsFeedback(
            RewardClaimResult claimResult,
            string expectedFeedbackFragment)
        {
            _claimHandler.Result =
                claimResult;

            RewardClaimSession session =
                CreateSession();

            _selectionController.Open(
                session);

            RewardClaimResult result =
                _selectionController.Select(
                    _definition);

            Assert.That(
                result,
                Is.EqualTo(claimResult));
            Assert.That(
                session.IsClaimed,
                Is.False);
            Assert.That(
                _selectionController.IsOpen,
                Is.True);
            Assert.That(
                _selectionView.IsVisible,
                Is.True);
            Assert.That(
                Time.timeScale,
                Is.EqualTo(0f));
            Assert.That(
                _playerController.IsControlEnabled,
                Is.False);
            Assert.That(
                _feedbackText.text,
                Does.Contain(
                    expectedFeedbackFragment)
                    .IgnoreCase);
        }

        [Test]
        public void Open_WithClaimedSession_ThrowsWithoutChangingModalState()
        {
            RewardClaimSession session =
                CreateSession();

            session.Commit(
                _definition);

            Assert.That(
                () =>
                    _selectionController.Open(
                        session),
                Throws.InvalidOperationException);

            Assert.That(
                _selectionController.IsOpen,
                Is.False);
            Assert.That(
                _selectionView.IsVisible,
                Is.False);
            Assert.That(
                Time.timeScale,
                Is.EqualTo(1f));
            Assert.That(
                _playerController.IsControlEnabled,
                Is.True);
        }

        [Test]
        public void Open_WithEmptyOffer_ThrowsWithoutChangingModalState()
        {
            RewardClaimSession emptySession =
                new RewardClaimSession(
                    new RewardOffer(
                        new ItemDefinition[0]));

            Assert.That(
                () =>
                    _selectionController.Open(
                        emptySession),
                Throws.ArgumentException);

            Assert.That(
                _selectionController.IsOpen,
                Is.False);
            Assert.That(
                Time.timeScale,
                Is.EqualTo(1f));
            Assert.That(
                _playerController.IsControlEnabled,
                Is.True);
        }

        [Test]
        public void DisableWhileOpen_RestoresCapturedState()
        {
            Time.timeScale = 0.6f;

            _selectionController.Open(
                CreateSession());

            _selectionController.enabled =
                false;

            Assert.That(
                _selectionController.IsOpen,
                Is.False);
            Assert.That(
                _selectionView.IsVisible,
                Is.False);
            Assert.That(
                Time.timeScale,
                Is.EqualTo(0.6f));
            Assert.That(
                _playerController.IsControlEnabled,
                Is.True);
        }

        [Test]
        public void Close_RestoresPreviouslyDisabledControlAsDisabled()
        {
            _playerController.SetControlEnabled(
                false);

            RewardClaimSession session =
                CreateSession();

            _selectionController.Open(
                session);

            _selectionController.Select(
                _definition);

            Assert.That(
                _selectionController.IsOpen,
                Is.False);
            Assert.That(
                _playerController.IsControlEnabled,
                Is.False);
        }

        [Test]
        public void Close_AfterPlayerDies_DoesNotReenablePlayerControl()
        {
            RewardClaimSession session =
                CreateSession();

            _selectionController.Open(
                session);

            ApplyLethalDamage();

            session.Commit(
                _definition);

            RewardClaimResult result =
                _selectionController.Select(
                    _definition);

            Assert.That(
                result,
                Is.EqualTo(
                    RewardClaimResult.AlreadyClaimed));
            Assert.That(
                _deathController.IsDead,
                Is.True);
            Assert.That(
                _selectionController.IsOpen,
                Is.False);
            Assert.That(
                Time.timeScale,
                Is.EqualTo(1f));
            Assert.That(
                _playerController.IsControlEnabled,
                Is.False);
        }

        private void CreateClaimController()
        {
            _claimObject =
                new GameObject(
                    "RewardSelection_ClaimController");

            _claimObject.SetActive(false);

            _claimController =
                _claimObject.AddComponent<
                    PlayerRewardClaimController>();

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
                    "RewardSelection_UI");

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

            Text header =
                CreateText(
                    modalRoot.transform,
                    "Header");

            Text remaining =
                CreateText(
                    modalRoot.transform,
                    "Remaining");

            _feedbackText =
                CreateText(
                    modalRoot.transform,
                    "Feedback");

            modalRoot.SetActive(false);

            _selectionView.Initialize(
                modalRoot,
                header,
                remaining,
                _feedbackText,
                new[] { _choiceView });

            _selectionController.Initialize(
                _selectionView,
                _claimController,
                _playerController,
                _deathController);

            _uiObject.SetActive(true);
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

        private RewardClaimSession CreateSession()
        {
            return new RewardClaimSession(
                new RewardOffer(
                    new ItemDefinition[]
                    {
                        _definition
                    }));
        }

        private void ApplyLethalDamage()
        {
            DamageInfo damageInfo =
                new DamageInfo(
                    _healthComponent.MaximumHealth,
                    source: null,
                    _playerObject.transform.position,
                    Vector3.forward);

            _healthComponent.ApplyDamage(
                in damageInfo);
        }

        private WeaponDefinition CreateWeaponDefinition()
        {
            WeaponDefinition definition =
                ScriptableObject.CreateInstance<
                    WeaponDefinition>();

            SetItemField(
                definition,
                "_stableId",
                "weapon.reward-selection-test");

            SetItemField(
                definition,
                "_displayName",
                "Reward Selection Test Weapon");

            SetItemField(
                definition,
                "_gameplayEffect",
                "Tests reward selection orchestration.");

            return definition;
        }

        private static void SetItemField(
            ItemDefinition definition,
            string fieldName,
            string value)
        {
            FieldInfo field =
                typeof(ItemDefinition)
                    .GetField(
                        fieldName,
                        BindingFlags.Instance |
                        BindingFlags.NonPublic);

            Assert.That(
                field,
                Is.Not.Null);

            field.SetValue(
                definition,
                value);
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

            List<GameObject> candidates =
                new List<GameObject>();

            foreach (string prefabGuid in prefabGuids)
            {
                string assetPath =
                    AssetDatabase.GUIDToAssetPath(
                        prefabGuid);

                GameObject prefab =
                    AssetDatabase.LoadAssetAtPath<
                        GameObject>(
                            assetPath);

                if (prefab == null ||
                    prefab.GetComponent<
                        PlayerDeathController>() == null)
                {
                    continue;
                }

                if (prefab.name == "Player")
                {
                    return prefab;
                }

                candidates.Add(
                    prefab);
            }

            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            Assert.Fail(
                candidates.Count == 0
                    ? "Player prefab with PlayerDeathController was not found."
                    : "Multiple player prefab candidates were found.");

            return null;
        }

        private sealed class TestRewardClaimHandler :
            IRewardClaimHandler
        {
            private readonly ItemDefinition
                _supportedDefinition;

            public RewardClaimResult Result
            {
                get;
                set;
            } = RewardClaimResult.Claimed;

            public ItemDefinition LastDefinition
            {
                get;
                private set;
            }

            public TestRewardClaimHandler(
                ItemDefinition supportedDefinition)
            {
                _supportedDefinition =
                    supportedDefinition;
            }

            public bool Supports(
                ItemDefinition definition)
            {
                return ReferenceEquals(
                    definition,
                    _supportedDefinition);
            }

            public RewardClaimResult TryClaim(
                ItemDefinition definition)
            {
                LastDefinition =
                    definition;

                return Result;
            }
        }
    }
}

#endif
