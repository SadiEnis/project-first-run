using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Enemies.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyController))]
    public sealed class EnemyDeathView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform _visual;

        [Header("Death Presentation")]
        [SerializeField]
        private Vector3 _deathLocalEulerAngles =
            new Vector3(0f, 0f, 90f);

        private EnemyController _enemyController;
        private Quaternion _initialLocalRotation;

        private void Awake()
        {
            _enemyController =
                GetComponent<EnemyController>();

            if (_visual == null)
            {
                Debug.LogError(
                    $"{nameof(EnemyDeathView)} on '{name}' " +
                    "requires a visual Transform.",
                    this);

                enabled = false;
                return;
            }

            _initialLocalRotation =
                _visual.localRotation;
        }

        private void OnEnable()
        {
            if (_enemyController == null ||
                _visual == null)
            {
                return;
            }

            _visual.localRotation =
                _initialLocalRotation;

            _enemyController.Died += HandleDied;
        }

        private void OnDisable()
        {
            if (_enemyController != null)
            {
                _enemyController.Died -= HandleDied;
            }
        }

        private void HandleDied(
            EnemyController enemy,
            DamageInfo damageInfo,
            DamageResult damageResult)
        {
            _visual.localRotation =
                _initialLocalRotation *
                Quaternion.Euler(_deathLocalEulerAngles);
        }
    }
}