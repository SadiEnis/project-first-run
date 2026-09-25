using UnityEngine;
using ProjectFirstRun.Weapons;

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

        private float _aimPitch;
        private readonly WeaponRecoilState _recoil = new WeaponRecoilState();
        public float AimPitch => _aimPitch;
        public float RecoilOffset => _recoil.Offset;
        public float CurrentPitch => Mathf.Clamp(_aimPitch - _recoil.Offset, _minimumPitch, _maximumPitch);

        public void ApplyRecoil(float degrees)
            => ApplyRecoil(new WeaponRecoilConfig(degrees, maximumOffset: Mathf.Max(12, degrees)));

        public void ApplyRecoil(WeaponRecoilConfig config)
        {
            if (_cameraPivot == null || !isActiveAndEnabled) return;
            _recoil.Kick(config);
            ApplyPitch();
        }

        private void ApplyPitch()
        {
            _recoil.Constrain(Mathf.Max(0, _aimPitch - _minimumPitch));
            _cameraPivot.localRotation = Quaternion.Euler(CurrentPitch, 0f, 0f);
        }

        private void OnDisable()
        {
            _recoil.Reset();
            if (_cameraPivot != null) ApplyPitch();
        }

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

            _aimPitch = Mathf.Clamp(NormalizeAngle(
                _cameraPivot.localEulerAngles.x), _minimumPitch, _maximumPitch);
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

            _aimPitch = Mathf.Clamp(
                _aimPitch - pitchDelta,
                _minimumPitch,
                _maximumPitch);

            _recoil.Tick(deltaTime);
            ApplyPitch();
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
