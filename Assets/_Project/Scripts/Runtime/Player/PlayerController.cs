using ProjectFirstRun.Input;
using UnityEngine;

namespace ProjectFirstRun.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(PlayerInputReader),
        typeof(PlayerMotor),
        typeof(PlayerLook))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Control")]
        [SerializeField]
        private bool _controlEnabled = true;

        [SerializeField]
        private bool _lockCursorWhileControlled = true;

        private PlayerInputReader _inputReader;
        private PlayerMotor _playerMotor;
        private PlayerLook _playerLook;
        
        public bool IsControlEnabled =>
            _controlEnabled;

        private void Awake()
        {
            _inputReader = GetComponent<PlayerInputReader>();
            _playerMotor = GetComponent<PlayerMotor>();
            _playerLook = GetComponent<PlayerLook>();
        }

        private void Start()
        {
            ApplyControlState();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            bool acceptsInput = _controlEnabled && _inputReader.IsGameplayInputEnabled;
            bool jumpPressed = _inputReader.ConsumeJumpPress(acceptsInput && deltaTime > 0f);

            if (acceptsInput)
            {
                _playerLook.Tick(
                    _inputReader.LookInput,
                    _inputReader.IsLookInputFromGamepad,
                    deltaTime);
            }

            Vector2 moveInput = acceptsInput
                ? _inputReader.MoveInput
                : Vector2.zero;

            bool isSprinting =
                acceptsInput &&
                _inputReader.IsSprintHeld;

            // Motor continues processing gravity even when player input is disabled.
            _playerMotor.Tick(
                moveInput,
                isSprinting,
                deltaTime,
                jumpPressed);
        }

        private void OnDisable()
        {
            SetCursorLocked(false);
        }

        public void SetControlEnabled(bool isEnabled)
        {
            _controlEnabled = isEnabled;
            ApplyControlState();
        }

        private void ApplyControlState()
        {
            _inputReader.SetGameplayInputEnabled(
                _controlEnabled);

            if (_lockCursorWhileControlled)
            {
                SetCursorLocked(_controlEnabled);
            }
        }

        private static void SetCursorLocked(bool isLocked)
        {
            Cursor.lockState = isLocked
                ? CursorLockMode.Locked
                : CursorLockMode.None;

            Cursor.visible = !isLocked;
        }
    }
}
