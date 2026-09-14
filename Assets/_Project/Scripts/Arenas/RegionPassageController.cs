using System;
using System.Collections.Generic;
using ProjectFirstRun.Combat;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectFirstRun.Arenas
{
    /// <summary>
    /// A walking passage: local -Z is source, +Z destination.
    /// The clearance volume must enclose the entire stationary blocker.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RegionPassageController : MonoBehaviour
    {
        [SerializeField] private Collider _clearanceVolume;
        [SerializeField] private Collider _blocker;
        [SerializeField] private Transform _player;
        [SerializeField] private HealthComponent _health;
        [SerializeField] private string _sourceId;
        [SerializeField] private string _destinationId;
        [SerializeField] private RegionTransitionDirection _direction;
        [SerializeField] private RegionTransitionRequirement _requirement;
        [SerializeField] private bool _singleUse;
        [SerializeField] private UnityEvent _opened = new UnityEvent();
        [SerializeField] private UnityEvent _closed = new UnityEvent();

        private readonly List<Collider> _playerColliders = new List<Collider>();
        private readonly HashSet<int> _contacts = new HashSet<int>();
        private readonly List<int> _previousContacts = new List<int>();
        private RegionTransition _transition;
        private RegionTransition.Attempt _attempt;
        private RegionEncounterSession _encounter;
        private Bounds _blockerBounds;
        private Matrix4x4 _blockerTransform;
        private bool _configured;

        public TransitionBarrier Barrier { get; } = new TransitionBarrier();
        public string CurrentRegionId { get; private set; }
        public bool DestinationAvailable { get; set; } = true;
        public bool IsTransitioning => _attempt != null;
        public event Action<string> TransitionCompleted;

        // Code-built fixtures and future scene composition use the same configuration.
        public void Configure(Collider volume, Collider blocker, Transform player,
            HealthComponent health, RegionTransition transition,
            RegionEncounterSession encounter = null)
        {
            if (_configured) throw new InvalidOperationException("Passage is already configured.");
            if (volume == null || player == null || health == null || transition == null)
                throw new ArgumentException("Volume, player, health and transition are required.");
            if (!volume.isTrigger || !volume.enabled || volume == blocker ||
                (blocker != null && (!blocker.enabled || blocker.isTrigger)))
                throw new ArgumentException("Use an enabled trigger and a separate enabled solid blocker.");
            if (volume.transform.IsChildOf(player) ||
                (blocker != null && blocker.transform.IsChildOf(player)))
                throw new ArgumentException("Passage geometry must be independent of the player.");
            if (transition.Traversal != RegionTransitionTraversal.Walk || transition.IsInProgress)
                throw new ArgumentException("This component owns an idle walking transition.");
            if (transition.Requirement == RegionTransitionRequirement.EncounterCompleted && encounter == null)
                throw new ArgumentException("An encounter-gated passage requires its encounter.");
            if (blocker != null && !Contains(volume.bounds, blocker.bounds))
                throw new ArgumentException("The clearance volume must enclose the entire blocker.");
            _clearanceVolume = volume;
            _blocker = blocker;
            _player = player;
            _health = health;
            _transition = transition;
            _encounter = encounter;
            _blockerBounds = blocker != null ? blocker.bounds : default;
            _blockerTransform = blocker != null ? blocker.transform.localToWorldMatrix : default;
            CurrentRegionId = transition.SourceId;
            _configured = true;
            Barrier.Opened += HandleOpened;
            Barrier.Closed += HandleClosed;
            RefreshOccupancy();
            Evaluate();
        }

        public void BindEncounter(RegionEncounterSession encounter)
        {
            if (_configured) throw new InvalidOperationException("Bind before configuration.");
            _encounter = encounter;
        }

        private void Start()
        {
            if (_configured || _clearanceVolume == null || _player == null || _health == null) return;
            Configure(_clearanceVolume, _blocker, _player, _health,
                new RegionTransition(_sourceId, _destinationId, _requirement,
                    RegionTransitionTraversal.Walk, _direction, _singleUse), _encounter);
        }

        private void FixedUpdate()
        {
            if (!_configured) return;
            RefreshOccupancy();
            Evaluate();
        }

        private void Update()
        {
            if (!_configured || Time.timeScale > 0f) return;
            RefreshOccupancy();
            Evaluate();
        }

        // Trigger callbacks update new occupancy promptly; FixedUpdate also handles
        // disabled/destroyed colliders and players already inside on enable.
        private void OnTriggerEnter(Collider other) => Observe(other);
        private void OnTriggerStay(Collider other) => Observe(other);
        private void Observe(Collider other)
        {
            if (_configured && IsPlayerCollider(other) && _clearanceVolume != null &&
                _clearanceVolume.bounds.Intersects(other.bounds))
            {
                int id = other.GetInstanceID();
                Barrier.NotifyPlayerEntered(id);
                if (!_previousContacts.Contains(id)) _previousContacts.Add(id);
                ApplyBlocker();
            }
        }

        private bool IsPlayerCollider(Collider other) =>
            other != null && other.enabled && !other.isTrigger &&
            other.gameObject.activeInHierarchy && _player != null &&
            other.transform.IsChildOf(_player);

        private void RefreshOccupancy()
        {
            _contacts.Clear();
            _playerColliders.Clear();
            if (_player != null && _clearanceVolume != null)
            {
                _player.GetComponentsInChildren(false, _playerColliders);
                foreach (Collider body in _playerColliders)
                {
                    if (!IsPlayerCollider(body) || !_clearanceVolume.bounds.Intersects(body.bounds))
                        continue;
                    int id = body.GetInstanceID();
                    _contacts.Add(id);
                    Barrier.NotifyPlayerEntered(id);
                }
            }
            foreach (int id in _previousContacts)
                if (!_contacts.Contains(id)) Barrier.NotifyPlayerCleared(id);
            _previousContacts.Clear();
            _previousContacts.AddRange(_contacts);
        }

        private void Evaluate()
        {
            bool valid = isActiveAndEnabled && Time.timeScale > 0f &&
                _player != null && _player.gameObject.activeInHierarchy &&
                _health != null && !_health.IsDead && ValidGeometry();
            if (!valid || !DestinationAvailable)
            {
                CancelAttempt();
                Barrier.RequestClose();
                ApplyBlocker();
                return;
            }

            if (_attempt != null && !Barrier.IsPlayerInside)
            {
                bool reachedTarget = ClearedTargetSide(_attempt.ToId == _transition.DestinationId);
                if (reachedTarget)
                {
                    var completed = _attempt;
                    _attempt = null;
                    if (_transition.Complete(completed))
                    {
                        CurrentRegionId = completed.ToId;
                        TransitionCompleted?.Invoke(CurrentRegionId);
                    }
                }
                else CancelAttempt();
            }

            bool encounterComplete = _encounter != null &&
                _encounter.Status == RegionEncounterStatus.Completed;
            bool allowed = _attempt != null ||
                _transition.CanBegin(CurrentRegionId, encounterComplete, DestinationAvailable);
            if (allowed) Barrier.Open();
            else Barrier.RequestClose();

            if (_attempt == null && allowed && Barrier.IsPlayerInside)
            {
                float side = Vector3.Dot(_player.position - _clearanceVolume.bounds.center,
                    _clearanceVolume.transform.forward);
                bool onEntrySide = CurrentRegionId == _transition.SourceId ? side < 0 : side > 0;
                if (onEntrySide)
                    _transition.TryBegin(CurrentRegionId, encounterComplete, DestinationAvailable, out _attempt);
            }
            ApplyBlocker();
        }

        private bool ValidGeometry() =>
            _clearanceVolume != null && _clearanceVolume.enabled &&
            _clearanceVolume.gameObject.activeInHierarchy && _clearanceVolume.isTrigger &&
            (_blocker == null || (_blocker.gameObject.activeInHierarchy &&
                _blocker.transform.localToWorldMatrix == _blockerTransform &&
                Contains(_clearanceVolume.bounds, _blockerBounds)));

        private bool ClearedTargetSide(bool forward)
        {
            Vector3 axis = _clearanceVolume.transform.forward * (forward ? 1 : -1);
            Vector3 absoluteAxis = new Vector3(Mathf.Abs(axis.x), Mathf.Abs(axis.y), Mathf.Abs(axis.z));
            Bounds volume = _clearanceVolume.bounds;
            float farEdge = Vector3.Dot(volume.extents, absoluteAxis);
            bool any = false;
            foreach (Collider body in _playerColliders)
            {
                if (!IsPlayerCollider(body)) continue;
                any = true;
                Bounds bounds = body.bounds;
                float nearestEdge = Vector3.Dot(bounds.center - volume.center, axis) -
                    Vector3.Dot(bounds.extents, absoluteAxis);
                if (nearestEdge <= farEdge) return false;
            }
            return any;
        }

        private static bool Contains(Bounds outer, Bounds inner) =>
            outer.Contains(inner.min) && outer.Contains(inner.max);

        private void ApplyBlocker()
        {
            if (_blocker != null)
                _blocker.enabled = isActiveAndEnabled && ValidGeometry() &&
                    Barrier.Status == TransitionBarrierStatus.Closed && !Barrier.IsPlayerInside;
        }
        private void HandleOpened() { ApplyBlocker(); _opened.Invoke(); }
        private void HandleClosed() { ApplyBlocker(); _closed.Invoke(); }
        private void CancelAttempt()
        {
            if (_attempt != null) _transition.Cancel(_attempt);
            _attempt = null;
        }
        private void OnDisable()
        {
            CancelAttempt();
            if (_blocker != null) _blocker.enabled = false;
        }
        private void OnDestroy()
        {
            Barrier.Opened -= HandleOpened;
            Barrier.Closed -= HandleClosed;
        }
    }
}
