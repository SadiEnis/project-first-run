#if UNITY_EDITOR

using ProjectFirstRun.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectFirstRun.Development.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class CombatDebugRaycaster : MonoBehaviour
    {
        [Header("Damage")]
        [SerializeField, Min(0.01f)]
        private float _damage = 25f;

        [SerializeField, Min(0.1f)]
        private float _range = 100f;

        [SerializeField]
        private LayerMask _hitMask = ~0;

        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Update()
        {
            bool damagePressed =
                Mouse.current?.leftButton.wasPressedThisFrame == true ||
                Gamepad.current?.rightTrigger.wasPressedThisFrame == true;

            bool resetPressed =
                Keyboard.current?.rKey.wasPressedThisFrame == true ||
                Gamepad.current?.buttonNorth.wasPressedThisFrame == true;

            if (damagePressed)
            {
                TryApplyDamage();
            }

            if (resetPressed)
            {
                TryResetHealth();
            }
        }

        private void TryApplyDamage()
        {
            Ray ray = CreateAimRay();

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    _range,
                    _hitMask,
                    QueryTriggerInteraction.Ignore))
            {
                return;
            }

            IDamageable damageable = FindDamageable(hit.collider);

            if (damageable == null)
            {
                Debug.Log(
                    $"Hit '{hit.collider.name}', but it is not damageable.",
                    hit.collider);

                return;
            }

            DamageInfo damageInfo = new DamageInfo(
                _damage,
                gameObject,
                hit.point,
                ray.direction);

            DamageResult result =
                damageable.ApplyDamage(in damageInfo);

            Debug.Log(
                $"Damage result — " +
                $"Target: {hit.collider.name}, " +
                $"Requested: {result.RequestedDamage}, " +
                $"Applied: {result.AppliedDamage}, " +
                $"Previous Health: {result.PreviousHealth}, " +
                $"Current Health: {result.CurrentHealth}, " +
                $"Lethal: {result.WasLethal}",
                hit.collider);
        }

        private void TryResetHealth()
        {
            Ray ray = CreateAimRay();

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    _range,
                    _hitMask,
                    QueryTriggerInteraction.Ignore))
            {
                return;
            }

            HealthComponent healthComponent =
                hit.collider.GetComponentInParent<HealthComponent>();

            if (healthComponent == null)
            {
                return;
            }

            healthComponent.ResetHealth();

            Debug.Log(
                $"Health reset — " +
                $"Target: {healthComponent.name}, " +
                $"Current Health: {healthComponent.CurrentHealth}",
                healthComponent);
        }

        private Ray CreateAimRay()
        {
            return _camera.ViewportPointToRay(
                new Vector3(0.5f, 0.5f, 0f));
        }

        private static IDamageable FindDamageable(Collider hitCollider)
        {
            MonoBehaviour[] behaviours =
                hitCollider.GetComponentsInParent<MonoBehaviour>(true);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IDamageable damageable)
                {
                    return damageable;
                }
            }

            return null;
        }

        private void OnValidate()
        {
            _damage = Mathf.Max(0.01f, _damage);
            _range = Mathf.Max(0.1f, _range);
        }
    }
}

#endif