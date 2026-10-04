using System;
using System.Collections.Generic;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using UnityEngine;
namespace ProjectFirstRun.Abilities.EnchantedStaff
{
    public sealed class EnchantedBeam : MonoBehaviour
    {
        private GameObject _source;
        private EnemyRegistry _registry;
        private float _damage, _speed, _radius, _lifetime, _age;
        private int _worldMask;
        private Vector3 _direction;
        private LineRenderer _line;
        private LineRenderer _trail;
        private Material _material;
        private bool _ended;
        private readonly Dictionary<(EnemyController, int), float> _lastHit = new Dictionary<(EnemyController, int), float>();
        public Vector3 Direction => _direction;
        public bool HasEnded => _ended;
        public int BounceCount { get; private set; }
        public static EnchantedBeam Launch(GameObject source, EnemyRegistry registry, EnchantedDefinition definition,
            EnchantedConfig config, float damage, Vector3 origin, Vector3 direction)
        {
            direction.y = 0;
            if (!float.IsFinite(direction.x) || !float.IsFinite(direction.z) || direction.sqrMagnitude < .000001f)
                throw new ArgumentException("Beam direction must be finite and horizontal.");
            var go = new GameObject("Enchanted beam");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, registry.gameObject.scene);
            go.transform.position = origin;
            var beam = go.AddComponent<EnchantedBeam>();
            beam._source = source; beam._registry = registry; beam._damage = damage;
            beam._speed = definition.Speed; beam._radius = definition.Radius; beam._worldMask = definition.WorldMask;
            beam._lifetime = config.Lifetime; beam._direction = direction.normalized;
            beam._material = new Material(definition.Material);
            var color = new Color(.65f, .2f, 1);
            if (beam._material.HasProperty("_BaseColor")) beam._material.SetColor("_BaseColor", color);
            if (beam._material.HasProperty("_EmissionColor")) beam._material.SetColor("_EmissionColor", color * 3);
            beam._line = go.AddComponent<LineRenderer>();
            beam._line.sharedMaterial = beam._material; beam._line.positionCount = 2;
            beam._line.startWidth = beam._line.endWidth = definition.Radius * 2;
            beam._line.useWorldSpace = true;
            var trailObject = new GameObject("Beam path");
            trailObject.transform.SetParent(go.transform, false);
            beam._trail = trailObject.AddComponent<LineRenderer>();
            beam._trail.sharedMaterial = beam._material;
            beam._trail.useWorldSpace = true;
            beam._trail.startWidth = beam._trail.endWidth = definition.Radius * .5f;
            beam._trail.numCornerVertices = 2;
            beam._trail.positionCount = 1;
            beam._trail.SetPosition(0, origin);
            beam.Draw();
            if (beam.InsideWorld(origin)) beam.End();
            return beam;
        }
        private bool SourceAlive => _source != null && _source.activeInHierarchy &&
            (_source.GetComponent<HealthComponent>() == null || !_source.GetComponent<HealthComponent>().IsDead);
        private bool World(Collider collider) => collider != null && !collider.isTrigger &&
            !collider.transform.IsChildOf(_source.transform) &&
            collider.GetComponentInParent<HealthComponent>() == null &&
            collider.GetComponentInParent<EnemyController>() == null;
        private bool InsideWorld(Vector3 point)
        {
            foreach (var collider in Physics.OverlapSphere(point, _radius * .99f, _worldMask, QueryTriggerInteraction.Ignore))
                if (World(collider)) return true;
            return false;
        }
        private void Hit(Collider collider, float time)
        {
            if (_ended || !SourceAlive) return;
            var enemy = collider.GetComponentInParent<EnemyController>();
            if (enemy == null || !enemy.IsInitialized || !enemy.isActiveAndEnabled || enemy.IsDead || enemy.Health.IsDead) return;
            bool registered = false;
            foreach (var candidate in _registry.ActiveEnemies) if (candidate == enemy) { registered = true; break; }
            if (!registered) return;
            var key = (enemy, enemy.SpawnVersion);
            if (_lastHit.TryGetValue(key, out var previous) && time - previous < .5f - .000001f) return;
            _lastHit[key] = time;
            var info = new DamageInfo(_damage, _source, collider.ClosestPoint(transform.position), _direction);
            enemy.Health.ApplyDamage(in info);
        }
        private void Travel(float seconds)
        {
            float left = _speed * seconds, used = 0;
            // Bounded corner work; unresolved travel is discarded, never teleported through geometry.
            for (int iteration = 0; iteration < 8 && left > .00001f && !_ended; iteration++)
            {
                if (!SourceAlive || _registry == null) { End(); return; }
                Vector3 start = transform.position;
                if (InsideWorld(start)) { End(); return; }
                float allowed = left; RaycastHit wall = default; bool blocked = false;
                foreach (var hit in Physics.SphereCastAll(start, _radius, _direction, left, _worldMask, QueryTriggerInteraction.Ignore))
                    if (World(hit.collider) && (!blocked || hit.distance < allowed))
                    { allowed = hit.distance; wall = hit; blocked = true; }
                foreach (var collider in Physics.OverlapSphere(start, _radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    Hit(collider, _age + used / _speed);
                var hits = Physics.SphereCastAll(start, _radius, _direction, allowed, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                foreach (var hit in hits)
                    if (!blocked || hit.distance < allowed - .0001f) Hit(hit.collider, _age + (used + hit.distance) / _speed);
                if (!SourceAlive) { End(); return; }
                transform.position = start + _direction * allowed;
                RecordTrailPoint();
                left -= allowed; used += allowed;
                if (!blocked) break;
                Vector3 normal = wall.normal; normal.y = 0;
                if (normal.sqrMagnitude < .01f) { End(); return; }
                normal.Normalize();
                _direction = Vector3.Reflect(_direction, normal).normalized;
                // Keep the beam on its launch plane, away from the surface.
                transform.position += normal * .002f;
                RecordTrailPoint();
                BounceCount++;
            }
        }
        private void Draw()
        {
            _line.SetPosition(0, transform.position);
            // Short forward-facing body: avoids a trail cutting across a ricochet corner.
            _line.SetPosition(1, transform.position - _direction * .55f);
        }
        private void RecordTrailPoint()
        {
            int count = _trail.positionCount;
            if ((transform.position - _trail.GetPosition(count - 1)).sqrMagnitude < .000001f) return;
            // Record every swept segment endpoint, including ricochets within one rendered frame.
            _trail.positionCount = count + 1;
            _trail.SetPosition(count, transform.position);
        }
        private void Update() => Advance(Time.deltaTime);
        public void Advance(float delta)
        {
            if (!float.IsFinite(delta) || delta < 0) throw new ArgumentOutOfRangeException(nameof(delta));
            if (_ended) return;
            if (!SourceAlive || _registry == null) { End(); return; }
            if (Time.timeScale <= 0 || delta == 0) return;
            float remaining = Mathf.Min(delta, _lifetime - _age);
            while (remaining > .000001f && !_ended)
            {
                float step = Mathf.Min(remaining, .025f);
                Travel(step); _age += step; remaining -= step;
            }
            if (_age >= _lifetime - .00001f) End();
            else if (!_ended) Draw();
        }
        public void End()
        {
            if (_ended) return;
            _ended = true; gameObject.SetActive(false); Destroy(gameObject);
        }
        private void OnDestroy() { if (_material != null) Destroy(_material); }
    }
}
