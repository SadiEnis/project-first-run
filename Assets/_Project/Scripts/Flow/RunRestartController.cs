using System;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Input;
using ProjectFirstRun.Player;
using ProjectFirstRun.UI.Hud;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Weapons;
using UnityEngine;

namespace ProjectFirstRun.Flow
{
    // Arrival runs before entry captures time/cursor; no persistent scene object is needed.
    [DefaultExecutionOrder(-1000)]
    public sealed class RunRestartController : MonoBehaviour
    {
        [SerializeField] private DemoOpeningController _demo;
        [SerializeField] private DemoPauseController _pause;
        [SerializeField] private DemoFinalController _final;
        [SerializeField] private RewardSelectionController _selection;
        private static string _arrivalScene;
        private IRunSceneReloader _reloader = new UnityRunSceneReloader();
        private readonly RunRestartOperation _operation = new();
        public bool IsLoading => _operation.State == RunRestartState.Loading;
        public string LastError => _operation.Error?.Message;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() => _arrivalScene = null;

        public void ValidateConfiguration()
        {
            if (_demo == null || _demo.Health == null || _selection == null)
                throw new InvalidOperationException("Assign run restart's demo, health and reward selection references.");
        }

        private void Awake()
        {
            ValidateConfiguration();
            if (_arrivalScene == null) return;
            bool arrived = _arrivalScene == gameObject.scene.path;
            _arrivalScene = null;
            if (!arrived) { DemoEntryController.CancelSkip(); return; }
            Time.timeScale = 1;
            AudioListener.pause = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public bool CanRequest(RunRestartSource source)
        {
            if (!isActiveAndEnabled || IsLoading || !_demo.IsReady || _demo.IsEntryBlocking || _selection.IsOpen) return false;
            switch (source)
            {
                case RunRestartSource.Pause:
                    return _pause != null && _pause.CanRestart && !_demo.Health.IsDead && (_final == null || !_final.IsEnding);
                case RunRestartSource.Death:
                    return _demo.Health.IsDead && (_final == null || !_final.IsEnding);
                case RunRestartSource.Completion:
                    return _final != null && _final.IsEnding && _final.Session.Phase == DemoFinalPhase.Completed && !_final.Session.ReplayRequested;
                default: return false;
            }
        }

        public bool Request(RunRestartSource source)
        {
            if (!CanRequest(source)) return false;
            float time = Time.timeScale;
            bool audio = AudioListener.pause, cursor = Cursor.visible;
            var cursorLock = Cursor.lockState;
            SetLocks(true);
            Time.timeScale = 0;
            AudioListener.pause = true;
            string path = gameObject.scene.path;
            _arrivalScene = path;
            DemoEntryController.SkipMenuOnNextLoad(path);
            if (_operation.TryStart(true, path, _reloader)) return true;
            _arrivalScene = null;
            DemoEntryController.CancelSkip();
            SetLocks(false);
            Time.timeScale = time;
            AudioListener.pause = audio;
            Cursor.lockState = cursorLock;
            Cursor.visible = cursor;
            return false;
        }

        // Narrow test seam: replacing a loader during a transition would break its ownership.
        internal void SetReloader(IRunSceneReloader reloader)
        {
            if (IsLoading) throw new InvalidOperationException("A run reload is already in progress.");
            _reloader = reloader ?? throw new ArgumentNullException(nameof(reloader));
        }

        private void SetLocks(bool blocked)
        {
            if (_demo == null || _demo.Health == null) return;
            var health = _demo.Health;
            health.SetDamageBlocked(this, blocked);
            health.GetComponent<PlayerInputReader>()?.SetGameplayBlocked(this, blocked);
            health.GetComponent<PlayerMotor>()?.SetSuspended(this, blocked);
            health.GetComponent<PlayerWeaponController>()?.SetWeaponBlocked(this, blocked);
            health.GetComponent<PlayerAbilityController>()?.SetAbilityBlocked(this, blocked);
        }

        // Only old-player locks are released. Global state belongs to the arriving scene.
        private void OnDisable() => SetLocks(false);
    }
}
