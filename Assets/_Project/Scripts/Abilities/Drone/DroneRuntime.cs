using System;
using ProjectFirstRun.Abilities.Execution;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;
namespace ProjectFirstRun.Abilities.Drone
{
    public sealed class DroneRuntime : IAbilityExecutor, IContinuousAbilityRuntime, IMapEnemyRegistryBinding
    {
        private EnemyRegistry _registry;
        private readonly GameObject _source;
        private readonly PlayerStatCollection _stats;
        private readonly Material _material;
        private readonly int _worldMask, _hitMask;
        private readonly DroneView[] _views = new DroneView[2];
        private readonly float[] _remaining = new float[2];
        private DroneConfig _config;
        public DroneRuntime(DroneDefinition definition, EnemyRegistry registry, GameObject source, PlayerStatCollection stats)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            _config = definition.CreateLevelConfigs()[0];
            _source = source != null ? source : throw new ArgumentNullException(nameof(source));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _material = definition.Material; _worldMask = definition.WorldMask; _hitMask = definition.HitMask;
            BindEnemyRegistry(registry);
        }
        public void BindEnemyRegistry(EnemyRegistry registry)
        {
            _registry = registry != null ? registry : throw new ArgumentNullException(nameof(registry));
            ClearViews();
        }
        internal void ApplyConfiguration(DroneConfig config)
        {
            if (config.Count > _config.Count) _remaining[1] = .5f / config.Rate;
            _config = config;
        }
        private void ClearViews()
        {
            for (int i = 0; i < _views.Length; i++)
            {
                if (_views[i] != null) { _views[i].gameObject.SetActive(false); UnityEngine.Object.Destroy(_views[i].gameObject); }
                _views[i] = null;
            }
        }
        private bool SourceAlive()
        {
            if (_source == null || !_source.activeInHierarchy) return false;
            var health = _source.GetComponent<HealthComponent>();
            return health == null || !health.IsDead;
        }
        private static bool Alive(EnemyController enemy) => enemy != null && enemy.IsInitialized &&
            enemy.isActiveAndEnabled && !enemy.IsDead && !enemy.Health.IsDead;
        private static Vector3 AimPoint(EnemyController enemy)
        {
            foreach (var collider in enemy.GetComponentsInChildren<Collider>())
                if (collider.enabled && !collider.isTrigger) return collider.bounds.center;
            return enemy.transform.position;
        }
        private bool Ignore(Collider collider) => collider == null || collider.isTrigger ||
            collider.transform.IsChildOf(_source.transform) || collider.GetComponentInParent<DroneView>() != null;
        private bool World(Collider collider) => !Ignore(collider) &&
            collider.GetComponentInParent<HealthComponent>() == null && collider.GetComponentInParent<EnemyController>() == null;
        private bool InsideWorld(Vector3 point)
        {
            foreach (var collider in Physics.OverlapSphere(point, .05f, _worldMask, QueryTriggerInteraction.Ignore))
                if (World(collider)) return true;
            return false;
        }
        private bool Visible(Vector3 origin, Vector3 point)
        {
            if (InsideWorld(origin)) return false;
            Vector3 delta = point - origin;
            foreach (var hit in Physics.RaycastAll(origin, delta.normalized, delta.magnitude, _worldMask, QueryTriggerInteraction.Ignore))
                if (World(hit.collider)) return false;
            return true;
        }
        private Vector3 Position(Vector3 anchor, int index)
        {
            Vector3 offset = _source.transform.right * (index == 0 ? .8f : -.8f) + Vector3.up * .35f + _source.transform.forward * .8f;
            float distance = offset.magnitude;
            foreach (var hit in Physics.SphereCastAll(anchor, .25f, offset.normalized, distance, _worldMask, QueryTriggerInteraction.Ignore))
                if (World(hit.collider)) distance = Mathf.Min(distance, Mathf.Max(0, hit.distance - .02f));
            return anchor + offset.normalized * distance;
        }
        private EnemyController Select(Vector3 origin)
        {
            EnemyController best = null; float nearest = _config.Range * _config.Range;
            foreach (var enemy in _registry.ActiveEnemies)
            {
                if (!Alive(enemy)) continue;
                Vector3 point = AimPoint(enemy); float squared = (point - origin).sqrMagnitude;
                if (squared > nearest || !Visible(origin, point)) continue;
                best = enemy; nearest = squared;
            }
            return best;
        }
        private bool Registered(EnemyController enemy)
        {
            foreach (var candidate in _registry.ActiveEnemies) if (candidate == enemy) return true;
            return false;
        }
        public void TickContinuous(float deltaTime, Vector3 origin, bool controlEnabled)
        {
            if (!float.IsFinite(deltaTime) || deltaTime < 0) throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (!SourceAlive()) { ClearViews(); return; }
            if (_registry == null || !controlEnabled || Time.timeScale <= 0 || deltaTime == 0) return;
            if (!float.IsFinite(origin.x) || !float.IsFinite(origin.y) || !float.IsFinite(origin.z))
                throw new ArgumentException("Drone origin must be finite.");
            for (int i = 0; i < _config.Count && SourceAlive(); i++)
            {
                if (_views[i] == null) _views[i] = DroneView.Create(_source, _material, i);
                Vector3 desired = Position(origin, i);
                _views[i].transform.position = desired;
                _remaining[i] = Mathf.Max(0, _remaining[i] - deltaTime);
                var enemy = Select(desired);
                if (enemy == null) continue;
                Vector3 direction = AimPoint(enemy) - desired;
                if (direction.sqrMagnitude < .000001f) continue;
                _views[i].transform.rotation = Quaternion.LookRotation(direction);
                if (_remaining[i] > 0 || InsideWorld(origin)) continue;
                float rate = _config.Rate * (enemy.Definition.Rank == EnemyRank.Boss ? _config.BossMultiplier : 1);
                Fire(i, desired, direction.normalized);
                _remaining[i] = 1 / rate;
            }
        }
        private void Fire(int index, Vector3 origin, Vector3 direction)
        {
            float damage = _stats.Evaluate(PlayerStatType.AbilityDamage, _config.Damage);
            if (!float.IsFinite(damage) || damage <= 0) throw new InvalidOperationException("Invalid drone damage.");
            Collider collider = null; float distance = _config.Range; Vector3 end = origin + direction * distance;
            // Initial overlaps must not allow rays to escape through cover or an enclosing enemy.
            foreach (var overlap in Physics.OverlapSphere(origin, .02f, _hitMask, QueryTriggerInteraction.Ignore))
            {
                if (Ignore(overlap)) continue;
                if (collider == null || World(overlap)) collider = overlap;
                distance = 0; end = origin;
            }
            if (collider == null)
                foreach (var hit in Physics.RaycastAll(origin, direction, _config.Range, _hitMask, QueryTriggerInteraction.Ignore))
                    if (!Ignore(hit.collider) && hit.distance <= distance)
                    { collider = hit.collider; distance = hit.distance; end = hit.point; }
            _views[index].ShowShot(end);
            if (collider == null) return;
            var enemy = collider.GetComponentInParent<EnemyController>();
            if (!Alive(enemy) || !Registered(enemy) || !SourceAlive()) return;
            var info = new DamageInfo(damage, _source, end, direction);
            enemy.Health.ApplyDamage(in info);
        }
        // Persistent fire is exclusively scheduled by TickContinuous.
        public AbilityExecutionResult TryExecute(in AbilityExecutionContext context) => AbilityExecutionResult.Failed;
    }
}
