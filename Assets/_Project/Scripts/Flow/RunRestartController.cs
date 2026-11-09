using System;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.UI.Hud;
using ProjectFirstRun.UI.Rewards;
using UnityEngine;
using ProjectFirstRun.Progression;

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
        [SerializeField] private GameFlowController _flow;
        [SerializeField] private DemoKeyAmbushController _ambush;
        [SerializeField] private RunEconomyController _economy;
        private GameFlowPolicy.Token _loading;
        private static string _arrivalScene;
        private IRunSceneReloader _reloader = new UnityRunSceneReloader();
        private readonly RunRestartOperation _operation = new();
        public bool HasEndedRun => _economy != null && _economy.Result != null;
        public bool IsLoading => _operation.State == RunRestartState.Loading;
        public string LastError => _economy != null && _economy.LastError != null ? _economy.LastError : _operation.Error?.Message;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() => _arrivalScene = null;

        public void ValidateConfiguration()
        {
            if (_demo == null || _flow == null || _demo.Health == null || _selection == null || _economy == null)
                throw new InvalidOperationException("Assign run restart's demo, health, reward selection and economy references.");
        }

        private void Awake()
        {
            ValidateConfiguration();
            if (_arrivalScene == null) return;
            bool arrived = _arrivalScene == gameObject.scene.path;
            _arrivalScene = null;
            if (!arrived) { DemoEntryController.CancelSkip(); return; }
            _flow.Refresh();
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
                case RunRestartSource.FailedEncounter:
                    return _ambush != null && _ambush.isActiveAndEnabled && _ambush.Session.Phase == KeyAmbushPhase.Failed &&
                        !_demo.Health.IsDead && (_final == null || !_final.IsEnding);
                default: return false;
            }
        }

        public bool Request(RunRestartSource source)
        {
            if (!CanRequest(source)) return false;
            if (!Settle(source)) return false;
            if (!_flow.TryAcquire(this, GameFlowRequest.Loading, out _loading)) return false;
            string path = gameObject.scene.path;
            _arrivalScene = path;
            DemoEntryController.SkipMenuOnNextLoad(path);
            if (_operation.TryStart(true, path, _reloader))
            {
                _flow.RetireForReload();
                return true;
            }
            _arrivalScene = null;
            DemoEntryController.CancelSkip();
            _flow.Release(ref _loading);
            return false;
        }

        public bool RequestQuit(RunRestartSource source)
        {
            if (!CanRequest(source) || !Settle(source)) return false;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            return true;
        }

        private bool Settle(RunRestartSource source) => _economy != null &&
            _economy.Complete(source == RunRestartSource.Completion ? RunEconomyOutcome.Victory : RunEconomyOutcome.Death);

        public void ObserveOutcome(RunEconomyOutcome outcome)
        {
            // Called every frame by terminal screens, but attempted only once until explicit retry.
            if (_economy != null && _economy.Result == null && _economy.LastError == null)
                _economy.Complete(outcome);
        }

        // Narrow test seam: replacing a loader during a transition would break its ownership.
        internal void SetReloader(IRunSceneReloader reloader)
        {
            if (IsLoading) throw new InvalidOperationException("A run reload is already in progress.");
            _reloader = reloader ?? throw new ArgumentNullException(nameof(reloader));
        }

        private void OnDisable()
        {
            if (_flow != null) _flow.Release(ref _loading);
            else _loading?.Dispose();
        }
    }
}
