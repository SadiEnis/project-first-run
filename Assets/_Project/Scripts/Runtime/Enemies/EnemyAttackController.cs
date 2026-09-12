using System;
using ProjectFirstRun.Combat;
using UnityEngine;

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

            _attackEnabled = false;
        }

        private void Update()
        {
            if (!_isInitialized ||
                !_attackEnabled ||
                _target == null ||
                _targetDamageable == null ||
                _targetDamageableObject == null)
            {
                return;
            }

            _attackState.Tick(Time.deltaTime);

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