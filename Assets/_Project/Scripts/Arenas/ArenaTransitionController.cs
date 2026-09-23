using System;
using System.Collections.Generic;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Player;
using UnityEngine;

namespace ProjectFirstRun.Arenas
{
    [DisallowMultipleComponent]
    public sealed class ArenaTransitionController : MonoBehaviour
    {
        [SerializeField] private RunSessionController _run;
        [SerializeField] private Transform _player;
        [SerializeField] private HealthComponent _playerHealth;
        [Tooltip("Entry points in the same order as the run's arenas.")]
        [SerializeField] private Transform[] _entryPoints;
        private bool _transitioning;

        public void Configure(RunSessionController run, Transform player,
            HealthComponent health, IReadOnlyList<Transform> entryPoints)
        {
            if (run == null || player == null || health == null || entryPoints == null)
                throw new ArgumentException("Run, player, health and entry points are required.");
            var entries = new Transform[entryPoints.Count];
            for (int i = 0; i < entries.Length; i++)
            {
                if (entryPoints[i] == null) throw new ArgumentException("Missing arena entry point.");
                entries[i] = entryPoints[i];
            }
            _run = run; _player = player; _playerHealth = health; _entryPoints = entries;
        }

        public bool CanTransition(int sourceArenaIndex) =>
            isActiveAndEnabled && !_transitioning && Time.timeScale > 0f &&
            _run != null && _run.isActiveAndEnabled && _run.IsInitialized &&
            _run.Status == RunSessionStatus.Transition &&
            _run.State.CurrentArenaIndex == sourceArenaIndex &&
            _player != null && _player.gameObject.activeInHierarchy &&
            _playerHealth != null && !_playerHealth.IsDead &&
            _entryPoints != null && _entryPoints.Length == _run.State.TotalArenas &&
            sourceArenaIndex >= 0 && sourceArenaIndex + 1 < _entryPoints.Length &&
            _entryPoints[sourceArenaIndex + 1] != null;

        public bool TryTransition(int sourceArenaIndex, Collider entrant)
        {
            if (!CanTransition(sourceArenaIndex) || entrant == null ||
                !entrant.enabled || !entrant.gameObject.activeInHierarchy ||
                !entrant.transform.IsChildOf(_player)) return false;

            _transitioning = true;
            try
            {
                Transform entry = _entryPoints[sourceArenaIndex + 1];
                Quaternion rotation = Quaternion.Euler(0f, entry.eulerAngles.y, 0f);
                if (_player.TryGetComponent(out PlayerMotor motor))
                    motor.Teleport(entry.position, rotation);
                else
                {
                    CharacterController character = _player.GetComponent<CharacterController>();
                    bool wasEnabled = character != null && character.enabled;
                    if (wasEnabled) character.enabled = false;
                    _player.SetPositionAndRotation(entry.position, rotation);
                    if (wasEnabled) character.enabled = true;
                }
                Physics.SyncTransforms();
                _run.AdvanceToNextArena();
                return true;
            }
            finally { _transitioning = false; }
        }
    }
}
