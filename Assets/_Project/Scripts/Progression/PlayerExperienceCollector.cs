using System;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Stats;
using UnityEngine;

namespace ProjectFirstRun.Progression
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerExperienceController), typeof(HealthComponent))]
    public sealed class PlayerExperienceCollector : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _attractionRadius = 3f;
        [SerializeField] private Vector3 _collectionOffset = new Vector3(0f, 0.75f, 0f);
        [SerializeField] private LayerMask _pickupLayers = 1 << 2;

        private Collider[] _nearbyColliders = new Collider[16];
        private PlayerExperienceController _experience;
        private HealthComponent _health;

        private PlayerStatsController _stats;
        public float AttractionRadius
        {
            get
            {
                if (_stats == null) _stats = GetComponent<PlayerStatsController>();
                float radius = _stats == null || !_stats.IsInitialized ? _attractionRadius :
                    _stats.Evaluate(PlayerStatType.PickupRadius, _attractionRadius);
                if (!float.IsFinite(radius) || radius < 0f)
                    throw new InvalidOperationException("Pickup radius must be finite and non-negative.");
                return radius;
            }
        }
        public Vector3 CollectionPosition => transform.TransformPoint(_collectionOffset);

        // Sets the base radius; active stat bonuses are evaluated separately.
        public void SetAttractionRadius(float radius)
        {
            if (float.IsNaN(radius) || float.IsInfinity(radius) || radius < 0f)
                throw new ArgumentOutOfRangeException(nameof(radius));
            _attractionRadius = radius;
        }

        public bool CanAttract(Vector3 position)
        {
            if (!CanCollect) return false;
            float radius = AttractionRadius;
            return radius > 0f && (position - CollectionPosition).sqrMagnitude <= radius * radius;
        }

        public PlayerExperienceController Experience
        {
            get
            {
                EnsureReferences();
                return _experience;
            }
        }

        public bool CanCollect
        {
            get
            {
                EnsureReferences();
                return isActiveAndEnabled && _experience.isActiveAndEnabled &&
                       _experience.IsInitialized && !_health.IsDead && Time.timeScale > 0f;
            }
        }

        private void Awake() => EnsureReferences();

        private void FixedUpdate()
        {
            if (!CanCollect)
                return;
            float radius = AttractionRadius;
            if (radius <= 0f) return;

            int count;
            // Grow only on saturation; never silently skip pickups in a dense pile.
            while (true)
            {
                count = Physics.OverlapSphereNonAlloc(CollectionPosition, radius,
                    _nearbyColliders, _pickupLayers, QueryTriggerInteraction.Collide);
                if (count < _nearbyColliders.Length)
                    break;
                Array.Resize(ref _nearbyColliders, _nearbyColliders.Length * 2);
            }

            for (int index = 0; index < count; index++)
            {
                Collider nearby = _nearbyColliders[index];
                _nearbyColliders[index] = null;
                if (nearby != null && nearby.TryGetComponent(out ExperiencePickup pickup))
                    pickup.AttractTowards(this, Time.fixedDeltaTime);
            }
        }

        private void OnValidate()
        {
            if (float.IsNaN(_attractionRadius) || float.IsInfinity(_attractionRadius))
                _attractionRadius = 3f;
            _attractionRadius = Mathf.Max(0f, _attractionRadius);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(CollectionPosition, Application.isPlaying ? AttractionRadius : _attractionRadius);
        }

        private void EnsureReferences()
        {
            if (_experience == null)
                _experience = GetComponent<PlayerExperienceController>();
            if (_health == null)
                _health = GetComponent<HealthComponent>();
        }
    }
}
