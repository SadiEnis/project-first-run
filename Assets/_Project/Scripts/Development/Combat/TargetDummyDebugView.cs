#if UNITY_EDITOR

using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Development.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent))]
    public sealed class TargetDummyDebugView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform _visual;

        [Header("Death Presentation")]
        [SerializeField]
        private Vector3 _deathRotation =
            new Vector3(0f, 0f, 90f);

        private HealthComponent _healthComponent;
        private Quaternion _initialLocalRotation;

        private void Awake()
        {
            _healthComponent = GetComponent<HealthComponent>();

            if (_visual == null)
            {
                Debug.LogError(
                    $"{nameof(TargetDummyDebugView)} on '{name}' requires a visual.",
                    this);

                enabled = false;
                return;
            }

            _initialLocalRotation = _visual.localRotation;
        }

        private void OnEnable()
        {
            _healthComponent.Died += HandleDied;
            _healthComponent.HealthReset += HandleHealthReset;
        }

        private void OnDisable()
        {
            _healthComponent.Died -= HandleDied;
            _healthComponent.HealthReset -= HandleHealthReset;
        }

        private void HandleDied(
            DamageInfo damageInfo,
            DamageResult damageResult)
        {
            _visual.localRotation =
                _initialLocalRotation *
                Quaternion.Euler(_deathRotation);
        }

        private void HandleHealthReset()
        {
            _visual.localRotation = _initialLocalRotation;
        }
    }
}

#endif