using System;
using System.Collections.Generic;
using ProjectFirstRun.Abilities.Targeting;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using UnityEngine;

namespace ProjectFirstRun.Abilities.Fireball
{
    /// <summary>Availability gate plus random, non-repeating target cycles for one volley.</summary>
    public sealed class FireballTargetSelector : IAbilityTargetSelector, IMapEnemyRegistryBinding
    {
        private EnemyRegistry _registry;
        private readonly GameObject _source;
        private readonly float _range;
        private readonly int _worldMask;
        private readonly Func<int, int> _randomIndex;
        public FireballTargetSelector(EnemyRegistry registry, GameObject source, float range, int worldMask,
            Func<int, int> randomIndex = null)
        {
            BindEnemyRegistry(registry);
            _source = source != null ? source : throw new ArgumentNullException(nameof(source));
            if (!float.IsFinite(range) || range <= 0) throw new ArgumentOutOfRangeException(nameof(range));
            _range = range; _worldMask = worldMask;
            var random = new System.Random(); _randomIndex = randomIndex ?? random.Next;
        }
        public void BindEnemyRegistry(EnemyRegistry registry) => _registry = registry != null
            ? registry : throw new ArgumentNullException(nameof(registry));
        public bool TrySelectTarget(Vector3 origin, out Transform target)
        {
            // Do not spend random draws on the runtime's availability probe.
            var candidates = Candidates(origin);
            target = candidates.Count == 0 ? null : candidates[0];
            return target != null;
        }
        public List<Transform> SelectVolley(Vector3 origin, int count)
        {
            if (count < 1 || count > 4) throw new ArgumentOutOfRangeException(nameof(count));
            var candidates = Candidates(origin); var result = new List<Transform>(count);
            if (candidates.Count == 0) return result;
            var remaining = new List<Transform>();
            while (result.Count < count)
            {
                if (remaining.Count == 0) remaining.AddRange(candidates);
                int index = _randomIndex(remaining.Count);
                if (index < 0 || index >= remaining.Count) throw new InvalidOperationException("Invalid random target index.");
                result.Add(remaining[index]); remaining.RemoveAt(index);
            }
            return result;
        }
        private List<Transform> Candidates(Vector3 origin)
        {
            if (!float.IsFinite(origin.x) || !float.IsFinite(origin.y) || !float.IsFinite(origin.z))
                throw new ArgumentException("Origin must be finite.", nameof(origin));
            if (_registry == null) throw new InvalidOperationException("Fireball registry is no longer available.");
            var result = new List<Transform>();
            if (_source == null) return result;
            foreach (var enemy in _registry.ActiveEnemies)
            {
                if (enemy == null || !enemy.IsInitialized || enemy.IsDead || !enemy.isActiveAndEnabled) continue;
                if (Eligible(enemy.transform, origin)) result.Add(enemy.transform);
            }
            return result;
        }
        public bool Eligible(Transform target, Vector3 origin)
        {
            if (target == null || !target.gameObject.activeInHierarchy) return false;
            var enemy = target.GetComponent<EnemyController>();
            if (enemy == null || !enemy.IsInitialized || enemy.IsDead || !enemy.isActiveAndEnabled) return false;
            Vector3 point = AimPoint(target), delta = point - origin;
            if (delta.sqrMagnitude > _range * _range) return false;
            foreach (var collider in Physics.OverlapSphere(origin, .001f, _worldMask, QueryTriggerInteraction.Ignore))
                if (World(collider)) return false;
            if (delta.sqrMagnitude > .000001f)
                foreach (var hit in Physics.RaycastAll(origin, delta.normalized, delta.magnitude, _worldMask, QueryTriggerInteraction.Ignore))
                    if (World(hit.collider)) return false;
            return true;
        }
        private bool World(Collider collider) => collider != null &&
            (_source == null || !collider.transform.IsChildOf(_source.transform)) &&
            collider.GetComponentInParent<FireballProjectile>() == null &&
            collider.GetComponentInParent<HealthComponent>() == null && collider.GetComponentInParent<EnemyController>() == null;
        public static Vector3 AimPoint(Transform target)
        {
            foreach (var collider in target.GetComponentsInChildren<Collider>())
                if (collider.enabled && !collider.isTrigger) return collider.bounds.center;
            return target.position;
        }
    }
}
