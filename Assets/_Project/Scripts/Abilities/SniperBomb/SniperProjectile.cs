using System;
using System.Collections.Generic;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using UnityEngine;
namespace ProjectFirstRun.Abilities.SniperBomb
{
    public sealed class SniperProjectile : MonoBehaviour
    {
        private GameObject _source;
        private EnemyRegistry _registry;
        private SniperDefinition _definition;
        private SniperConfig _config;
        private EnemyController _target;
        private int _targetVersion;
        private bool _tracking;
        private Vector3 _lastKnown;
        private float _damage, _age, _delay;
        private Material _material;
        public bool HasEnded { get; private set; }
        public bool DidExplode { get; private set; }
        public EnemyController Target => _target;
        public static SniperProjectile Launch(GameObject source, EnemyRegistry registry, SniperDefinition definition,
            SniperConfig config, float damage, Vector3 origin, EnemyController target, float delay = 0)
        {
            var go = new GameObject("Sniper bomb");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, registry.gameObject.scene);
            go.transform.position = origin;
            var bomb = go.AddComponent<SniperProjectile>();
            bomb._source = source; bomb._registry = registry; bomb._definition = definition; bomb._config = config;
            bomb._damage = damage; bomb._delay = delay;
            bomb.SetTarget(target);
            bomb._material = new Material(definition.Material);
            var color = new Color(1, .55f, .08f);
            if (bomb._material.HasProperty("_BaseColor")) bomb._material.SetColor("_BaseColor", color);
            if (bomb._material.HasProperty("_EmissionColor")) bomb._material.SetColor("_EmissionColor", color * 2);
            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.transform.SetParent(go.transform, false);
            body.transform.localScale = Vector3.one * definition.ProjectileRadius * 2;
            var collider = body.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
            body.GetComponent<Renderer>().sharedMaterial = bomb._material;
            foreach (var overlap in Physics.OverlapSphere(origin, definition.ProjectileRadius, definition.WorldMask, QueryTriggerInteraction.Ignore))
                if (SniperWorld.World(overlap, source, definition.WorldMask)) { bomb.Cancel(); break; }
            return bomb;
        }
        private void SetTarget(EnemyController target)
        {
            _target = target;
            _tracking = target != null;
            if (target == null) return;
            _targetVersion = target.SpawnVersion; _lastKnown = SniperWorld.Aim(target);
        }
        private void Track()
        {
            if (!_tracking) return; // Failed reacquisition commits to the last-known point.
            if (SniperWorld.Registered(_registry, _target) && _target.SpawnVersion == _targetVersion)
            { _lastKnown = SniperWorld.Aim(_target); return; }
            _target = null;
            _tracking = false;
            if (_config.Retarget)
                SetTarget(SniperWorld.Select(_registry, _source, transform.position, _definition.Range, _definition.WorldMask));
        }
        private bool Obstacle(Collider collider)
        {
            if (collider == null || collider.isTrigger || collider.transform.IsChildOf(_source.transform)) return false;
            return SniperWorld.World(collider, _source, _definition.WorldMask) ||
                SniperWorld.Registered(_registry, collider.GetComponentInParent<EnemyController>());
        }
        private void Update() => Advance(Time.deltaTime);
        public void Advance(float delta)
        {
            if (!float.IsFinite(delta) || delta < 0) throw new ArgumentOutOfRangeException(nameof(delta));
            if (HasEnded) return;
            if (!SniperWorld.SourceAlive(_source) || _registry == null) { Cancel(); return; }
            if (Time.timeScale <= 0 || delta == 0) return;
            float remaining = Mathf.Min(delta, _definition.Lifetime - _age);
            while (remaining > .000001f && !HasEnded)
            {
                float step = Mathf.Min(remaining, .025f);
                _age += step; remaining -= step;
                if (_delay > 0) { _delay = Mathf.Max(0, _delay - step); continue; }
                Track();
                Vector3 origin = transform.position;
                foreach (var overlap in Physics.OverlapSphere(origin, _definition.ProjectileRadius,
                             Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    if (Obstacle(overlap)) { Explode(); return; }
                Vector3 offset = _lastKnown - origin;
                float distance = offset.magnitude;
                if (distance <= .001f) { Explode(); return; }
                Vector3 direction = offset / distance;
                float travel = Mathf.Min(distance, _definition.Speed * step);
                bool hitAnything = false;
                foreach (var hit in Physics.SphereCastAll(origin, _definition.ProjectileRadius, direction, travel,
                             Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    if (Obstacle(hit.collider) && hit.distance <= travel)
                    { travel = hit.distance; hitAnything = true; }
                transform.position = origin + direction * travel;
                if (hitAnything || travel >= distance - .00001f) { Explode(); return; }
            }
            if (!HasEnded && _age >= _definition.Lifetime - .00001f) Explode();
        }
        private void Explode()
        {
            if (HasEnded) return;
            HasEnded = true;
            if (!SniperWorld.SourceAlive(_source) || _registry == null) { Finish(); return; }
            DidExplode = true;
            var enemies = new List<EnemyController>(_registry.ActiveEnemies);
            Vector3 center = transform.position; // Swept sphere center is on the near side of cover.
            foreach (var enemy in enemies)
            {
                if (!SniperWorld.SourceAlive(_source)) break;
                if (!SniperWorld.Alive(enemy)) continue;
                Vector3 point = SniperWorld.Aim(enemy); bool eligible = false;
                foreach (var collider in enemy.GetComponentsInChildren<Collider>())
                {
                    if (!collider.enabled || collider.isTrigger) continue;
                    var contact = collider.ClosestPoint(center);
                    if ((contact - center).sqrMagnitude <= _config.Radius * _config.Radius &&
                        !SniperWorld.Covered(center, contact, _source, _definition.WorldMask))
                    { point = contact; eligible = true; break; }
                }
                if (!eligible) continue;
                var info = new DamageInfo(_damage, _source, point, (point - center).normalized);
                enemy.Health.ApplyDamage(in info);
            }
            SniperBlast.Show(center, _config.Radius, _material, gameObject.scene);
            Finish();
        }
        public void Cancel() { if (HasEnded) return; HasEnded = true; Finish(); }
        private void Finish() { gameObject.SetActive(false); Destroy(gameObject); }
        private void OnDestroy() { if (_material != null) Destroy(_material); }
    }
}
