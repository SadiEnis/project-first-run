using System.Collections.Generic;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ProjectFirstRun.Abilities.AcidBottle
{
    /// <summary>One owner/map coordinator prevents duplicate ticks across overlapping puddles.</summary>
    public sealed class AcidFieldSystem : MonoBehaviour
    {
        private sealed class Area
        {
            public Vector3 Position;
            public AcidConfig Config;
            public float Damage, Remaining;
            public GameObject Visual;
        }
        private readonly List<Area> _areas = new List<Area>();
        private readonly Dictionary<EnemyController, float> _exposure = new Dictionary<EnemyController, float>();
        private readonly HashSet<EnemyMotor> _slowed = new HashSet<EnemyMotor>();
        private readonly HashSet<EnemyController> _present = new HashSet<EnemyController>();
        private GameObject _source;
        private EnemyRegistry _registry;
        private int _mask;
        private Material _material;
        public GameObject Source => _source;
        public int WorldMask => _mask;
        public static AcidFieldSystem Create(GameObject source, EnemyRegistry registry, int mask, Material material)
        {
            var root = new GameObject("Acid fields (map owned)");
            SceneManager.MoveGameObjectToScene(root, SceneManager.GetActiveScene());
            var system = root.AddComponent<AcidFieldSystem>();
            system._source = source; system._registry = registry; system._mask = mask;
            system._material = new Material(material);
            Color green = new Color(.25f, 1, .08f, 1);
            if (system._material.HasProperty("_BaseColor")) system._material.SetColor("_BaseColor", green);
            if (system._material.HasProperty("_Color")) system._material.SetColor("_Color", green);
            if (system._material.HasProperty("_EmissionColor")) system._material.SetColor("_EmissionColor", green * 2);
            return system;
        }
        public GameObject Visual(string label, PrimitiveType shape, Vector3 position, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(shape); go.name = label; go.transform.SetParent(transform, false);
            go.transform.position = position; go.transform.localScale = scale;
            var collider = go.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
            go.GetComponent<Renderer>().sharedMaterial = _material; return go;
        }
        public void AddArea(Vector3 point, Vector3 normal, AcidConfig config, float damage)
        {
            if (!AcidWorld.SourceAlive(_source)) return;
            var visual = Visual("Acid puddle", PrimitiveType.Cylinder, point + normal * .02f,
                new Vector3(config.Radius * 2, .012f, config.Radius * 2));
            visual.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            _areas.Add(new Area { Position = point, Config = config, Damage = damage, Remaining = config.Duration, Visual = visual });
        }
        private bool Covers(Area area, EnemyController enemy)
        {
            Vector3 feet = enemy.transform.position, delta = feet - area.Position;
            if (Mathf.Abs(delta.y) > .6f) return false;
            delta.y = 0;
            return delta.sqrMagnitude <= area.Config.Radius * area.Config.Radius &&
                AcidWorld.Visible(area.Position + Vector3.up * .15f, feet + Vector3.up * .15f, _mask, _source);
        }
        private void Update()
        {
            if (!AcidWorld.SourceAlive(_source) || _registry == null)
            { Shutdown(); return; }
            float delta = Time.deltaTime;
            if (delta <= 0) return;
            // Snapshot registry: lethal ticks may unregister enemies synchronously.
            var enemies = new List<EnemyController>(_registry.ActiveEnemies);
            _present.Clear();
            var keepSlowed = new HashSet<EnemyMotor>();
            foreach (var enemy in enemies)
            {
                if (!AcidWorld.SourceAlive(_source)) { Shutdown(); return; }
                if (!AcidWorld.Alive(enemy)) continue;
                float coveredTime = 0, damage = 0, slow = 0;
                foreach (var area in _areas)
                {
                    if (area.Remaining <= 0 || !Covers(area, enemy)) continue;
                    coveredTime = Mathf.Max(coveredTime, Mathf.Min(delta, area.Remaining));
                    damage = Mathf.Max(damage, area.Damage);
                    if (area.Remaining > delta) slow = Mathf.Max(slow, area.Config.Slow);
                }
                if (coveredTime <= 0) continue;
                _present.Add(enemy);
                _exposure.TryGetValue(enemy, out float clock); clock += coveredTime;
                while (clock + .000001f >= .5f && AcidWorld.Alive(enemy) && AcidWorld.SourceAlive(_source))
                {
                    clock = Mathf.Max(0, clock - .5f);
                    var info = new DamageInfo(damage, _source, enemy.transform.position, Vector3.up);
                    enemy.Health.ApplyDamage(in info);
                }
                _exposure[enemy] = clock;
                if (!AcidWorld.SourceAlive(_source)) { Shutdown(); return; }
                var motor = enemy.GetComponent<EnemyMotor>();
                if (slow > 0 && motor != null && AcidWorld.Alive(enemy))
                { motor.SetMovementModifier(this, 1 - slow); _slowed.Add(motor); keepSlowed.Add(motor); }
            }
            foreach (var enemy in new List<EnemyController>(_exposure.Keys))
                if (!_present.Contains(enemy) || !AcidWorld.Alive(enemy)) _exposure.Remove(enemy);
            foreach (var motor in new List<EnemyMotor>(_slowed))
                if (motor == null || !keepSlowed.Contains(motor))
                { if (motor != null) motor.RemoveMovementModifier(this); _slowed.Remove(motor); }
            for (int i = _areas.Count - 1; i >= 0; i--)
            {
                _areas[i].Remaining -= delta;
                if (_areas[i].Remaining <= 0)
                { if (_areas[i].Visual != null) Destroy(_areas[i].Visual); _areas.RemoveAt(i); }
            }
        }
        public void Shutdown() { gameObject.SetActive(false); Destroy(gameObject); }
        private void OnDisable()
        {
            foreach (var motor in _slowed) if (motor != null) motor.RemoveMovementModifier(this);
            _slowed.Clear(); _exposure.Clear();
        }
        private void OnDestroy() { if (_material != null) Destroy(_material); }
    }
}
