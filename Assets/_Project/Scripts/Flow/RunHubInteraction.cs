using System;
using ProjectFirstRun.Input;
using ProjectFirstRun.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectFirstRun.Flow
{
    /// <summary>Owns exploration, vendor access and the exit threshold; no economy or purchase rules.</summary>
    [DefaultExecutionOrder(100)]
    public sealed class RunHubInteraction : MonoBehaviour
    {
        [SerializeField] private RunHubController _hub;
        [SerializeField] private PlayerController _player;
        [SerializeField] private Camera _camera;
        [SerializeField] private Collider _vendor;
        [SerializeField] private Collider _exit;
        [SerializeField, Min(.5f)] private float _reach = 3;
        private PlayerInputReader _input;
        private PlayerMotor _motor;
        private bool _insideExit, _initialized;
        public bool ShopOpen { get; private set; }
        public bool MenuOpen { get; private set; }
        public bool HasModal => ShopOpen || MenuOpen;
        public bool CanTrade { get; private set; }
        public string Status { get; private set; }
        public event Action Changed;

        public void ValidateConfiguration()
        {
            if (_hub == null || _player == null || _camera == null || _vendor == null || _exit == null)
                throw new InvalidOperationException("Assign the Hub player, camera, vendor and exit.");
            if (_player.GetComponent<PlayerInputReader>() == null || _player.GetComponent<PlayerMotor>() == null ||
                !_exit.isTrigger || !float.IsFinite(_reach) || _reach <= 0)
                throw new InvalidOperationException("Hub exploration requires input, motor and an exit trigger.");
        }
        private void Start()
        {
            ValidateConfiguration();
            _input = _player.GetComponent<PlayerInputReader>();
            _motor = _player.GetComponent<PlayerMotor>();
            _player.SetCursorManaged(this, true);
            _initialized = true;
            ApplyAccess();
        }
        private void Update()
        {
            if (!_initialized) return;
            if (_hub.IsLoading) { ApplyAccess(); return; }
            var keyboard = Keyboard.current;
            var pad = Gamepad.current;
            bool pause = (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) ||
                (pad != null && pad.startButton.wasPressedThisFrame);
            bool back = pad != null && pad.buttonEast.wasPressedThisFrame;
            if (HasModal)
            {
                if (pause || back) Close();
                return;
            }
            if (pause) { MenuOpen = true; Opened(); return; }
            CanTrade = Physics.Raycast(_camera.transform.position, _camera.transform.forward, out var hit,
                _reach, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && hit.collider == _vendor;
            if (CanTrade && _input.TryConsumeInteractionPress()) { OpenShop(); return; }
            bool inside = _exit.bounds.Contains(_player.transform.position + Vector3.up);
            if (inside && (!_insideExit || _input.TryConsumeInteractionPress())) TryEnterRun();
            _insideExit = inside;
        }
        public bool OpenShop()
        {
            if (!_initialized || _hub.IsLoading || HasModal || !CanTrade) return false;
            _hub.RefreshBalance();
            ShopOpen = true;
            Opened();
            return true;
        }
        private void Opened()
        {
            Status = null;
            ApplyAccess();
            Changed?.Invoke();
        }
        public void Close()
        {
            if (!_initialized || _hub.IsLoading) return;
            ShopOpen = MenuOpen = false;
            CanTrade = false;
            ApplyAccess();
            Changed?.Invoke();
        }
        public bool TryEnterRun()
        {
            if (!_initialized || HasModal || _hub.IsLoading) return false;
            bool started = _hub.StartRun();
            Status = started ? "Entering the dungeon..." : "Could not start the run. Visit the merchant to check your save.";
            ApplyAccess();
            return started;
        }
        private void ApplyAccess()
        {
            bool blocked = HasModal || _hub.IsLoading;
            _input.SetGameplayBlocked(this, blocked);
            _motor.SetSuspended(this, blocked);
            Cursor.lockState = blocked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = blocked;
        }
        private void OnDisable()
        {
            if (!_initialized) return;
            _input.SetGameplayBlocked(this, false);
            _motor.SetSuspended(this, false);
            // A departing Hub must not release the arriving scene's cursor.
            if (!_hub.IsLoading)
            {
                _player.SetCursorManaged(this, false);
                Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            }
            _initialized = false;
        }
    }
}
