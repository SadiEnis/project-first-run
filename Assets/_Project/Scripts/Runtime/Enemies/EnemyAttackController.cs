using System;
using ProjectFirstRun.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectFirstRun.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyController))]
    public sealed class EnemyAttackController : MonoBehaviour
    {
        private EnemyController _enemyController;

        private Transform _target;
        private IDamageable _targetDamageable;
        private UnityEngine.Object _targetDamageableObject;

        private EnemyAttackState _attackState;
        private EnemyChargeState _chargeState;
        private EnemyMotor _motor;
        public EnemyChargeState ChargeState => _chargeState;
        private EnemyRangedState _rangedState;
        public EnemyRangedState RangedState => _rangedState;

        private bool _isInitialized;
        private bool _attackEnabled;

        public event Action<DamageInfo, DamageResult> AttackPerformed;
        public event Action<DamageInfo, DamageResult> DamageApplied;

        public bool IsInitialized =>
            _isInitialized;

        public bool IsAttackEnabled =>
            _attackEnabled;

        public Transform Target =>
            _target;

        public float CooldownRemaining =>
            _attackState != null
                ? _attackState.CooldownRemaining
                : 0f;

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            EnsureReferences();

            _enemyController.Died += HandleEnemyDied;

            if (_isInitialized &&
                !_enemyController.IsDead)
            {
                Resume();
            }
        }

        private void OnDisable()
        {
            if (_enemyController != null)
            {
                _enemyController.Died -= HandleEnemyDied;
            }

            Stop();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            if (!float.IsFinite(deltaTime) || deltaTime < 0)
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (_chargeState != null)
            {
                TickCharge(deltaTime);
                return;
            }
            if (_rangedState != null)
            {
                TickRanged(deltaTime);
                return;
            }
            if (deltaTime == 0f || _enemyController.IsDead || !_enemyController.isActiveAndEnabled) return;
            if (!_isInitialized ||
                !_attackEnabled ||
                _target == null ||
                _targetDamageable == null ||
                _targetDamageableObject == null)
            {
                return;
            }

            _attackState.Tick(deltaTime);

            if (!_attackState.IsReady)
            {
                return;
            }

            Vector3 offset =
                _target.position - transform.position;

            Vector3 planarOffset =
                Vector3.ProjectOnPlane(
                    offset,
                    Vector3.up);

            bool targetIsInRange =
                planarOffset.sqrMagnitude <=
                _attackState.RangeSquared;

            EnemyAttackAttemptResult attemptResult =
                _attackState.TryCommitAttack(
                    targetIsInRange);

            if (attemptResult !=
                EnemyAttackAttemptResult.Performed)
            {
                return;
            }

            PerformAttack(offset);
        }

        public void Initialize(
            EnemyDefinition definition,
            Transform target,
            IDamageable targetDamageable)
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

            if (targetDamageable == null)
            {
                throw new ArgumentNullException(
                    nameof(targetDamageable));
            }

            if (targetDamageable is not UnityEngine.Object
                damageableObject)
            {
                throw new ArgumentException(
                    "The target damageable must be a Unity object.",
                    nameof(targetDamageable));
            }

            EnsureReferences();

            EnemyAttackConfig config =
                definition.CreateAttackConfig();

            definition.ValidateBehavior();
            _chargeState = definition.Behavior == EnemyBehavior.Charger
                ? new EnemyChargeState(definition.CreateChargeConfig()) : null;
            _rangedState = definition.Behavior == EnemyBehavior.Ranger
                ? new EnemyRangedState(definition.CreateRangedConfig()) : null;
            _motor = GetComponent<EnemyMotor>();

            _target = target;
            _targetDamageable = targetDamageable;
            _targetDamageableObject = damageableObject;

            _attackState =
                new EnemyAttackState(in config);

            _isInitialized = true;
            _attackEnabled =
                isActiveAndEnabled &&
                !_enemyController.IsDead;
        }

        public void Resume()
        {
            if (!_isInitialized ||
                _enemyController.IsDead)
            {
                return;
            }

            _attackEnabled = true;
        }

        public void Stop()
        {
            _attackEnabled = false;
            if (_chargeState != null)
            {
                _chargeState.Cancel();
                if (_motor != null) _motor.Stop();
            }
            if (_rangedState != null)
            {
                _rangedState.Cancel();
                if (_motor != null) _motor.Stop();
            }
        }

        private void TickRanged(float deltaTime)
        {
            if (!_isInitialized || !_attackEnabled) return;
            if (_enemyController.IsDead || !_enemyController.isActiveAndEnabled ||
                _target == null || !_target.gameObject.activeInHierarchy || _targetDamageableObject == null ||
                (_targetDamageableObject is HealthComponent health && health.IsDead))
            {
                _rangedState.Cancel(); _motor.Stop(); return;
            }
            if (deltaTime == 0f) return;

            Vector3 offset = _target.position - transform.position;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (_rangedState.Phase == EnemyRangedPhase.Cooldown)
            {
                _rangedState.Tick(deltaTime);
                if (_rangedState.Phase == EnemyRangedPhase.Cooldown) return;
            }

            if (distance < _rangedState.Config.PreferredMin)
            {
                _rangedState.CancelWindup();
                _rangedState.SetApproaching();
                TryRetreat(offset);
                return;
            }
            if (distance > _rangedState.Config.PreferredMax)
            {
                _rangedState.CancelWindup();
                _rangedState.SetApproaching();
                _motor.Resume();
                return;
            }

            _motor.Stop();
            _rangedState.SetHolding();
            bool hasLineOfSight = HasLineOfSight();
            if (_rangedState.Phase == EnemyRangedPhase.Windup)
            {
                if (!hasLineOfSight)
                {
                    _rangedState.CancelWindup();
                    return;
                }
                _rangedState.Tick(deltaTime);
                if (_rangedState.TryRelease())
                {
                    FireRangedProjectile(_rangedState.Direction);
                    _rangedState.CommitRelease();
                }
                return;
            }
            _rangedState.TryBeginWindup(offset, hasLineOfSight);
        }

        private void TryRetreat(Vector3 offsetToTarget)
        {
            if (!_motor.CanNavigate || offsetToTarget.sqrMagnitude <= Mathf.Epsilon) return;
            Vector3 away = -offsetToTarget.normalized;
            Vector3 desired = _target.position + away * (_rangedState.Config.PreferredMax + 0.5f);
            if (NavMesh.SamplePosition(desired, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                _motor.TrySetDestination(hit.position);
            else
                _motor.Stop();
        }

        private bool HasLineOfSight()
        {
            Vector3 origin = transform.position + Vector3.up;
            Vector3 destination = _target.position + Vector3.up;
            RaycastHit[] hits = Physics.RaycastAll(origin, destination - origin,
                Vector3.Distance(origin, destination), Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null || hit.collider.transform.IsChildOf(transform)) continue;
                if (hit.collider.transform.IsChildOf(_target) || hit.collider.transform == _target) continue;
                return false;
            }
            return true;
        }

        private void FireRangedProjectile(Vector3 direction)
        {
            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = $"{name}_RangerProjectile";
            projectileObject.transform.position = transform.position + Vector3.up;
            projectileObject.transform.localScale = Vector3.one * (_rangedState.Config.ProjectileRadius * 2f);
            SphereCollider sphere = projectileObject.GetComponent<SphereCollider>();
            if (sphere == null) sphere = projectileObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            EnemyRangedProjectile projectile = projectileObject.AddComponent<EnemyRangedProjectile>();
            projectile.Initialize(direction, _attackState.Damage, _rangedState.Config.ProjectileSpeed,
                _rangedState.Config.ProjectileLifetime, _rangedState.Config.ProjectileRadius, gameObject);
            Renderer renderer = projectileObject.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = new Color(0.2f, 0.65f, 1f);
        }

        private void TickCharge(float deltaTime)
        {
            if (!_isInitialized || !_attackEnabled) return;
            if (_enemyController.IsDead || !_enemyController.isActiveAndEnabled ||
                _target == null || !_target.gameObject.activeInHierarchy || _targetDamageableObject == null ||
                (_targetDamageableObject is HealthComponent health && health.IsDead))
            {
                _chargeState.Cancel();
                _motor.Stop();
                return;
            }
            if (deltaTime == 0) return;

            switch (_chargeState.Phase)
            {
                case EnemyChargePhase.Pursuing:
                    if (_motor.CanNavigate && _chargeState.TryBegin(_target.position - transform.position))
                    {
                        _motor.Stop();
                        transform.rotation = Quaternion.LookRotation(_chargeState.Direction);
                    }
                    else if (!_motor.IsMovementEnabled) _motor.Resume();
                    break;
                case EnemyChargePhase.Windup:
                    _motor.Stop();
                    _chargeState.Tick(deltaTime);
                    break;
                case EnemyChargePhase.Charging:
                    Vector3 start = transform.position;
                    float moved = _motor.MoveCharge(_chargeState.Direction, _chargeState.StepDistance(deltaTime), out bool blocked);
                    Vector3 end = transform.position;
                    // Sweep against the target position so a long frame cannot skip a hit.
                    Vector3 segment = end - start;
                    float t = segment.sqrMagnitude > 0.000001f
                        ? Mathf.Clamp01(Vector3.Dot(_target.position - start, segment) / segment.sqrMagnitude) : 0;
                    if (Vector3.Distance(_target.position, start + segment * t) <= _chargeState.Config.HitRadius &&
                        _chargeState.TryCommitHit()) PerformAttack(_chargeState.Direction);
                    _chargeState.Advance(moved, blocked);
                    break;
                case EnemyChargePhase.Recovery:
                    _motor.Stop();
                    _chargeState.Tick(deltaTime);
                    if (_chargeState.Phase == EnemyChargePhase.Pursuing) _motor.Resume();
                    break;
            }
        }

        private void PerformAttack(
            Vector3 offsetToTarget)
        {
            Vector3 hitDirection =
                offsetToTarget.sqrMagnitude >
                Mathf.Epsilon
                    ? offsetToTarget.normalized
                    : transform.forward;

            DamageInfo damageInfo =
                new DamageInfo(
                    _attackState.Damage,
                    gameObject,
                    _target.position,
                    hitDirection);

            DamageResult damageResult =
                _targetDamageable.ApplyDamage(
                    in damageInfo);

            AttackPerformed?.Invoke(
                damageInfo,
                damageResult);

            if (damageResult.WasApplied)
            {
                DamageApplied?.Invoke(
                    damageInfo,
                    damageResult);
            }
        }

        private void HandleEnemyDied(
            EnemyController enemy,
            DamageInfo damageInfo,
            DamageResult damageResult)
        {
            Stop();
        }

        private void EnsureReferences()
        {
            if (_enemyController == null)
            {
                _enemyController =
                    GetComponent<EnemyController>();
            }
        }
    }
}
