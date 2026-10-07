using System;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Arenas
{
    // Scene-local completion reward. Ownership is the encounter, never the global registry.
    public sealed class EncounterChestReward : MonoBehaviour
    {
        [SerializeField] private PreparedRegionEncounter _encounter;
        [SerializeField] private ChestSpawner _spawner;
        [SerializeField] private ChestDefinition _definition;
        [SerializeField] private Transform _anchor;
        [SerializeField] private HealthComponent _playerHealth;
        private bool _attempted;
        public bool Awarded { get; private set; }
        public ChestController Reward { get; private set; }
        public string LastError { get; private set; }
        public PreparedRegionEncounter Encounter => _encounter;

        public void ValidateConfiguration()
        {
            if (_encounter == null || _spawner == null || _definition == null ||
                _anchor == null || _playerHealth == null)
                throw new InvalidOperationException("Assign encounter reward scene references.");
            _definition.Validate();
        }

        private void Start() => ValidateConfiguration();

        private void Update()
        {
            if (_attempted || Time.timeScale <= 0 || _playerHealth == null || _playerHealth.IsDead ||
                _encounter == null || !_encounter.IsInitialized ||
                _encounter.Status != ArenaSessionStatus.Victory) return;
            // Never retry a failed spawn every frame, nor re-award after a chest was removed.
            _attempted = true;
            try
            {
                ValidateConfiguration();
                var request = new ChestSpawnRequest(_definition, _anchor.position, _anchor.rotation);
                Reward = _spawner.Spawn(in request).ChestController;
                Awarded = true;
            }
            catch (Exception exception) { LastError = exception.Message; }
        }
    }
}
