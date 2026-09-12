using System.Collections;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Interaction;
using ProjectFirstRun.Input;
using ProjectFirstRun.Player;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.UI.Rewards;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Chests.Interaction
{
    public sealed class PlayerChestInteractorTests :
        InputTestFixture
    {
        private GameObject _playerObject;
        private GameObject _originObject;
        private GameObject _chestObject;
        private GameObject _buildObject;
        private GameObject _selectionObject;
        private GameObject _blockerObject;

        private PlayerController _playerController;
        private PlayerInputReader _inputReader;
        private PlayerChestInteractor _interactor;
        private ChestController _chestController;
        private PlayerBuildController _buildController;
        private RewardSelectionController _selectionController;

        private RewardItemPool _rewardItemPool;
        private ChestDefinition _chestDefinition;

        private Keyboard _keyboard;
        private Gamepad _gamepad;

        [SetUp]
        public void SetUp()
        {
            _keyboard =
                InputSystem.AddDevice<
                    Keyboard>();

            _gamepad =
                InputSystem.AddDevice<
                    Gamepad>();

            CreateBuildAndSelection();
            CreatePlayer();
            CreateChest();

            Physics.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            DestroyImmediateIfExists(
                _blockerObject);

            DestroyImmediateIfExists(
                _chestObject);

            DestroyImmediateIfExists(
                _selectionObject);

            DestroyImmediateIfExists(
                _buildObject);

            DestroyImmediateIfExists(
                _playerObject);

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

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        [Test]
        public void TryInteract_WithFocusedChest_AttemptsExactChest()
        {
            ChestController attemptedChest = null;
            ChestOpenResult attemptedResult =
                ChestOpenResult.SelectionOpened;

            _interactor.InteractionAttempted +=
                (chest, result) =>
                {
                    attemptedChest = chest;
                    attemptedResult = result;
                };

            bool didInteract =
                _interactor.TryInteract(
                    out ChestOpenResult result);

            Assert.That(
                didInteract,
                Is.True);

            Assert.That(
                result,
                Is.EqualTo(
                    ChestOpenResult.NoEligibleRewards));

            Assert.That(
                attemptedChest,
                Is.SameAs(_chestController));

            Assert.That(
                attemptedResult,
                Is.EqualTo(result));
        }

        [Test]
        public void TryInteract_WithChestOutOfRange_ReturnsFalse()
        {
            _chestObject.transform.position =
                new Vector3(
                    0f,
                    1f,
                    5f);

            Physics.SyncTransforms();

            Assert.That(
                _interactor.TryInteract(
                    out _),
                Is.False);
        }

        [Test]
        public void TryInteract_WithOccludingObject_ReturnsFalse()
        {
            _blockerObject =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube);

            _blockerObject.name =
                "ChestInteractor_Blocker";

            _blockerObject.transform.position =
                new Vector3(
                    0f,
                    1f,
                    1f);

            Physics.SyncTransforms();

            Assert.That(
                _interactor.TryInteract(
                    out _),
                Is.False);
        }

        [Test]
        public void TryInteract_WhenPlayerControlDisabled_ReturnsFalse()
        {
            _playerController.SetControlEnabled(
                false);

            Assert.That(
                _interactor.TryInteract(
                    out _),
                Is.False);
        }

        [UnityTest]
        public IEnumerator Update_WithKeyboardInteract_AttemptsChest()
        {
            // Let PlayerController.Start apply its initial input-map state.
            yield return null;

            int attemptCount = 0;

            _interactor.InteractionAttempted +=
                (_, _) => attemptCount++;

            Press(
                _keyboard.eKey,
                queueEventOnly: true);

            yield return null;

            Assert.That(
                _inputReader.WasInteractPressedThisFrame,
                Is.True);

            Assert.That(
                _playerController.IsControlEnabled,
                Is.True);

            Assert.That(
                _interactor.isActiveAndEnabled,
                Is.True);

            AssertFocusedChestRaycast();

            // The test coroutine can resume before MonoBehaviour.Update.
            yield return null;

            Assert.That(
                attemptCount,
                Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Update_WithGamepadInteract_AttemptsChest()
        {
            // Let PlayerController.Start apply its initial input-map state.
            yield return null;

            int attemptCount = 0;

            _interactor.InteractionAttempted +=
                (_, _) => attemptCount++;

            Press(
                _gamepad.buttonSouth,
                queueEventOnly: true);

            yield return null;

            Assert.That(
                _inputReader.WasInteractPressedThisFrame,
                Is.True);

            Assert.That(
                _playerController.IsControlEnabled,
                Is.True);

            Assert.That(
                _interactor.isActiveAndEnabled,
                Is.True);

            AssertFocusedChestRaycast();

            // The test coroutine can resume before MonoBehaviour.Update.
            yield return null;

            Assert.That(
                attemptCount,
                Is.EqualTo(1));
        }

        private void CreateBuildAndSelection()
        {
            _buildObject =
                new GameObject(
                    "ChestInteractor_Build");

            _buildController =
                _buildObject.AddComponent<
                    PlayerBuildController>();

            _buildController.Initialize(
                PlayerBuildCapacity.CreateDefault());

            _selectionObject =
                new GameObject(
                    "ChestInteractor_Selection");

            _selectionController =
                _selectionObject.AddComponent<
                    RewardSelectionController>();
        }

        private void CreatePlayer()
        {
            _playerObject =
                new GameObject(
                    "ChestInteractor_Player");

            _playerObject.SetActive(
                false);

            _originObject =
                new GameObject(
                    "InteractionOrigin");

            _originObject.transform.SetParent(
                _playerObject.transform,
                false);

            _originObject.transform.localPosition =
                Vector3.up;

            _playerObject.AddComponent<
                CharacterController>();

            _inputReader =
                _playerObject.AddComponent<
                    PlayerInputReader>();

            PlayerMotor playerMotor =
                _playerObject.AddComponent<
                    PlayerMotor>();

            SetPrivateField(
                playerMotor,
                typeof(PlayerMotor),
                "_gravity",
                0f);

            SetPrivateField(
                playerMotor,
                typeof(PlayerMotor),
                "_groundedVerticalSpeed",
                0f);

            PlayerLook playerLook =
                _playerObject.AddComponent<
                    PlayerLook>();

            SetPrivateField(
                playerLook,
                typeof(PlayerLook),
                "_cameraPivot",
                _originObject.transform);

            _playerController =
                _playerObject.AddComponent<
                    PlayerController>();

            SetPrivateField(
                _playerController,
                typeof(PlayerController),
                "_lockCursorWhileControlled",
                false);

            _interactor =
                _playerObject.AddComponent<
                    PlayerChestInteractor>();

            _interactor.Initialize(
                _originObject.transform,
                3f,
                ~0);

            _playerObject.SetActive(
                true);
        }

        private void CreateChest()
        {
            _rewardItemPool =
                ScriptableObject.CreateInstance<
                    RewardItemPool>();

            _chestObject =
                new GameObject(
                    "ChestInteractor_Chest");

            _chestObject.transform.position =
                new Vector3(
                    0f,
                    1f,
                    2f);

            _chestObject.AddComponent<
                BoxCollider>();

            _chestController =
                _chestObject.AddComponent<
                    ChestController>();

            _chestDefinition =
                ScriptableObject.CreateInstance<
                    ChestDefinition>();

            SetPrivateField(
                _chestDefinition,
                typeof(ChestDefinition),
                "_stableId",
                "chest.interactor-test");

            SetPrivateField(
                _chestDefinition,
                typeof(ChestDefinition),
                "_rewardItemPool",
                _rewardItemPool);

            SetPrivateField(
                _chestDefinition,
                typeof(ChestDefinition),
                "_requestedChoiceCount",
                3);

            SetPrivateField(
                _chestDefinition,
                typeof(ChestDefinition),
                "_worldPrefab",
                _chestObject);

            _chestController.Initialize(
                _chestDefinition,
                _buildController,
                _selectionController,
                new RewardOfferGenerator(
                    new RewardCandidateFilter(),
                    new FirstRandomSource()));
        }

        private void AssertFocusedChestRaycast()
        {
            Physics.SyncTransforms();

            bool didHit =
                Physics.Raycast(
                    _interactor.InteractionOrigin.position,
                    _interactor.InteractionOrigin.forward,
                    out RaycastHit hit,
                    _interactor.InteractionDistance,
                    ~0,
                    QueryTriggerInteraction.Collide);

            Assert.That(
                didHit,
                Is.True);

            Assert.That(
                hit.collider.GetComponentInParent<
                    ChestController>(),
                Is.SameAs(_chestController));
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
    }
}
