using System;
using UnityEngine;

namespace ProjectFirstRun.Chests.Spawning
{
    [DisallowMultipleComponent]
    public sealed class ChestSpawnPlacement : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float _firstRingRadius = 2.5f;
        [SerializeField, Min(0.1f)] private float _ringSpacing = 1.75f;
        [SerializeField] private LayerMask _groundLayers = 1 << 7;
        [SerializeField] private LayerMask _obstructionLayers = Physics.DefaultRaycastLayers;

        public void ValidatePrefab(GameObject prefab)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            BoxCollider box = prefab.GetComponent<BoxCollider>();
            if (box == null || !box.enabled || box.isTrigger ||
                prefab.GetComponentsInChildren<Collider>(true).Length != 1)
                throw new InvalidOperationException("Chest placement requires one enabled solid root BoxCollider.");
            Vector3 scale = prefab.transform.localScale;
            if (!IsPositive(scale.x) || !IsPositive(scale.y) || !IsPositive(scale.z) ||
                !IsPositive(box.size.x) || !IsPositive(box.size.y) || !IsPositive(box.size.z) ||
                !IsPositive(_firstRingRadius) || !IsPositive(_ringSpacing))
                throw new InvalidOperationException("Chest placement dimensions and distances must be finite and positive.");
        }

        public bool TryFind(Transform player, GameObject prefab, out Vector3 position, out Quaternion rotation)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            return TryFind(player.position, player.forward, prefab, false, out position, out rotation);
        }

        public bool TryFind(Vector3 origin, Vector3 facing, GameObject prefab, bool includeOrigin,
            out Vector3 position, out Quaternion rotation)
        {
            if (!IsFinite(origin) || !IsFinite(facing))
                throw new ArgumentException("Chest placement origin and facing must be finite.");
            ValidatePrefab(prefab);
            position = default;
            Vector3 forward = Vector3.ProjectOnPlane(facing, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
            rotation = Quaternion.LookRotation(forward);
            BoxCollider box = prefab.GetComponent<BoxCollider>();
            Vector3 half = Vector3.Scale(box.size, prefab.transform.localScale) * 0.5f;
            Vector3 localCenter = Vector3.Scale(box.center, prefab.transform.localScale);

            for (int ring = includeOrigin ? -1 : 0; ring < 3; ring++)
            {
                float radius = ring < 0 ? 0f : _firstRingRadius + ring * _ringSpacing;
                for (int sample = 0; sample < (ring < 0 ? 1 : 8); sample++)
                {
                    // Front, front-right, front-left, right, left, then the rear candidates.
                    float angle = sample == 0 ? 0f : ((sample + 1) / 2) * 45f * (sample % 2 == 1 ? 1f : -1f);
                    Vector3 candidate = origin + Quaternion.AngleAxis(angle, Vector3.up) * forward * radius;
                    if (!TryGround(candidate, out RaycastHit ground) ||
                        Mathf.Abs(ground.point.y - origin.y) > 1.5f)
                        continue;

                    Vector3 root = ground.point + Vector3.up * (half.y - localCenter.y + 0.02f);
                    Vector3 center = root + rotation * localCenter;
                    if (!HasFootprintSupport(center, half, rotation, ground.point.y)) continue;
                    if (Physics.CheckBox(center, half + new Vector3(0.1f, 0f, 0.1f), rotation,
                            _obstructionLayers, QueryTriggerInteraction.Ignore)) continue;
                    if (Physics.Linecast(origin + Vector3.up, center,
                            _obstructionLayers, QueryTriggerInteraction.Ignore)) continue;

                    position = root;
                    return true;
                }
            }

            return false;
        }

        private bool HasFootprintSupport(Vector3 center, Vector3 half, Quaternion rotation, float groundY)
        {
            for (int x = -1; x <= 1; x += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 corner = center + rotation * new Vector3(x * half.x, 0f, z * half.z);
                corner.y = groundY;
                if (!TryGround(corner, out RaycastHit support) || Mathf.Abs(support.point.y - groundY) > 0.05f)
                    return false;
            }
            return true;
        }

        private bool TryGround(Vector3 point, out RaycastHit hit) =>
            Physics.Raycast(point + Vector3.up * 3f, Vector3.down, out hit, 6f,
                _groundLayers, QueryTriggerInteraction.Ignore) && hit.normal.y > 0.99f;

        private static bool IsPositive(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;

        private static bool IsFinite(Vector3 value) =>
            float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
    }
}
