using System;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Input;
using ProjectFirstRun.Player;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Weapons;
using UnityEngine;
using ProjectFirstRun.Flow;

namespace ProjectFirstRun.Development.Arenas
{
    [DefaultExecutionOrder(250)]
    public sealed class DemoFinalController : MonoBehaviour
    {
        [SerializeField] private DemoKeyAmbushController _keyAmbush;
        [SerializeField] private RunRestartController _restart;
        [SerializeField] private RunEndController _runEnd;
        [SerializeField] private ProjectFirstRun.Progression.RunEconomyController _economy;
        [SerializeField] private GameFlowController _flow;
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
        private readonly DemoMenuInput _completionMenu = new();
        public DemoFinalSession Session { get; } = new();
        public string LastError { get; private set; }
        public bool IsEnding => isActiveAndEnabled &&
            (Session.Phase == DemoFinalPhase.Ending || Session.Phase == DemoFinalPhase.Completed);
        public bool IsGateOpen => _gate != null && !_gate.activeSelf;
        public DemoEndingPresentation Presentation => _presentation;

        public void ValidateConfiguration()
        {
            if (_economy == null || _keyAmbush == null || _flow == null || _restart == null || _runEnd == null || _health == null || _selection == null || _gate == null ||
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

        private void OnEnable() => Session.Completed += OnCompleted;
        private void OnCompleted() => _runEnd.CompleteVictory();

        private void Start()
        {
            try { ValidateConfiguration(); }
            catch (Exception error) { LastError = error.Message; }
        }

        private void Update()
        {
            int choice = _completionMenu.Poll(IsEnding && Session.Phase == DemoFinalPhase.Completed && !Session.ReplayRequested);
            if (choice == 0) Replay();
            else if (choice == 1) _restart.RequestQuit(RunRestartSource.Completion);
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
                _flow.Refresh();
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
            _locked = true;
            _flow.Refresh();
            return true;
        }

        public void Replay()
        {
            if (!_restart.RequestReturnToHub(RunRestartSource.Completion)) return;
            Session.TryRequestReplay();
        }

        private void OnDisable()
        {
            Session.Completed -= OnCompleted;
            _completionMenu.Tick(false, 0, false);
            if (_flow != null) _flow.Refresh();
            if (!_locked) return;
            _locked = false;
            if (_presentation != null) _presentation.RestoreView();
        }

        private void OnGUI()
        {
            int oldDepth = GUI.depth;
            GUI.depth = -1000;
            if (IsEnding) _presentation.DrawOverlay();
            if (LastError != null) GUI.Box(new Rect(20, 250, 650, 70), "Final error: " + LastError);
            if (IsEnding && Session.Phase == DemoFinalPhase.Completed)
            {
                float width = Mathf.Min(560, Screen.width - 32);
                var panel = new Rect((Screen.width - width) / 2, Mathf.Max(8, (Screen.height - 440) / 2), width, 440);
                GUI.Box(panel, "PROJECT FIRST RUN — DEMO COMPLETE");
                GUI.BeginGroup(panel);
                GUI.Label(new Rect(24, 32, width - 48, 26), "This was only the beginning. Thanks for playing.");
                ProjectFirstRun.UI.Hud.RunEconomySummaryView.Draw(new Rect(24, 72, width - 48, 196), _economy);
                if (_economy.IsSettled && _restart.LastError != null)
                    GUI.Label(new Rect(24, 268, width - 48, 24), "Return failed. Your reward is saved. Please try again.");
                bool wasEnabled = GUI.enabled;
                GUI.enabled = wasEnabled && !Session.ReplayRequested;
                float buttonWidth = Mathf.Min(380, width - 48);
                float buttonLeft = (width - buttonWidth) / 2;
                if (_completionMenu.Button(new Rect(buttonLeft, 300, buttonWidth, 35), 0, "Return to hub")) Replay();
                if (_completionMenu.Button(new Rect(buttonLeft, 345, buttonWidth, 35), 1, "Quit")) _restart.RequestQuit(RunRestartSource.Completion);
                GUI.enabled = wasEnabled;
                ProjectFirstRun.UI.Hud.InputPrompts.Draw(new Rect(12, 395, width - 24, 30), DemoMenuInput.Hint);
                GUI.EndGroup();
            }
            GUI.depth = oldDepth;
        }
    }
}
