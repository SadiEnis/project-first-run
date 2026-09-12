using System;
using UnityEngine;

namespace ProjectFirstRun.Chests
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ChestController))]
    public sealed class ChestView :
        MonoBehaviour
    {
        [SerializeField]
        private ChestController _chestController;

        [SerializeField]
        private GameObject _visualRoot;

        [SerializeField]
        private Collider _interactionCollider;

        private bool _isListening;

        public GameObject VisualRoot =>
            _visualRoot;

        public Collider InteractionCollider =>
            _interactionCollider;

        private void Awake()
        {
            EnsureControllerReference();
            ValidateReferences();
            StartListening();
            ApplyCurrentState();
        }

        private void OnEnable()
        {
            StartListening();
            ApplyCurrentState();
        }

        private void OnDisable()
        {
            StopListening();
        }

        private void OnDestroy()
        {
            StopListening();
        }

        public void Initialize(
            ChestController chestController,
            GameObject visualRoot,
            Collider interactionCollider)
        {
            StopListening();

            _chestController = chestController;
            _visualRoot = visualRoot;
            _interactionCollider = interactionCollider;

            ValidateReferences();
            StartListening();
            ApplyCurrentState();
        }

        private void HandleChestOpened(
            ChestController chestController)
        {
            if (chestController !=
                _chestController)
            {
                return;
            }

            SetOpenedPresentation();
        }

        private void ApplyCurrentState()
        {
            if (_chestController == null ||
                _visualRoot == null ||
                _interactionCollider == null)
            {
                return;
            }

            bool isOpened =
                _chestController.Status ==
                ChestStatus.Opened;

            _visualRoot.SetActive(
                !isOpened);

            _interactionCollider.enabled =
                !isOpened;
        }

        private void SetOpenedPresentation()
        {
            _interactionCollider.enabled = false;
            _visualRoot.SetActive(false);
        }

        private void StartListening()
        {
            if (_isListening ||
                _chestController == null)
            {
                return;
            }

            _chestController.ChestOpened +=
                HandleChestOpened;

            _isListening = true;
        }

        private void StopListening()
        {
            if (!_isListening)
            {
                return;
            }

            if (_chestController != null)
            {
                _chestController.ChestOpened -=
                    HandleChestOpened;
            }

            _isListening = false;
        }

        private void EnsureControllerReference()
        {
            if (_chestController == null)
            {
                _chestController =
                    GetComponent<ChestController>();
            }
        }

        private void ValidateReferences()
        {
            if (_chestController == null ||
                _visualRoot == null ||
                _interactionCollider == null)
            {
                throw new InvalidOperationException(
                    "Chest view references are incomplete.");
            }

            if (_chestController.gameObject !=
                gameObject)
            {
                throw new InvalidOperationException(
                    "Chest view and controller must share a GameObject.");
            }

            if (!_interactionCollider.transform.IsChildOf(
                    transform) &&
                _interactionCollider.transform != transform)
            {
                throw new InvalidOperationException(
                    "Chest interaction collider must belong to the chest hierarchy.");
            }

            if (!_visualRoot.transform.IsChildOf(
                    transform))
            {
                throw new InvalidOperationException(
                    "Chest visual root must be a child of the chest.");
            }
        }
    }
}
