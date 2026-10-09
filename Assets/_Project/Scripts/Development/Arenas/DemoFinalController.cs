using System;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Input;
using ProjectFirstRun.Player;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Weapons;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Development.Arenas
{
    [DefaultExecutionOrder(250)]
    public sealed class DemoFinalController : MonoBehaviour
    {
        [SerializeField] private DemoKeyAmbushController _keyAmbush;
        [SerializeField] private HealthComponent _health;
        [SerializeField] private RewardSelectionController _selection;
        [SerializeField] private GameObject _gate;
        [SerializeField] private BoxCollider _entryVolume;
        [SerializeField] private DemoEndingPresentation _presentation;
        private Camera _view;
        private PlayerInputReader _input;
        private PlayerMotor _motor;
        private PlayerController _control;
        private PlayerWeaponController _weapon;
        private PlayerAbilityController _abilities;
        private CharacterController _body;
        private bool _locked;
        private float _previousTimeScale;
        private CursorLockMode _previousCursorLock;
        private bool _previousCursorVisible;
        public DemoFinalSession Session { get; } = new();
        public string LastError { get; private set; }
        public bool IsEnding => isActiveAndEnabled &&
            (Session.Phase == DemoFinalPhase.Ending || Session.Phase == DemoFinalPhase.Completed);
        public bool IsGateOpen => _gate != null && !_gate.activeSelf;
        public DemoEndingPresentation Presentation => _presentation;

        public void ValidateConfiguration()
        {
            if (_keyAmbush == null || _health == null || _selection == null || _gate == null ||
                _entryVolume == null || !_entryVolume.isTrigger || !_entryVolume.enabled ||
                _gate.GetComponent<Collider>() == null || _gate.GetComponent<Collider>().isTrigger ||
                transform.IsChildOf(_gate.transform) || _presentation == null || !_presentation.isActiveAndEnabled)
                throw new InvalidOperationException("Assign final key condition, player, reward UI, solid gate and independent entry trigger.");
            _input = _health.GetComponent<PlayerInputReader>();
            _motor = _health.GetComponent<PlayerMotor>();
            _control = _health.GetComponent<PlayerController>();
            _weapon = _health.GetComponent<PlayerWeaponController>();
            _abilities = _health.GetComponent<PlayerAbilityController>();
            _body = _health.GetComponent<CharacterController>();
            _view = _health.GetComponentInChildren<Camera>();
            if (_input == null || _motor == null || _control == null || _weapon == null || _abilities == null || _body == null || _view == null)
                throw new InvalidOperationException("Final entry requires the existing player input, motor, control, weapons, abilities and capsule.");
            _presentation.ValidateConfiguration();
        }

        private void Start()
        {
            try { ValidateConfiguration(); }
            catch (Exception error) { LastError = error.Message; }
        }

        private void Update()
        {
            if (LastError != null || _health == null || _body == null) return;
            if (_health.IsDead) { Session.TryDie(); return; }
            if (Session.Phase != DemoFinalPhase.Playing) return;
            if (_keyAmbush.CanEnterFinal) _gate.SetActive(false);
            TryBegin();
        }

        private void LateUpdate()
        {
            if (Session.Phase != DemoFinalPhase.Ending || !_locked || LastError != null) return;
            _presentation.Tick(Time.unscaledDeltaTime);
            if (_presentation.IsComplete && Session.TryComplete())
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public bool TryBegin()
        {
            if (!isActiveAndEnabled || LastError != null || _body == null) return false;
            // Poll only this player's capsule, not arbitrary colliders crossing the trigger.
            bool inside = _body.enabled && _entryVolume.enabled && _entryVolume.gameObject.activeInHierarchy &&
                _entryVolume.bounds.Contains(_body.bounds.min) && _entryVolume.bounds.Contains(_body.bounds.max);
            bool allowed = Time.timeScale > 0 && !_selection.IsOpen && _input.IsGameplayInputEnabled && _control.IsControlEnabled;
            if (Session.Phase == DemoFinalPhase.Playing && !_health.IsDead && _keyAmbush.CanEnterFinal && IsGateOpen && allowed && inside)
            {
                try { ValidateConfiguration(); }
                catch (Exception error) { LastError = error.Message; return false; }
            }
            if (!Session.TryBegin(!_health.IsDead, _keyAmbush.CanEnterFinal && IsGateOpen, allowed, inside)) return false;
            _presentation.Begin(_view.transform);
            _previousTimeScale = Time.timeScale;
            _previousCursorLock = Cursor.lockState;
            _previousCursorVisible = Cursor.visible;
            _locked = true;
            _health.SetDamageBlocked(this, true);
            _input.SetGameplayBlocked(this, true);
            _motor.SetSuspended(this, true);
            _weapon.SetWeaponBlocked(this, true);
            _abilities.SetAbilityBlocked(this, true);
            // Freeze existing projectiles/continuous effects; the presentation uses unscaled time.
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return true;
        }

        public void Replay()
        {
            if (!IsEnding || !Session.TryRequestReplay()) return;
            try
            {
                Time.timeScale = 1;
#if UNITY_EDITOR
                var operation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                    gameObject.scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
                var operation = SceneManager.LoadSceneAsync(gameObject.scene.path, LoadSceneMode.Single);
#endif
                if (operation == null) throw new InvalidOperationException("Demo scene reload did not start.");
            }
            catch (Exception error)
            {
                Time.timeScale = 0;
                Session.CancelFailedReplay();
                LastError = "Replay failed: " + error.Message;
            }
        }

        private void OnDisable()
        {
            if (!_locked) return;
            _locked = false;
            if (_presentation != null) _presentation.RestoreView();
            if (_health != null) _health.SetDamageBlocked(this, false);
            if (_input != null) _input.SetGameplayBlocked(this, false);
            if (_motor != null) _motor.SetSuspended(this, false);
            if (_weapon != null) _weapon.SetWeaponBlocked(this, false);
            if (_abilities != null) _abilities.SetAbilityBlocked(this, false);
            if (Time.timeScale == 0) Time.timeScale = _previousTimeScale;
            Cursor.lockState = _previousCursorLock;
            Cursor.visible = _previousCursorVisible;
        }

        private void OnGUI()
        {
            int oldDepth = GUI.depth;
            GUI.depth = -1000;
            if (IsEnding) _presentation.DrawOverlay();
            if (LastError != null) GUI.Box(new Rect(20, 250, 650, 70), "Final error: " + LastError);
            if (IsEnding && Session.Phase == DemoFinalPhase.Completed)
            {
                GUI.Box(new Rect(Screen.width / 2f - 240, Screen.height / 2f - 100, 480, 210),
                    "PROJECT FIRST RUN — DEMO COMPLETE");
                GUI.Label(new Rect(Screen.width / 2f - 190, Screen.height / 2f - 65, 400, 40), "This was only the beginning. Thanks for playing.");
                bool wasEnabled = GUI.enabled;
                GUI.enabled = wasEnabled && !Session.ReplayRequested;
                if (GUI.Button(new Rect(Screen.width / 2f - 190, Screen.height / 2f - 5, 380, 35), "Play again")) Replay();
                if (GUI.Button(new Rect(Screen.width / 2f - 190, Screen.height / 2f + 40, 380, 35), "Quit")) Application.Quit();
                GUI.enabled = wasEnabled;
            }
            GUI.depth = oldDepth;
        }
    }
}
