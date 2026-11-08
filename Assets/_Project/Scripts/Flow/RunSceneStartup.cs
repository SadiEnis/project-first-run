using System;
using System.Collections;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Progression;
using UnityEngine;

namespace ProjectFirstRun.Flow
{
    /// <summary>Coordinates existing service owners; contains no encounter or reward rules.</summary>
    [DisallowMultipleComponent]
    public sealed class RunSceneStartup : MonoBehaviour
    {
        [SerializeField] private PlayerStartingLoadoutInitializer _loadout;
        [SerializeField] private PlayerExperienceController _experience;
        [SerializeField] private ChestSpawner _chests;
        [SerializeField] private PlayerAbilityRuntimeInstaller _abilities;
        [SerializeField, Min(.1f)] private float _timeoutSeconds = 10;

        public bool IsReady { get; private set; }
        public string LastError { get; private set; }
        public PlayerStartingLoadoutInitializer Loadout => _loadout;

        public void ValidateConfiguration()
        {
            if (_loadout == null || _experience == null || _chests == null || _abilities == null)
                throw new InvalidOperationException("Assign run startup loadout, experience, chests and abilities.");
            if (_experience.gameObject != _loadout.gameObject)
                throw new InvalidOperationException("Run startup experience and loadout must belong to the same player.");
            if (float.IsNaN(_timeoutSeconds) || float.IsInfinity(_timeoutSeconds) || _timeoutSeconds <= 0)
                throw new InvalidOperationException("Run startup timeout must be finite and positive.");
            _loadout.ValidateConfiguredSettings();
        }

        private void Awake()
        {
            try
            {
                ValidateConfiguration();
                if (!_loadout.IsInitialized) _loadout.InitializeConfigured();
            }
            catch (Exception error) { Fail(error.Message); }
        }

        private IEnumerator Start()
        {
            if (LastError != null) yield break;
            float deadline = Time.realtimeSinceStartup + _timeoutSeconds;
            while (true)
            {
                string pending = PendingServices();
                if (pending.Length == 0) { IsReady = true; yield break; }
                if (Time.realtimeSinceStartup >= deadline)
                {
                    Fail("Run startup timed out waiting for: " + pending);
                    yield break;
                }
                yield return null;
            }
        }

        private string PendingServices()
        {
            string pending = "";
            if (_loadout == null || !_loadout.IsInitialized) pending += "loadout; ";
            if (_experience == null || !_experience.IsInitialized) pending += "experience; ";
            if (_chests == null || !_chests.IsInitialized) pending += "chests; ";
            if (_abilities == null || !_abilities.IsInstalled) pending += "abilities; ";
            return pending;
        }

        private void Fail(string message)
        {
            IsReady = false;
            LastError = message;
            Debug.LogError(message, this);
        }
    }
}
