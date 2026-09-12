using System.Collections;
using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Enemies.Lifecycle
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyController))]
    public sealed class EnemyDeathLifecycle : MonoBehaviour
    {
        [Header("Cleanup")]
        [SerializeField, Min(0f)]
        private float _destroyDelay = 1f;

        [SerializeField]
        private Collider[] _collidersToDisable;

        private EnemyController _enemyController;
        private bool _deathSequenceStarted;
        private Coroutine _cleanupRoutine;

        public bool IsDeathSequenceStarted =>
            _deathSequenceStarted;

        public float DestroyDelay =>
            _destroyDelay;

        private void Awake()
        {
            _enemyController =
                GetComponent<EnemyController>();
        }

        private void OnEnable()
        {
            _enemyController.Died +=
                HandleEnemyDied;
        }

        private void OnDisable()
        {
            if (_enemyController != null)
            {
                _enemyController.Died -=
                    HandleEnemyDied;
            }
        }

        private void HandleEnemyDied(
            EnemyController enemy,
            DamageInfo damageInfo,
            DamageResult damageResult)
        {
            if (_deathSequenceStarted)
            {
                return;
            }

            BeginDeathSequence();
        }

        private void BeginDeathSequence()
        {
            _deathSequenceStarted = true;

            DisableColliders();

            if (_destroyDelay <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            _cleanupRoutine =
                StartCoroutine(
                    DestroyAfterDelay());
        }

        private void DisableColliders()
        {
            if (_collidersToDisable == null)
            {
                return;
            }

            foreach (Collider targetCollider
                     in _collidersToDisable)
            {
                if (targetCollider != null)
                {
                    targetCollider.enabled = false;
                }
            }
        }

        private IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds(
                _destroyDelay);

            _cleanupRoutine = null;

            Destroy(gameObject);
        }

        private void OnValidate()
        {
            if (float.IsNaN(_destroyDelay) ||
                float.IsInfinity(_destroyDelay))
            {
                _destroyDelay = 1f;
                return;
            }

            _destroyDelay =
                Mathf.Max(0f, _destroyDelay);
        }
    }
}