using System;
using ProjectFirstRun.Input;
using ProjectFirstRun.Player;
using UnityEngine;

namespace ProjectFirstRun.Chests.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(PlayerInputReader),
        typeof(PlayerController))]
    public sealed class PlayerChestInteractor :
        MonoBehaviour
    {
        [Header("Raycast")]
        [SerializeField]
        private Transform _interactionOrigin;

        [SerializeField]
        [Min(0.01f)]
        private float _interactionDistance = 3f;

        [SerializeField]
        private LayerMask _interactionLayers = Physics.DefaultRaycastLayers;

        private PlayerInputReader _inputReader;
        private PlayerController _playerController;

        public event Action<
            ChestController,
            ChestOpenResult>
            InteractionAttempted;

        public Transform InteractionOrigin =>
            _interactionOrigin;

        public float InteractionDistance =>
            _interactionDistance;

        private void Awake()
        {
            EnsureReferences();
            ValidateConfiguration();
        }

        private void Update()
        {
            if (!_playerController.IsControlEnabled ||
                !_inputReader.WasInteractPressedThisFrame)
            {
                return;
            }

            TryInteract(
                out _);
        }

        public void Initialize(
            Transform interactionOrigin,
            float interactionDistance,
            LayerMask interactionLayers)
        {
            _interactionOrigin =
                interactionOrigin;

            _interactionDistance =
                interactionDistance;

            _interactionLayers =
                interactionLayers;

            EnsureReferences();
            ValidateConfiguration();
        }

        public bool TryInteract(
            out ChestOpenResult result)
        {
            EnsureReferences();
            ValidateConfiguration();

            result = default;

            if (!_playerController.IsControlEnabled)
            {
                return false;
            }

            if (!Physics.Raycast(
                    _interactionOrigin.position,
                    _interactionOrigin.forward,
                    out RaycastHit hit,
                    _interactionDistance,
                    _interactionLayers,
                    QueryTriggerInteraction.Collide))
            {
                return false;
            }

            ChestController chestController =
                hit.collider.GetComponentInParent<
                    ChestController>();

            if (chestController == null)
            {
                return false;
            }

            result =
                chestController.TryOpen();

            InteractionAttempted?.Invoke(
                chestController,
                result);

            return true;
        }

        private void EnsureReferences()
        {
            if (_inputReader == null)
            {
                _inputReader =
                    GetComponent<
                        PlayerInputReader>();
            }

            if (_playerController == null)
            {
                _playerController =
                    GetComponent<
                        PlayerController>();
            }

            if (_inputReader == null ||
                _playerController == null)
            {
                throw new InvalidOperationException(
                    "Player chest interaction dependencies are incomplete.");
            }
        }

        private void ValidateConfiguration()
        {
            if (_interactionOrigin == null)
            {
                throw new InvalidOperationException(
                    "Player chest interaction requires a raycast origin.");
            }

            if (float.IsNaN(
                    _interactionDistance) ||
                float.IsInfinity(
                    _interactionDistance) ||
                _interactionDistance <= 0f)
            {
                throw new InvalidOperationException(
                    "Player chest interaction distance must be finite and positive.");
            }

            if (_interactionLayers.value == 0)
            {
                throw new InvalidOperationException(
                    "Player chest interaction requires at least one physics layer.");
            }
        }

        private void OnValidate()
        {
            if (float.IsNaN(
                    _interactionDistance) ||
                float.IsInfinity(
                    _interactionDistance) ||
                _interactionDistance <= 0f)
            {
                _interactionDistance = 3f;
            }
        }
    }
}
