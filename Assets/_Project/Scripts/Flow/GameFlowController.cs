using System;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Player;
using ProjectFirstRun.UI.Hud;
using UnityEngine;

namespace ProjectFirstRun.Flow
{
    [DefaultExecutionOrder(-1200)]
    public sealed class GameFlowController : MonoBehaviour
    {
        [SerializeField] private DemoOpeningController _demo;
        [SerializeField] private DemoEntryController _entry;
        [SerializeField] private DemoFinalController _final;
        [SerializeField] private DemoPauseController _pause;
        [SerializeField] private GameplayAccessController _access;
        [SerializeField] private DemoKeyAmbushController _ambush;
        private readonly GameFlowPolicy _policy = new();
        private readonly object _terminalOwner = new();
        private GameFlowPolicy.Token _terminal;
        private GameFlowRequest _terminalRequest;
        private PlayerController _control;
        private bool _initialized, _retiring;
        public GameFlowAccess CurrentAccess => _policy.Evaluate(Context);

        private GameFlowContext Context
        {
            get
            {
                var phase = GameFlowPhase.Playing;
                if (_demo.Health.IsDead) phase = GameFlowPhase.Dead;
                else if (_ambush != null && _ambush.isActiveAndEnabled && _ambush.Session.Phase == KeyAmbushPhase.Failed)
                    phase = GameFlowPhase.EncounterFailed;
                else if (_final != null && _final.IsEnding)
                    phase = _final.Session.Phase == DemoFinalPhase.Completed ? GameFlowPhase.Completed : GameFlowPhase.Ending;
                else if (_entry != null && _entry.IsBlocking)
                    phase = _entry.Phase == DemoEntryPhase.Menu ? GameFlowPhase.EntryMenu : GameFlowPhase.EntryApproach;
                return new GameFlowContext(phase, _demo.IsReady, _control != null && _control.IsControlEnabled,
                    _pause != null && _pause.IsClosing);
            }
        }

        public void ValidateConfiguration()
        {
            if (_demo == null || _demo.Health == null || _access == null)
                throw new InvalidOperationException("Assign flow demo/player and access application component.");
            _access.ValidateConfiguration();
        }

        private void Awake()
        {
            ValidateConfiguration();
            _control = _demo.Health.GetComponent<PlayerController>();
            _access.Initialize();
            _initialized = true;
            _demo.Health.Died += OnDeath;
            _demo.Health.HealthReset += Refresh;
            Refresh();
        }

        private void OnDeath(DamageInfo info, DamageResult result) => Refresh();
        private void Update() => Refresh();
        private void LateUpdate() => Refresh();

        private void SyncTerminal()
        {
            var phase = Context.Phase;
            bool terminal = phase == GameFlowPhase.Dead || phase == GameFlowPhase.Ending || phase == GameFlowPhase.Completed || phase == GameFlowPhase.EncounterFailed;
            if (!terminal) { _terminal?.Dispose(); _terminal = null; return; }
            var request = phase == GameFlowPhase.Dead ? GameFlowRequest.Death :
                phase == GameFlowPhase.EncounterFailed ? GameFlowRequest.Failure :
                phase == GameFlowPhase.Ending ? GameFlowRequest.Ending : GameFlowRequest.Completed;
            if (_terminal != null && _terminal.IsActive && request == _terminalRequest) return;
            if (_terminal != null && _policy.TryChange(_terminal, request, Context))
            { _terminalRequest = request; return; }
            _terminal?.Dispose();
            if (_policy.TryAcquire(_terminalOwner, request, Context, out _terminal)) _terminalRequest = request;
        }

        public bool TryAcquire(object owner, GameFlowRequest request, out GameFlowPolicy.Token token)
        {
            token = null;
            if (!_initialized || !isActiveAndEnabled || _retiring) return false;
            SyncTerminal();
            if (!_policy.TryAcquire(owner, request, Context, out token)) return false;
            Refresh();
            return true;
        }

        public bool TryChange(GameFlowPolicy.Token token, GameFlowRequest request)
        {
            if (!_initialized || _retiring || !_policy.TryChange(token, request, Context)) return false;
            Refresh();
            return true;
        }

        public void Release(ref GameFlowPolicy.Token token)
        {
            token?.Dispose(); token = null;
            Refresh();
        }

        public void Refresh()
        {
            if (!_initialized || !isActiveAndEnabled || _retiring || _demo == null || _demo.Health == null) return;
            SyncTerminal();
            _access.Apply(CurrentAccess);
        }

        public void RetireForReload()
        {
            _retiring = true;
            _access.RetireForReload();
        }

        private void OnDisable()
        {
            if (!_initialized) return;
            _initialized = false;
            if (_demo != null && _demo.Health != null)
            {
                _demo.Health.Died -= OnDeath;
                _demo.Health.HealthReset -= Refresh;
            }
            _policy.Reset();
            if (_access != null) _access.Shutdown();
        }
    }
}
