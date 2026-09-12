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

            _destinationUpdateTimer -= Time.deltaTime;

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