using System;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectFirstRun.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class EnemyMotor : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Transform _target;

        private float _destinationUpdateInterval;
        private float _destinationUpdateTimer;

        private bool _isInitialized;
        private bool _movementEnabled;
        private bool _destinationOverride;

        public bool CanNavigate => CanUseAgent();

        public bool TrySetDestination(Vector3 destination)
        {
            if (!CanUseAgent() || !float.IsFinite(destination.x) ||
                !float.IsFinite(destination.y) || !float.IsFinite(destination.z))
                return false;
            _movementEnabled = true;
            _destinationOverride = true;
            _agent.isStopped = false;
            return _agent.SetDestination(destination);
        }

        // A committed charge is a straight, swept move, never a path to the moving target.
        public float MoveCharge(Vector3 direction, float distance, out bool blocked)
        {
            blocked = true;
            if (!CanUseAgent() || distance <= 0f) return 0f;
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
        }

        private void Update()
        {
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

            _target = target;

            _destinationUpdateInterval =
                definition.DestinationUpdateInterval;

            _agent.speed =
                definition.MovementSpeed;

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

            _movementEnabled = true;
            _destinationUpdateTimer = 0f;

            if (CanUseAgent())
            {
                _agent.isStopped = false;
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

            _agent.isStopped = true;
            _agent.ResetPath();
        }

        private void TryUpdateDestination()
        {
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
