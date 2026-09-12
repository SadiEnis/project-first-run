using UnityEngine;

namespace ProjectFirstRun.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerLook : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform _cameraPivot;

        [Header("Sensitivity")]
        [SerializeField, Min(0f)]
        private float _mouseSensitivity = 0.1f;

        [SerializeField, Min(0f)]
        private float _gamepadLookSpeed = 160f;

        [Header("Vertical Limits")]
        [SerializeField]
        private float _minimumPitch = -85f;

        [SerializeField]
        private float _maximumPitch = 85f;

        private float _currentPitch;

        private void Awake()
        {
            if (_cameraPivot == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerLook)} on '{name}' requires a camera pivot.",
                    this);

                enabled = false;
                return;
            }

            _currentPitch = NormalizeAngle(
                _cameraPivot.localEulerAngles.x);
        }

        public void Tick(
            Vector2 lookInput,
            bool isGamepadInput,
            float deltaTime)
        {
            if (_cameraPivot == null)
            {
                return;
            }

            float sensitivity = isGamepadInput
                ? _gamepadLookSpeed * deltaTime
                : _mouseSensitivity;

            float yawDelta = lookInput.x * sensitivity;
            float pitchDelta = lookInput.y * sensitivity;

            transform.Rotate(
                Vector3.up,
                yawDelta,
                Space.Self);

            _currentPitch = Mathf.Clamp(
                _currentPitch - pitchDelta,
                _minimumPitch,
                _maximumPitch);

            _cameraPivot.localRotation =
                Quaternion.Euler(_currentPitch, 0f, 0f);
        }

        private void OnValidate()
        {
            _mouseSensitivity = Mathf.Max(
                0f,
                _mouseSensitivity);

            _gamepadLookSpeed = Mathf.Max(
                0f,
                _gamepadLookSpeed);

            if (_minimumPitch > _maximumPitch)
            {
                float previousMinimum = _minimumPitch;
                _minimumPitch = _maximumPitch;
                _maximumPitch = previousMinimum;
            }
        }

        private static float NormalizeAngle(float angle)
        {
            return angle > 180f
                ? angle - 360f
                : angle;
        }
    }
}