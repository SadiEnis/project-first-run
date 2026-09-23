using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectFirstRun.Player;
using ProjectFirstRun.Combat;

namespace ProjectFirstRun.Arenas
{
    [DisallowMultipleComponent]
    public sealed class RunSessionController : MonoBehaviour
    {
        private readonly List<IArenaSession> _arenaSessions =
            new List<IArenaSession>();
        private readonly List<Action> _victoryHandlers =
            new List<Action>();
        private readonly List<Action> _defeatHandlers =
            new List<Action>();

        private RunSessionState _state;
        private bool _isInitialized;
        private IPlayerDeathSource _playerDeathSource;

        public event Action SessionStarted;
        public event Action<int> ArenaStarted;
        public event Action<int> ArenaTransitionReady;
        public event Action Victory;
        public event Action Defeat;
        public event Action SessionRestarted;

        public bool IsInitialized => _isInitialized;
        public RunSessionState State => _state;
        public RunSessionStatus Status => _state != null
            ? _state.Status
            : RunSessionStatus.Ready;

        public bool CanRestart => _state != null && _state.IsFinished;

        public void Initialize(IReadOnlyList<IArenaSession> arenaSessions,
            IPlayerDeathSource playerDeathSource = null)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(RunSessionController)} has already been initialized.");
            }

            if (arenaSessions == null)
            {
                throw new ArgumentNullException(nameof(arenaSessions));
            }

            if (arenaSessions.Count == 0)
            {
                throw new ArgumentException(
                    "At least one arena session is required.",
                    nameof(arenaSessions));
            }

            var unique = new HashSet<IArenaSession>();
            // Validate the entire list before subscribing so failed setup is retryable.
            for (int index = 0; index < arenaSessions.Count; index++)
            {
                IArenaSession arenaSession = arenaSessions[index];
                if (arenaSession == null)
                {
                    throw new ArgumentException(
                        $"Arena session at index {index} is null.",
                        nameof(arenaSessions));
                }

                if (!arenaSession.IsInitialized || arenaSession.Status != ArenaSessionStatus.Ready ||
                    !unique.Add(arenaSession))
                {
                    throw new InvalidOperationException(
                        $"Arena session at index {index} must be initialized first.");
                }

            }

            for (int index = 0; index < arenaSessions.Count; index++)
            {
                IArenaSession arenaSession = arenaSessions[index];
                _arenaSessions.Add(arenaSession);
                int capturedIndex = index;
                Action victoryHandler = () => HandleArenaVictory(capturedIndex);
                Action defeatHandler = () => HandleArenaDefeat(capturedIndex);
                _victoryHandlers.Add(victoryHandler);
                _defeatHandlers.Add(defeatHandler);
                arenaSession.Victory += victoryHandler;
                arenaSession.Defeat += defeatHandler;
            }

            _state = new RunSessionState(_arenaSessions.Count);
            _playerDeathSource = playerDeathSource;
            if (_playerDeathSource != null) _playerDeathSource.PlayerDied += HandlePlayerDied;
            _isInitialized = true;
        }

        public void Begin()
        {
            EnsureInitialized();

            if (!_state.IsReady)
            {
                throw new InvalidOperationException(
                    $"Run cannot begin while in '{_state.Status}' state.");
            }

            _state.Begin();
            BeginCurrentArena();
            SessionStarted?.Invoke();
        }

        public void AdvanceToNextArena()
        {
            EnsureInitialized();

            if (!_state.IsTransition)
            {
                throw new InvalidOperationException(
                    "The run is not waiting for an arena transition.");
            }

            _state.BeginNextArena();
            BeginCurrentArena();
        }

        /// <summary>Restarts only after the caller has reset all world/player state.</summary>
        public void Restart(Action resetRunWorld)
        {
            EnsureInitialized();
            if (!_state.IsFinished)
                throw new InvalidOperationException("A run can only restart after victory or defeat.");
            if (resetRunWorld == null) throw new ArgumentNullException(nameof(resetRunWorld));

            resetRunWorld();
            for (int index = 0; index < _arenaSessions.Count; index++)
            {
                if (!_arenaSessions[index].IsInitialized || _arenaSessions[index].Status != ArenaSessionStatus.Ready)
                    throw new InvalidOperationException("Reset callback must leave every arena Ready.");
            }
            if (_playerDeathSource != null && _playerDeathSource.IsDead)
                throw new InvalidOperationException("Reset callback must revive the player.");

            _state = new RunSessionState(_arenaSessions.Count);
            Begin();
            SessionRestarted?.Invoke();
        }

        public void CompleteFinalObjective()
        {
            EnsureInitialized();
            if (_state.IsVictory || _state.IsDefeat) return;
            if (!_state.IsRunning) throw new InvalidOperationException("The final objective requires a running run.");
            _state.CompleteFinalObjective();
            Victory?.Invoke();
        }

        private void HandleArenaVictory(int arenaIndex)
        {
            if (!_isInitialized || !_state.IsRunning ||
                _state.CurrentArenaIndex != arenaIndex)
            {
                return;
            }

            _state.CompleteCurrentArena();

            if (_state.IsVictory)
            {
                Victory?.Invoke();
                return;
            }

            ArenaTransitionReady?.Invoke(arenaIndex);
        }

        private void HandleArenaDefeat(int arenaIndex)
        {
            if (!_isInitialized || !_state.IsRunning ||
                _state.CurrentArenaIndex != arenaIndex)
            {
                return;
            }

            _state.MarkDefeat();
            Defeat?.Invoke();
        }

        private void BeginCurrentArena()
        {
            int arenaIndex = _state.CurrentArenaIndex;
            try
            {
                if (_playerDeathSource != null && _playerDeathSource.IsDead)
                    throw new InvalidOperationException("Cannot start an arena with a dead player.");
                _arenaSessions[arenaIndex].Begin();
            }
            catch
            {
                if (!_state.IsFinished) { _state.MarkDefeat(); Defeat?.Invoke(); }
                throw;
            }
            ArenaStarted?.Invoke(arenaIndex);
        }

        private void HandlePlayerDied(DamageInfo info, DamageResult result)
        {
            if (!_isInitialized || _state.IsFinished || _state.IsReady) return;
            _state.MarkDefeat();
            Defeat?.Invoke();
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(RunSessionController)} must be initialized before use.");
            }
        }

        private void OnDestroy()
        {
            if (_playerDeathSource != null) _playerDeathSource.PlayerDied -= HandlePlayerDied;
            for (int index = 0; index < _arenaSessions.Count; index++)
            {
                IArenaSession arenaSession = _arenaSessions[index];
                arenaSession.Victory -= _victoryHandlers[index];
                arenaSession.Defeat -= _defeatHandlers[index];
            }
        }
    }
}
