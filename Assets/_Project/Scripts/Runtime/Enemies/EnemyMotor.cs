using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using ProjectFirstRun.Combat;

namespace ProjectFirstRun.Enemies
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class EnemyMotor : MonoBehaviour, IKnockbackReceiver
    {
        private NavMeshAgent _agent;
        private Transform _target;

        private float _destinationUpdateInterval;
        private float _destinationUpdateTimer;

        private bool _isInitialized;
        private bool _movementEnabled;
        private bool _destinationOverride;
        private EnemyController _enemy;
        private bool _investigationStarted;
        private bool _investigationFinished;
        private Vector3 _investigationPoint;
        private bool SightAllowsMovement => _enemy == null || !_enemy.RequiresPerception ||
            (_enemy.Perception != null && _enemy.Perception.HasSight);

        // Called only by the attack owner. A hidden position receives one path request,
        // including partial paths; reaching the accessible endpoint consumes that request.
        public void Investigate(Vector3 point, float tolerance)
        {
            if (!CanUseAgent()) return;
            if (!_investigationStarted || point != _investigationPoint)
            {
                Stop();
                _investigationStarted = true; _investigationFinished = false;
                _investigationPoint = point;
                _movementEnabled = true; _destinationOverride = true;
                _agent.isStopped = IsStunned;
                if (!_agent.SetDestination(point)) _investigationFinished = true;
            }
            if (Vector3.Distance(transform.position, point) <= tolerance ||
                (!_agent.pathPending && (_agent.pathStatus == NavMeshPathStatus.PathInvalid ||
                (_agent.hasPath && _agent.remainingDistance <= tolerance)))) _investigationFinished = true;
            if (_investigationFinished && _movementEnabled) Stop();
        }

        public void ClearInvestigation() { _investigationStarted = false; _investigationFinished = false; }
        private float _baseSpeed;
        private float _stunUntil;
        private bool _stunStoppedAgent;
        public bool IsStunned => Time.time < _stunUntil;
        public void ApplyStun(float duration)
        {
            if (!float.IsFinite(duration) || duration <= 0) throw new ArgumentOutOfRangeException(nameof(duration));
            if (!isActiveAndEnabled || !_isInitialized) return;
            _stunUntil = Mathf.Max(_stunUntil, Time.time + duration);
            if (CanUseAgent())
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
                _stunStoppedAgent = true;
            }
        }
        public void ClearStun()
        {
            _stunUntil = 0;
            if (_stunStoppedAgent && CanUseAgent()) _agent.isStopped = !_movementEnabled;
            _stunStoppedAgent = false;
        }
        private readonly Dictionary<object, float> _movementModifiers = new Dictionary<object, float>();
        public float MovementMultiplier { get; private set; } = 1;
        public void SetMovementModifier(object owner, float multiplier)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (!float.IsFinite(multiplier) || multiplier <= 0 || multiplier > 1)
                throw new ArgumentOutOfRangeException(nameof(multiplier));
            _movementModifiers[owner] = multiplier; RefreshMovementSpeed();
        }
        public void RemoveMovementModifier(object owner)
        {
            if (owner != null && _movementModifiers.Remove(owner)) RefreshMovementSpeed();
        }
        private void RefreshMovementSpeed()
        {
            MovementMultiplier = 1;
            foreach (var value in _movementModifiers.Values) MovementMultiplier = Mathf.Min(MovementMultiplier, value);
            if (_agent != null && _isInitialized) _agent.speed = _baseSpeed * MovementMultiplier;
        }
        private void OnDisable()
        {
            ClearStun();
            _movementModifiers.Clear(); RefreshMovementSpeed();
        }

        public bool CanNavigate => CanUseAgent();

        public bool TryPush(Vector3 direction, float distance)
        {
            var enemy = GetComponent<EnemyController>();
            if (!isActiveAndEnabled || !_isInitialized || !CanUseAgent() || enemy == null ||
                !enemy.isActiveAndEnabled || !enemy.IsInitialized || enemy.IsDead || enemy.Health.IsDead ||
                !float.IsFinite(distance) || distance <= 0 || !float.IsFinite(direction.x) ||
                !float.IsFinite(direction.y) || !float.IsFinite(direction.z)) return false;
            direction.y = 0;
            if (direction.sqrMagnitude < .000001f) return false;
            direction.Normalize();
            bool wasStopped = _agent.isStopped;
            Vector3 origin = transform.position;
            float allowed = distance;
            if (_agent.Raycast(origin + direction * distance, out NavMeshHit navHit))
                allowed = Mathf.Min(allowed, Mathf.Max(0, Vector3.Distance(origin, navHit.position) - .02f));
            float radius = _agent.radius;
            Vector3 bottom = origin + Vector3.up * (radius + .05f);
            Vector3 top = origin + Vector3.up * Mathf.Max(radius + .05f, _agent.height - radius);
            foreach (var hit in Physics.CapsuleCastAll(bottom, top, radius, direction, allowed,
                         Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.IsChildOf(transform)) continue;
                allowed = Mathf.Min(allowed, Mathf.Max(0, hit.distance - .02f));
            }
            // Do not Stop/Resume or clear paths: attack/navigation ownership and cooldowns remain intact.
            if (allowed <= .001f) return false;
            _agent.Move(direction * allowed);
            // Preserve the navigation owner's stopped state across native agent operations.
            _agent.isStopped = wasStopped;
            Physics.SyncTransforms();
            return Vector3.Distance(origin, transform.position) > .001f;
        }

        public bool TrySetDestination(Vector3 destination)
        {
            if (!SightAllowsMovement) return false;
            if (!CanUseAgent() || !float.IsFinite(destination.x) ||
                !float.IsFinite(destination.y) || !float.IsFinite(destination.z))
                return false;
            _movementEnabled = true;
            _destinationOverride = true;
            _agent.isStopped = IsStunned;
            return _agent.SetDestination(destination);
        }

        // A committed charge is a straight, swept move, never a path to the moving target.
        public float MoveCharge(Vector3 direction, float distance, out bool blocked)
        {
            blocked = true;
            if (IsStunned) { blocked = false; return 0f; }
            if (!CanUseAgent() || distance <= 0f) return 0f;
            distance *= MovementMultiplier;
            Stop();
            Vector3 origin = transform.position;
            float allowed = distance;
            if (_agent.Raycast(origin + direction * distance, out NavMeshHit navHit))
                allowed = Mathf.Min(allowed, Mathf.Max(0f, Vector3.Distance(origin, navHit.position) - 0.02f));

            // Detect solid scene obstacles not yet represented by a carved/baked NavMesh.
            float radius = _agent.radius;
            Vector3 bottom = origin + Vector3.up * (radius + 0.05f);
            Vector3 top = origin + Vector3.up * Mathf.Max(radius + 0.05f, _agent.height - radius);
            foreach (RaycastHit hit in Physics.CapsuleCastAll(bottom, top, radius, direction,
                         allowed, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                Transform hitTransform = hit.collider.transform;
                if (hitTransform.IsChildOf(transform) ||
                    (_target != null && (hitTransform.IsChildOf(_target) || _target.IsChildOf(hitTransform))) ||
                    hit.collider.GetComponentInParent<EnemyController>() != null) continue;
                allowed = Mathf.Min(allowed, Mathf.Max(0f, hit.distance - 0.02f));
            }

            _agent.Move(direction * allowed);
            float moved = Vector3.Distance(origin, transform.position);
            blocked = allowed < distance - 0.001f || moved < allowed - 0.01f;
            return moved;
        }

        public bool IsInitialized =>
            _isInitialized;
        
        public bool IsMovementEnabled =>
            _movementEnabled;

        public Transform Target =>
            _target;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _enemy = GetComponent<EnemyController>();
        }

        private void Update()
        {
            if (IsStunned) return;
            if (_stunStoppedAgent) ClearStun();
            if (!_isInitialized ||
                !_movementEnabled ||
                _target == null)
            {
                return;
            }

            if (Time.deltaTime <= 0f) return;
            _destinationUpdateTimer -= Time.deltaTime;

            if (_destinationOverride)
                return;

            if (_destinationUpdateTimer > 0f)
            {
                return;
            }

            _destinationUpdateTimer =
                _destinationUpdateInterval;

            TryUpdateDestination();
        }

        public void Initialize(
            EnemyDefinition definition,
            Transform target)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            if (target == null)
            {
                throw new ArgumentNullException(
                    nameof(target));
            }

            EnsureAgentReference();
            _enemy = GetComponent<EnemyController>();
            ClearInvestigation();

            _target = target;

            _destinationUpdateInterval =
                definition.DestinationUpdateInterval;

            _baseSpeed = definition.MovementSpeed;
            ClearStun();
            _movementModifiers.Clear(); MovementMultiplier = 1;
            _agent.speed = _baseSpeed;

            _agent.acceleration =
                definition.Acceleration;

            _agent.angularSpeed =
                definition.AngularSpeed;

            _agent.stoppingDistance =
                definition.StoppingDistance;

            _agent.autoBraking = true;
            _agent.updatePosition = true;
            _agent.updateRotation = true;

            _destinationUpdateTimer = 0f;
            _isInitialized = true;
            _movementEnabled = true;
            _destinationOverride = false;

            Resume();
        }

        public void Resume()
        {
            if (!_isInitialized)
            {
                return;
            }

            if (!SightAllowsMovement) { if (_movementEnabled) Stop(); return; }
            if (_enemy != null && _enemy.RequiresPerception) _destinationOverride = false;
            ClearInvestigation();
            _movementEnabled = true;
            _destinationUpdateTimer = 0f;

            if (CanUseAgent())
            {
                _agent.isStopped = IsStunned;
                TryUpdateDestination();
            }
        }

        public void Stop()
        {
            _movementEnabled = false;
            _destinationOverride = false;

            if (!CanUseAgent())
            {
                return;
            }

            _agent.ResetPath();
            _agent.isStopped = true;
        }

        private void TryUpdateDestination()
        {
            if (!SightAllowsMovement) { if (_movementEnabled) Stop(); return; }
            if (IsStunned) return;
            if (_target == null ||
                !CanUseAgent())
            {
                return;
            }

            if (_agent.isStopped)
            {
                _agent.isStopped = false;
            }

            _agent.SetDestination(_target.position);
        }

        private bool CanUseAgent()
        {
            return _agent != null &&
                   _agent.enabled &&
                   _agent.isActiveAndEnabled &&
                   _agent.isOnNavMesh;
        }

        private void EnsureAgentReference()
        {
            if (_agent == null)
            {
                _agent = GetComponent<NavMeshAgent>();
            }
        }
    }
}
