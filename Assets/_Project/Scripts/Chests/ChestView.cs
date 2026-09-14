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
        private ChestDefinition _presentedDefinition;
        private TextMesh _typeLabel;

        public void ApplyDefinition(ChestDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            ValidateReferences();
            if (_presentedDefinition == definition) return;
            _presentedDefinition = definition;

            // Per-instance property blocks leave shared prefab materials unchanged.
            if (definition.Rarity == ChestRarity.Common)
            {
                var tint = new MaterialPropertyBlock();
                foreach (Renderer renderer in _visualRoot.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.GetPropertyBlock(tint);
                    tint.SetColor("_BaseColor", new Color(0.55f, 0.55f, 0.55f, 1f));
                    tint.SetColor("_Color", new Color(0.55f, 0.55f, 0.55f, 1f));
                    renderer.SetPropertyBlock(tint);
                }
            }
            string labelText = definition.DisplayName;
            if (definition.Rarity != ChestRarity.Common)
                labelText += $" ({definition.Rarity})";

            _typeLabel = CreateLabel("Type label", new Vector3(0f, 1.65f, 0f), Quaternion.identity, labelText);
            AlignLabelToCamera();
        }

        private void LateUpdate()
        {
            AlignLabelToCamera();
        }

        private void AlignLabelToCamera()
        {
            if (_typeLabel == null || !_typeLabel.gameObject.activeInHierarchy) return;
            Camera viewingCamera = Camera.main;
            if (viewingCamera == null) return;

            // One camera-facing label avoids mirrored text from double-sided font materials.
            _typeLabel.transform.rotation = viewingCamera.transform.rotation;
        }

        private TextMesh CreateLabel(string objectName, Vector3 position, Quaternion rotation, string text)
        {
            var labelObject = new GameObject(objectName);
            labelObject.layer = gameObject.layer;
            labelObject.transform.SetParent(_visualRoot.transform, false);
            labelObject.transform.localPosition = position;
            labelObject.transform.localRotation = rotation;
            var label = labelObject.AddComponent<TextMesh>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.GetComponent<MeshRenderer>().sharedMaterial = label.font.material;
            label.fontSize = 48;
            label.characterSize = 0.05f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = Color.white;
            label.text = text;
            return label;
        }

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
            if (_chestController.IsInitialized) ApplyDefinition(_chestController.Definition);
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
