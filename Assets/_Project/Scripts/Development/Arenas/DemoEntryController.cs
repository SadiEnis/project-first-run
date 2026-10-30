using System;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Input;
using ProjectFirstRun.Player;
using ProjectFirstRun.Weapons;
using ProjectFirstRun.UI.Hud;
using ProjectFirstRun.Flow;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ProjectFirstRun.Development.Arenas
{
    public enum DemoEntryPhase { Menu, Approaching, Playing }

    [DefaultExecutionOrder(-500)]
    public sealed class DemoEntryController : MonoBehaviour
    {
        [SerializeField] private DemoOpeningController _demo;
        [SerializeField] private GameFlowController _flow;
        private GameFlowPolicy.Token _flowToken;
        [SerializeField] private DemoEntryView _view;
        [SerializeField] private CinemachineCamera _entryCamera;
        [SerializeField] private CinemachineCamera _fpsCamera;
        [SerializeField, Min(.1f)] private float _approachSeconds = 2.4f;
        private readonly DemoMenuInput _menu = new();
        private Camera _camera;
        private CinemachineBrain _brain;
        private PlayerInputReader _input;
        private PlayerMotor _motor;
        private PlayerWeaponController _weapon;
        private PlayerAbilityController _abilities;
        private EventSystem _events;
        private bool _previousNavigation;
        private Vector3 _localPosition;
        private Quaternion _localRotation;
        private Renderer[] _viewRenderers;
        private bool[] _rendererStates;
        private float _endFov, _near, _far, _elapsed;
        private bool _locked, _captured, _pointerMode = true;
        private static string _skipNextScene;
        public DemoEntryPhase Phase { get; private set; }
        public bool IsBlocking => isActiveAndEnabled && Phase != DemoEntryPhase.Playing;
        public string LastError { get; private set; }
        public float Progress => Mathf.Clamp01(_elapsed / Mathf.Max(.1f, _approachSeconds));
        public static float Ease(float progress) => Mathf.SmoothStep(0, 1, Mathf.Clamp01(progress));
        public static void SkipMenuOnNextLoad(string scenePath) => _skipNextScene = scenePath;
        public static void CancelSkip() => _skipNextScene = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() => _skipNextScene = null;

        public void ValidateConfiguration()
        {
            if (_demo == null || _flow == null || _demo.Health == null || _view == null || _entryCamera == null ||
                _fpsCamera == null || _entryCamera == _fpsCamera ||
                !float.IsFinite(_approachSeconds) || _approachSeconds < .1f)
                throw new InvalidOperationException("Assign entry Canvas, distinct Cinemachine cameras and blend duration.");
            _view.ValidateConfiguration();
            var player = _demo.Health.gameObject;
            _weapon = player.GetComponent<PlayerWeaponController>();
            _input = player.GetComponent<PlayerInputReader>();
            _motor = player.GetComponent<PlayerMotor>();
            _abilities = player.GetComponent<PlayerAbilityController>();
            _camera = _weapon != null ? _weapon.AimCamera : null;
            _brain = _camera != null ? _camera.GetComponent<CinemachineBrain>() : null;
            if (_camera == null || _brain == null || _input == null || _motor == null || _abilities == null)
                throw new InvalidOperationException("Entry requires the saved FPS Brain, input, motor, weapons and abilities.");
        }

        private void Awake()
        {
            try
            {
                ValidateConfiguration();
                _brain.enabled = false;
                _entryCamera.enabled = _fpsCamera.enabled = false;
                _view.StartButton.onClick.AddListener(OnStartClicked);
                if (_skipNextScene == gameObject.scene.path)
                {
                    _skipNextScene = null;
                    Phase = DemoEntryPhase.Playing;
                    _view.Hide();
                    _flow.Refresh();
                    return;
                }
                _skipNextScene = null;
                _localPosition = _camera.transform.localPosition;
                _localRotation = _camera.transform.localRotation;
                _endFov = _camera.fieldOfView;
                _near = _camera.nearClipPlane;
                _far = _camera.farClipPlane;
                // Destination is the actual authored player eye, never a separately guessed position.
                _fpsCamera.transform.SetPositionAndRotation(_camera.transform.position, _camera.transform.rotation);
                _fpsCamera.Lens = LensSettings.FromCamera(_camera);
                _viewRenderers = _camera.GetComponentsInChildren<Renderer>(true);
                _rendererStates = new bool[_viewRenderers.Length];
                for (int i = 0; i < _viewRenderers.Length; i++)
                {
                    _rendererStates[i] = _viewRenderers[i].enabled;
                    _viewRenderers[i].enabled = false;
                }
                _captured = true;
                if (!_flow.TryAcquire(this, GameFlowRequest.EntryMenu, out _flowToken))
                    throw new InvalidOperationException("Entry flow ownership could not be acquired.");
                _locked = true;
                _brain.UpdateMethod = CinemachineBrain.UpdateMethods.ManualUpdate;
                _brain.IgnoreTimeScale = true;
                _brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, _approachSeconds);
                // Private channel prevents unrelated scene cameras taking over the opening.
                _brain.ChannelMask = _entryCamera.OutputChannel = _fpsCamera.OutputChannel = (OutputChannels)128;
                _entryCamera.Priority = 20;
                _fpsCamera.Priority = 10;
                _entryCamera.enabled = _fpsCamera.enabled = true;
                _brain.enabled = true;
                _brain.ManualUpdate();
                _view.Present(1, false, true, 0, "Preparing demo...");
            }
            catch (Exception error)
            {
                LastError = error.Message;
                Debug.LogException(error, this);
            }
        }

        private void OnStartClicked() { if (_menu.IsArmed) Begin(); }
        public bool Begin()
        {
            if (!IsBlocking || Phase != DemoEntryPhase.Menu || LastError != null || !_demo.IsReady) return false;
            Phase = DemoEntryPhase.Approaching;
            if (!_flow.TryChange(_flowToken, GameFlowRequest.EntryApproach))
            { Phase = DemoEntryPhase.Menu; return false; }
            _elapsed = 0;
            _fpsCamera.Priority = 30;
            _menu.Tick(false, 0, false);
            _view.Present(1, false, _pointerMode, 0, "");
            return true;
        }

        private void Update()
        {
            if (!IsBlocking) return;
            // Pointer events stay with the existing UI module; navigation/submit have one owner.
            if (_events == null && EventSystem.current != null)
            {
                _events = EventSystem.current;
                _previousNavigation = _events.sendNavigationEvents;
                _events.sendNavigationEvents = false;
            }
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            var pad = Gamepad.current;
            if (mouse != null && (mouse.delta.ReadValue().sqrMagnitude > .01f || mouse.leftButton.wasPressedThisFrame)) _pointerMode = true;
            if ((keyboard != null && (keyboard.upArrowKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame ||
                 keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)) ||
                (pad != null && (pad.leftStick.ReadValue().sqrMagnitude > .36f || pad.dpad.ReadValue().sqrMagnitude > .1f ||
                 pad.buttonSouth.wasPressedThisFrame))) _pointerMode = false;
            bool menuVisible = Phase == DemoEntryPhase.Menu && LastError == null && _demo.IsReady;
            if (_menu.Poll(menuVisible) == 0) Begin();
            float alpha = Phase == DemoEntryPhase.Menu ? 1 : 1 - Mathf.Clamp01(_elapsed / .35f);
            _view.Present(alpha, menuVisible && Phase == DemoEntryPhase.Menu && _menu.IsArmed,
                _pointerMode, _menu.Selected, LastError ?? (_demo.IsReady ? "" : "Preparing demo..."));
            // Settings deliberately has no click action.
        }

        private void LateUpdate()
        {
            if (!IsBlocking || !_captured || LastError != null) return;
            _brain.ManualUpdate();
            if (Phase == DemoEntryPhase.Approaching)
            {
                _elapsed += Time.unscaledDeltaTime;
                if (Progress >= 1 && !_brain.IsBlending && ReferenceEquals(_brain.ActiveVirtualCamera, _fpsCamera) && !ActivationHeld())
                    Complete();
            }
        }

        private static bool ActivationHeld()
        {
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            var pad = Gamepad.current;
            return (mouse != null && mouse.leftButton.isPressed) ||
                   (keyboard != null && (keyboard.enterKey.isPressed || keyboard.numpadEnterKey.isPressed || keyboard.spaceKey.isPressed)) ||
                   (pad != null && (pad.buttonSouth.isPressed || pad.rightTrigger.isPressed));
        }

        private void RestoreCamera()
        {
            if (_brain != null) _brain.enabled = false;
            if (_entryCamera != null) _entryCamera.enabled = false;
            if (_fpsCamera != null) _fpsCamera.enabled = false;
            if (!_captured) return;
            if (_camera != null)
            {
                _camera.transform.localPosition = _localPosition;
                _camera.transform.localRotation = _localRotation;
                _camera.fieldOfView = _endFov;
                _camera.nearClipPlane = _near;
                _camera.farClipPlane = _far;
            }
            for (int i = 0; i < _viewRenderers.Length; i++)
                if (_viewRenderers[i] != null) _viewRenderers[i].enabled = _rendererStates[i];
            _captured = false;
        }

        private void Complete()
        {
            RestoreCamera();
            Phase = DemoEntryPhase.Playing;
            ReleaseLocks();
            _view.Hide();
        }

        private void ReleaseLocks()
        {
            if (_events != null) { _events.sendNavigationEvents = _previousNavigation; _events = null; }
            if (!_locked) return;
            _locked = false;
            if (_flow != null) _flow.Release(ref _flowToken);
            else _flowToken?.Dispose();
        }

        private void OnDisable()
        {
            Phase = DemoEntryPhase.Playing;
            RestoreCamera();
            ReleaseLocks();
            if (_view != null)
            {
                _view.Hide();
                if (_view.StartButton != null) _view.StartButton.onClick.RemoveListener(OnStartClicked);
            }
            _menu.Tick(false, 0, false);
            Phase = DemoEntryPhase.Playing;
        }
    }
}
