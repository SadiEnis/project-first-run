using UnityEngine;
using ProjectFirstRun.Stats;

namespace ProjectFirstRun.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)]
        private float _walkSpeed = 5f;

        [SerializeField, Min(0f)]
        private float _sprintSpeed = 8f;

        [Header("Gravity")]
        [SerializeField]
        private float _gravity = -25f;

        [SerializeField]
        private float _groundedVerticalSpeed = -2f;

        private CharacterController _characterController;
        private float _verticalVelocity;
        private PlayerStatsController _stats;

        public float EvaluateMovementSpeed(bool isSprinting)
        {
            if (_stats == null) _stats = GetComponent<PlayerStatsController>();
            float baseSpeed = isSprinting ? _sprintSpeed : _walkSpeed;
            float speed = _stats == null ? baseSpeed : _stats.Evaluate(PlayerStatType.MoveSpeed, baseSpeed);
            if (!float.IsFinite(speed) || speed < 0f)
                throw new System.InvalidOperationException("Movement speed must be finite and non-negative.");
            return speed;
        }

        public bool IsGrounded => _characterController.isGrounded;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            bool wasEnabled = _characterController.enabled;
            if (wasEnabled) _characterController.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            _verticalVelocity = 0f;
            if (wasEnabled) _characterController.enabled = true;
        }

        public void Tick(
            Vector2 moveInput,
            bool isSprinting,
            float deltaTime)
        {
            Vector3 movementDirection =
                transform.right * moveInput.x +
                transform.forward * moveInput.y;

            movementDirection.y = 0f;

            // Analog stick magnitude is preserved while diagonal speed is limited.
            movementDirection = Vector3.ClampMagnitude(
                movementDirection,
                1f);

            float movementSpeed =
                EvaluateMovementSpeed(isSprinting);

            UpdateVerticalVelocity(deltaTime);

            Vector3 velocity =
                movementDirection * movementSpeed +
                Vector3.up * _verticalVelocity;

            _characterController.Move(velocity * deltaTime);
        }

        private void UpdateVerticalVelocity(float deltaTime)
        {
            if (_characterController.isGrounded &&
                _verticalVelocity < 0f)
            {
                _verticalVelocity = _groundedVerticalSpeed;
                return;
            }

            _verticalVelocity += _gravity * deltaTime;
        }

        private void OnValidate()
        {
            _walkSpeed = Mathf.Max(0f, _walkSpeed);
            _sprintSpeed = Mathf.Max(_walkSpeed, _sprintSpeed);

            if (_gravity > 0f)
            {
                _gravity = -_gravity;
            }

            if (_groundedVerticalSpeed > 0f)
            {
                _groundedVerticalSpeed = -_groundedVerticalSpeed;
            }
        }
    }
}
