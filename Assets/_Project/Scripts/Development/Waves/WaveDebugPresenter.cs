#if UNITY_EDITOR

using ProjectFirstRun.Waves;
using ProjectFirstRun.Arenas;
using UnityEngine;

namespace ProjectFirstRun.Development.Waves
{
    [DisallowMultipleComponent]
    public sealed class WaveDebugPresenter : MonoBehaviour
    {
        [SerializeField]
        private WaveController _waveController;
        
        [SerializeField]
        private ArenaSessionController _arenaSessionController;

        private string _lastEvent =
            "Waiting...";

        private void Awake()
        {
            if (_waveController != null)
            {
                return;
            }

            Debug.LogError(
                $"{nameof(WaveDebugPresenter)} requires " +
                $"a {nameof(WaveController)}.",
                this);

            enabled = false;
        }

        private void OnEnable()
        {
            if (_waveController == null)
            {
                return;
            }

            _waveController.WaveStarted +=
                HandleWaveStarted;

            _waveController.EnemyDefeated +=
                HandleEnemyDefeated;

            _waveController.WaveCompleted +=
                HandleWaveCompleted;

            _waveController.SequenceCompleted +=
                HandleSequenceCompleted;
        }

        private void OnDisable()
        {
            if (_waveController == null)
            {
                return;
            }

            _waveController.WaveStarted -=
                HandleWaveStarted;

            _waveController.EnemyDefeated -=
                HandleEnemyDefeated;

            _waveController.WaveCompleted -=
                HandleWaveCompleted;

            _waveController.SequenceCompleted -=
                HandleSequenceCompleted;
            
            if (_arenaSessionController != null)
            {
                _arenaSessionController.SessionStarted -=
                    HandleSessionStarted;

                _arenaSessionController.Victory -=
                    HandleVictory;

                _arenaSessionController.Defeat -=
                    HandleDefeat;
            }
        }

        private void OnGUI()
        {
            if (_waveController == null)
            {
                return;
            }

            GUI.Box(
                new Rect(20f, 260f, 300f, 215f),
                "Wave / Arena Debug");

            GUI.Label(
                new Rect(35f, 290f, 260f, 25f),
                $"Status: {GetStatus()}");

            GUI.Label(
                new Rect(35f, 315f, 260f, 25f),
                $"Wave: {GetWaveNumber()}");

            GUI.Label(
                new Rect(35f, 340f, 260f, 25f),
                $"Living: {GetLivingEnemyCount()}");

            GUI.Label(
                new Rect(35f, 365f, 260f, 25f),
                $"Tracked: {_waveController.TrackedEnemyCount}");
            
            GUI.Label(
                new Rect(35f, 390f, 260f, 25f),
                $"Arena: {GetArenaStatus()}");

            GUI.Label(
                new Rect(35f, 415f, 260f, 25f),
                $"Last: {_lastEvent}");
        }
        
        private void HandleSessionStarted()
        {
            _lastEvent =
                "Arena started";
        }

        private void HandleVictory()
        {
            _lastEvent =
                "Arena Victory";
        }

        private void HandleDefeat()
        {
            _lastEvent =
                "Arena Defeat";
        }

        private string GetStatus()
        {
            if (_waveController.SequenceState == null)
            {
                return "Not Initialized";
            }

            return _waveController
                .SequenceState
                .Status
                .ToString();
        }

        private string GetWaveNumber()
        {
            if (_waveController.CurrentWaveIndex < 0)
            {
                return "-";
            }

            return
                $"{_waveController.CurrentWaveIndex + 1} / " +
                $"{_waveController.SequenceState.TotalWaves}";
        }

        private int GetLivingEnemyCount()
        {
            if (_waveController.CurrentWaveProgress == null)
            {
                return 0;
            }

            return _waveController
                .CurrentWaveProgress
                .LivingEnemies;
        }
        
        private string GetArenaStatus()
        {
            if (_arenaSessionController == null)
            {
                return "-";
            }

            return _arenaSessionController.Status.ToString();
        }

        private void HandleWaveStarted(
            int waveIndex,
            EnemyWaveDefinition definition)
        {
            _lastEvent =
                $"Wave {waveIndex + 1} started";
        }

        private void HandleEnemyDefeated(
            int waveIndex,
            ProjectFirstRun.Enemies.EnemyController enemy)
        {
            _lastEvent =
                $"{enemy.name} defeated";
        }

        private void HandleWaveCompleted(
            int waveIndex,
            EnemyWaveDefinition definition)
        {
            _lastEvent =
                $"Wave {waveIndex + 1} completed";
        }

        private void HandleSequenceCompleted()
        {
            _lastEvent =
                "Sequence completed";
        }
    }
}

#endif