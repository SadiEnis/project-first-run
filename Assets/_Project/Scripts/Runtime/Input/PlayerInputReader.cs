using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectFirstRun.Input
{
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        private ProjectFirstRunInputActions _inputActions;
        private bool _jumpPending;
        private int _jumpFrame = -1;
        private bool _jumpNeedsRelease;

        public bool ConsumeJumpPress(bool allowJump)
        {
            bool pressed = allowJump && IsGameplayInputEnabled && Time.timeScale > 0f &&
                _jumpPending && _jumpFrame == Time.frameCount;
            _jumpPending = false;
            return pressed;
        }

        private void HandleJump(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (_jumpNeedsRelease || Time.timeScale <= 0f) return;
            _jumpPending = true;
            _jumpFrame = Time.frameCount;
        }

        private void Update()
        {
            if (!_jumpNeedsRelease) return;
            foreach (var control in _inputActions.Gameplay.Jump.controls)
                if (control is UnityEngine.InputSystem.Controls.ButtonControl button && button.isPressed)
                    return;
            _jumpNeedsRelease = false;
        }

        public bool IsGameplayInputEnabled => _inputActions != null && _inputActions.Gameplay.enabled;

        public Vector2 MoveInput =>
            _inputActions.Gameplay.Move.ReadValue<Vector2>();

        public Vector2 LookInput =>
            _inputActions.Gameplay.Look.ReadValue<Vector2>();

        public bool IsSprintHeld =>
            _inputActions.Gameplay.Sprint.IsPressed();

        public bool WasPausePressedThisFrame =>
            _inputActions.Gameplay.Pause.WasPressedThisFrame();
        
        public bool IsFireHeld =>
            _inputActions.Gameplay.Fire.IsPressed();

        public bool WasFirePressedThisFrame =>
            _inputActions.Gameplay.Fire.WasPressedThisFrame();

        public bool WasReloadPressedThisFrame =>
            _inputActions.Gameplay.Reload.WasPressedThisFrame();

        public bool WasSwitchWeaponPressedThisFrame =>
            _inputActions.Gameplay.SwitchWeapon.WasPressedThisFrame();

        public bool WasInteractPressedThisFrame =>
            _inputActions.Gameplay.Interact.WasPressedThisFrame();

        public bool IsLookInputFromGamepad =>
            _inputActions.Gameplay.Look.activeControl?.device is Gamepad;

        private void Awake()
        {
            _inputActions = new ProjectFirstRunInputActions();
            _inputActions.Gameplay.Jump.performed += HandleJump;
        }

        private void OnEnable()
        {
            _jumpPending = false;
            _jumpNeedsRelease = true;
            _inputActions.Gameplay.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Gameplay.Disable();
            _jumpPending = false;
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();
        }

        public void SetGameplayInputEnabled(bool isEnabled)
        {
            if (IsGameplayInputEnabled == isEnabled) return;
            _jumpPending = false;
            _jumpNeedsRelease = true;
            if (isEnabled)
            {
                _inputActions.Gameplay.Enable();
                return;
            }

            _inputActions.Gameplay.Disable();
        }
    }
}
