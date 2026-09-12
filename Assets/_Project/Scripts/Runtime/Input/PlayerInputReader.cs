using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectFirstRun.Input
{
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        private ProjectFirstRunInputActions _inputActions;

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
        }

        private void OnEnable()
        {
            _inputActions.Gameplay.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Gameplay.Disable();
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();
        }

        public void SetGameplayInputEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                _inputActions.Gameplay.Enable();
                return;
            }

            _inputActions.Gameplay.Disable();
        }
    }
}
