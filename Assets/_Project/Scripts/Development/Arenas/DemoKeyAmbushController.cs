using System;
using System.Collections.Generic;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Chests.Interaction;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Input;
using ProjectFirstRun.Player;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Development.Arenas
{
    /// <summary>Scene-authored demo encounter. No global inventory, free victory or whole-scene generation.</summary>
    public sealed class DemoKeyAmbushController : MonoBehaviour
    {
        [SerializeField] private HealthComponent _health;
        [SerializeField] private Collider _key;
        [SerializeField] private GameObject _entranceGate;
        [SerializeField] private GameObject _exitGate;
        [SerializeField] private BoxCollider _entranceClearance;
        [SerializeField] private BoxCollider _roomVolume;
        [SerializeField] private PreparedRegionEncounter _firstGroup;
        [SerializeField] private PreparedRegionEncounter _secondGroup;
        [SerializeField] private Transform[] _rescuePoints;
        [SerializeField, Min(.1f)] private float _pickupRange = 2f;
        private PlayerController _control;
        private PlayerInputReader _input;
        private PlayerChestInteractor _interactor;
        private CharacterController _body;
        private readonly List<EnemyController> _tracked = new();
        private readonly HashSet<EnemyController> _dead = new();
        private readonly HashSet<EnemyController> _rescued = new();
        private bool _cleanupDone;
        private bool _restarting;
        public KeyAmbushSession Session { get; } = new();
        public bool CanEnterFinal => _health != null && !_health.IsDead && Session.CanEnterFinal;

        public void ValidateConfiguration()
        {
            if (_health == null || _key == null || _entranceGate == null || _exitGate == null ||
                _entranceGate == _exitGate || _entranceClearance == null || !_entranceClearance.isTrigger ||
                _roomVolume == null || !_roomVolume.isTrigger || _firstGroup == null || _secondGroup == null ||
                _firstGroup == _secondGroup || _rescuePoints == null || _rescuePoints.Length == 0 ||
                !float.IsFinite(_pickupRange) || _pickupRange <= 0)
                throw new InvalidOperationException("Assign the key, independent gates, room/clearance triggers, two groups, rescue points and player.");
            _control = _health.GetComponent<PlayerController>();
            _input = _health.GetComponent<PlayerInputReader>();
            _interactor = _health.GetComponent<PlayerChestInteractor>();
            _body = _health.GetComponent<CharacterController>();
            if (_control == null || _input == null || _interactor == null || _body == null)
                throw new InvalidOperationException("Key interaction requires the existing player control, input, chest ray origin and capsule.");
            foreach (var point in _rescuePoints)
                if (point == null) throw new InvalidOperationException("Missing ambush rescue point.");
            var gate = _entranceGate.GetComponent<Collider>();
            if (gate == null || gate.isTrigger || _exitGate.GetComponent<Collider>() == null)
                throw new InvalidOperationException("Both gates require solid colliders.");
            if (gate.gameObject.activeInHierarchy &&
                (!_entranceClearance.bounds.Contains(gate.bounds.min) || !_entranceClearance.bounds.Contains(gate.bounds.max)))
                throw new InvalidOperationException("Entrance clearance must contain its entire gate before it is opened.");
        }

        private void Start()
        {
            try
            {
                ValidateConfiguration();
                _entranceGate.SetActive(false);
                _exitGate.SetActive(true);
            }
            catch (Exception error) { Fail(error.Message); }
        }

        public bool CanCollect()
        {
            if (Session.Phase != KeyAmbushPhase.AwaitingKey || _health == null || _health.IsDead ||
                _control == null || !_control.IsControlEnabled || !_input.IsGameplayInputEnabled || Time.timeScale <= 0 ||
                !_body.enabled || !_key.enabled || !_key.gameObject.activeInHierarchy) return false;
            Bounds body = _body.bounds;
            if (!_roomVolume.bounds.Contains(body.min) || !_roomVolume.bounds.Contains(body.max)) return false;
            if (Physics.ComputePenetration(_entranceClearance, _entranceClearance.transform.position,
                _entranceClearance.transform.rotation, _body, _body.transform.position, _body.transform.rotation, out _, out _)) return false;
            Transform origin = _interactor.InteractionOrigin;
            // Ignore gameplay trigger volumes but never ignore a solid wall or a nearer chest.
            return Physics.Raycast(origin.position, origin.forward, out var hit, _pickupRange, ~0,
                QueryTriggerInteraction.Ignore) && hit.collider == _key;
        }

        public bool TryCollect()
        {
            if (!CanCollect() || !_input.TryConsumeInteractionPress()) return false;
            if (!Session.TryCollect(true, true, true)) return false;
            _entranceGate.SetActive(true);
            _key.gameObject.SetActive(false);
            return true;
        }

        private void Update()
        {
            if (_health != null && _health.IsDead)
            {
                Session.Cancel(); Cleanup(); return;
            }
            if (Session.Phase == KeyAmbushPhase.Failed || Session.Phase == KeyAmbushPhase.Cancelled) return;
            if (Time.timeScale <= 0) return;
            if (Session.Phase == KeyAmbushPhase.AwaitingKey) { TryCollect(); return; }
            try
            {
                Session.Tick(Time.deltaTime);
                int index = Session.RequestedGroup;
                if (index >= 0)
                {
                    var group = index == 0 ? _firstGroup : _secondGroup;
                    if (!group.IsInitialized) throw new InvalidOperationException("Ambush group did not initialize.");
                    if (group.Status == ArenaSessionStatus.Defeat)
                        throw new InvalidOperationException(group.LastError?.Message ?? "Ambush preparation was cancelled.");
                    group.RequestPreparation();
                    if (group.IsReadyForPassage)
                    {
                        Track(group);
                        // Validate every spawn, even when no player-proximity relocation is needed.
                        foreach (var enemy in group.Enemies) ValidateNavigation(enemy);
                        foreach (var enemy in group.Enemies)
                            if (Vector3.Distance(enemy.transform.position, _health.transform.position) < 2f && !TryRescue(enemy))
                                throw new InvalidOperationException("No safe ambush spawn away from the player.");
                        group.Begin();
                        Session.TryStartGroup(index);
                        foreach (var enemy in group.Enemies) enemy.Perception?.AlarmAt(_health.transform.position);
                    }
                }
                if (Session.Phase == KeyAmbushPhase.FightingFirst || Session.Phase == KeyAmbushPhase.FightingSecond)
                {
                    int active = Session.Phase == KeyAmbushPhase.FightingFirst ? 0 : 1;
                    var group = active == 0 ? _firstGroup : _secondGroup;
                    if (group.Status == ArenaSessionStatus.Defeat)
                        throw new InvalidOperationException(group.LastError?.Message ?? "Ambush interrupted.");
                    if (group.Status == ArenaSessionStatus.Victory) Session.CompleteGroup(active);
                    else CheckEnemies();
                }
                if (CanEnterFinal) _exitGate.SetActive(false);
            }
            catch (Exception error) { Fail(error.Message); }
        }

        private void Track(PreparedRegionEncounter group)
        {
            Untrack();
            foreach (var enemy in group.Enemies)
            {
                if (enemy == null) throw new InvalidOperationException("Prepared ambush enemy is missing.");
                _tracked.Add(enemy);
                enemy.Died += OnEnemyDied;
            }
        }

        private void OnEnemyDied(EnemyController enemy, DamageInfo info, DamageResult result) => _dead.Add(enemy);

        private static void ValidateNavigation(EnemyController enemy)
        {
            var agent = enemy.GetComponent<NavMeshAgent>();
            // A dormant agent can retain isOnNavMesh after its surface data is removed.
            // Query the current navigation data as well as the agent's binding state.
            if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh ||
                !NavMesh.SamplePosition(enemy.transform.position, out _, .5f,
                    new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask }))
                throw new InvalidOperationException(
                    "Ambush enemy '" + enemy.name + "' is not ready on the NavMesh. " +
                    "Stop Play, run Project First Run > Demo > Rebake Demo Navigation and save the scene. " +
                    "Check the spawn points and enabled NavMeshAgent if the problem persists.");
        }

        private void CheckEnemies()
        {
            foreach (var enemy in _tracked)
            {
                if (_dead.Contains(enemy)) continue;
                if (enemy == null) throw new InvalidOperationException("A living ambush enemy disappeared.");
                if (enemy.IsDead || enemy.Health.IsDead) continue;
                if (!enemy.gameObject.activeInHierarchy) throw new InvalidOperationException("A living ambush enemy was disabled.");
                if (_roomVolume.bounds.Contains(enemy.transform.position + Vector3.up)) continue;
                if (_rescued.Contains(enemy) || !TryRescue(enemy))
                    throw new InvalidOperationException("An ambush enemy escaped and could not be safely recovered.");
                _rescued.Add(enemy);
            }
        }

        private bool TryRescue(EnemyController enemy)
        {
            var agent = enemy.GetComponent<NavMeshAgent>();
            if (agent == null || !agent.enabled) return false;
            foreach (var point in _rescuePoints)
            {
                if (Vector3.Distance(point.position, _health.transform.position) < 2f ||
                    !NavMesh.SamplePosition(point.position, out var hit, .5f, agent.areaMask) ||
                    !_roomVolume.bounds.Contains(hit.position + Vector3.up)) continue;
                if (Physics.CheckCapsule(hit.position + Vector3.up * .5f, hit.position + Vector3.up * 1.5f,
                    .4f, ~0, QueryTriggerInteraction.Ignore)) continue;
                bool occupied = false;
                foreach (var other in _tracked)
                    if (other != null && other != enemy && !other.Health.IsDead &&
                        Vector3.Distance(other.transform.position, hit.position) < 1f) { occupied = true; break; }
                if (occupied) continue;
                if (agent.Warp(hit.position)) return true;
            }
            return false;
        }

        private void Fail(string message) { Session.Fail(message); Cleanup(); }
        private void Cleanup()
        {
            if (_cleanupDone) return;
            _cleanupDone = true;
            Untrack();
            if (_firstGroup != null) _firstGroup.Cancel();
            if (_secondGroup != null) _secondGroup.Cancel();
        }
        private void Untrack()
        {
            foreach (var enemy in _tracked) if (enemy != null) enemy.Died -= OnEnemyDied;
            _tracked.Clear(); _dead.Clear(); _rescued.Clear();
        }
        private void OnDisable() { Session.Cancel(); Cleanup(); }

        public void RestartFailedRun()
        {
            if (_restarting || Session.Phase != KeyAmbushPhase.Failed) return;
            _restarting = true;
            Time.timeScale = 1;
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                gameObject.scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
            SceneManager.LoadSceneAsync(gameObject.scene.path, LoadSceneMode.Single);
#endif
        }

        private void OnGUI()
        {
            if (Session.Phase == KeyAmbushPhase.Failed)
            {
                GUI.Box(new Rect(20, 180, 600, 100), "Ambush error: " + Session.Error);
                if (GUI.Button(new Rect(40, 230, 300, 30), "Restart run")) RestartFailedRun();
                return;
            }
            if (_health == null || _health.IsDead || Time.timeScale <= 0) return;
            string prompt = CanCollect() ? "Take key: E / RB-R1" :
                Session.Phase == KeyAmbushPhase.Intermission ? "Next group incoming..." :
                Session.RequestedGroup >= 0 ? "Ambush forming behind the rear screen. Keep clear of the spawn pockets." :
                CanEnterFinal ? "Ambush cleared. Use the separate downward exit. Finale coming next." :
                Session.HasKey ? "Key secured. Clear both ambush groups." : null;
            if (prompt != null) GUI.Box(new Rect(Screen.width / 2f - 300, 135, 600, 35), prompt);
        }
    }
}
