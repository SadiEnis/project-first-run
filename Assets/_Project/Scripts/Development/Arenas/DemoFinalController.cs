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
        private PlayerInputReader _input;
        private PlayerMotor _motor;
        private PlayerController _control;
        private PlayerWeaponController _weapon;
        private PlayerAbilityController _abilities;
        private CharacterController _body;
        private bool _locked, _restarting;
        private float _previousTimeScale;
        private CursorLockMode _previousCursorLock;
        private bool _previousCursorVisible;
        public DemoFinalSession Session { get; } = new();
        public string LastError { get; private set; }
        public bool IsEnding => isActiveAndEnabled &&
            (Session.Phase == DemoFinalPhase.Ending || Session.Phase == DemoFinalPhase.Completed);
        public bool IsGateOpen => _gate != null && !_gate.activeSelf;

        public void ValidateConfiguration()
        {
            if (_keyAmbush == null || _health == null || _selection == null || _gate == null ||
                _entryVolume == null || !_entryVolume.isTrigger || !_entryVolume.enabled ||
                _gate.GetComponent<Collider>() == null || _gate.GetComponent<Collider>().isTrigger ||
                transform.IsChildOf(_gate.transform))
                throw new InvalidOperationException("Assign final key condition, player, reward UI, solid gate and independent entry trigger.");
            _input = _health.GetComponent<PlayerInputReader>();
            _motor = _health.GetComponent<PlayerMotor>();
            _control = _health.GetComponent<PlayerController>();
            _weapon = _health.GetComponent<PlayerWeaponController>();
            _abilities = _health.GetComponent<PlayerAbilityController>();
            _body = _health.GetComponent<CharacterController>();
            if (_input == null || _motor == null || _control == null || _weapon == null || _abilities == null || _body == null)
                throw new InvalidOperationException("Final entry requires the existing player input, motor, control, weapons, abilities and capsule.");
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

        public bool TryBegin()
        {
            if (!isActiveAndEnabled || LastError != null || _body == null) return false;
            // Poll only this player's capsule, not arbitrary colliders crossing the trigger.
            bool inside = _body.enabled && _entryVolume.enabled && _entryVolume.gameObject.activeInHierarchy &&
                _entryVolume.bounds.Contains(_body.bounds.min) && _entryVolume.bounds.Contains(_body.bounds.max);
            bool allowed = Time.timeScale > 0 && !_selection.IsOpen && _input.IsGameplayInputEnabled && _control.IsControlEnabled;
            if (!Session.TryBegin(!_health.IsDead, _keyAmbush.CanEnterFinal && IsGateOpen, allowed, inside)) return false;
            _previousTimeScale = Time.timeScale;
            _previousCursorLock = Cursor.lockState;
            _previousCursorVisible = Cursor.visible;
            _locked = true;
            _health.SetDamageBlocked(this, true);
            _input.SetGameplayBlocked(this, true);
            _motor.SetSuspended(this, true);
            _weapon.SetWeaponBlocked(this, true);
            _abilities.SetAbilityBlocked(this, true);
            // Freeze existing projectiles/continuous effects too. The later presentation
            // runs on unscaled time; this is not a player-death or a completed demo yet.
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return true;
        }

        public void RestartPreview()
        {
            if (!IsEnding || _restarting) return;
            _restarting = true;
            Time.timeScale = 1;
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                gameObject.scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
            SceneManager.LoadSceneAsync(gameObject.scene.path, LoadSceneMode.Single);
#endif
        }

        private void OnDisable()
        {
            if (!_locked) return;
            _locked = false;
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
            if (LastError != null) GUI.Box(new Rect(20, 250, 650, 70), "Final configuration error: " + LastError);
            if (!IsEnding) return;
            GUI.Box(new Rect(Screen.width / 2f - 220, Screen.height / 2f - 70, 440, 150),
                "Final entry reached — cinematic comes in the next increment.");
            if (GUI.Button(new Rect(Screen.width / 2f - 190, Screen.height / 2f - 20, 380, 35), "Restart preview")) RestartPreview();
            if (GUI.Button(new Rect(Screen.width / 2f - 190, Screen.height / 2f + 25, 380, 35), "Quit")) Application.Quit();
        }
    }
}
