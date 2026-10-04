using System;
using System.Collections.Generic;
using ProjectFirstRun.Abilities.Execution;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Stats;
using UnityEngine;
namespace ProjectFirstRun.Abilities.Shuriken
{
    public sealed class ShurikenRuntime : IAbilityExecutor, IContinuousAbilityRuntime, IMapEnemyRegistryBinding
    {
        private EnemyRegistry _registry;
        private readonly GameObject _source;
        private readonly PlayerStatCollection _stats;
        private readonly ShurikenDefinition _definition;
        private ShurikenConfig _config, _active;
        private ShurikenView _view;
        private bool _running;
        private float _elapsed, _cooldown, _damage, _bleed;
        private int _turn;
        private Vector3 _previousCenter;
        private readonly HashSet<(EnemyController, int)>[] _hits =
            { new HashSet<(EnemyController, int)>(), new HashSet<(EnemyController, int)>() };
        public bool IsOrbiting => _running;
        public float CooldownRemaining => _cooldown;
        public ShurikenRuntime(ShurikenDefinition definition, EnemyRegistry registry, GameObject source, PlayerStatCollection stats)
        {
            _definition = definition != null ? definition : throw new ArgumentNullException(nameof(definition));
            _config = definition.CreateLevelConfigs()[0];
            _source = source != null ? source : throw new ArgumentNullException(nameof(source));
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            BindEnemyRegistry(registry);
        }
        public void BindEnemyRegistry(EnemyRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            CancelOrbit(); _registry = registry;
        }
        internal void ApplyConfiguration(ShurikenConfig config) => _config = config;
        private bool SourceAlive => _source != null && _source.activeInHierarchy &&
            (_source.GetComponent<HealthComponent>() == null || !_source.GetComponent<HealthComponent>().IsDead);
        private static bool Alive(EnemyController enemy) => enemy != null && enemy.IsInitialized &&
            enemy.isActiveAndEnabled && !enemy.IsDead && !enemy.Health.IsDead;
        public void CancelOrbit()
        {
            if (_running) _cooldown = _active.Cooldown * AbilityCooldownScaling.Multiplier(_stats);
            _running = false;
            foreach (var set in _hits) set.Clear();
            if (_view != null)
            {
                var view = _view; _view = null; view.Cancelled = null;
                view.gameObject.SetActive(false); UnityEngine.Object.Destroy(view.gameObject);
            }
        }
        private Vector3 Position(Vector3 center, float elapsed, int index)
        {
            float angle = elapsed / _active.TurnSeconds * Mathf.PI * 2 + index * Mathf.PI;
            return center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * _active.Radius;
        }
        private bool World(Collider collider) => collider != null && !collider.isTrigger &&
            !collider.transform.IsChildOf(_source.transform) &&
            collider.GetComponentInParent<HealthComponent>() == null &&
            collider.GetComponentInParent<EnemyController>() == null &&
            collider.GetComponentInParent<ShurikenView>() == null;
        private bool Visible(Vector3 from, Vector3 to)
        {
            foreach (var overlap in Physics.OverlapSphere(from, .02f, _definition.WorldMask, QueryTriggerInteraction.Ignore))
                if (World(overlap)) return false;
            var delta = to - from;
            if (delta.sqrMagnitude < .000001f) return true;
            foreach (var hit in Physics.RaycastAll(from, delta.normalized, delta.magnitude, _definition.WorldMask, QueryTriggerInteraction.Ignore))
                if (World(hit.collider)) return false;
            return true;
        }
        private void Sweep(int index, Vector3 from, Vector3 to, Vector3 center)
        {
            var registered = new HashSet<EnemyController>(_registry.ActiveEnemies);
            foreach (var collider in Physics.OverlapCapsule(from, to, _definition.ContactRadius,
                         Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (!SourceAlive || !_running) break;
                var enemy = collider.GetComponentInParent<EnemyController>();
                if (!Alive(enemy) || !registered.Contains(enemy) || _hits[index].Contains((enemy, enemy.SpawnVersion))) continue;
                Vector3 segment = to - from;
                float t = segment.sqrMagnitude > .000001f
                    ? Mathf.Clamp01(Vector3.Dot(collider.bounds.center - from, segment) / segment.sqrMagnitude) : 0;
                Vector3 sample = from + segment * t;
                Vector3 contact = collider.ClosestPoint(sample);
                if (!Visible(center, contact) || !Visible(from, contact) || !Visible(sample, contact)) continue;
                _hits[index].Add((enemy, enemy.SpawnVersion));
                var info = new DamageInfo(_damage, _source, contact, (contact - center).normalized);
                var result = enemy.Health.ApplyDamage(in info);
                if (result.WasApplied && Alive(enemy) && _bleed > 0) ShurikenBleed.Apply(_source, enemy, _bleed);
            }
        }
        public void TickContinuous(float deltaTime, Vector3 origin, bool controlEnabled)
        {
            if (!float.IsFinite(deltaTime) || deltaTime < 0) throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (!SourceAlive || !controlEnabled || _registry == null) { CancelOrbit(); return; }
            if (Time.timeScale <= 0 || deltaTime == 0) return;
            if (!float.IsFinite(origin.x) || !float.IsFinite(origin.y) || !float.IsFinite(origin.z))
                throw new ArgumentException("Shuriken origin must be finite.");
            // Anchor at the player's torso, not a forward weapon muzzle.
            var body = _source.GetComponent<Collider>();
            Vector3 center = body != null && body.enabled ? body.bounds.center : _source.transform.position + Vector3.up;
            if (_running && (_view == null || Vector3.Distance(center, _previousCenter) > 6))
            { CancelOrbit(); return; }
            float remaining = deltaTime;
            if (!_running)
            {
                float wait = Mathf.Min(remaining, _cooldown); _cooldown -= wait; remaining -= wait;
                if (_cooldown > 0 || remaining <= 0) return;
                _active = _config;
                _damage = _stats.Evaluate(PlayerStatType.AbilityDamage, _active.Damage);
                _bleed = _active.BleedDamage > 0 ? _stats.Evaluate(PlayerStatType.AbilityDamage, _active.BleedDamage) : 0;
                if (!float.IsFinite(_damage) || _damage <= 0 || !float.IsFinite(_bleed) || _bleed < 0)
                    throw new InvalidOperationException("Invalid shuriken damage.");
                _view = ShurikenView.Create(_source, _definition.Material, _active.Count, _registry.gameObject.scene);
                _view.Cancelled = CancelOrbit;
                _running = true; _elapsed = 0; _turn = 0; _previousCenter = center;
                foreach (var set in _hits) set.Clear();
            }
            float consumed = 0, duration = remaining;
            Vector3 startCenter = _previousCenter;
            float movementStep = Vector3.Distance(center, startCenter) > .001f
                ? duration * .1f / Vector3.Distance(center, startCenter) : duration;
            while (remaining > .000001f && _running && SourceAlive)
            {
                float boundary = (_turn + 1) * _active.TurnSeconds;
                float step = Mathf.Min(remaining, _active.TurnSeconds / 72, boundary - _elapsed, movementStep);
                Vector3 fromCenter = Vector3.Lerp(startCenter, center, consumed / duration);
                Vector3 toCenter = Vector3.Lerp(startCenter, center, (consumed + step) / duration);
                for (int i = 0; i < _active.Count && _running; i++)
                    Sweep(i, Position(fromCenter, _elapsed, i), Position(toCenter, _elapsed + step, i), toCenter);
                if (!_running || !SourceAlive) { CancelOrbit(); return; }
                _elapsed += step; consumed += step; remaining -= step;
                if (_elapsed >= boundary - .00001f)
                {
                    _elapsed = boundary; _turn++;
                    foreach (var set in _hits) set.Clear();
                    if (_turn == 2)
                    {
                        CancelOrbit();
                        _cooldown = Mathf.Max(0, _cooldown - remaining);
                        return; // At most one new set per tick, even after a hitch.
                    }
                }
            }
            _previousCenter = center;
            for (int i = 0; i < _active.Count && _view != null; i++)
                _view.Position(i, Position(center, _elapsed, i), _elapsed / _active.TurnSeconds * 360);
        }
        public AbilityExecutionResult TryExecute(in AbilityExecutionContext context) => AbilityExecutionResult.Failed;
    }
}
