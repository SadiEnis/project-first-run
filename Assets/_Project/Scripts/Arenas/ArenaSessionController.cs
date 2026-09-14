using System;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Player;
using ProjectFirstRun.Waves;
using UnityEngine;

namespace ProjectFirstRun.Arenas
{
    [DisallowMultipleComponent]
    public sealed class ArenaSessionController : MonoBehaviour
    {
        private WaveController _waveController;
        private IPlayerDeathSource _playerDeathSource;
        private ArenaSessionState _state;

        private bool _isInitialized;

        public event Action SessionStarted;
        public event Action Victory;
        public event Action Defeat;

        public bool IsInitialized =>
            _isInitialized;

        public ArenaSessionState State =>
            _state;

        public ArenaSessionStatus Status =>
            _state != null
                ? _state.Status
                : ArenaSessionStatus.Ready;

        public void Initialize(
            WaveController waveController,
            IPlayerDeathSource playerDeathSource)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(ArenaSessionController)} " +
                    "has already been initialized.");
            }

            if (waveController == null)
            {
                throw new ArgumentNullException(
                    nameof(waveController));
            }

            if (playerDeathSource == null)
            {
                throw new ArgumentNullException(
                    nameof(playerDeathSource));
            }

            if (!waveController.IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(WaveController)} must be initialized " +
                    $"before {nameof(ArenaSessionController)}.");
            }

            _waveController = waveController;
            _playerDeathSource = playerDeathSource;
            _state = new ArenaSessionState();

            SubscribeToEvents();

            _isInitialized = true;
        }

        public void Begin()
        {
            EnsureInitialized();

            if (!_state.IsReady)
            {
                throw new InvalidOperationException(
                    $"Arena session cannot begin while in " +
                    $"'{_state.Status}' state.");
            }

            if (_playerDeathSource.IsDead)
            {
                throw new InvalidOperationException(
                    "Arena session cannot begin while the player is dead.");
            }

            /*
             * Begin the wave system first.
             *
             * If WaveController.Begin() fails, the arena session
             * remains Ready instead of becoming stuck in Running.
             */
            _waveController.Begin();

            _state.Begin();

            SessionStarted?.Invoke();
        }

        public void Restart()
        {
            EnsureInitialized();

            if (!_state.IsFinished)
            {
                throw new InvalidOperationException(
                    "Arena session can only restart after victory or defeat.");
            }

            if (_playerDeathSource.IsDead)
            {
                throw new InvalidOperationException(
                    "Arena session cannot restart while the player is dead.");
            }

            _waveController.Restart();
            _state = new ArenaSessionState();
            _state.Begin();
            SessionStarted?.Invoke();
        }

        private void HandleSequenceCompleted()
        {
            if (_state == null ||
                !_state.IsRunning)
            {
                return;
            }

            _state.MarkVictory();

            Victory?.Invoke();
        }

        private void HandlePlayerDied(
            DamageInfo damageInfo,
            DamageResult damageResult)
        {
            if (_state == null ||
                !_state.IsRunning)
            {
                return;
            }

            _waveController.Stop();
            _state.MarkDefeat();

            Defeat?.Invoke();
        }

        private void SubscribeToEvents()
        {
            _waveController.SequenceCompleted +=
                HandleSequenceCompleted;

            _playerDeathSource.PlayerDied +=
                HandlePlayerDied;
        }

        private void UnsubscribeFromEvents()
        {
            if (_waveController != null)
            {
                _waveController.SequenceCompleted -=
                    HandleSequenceCompleted;
            }

            if (_playerDeathSource != null)
            {
                _playerDeathSource.PlayerDied -=
                    HandlePlayerDied;
            }
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(ArenaSessionController)} " +
                    "must be initialized before beginning.");
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
    }
}
