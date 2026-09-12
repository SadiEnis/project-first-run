#if UNITY_EDITOR

using ProjectFirstRun.Arenas;
using ProjectFirstRun.Development.Waves;
using ProjectFirstRun.Player;
using ProjectFirstRun.Waves;
using UnityEngine;

namespace ProjectFirstRun.Development.Arenas
{
    [DisallowMultipleComponent]
    public sealed class ArenaSessionDevelopmentBootstrap : MonoBehaviour
    {
        [SerializeField]
        private ArenaSessionController _arenaSessionController;

        [SerializeField]
        private WaveController _waveController;

        [SerializeField]
        private PlayerDeathController _playerDeathController;
        
        [SerializeField]
        private WaveDevelopmentBootstrap _waveBootstrap;

        private void Start()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            _waveBootstrap.InitializeWaveSystem();

            _arenaSessionController.Initialize(
                _waveController,
                _playerDeathController);

            _arenaSessionController.Begin();
        }

        private bool ValidateReferences()
        {
            bool isValid = true;

            if (_arenaSessionController == null)
            {
                LogMissing(nameof(_arenaSessionController));
                isValid = false;
            }

            if (_waveController == null)
            {
                LogMissing(nameof(_waveController));
                isValid = false;
            }

            if (_playerDeathController == null)
            {
                LogMissing(nameof(_playerDeathController));
                isValid = false;
            }

            return isValid;
        }

        private void LogMissing(
            string fieldName)
        {
            Debug.LogError(
                $"{nameof(ArenaSessionDevelopmentBootstrap)} " +
                $"requires {fieldName}.",
                this);
        }
    }
}

#endif