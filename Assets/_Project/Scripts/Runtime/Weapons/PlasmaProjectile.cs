using System;
using System.Collections.Generic;
using ProjectFirstRun.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Weapons
{
    public sealed class PlasmaProjectile : MonoBehaviour
    {
        [SerializeField] private GameObject _burnVisualPrefab;
        private GameObject _source;
        private HealthComponent _sourceHealth;
        private PlasmaConfig _config;
        private float _damage, _burnDamage, _range, _life;
        private Vector3 _direction;
        private int _mask;
        private bool _initialized, _resolved;
        private Collider _launchBlocker;
        private Action<HitscanShotResult> _damageApplied;
        private readonly HashSet<IDamageable> _hit = new HashSet<IDamageable>();
        public float Damage => _damage;
        public PlasmaConfig Config => _config;
        public bool IsResolved => _resolved;
        public void ValidatePrefab()
        {
            if (!enabled || !gameObject.activeSelf || transform.localScale != Vector3.one ||
                GetComponentInChildren<Renderer>(true) == null || _burnVisualPrefab == null ||
                !_burnVisualPrefab.activeSelf || _burnVisualPrefab.GetComponentInChildren<Renderer>(true) == null)
                throw new InvalidOperationException("Plasma requires an active visible projectile and burn visual prefab with unit projectile root scale.");
            if (GetComponentInChildren<Collider>(true) != null || _burnVisualPrefab.GetComponentInChildren<Collider>(true) != null)
                throw new InvalidOperationException("Plasma visuals must not participate in physics queries.");
        }
        public static PlasmaProjectile Launch(PlasmaProjectile prefab, Camera camera, Transform muzzle, GameObject source,
            PlasmaConfig config, float damage, float burnDamage, float range, int mask, Action<HitscanShotResult> applied)
        {
            prefab.ValidatePrefab(); config.Validate();
            Vector3 aim = RocketWorldQueries.AimPoint(camera, range, mask, source);
            Vector3 start = muzzle.position, gap = start - camera.transform.position;
            Collider blocker;
            if (RocketWorldQueries.Sweep(camera.transform.position, config.Radius,
                gap.sqrMagnitude > .000001f ? gap.normalized : camera.transform.forward, gap.magnitude,
                mask, source, out blocker, out float travel, out _)) start = camera.transform.position + gap.normalized * travel;
            Vector3 direction = aim - start;
            if (direction.sqrMagnitude < .000001f) direction = camera.transform.forward;
            var projectile = Instantiate(prefab, start, Quaternion.LookRotation(direction));
            SceneManager.MoveGameObjectToScene(projectile.gameObject, SceneManager.GetActiveScene());
            projectile.Initialize(source, config, damage, burnDamage, range, mask, direction, applied);
            projectile._launchBlocker = blocker;
            return projectile;
        }
        public void Initialize(GameObject source, PlasmaConfig config, float damage, float burnDamage, float range,
            int mask, Vector3 direction, Action<HitscanShotResult> applied = null)
        {
            if (_initialized) throw new InvalidOperationException("Already initialized.");
            config.Validate(); RocketConfig.Positive(damage, nameof(damage));
            RocketConfig.Positive(burnDamage, nameof(burnDamage)); RocketConfig.Positive(range, nameof(range));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (!float.IsFinite(direction.sqrMagnitude) || direction.sqrMagnitude < .000001f)
                throw new ArgumentOutOfRangeException(nameof(direction));
            _source = source; _sourceHealth = source.GetComponent<HealthComponent>(); _config = config;
            _damage = damage; _burnDamage = burnDamage; _range = range; _life = config.Lifetime;
            _mask = mask; _direction = direction.normalized; _damageApplied = applied; _initialized = true;
        }
        private void Update() => Advance(Time.deltaTime);
        public void Advance(float delta)
        {
            if (!float.IsFinite(delta) || delta < 0) throw new ArgumentOutOfRangeException(nameof(delta));
            if (!_initialized || _resolved) return;
            if (!SourceAlive()) { Resolve(); return; }
            if (delta == 0) return;
            if (_launchBlocker != null)
            {
                Contact(_launchBlocker, _launchBlocker.ClosestPoint(transform.position));
                _launchBlocker = null;
                if (_resolved) return;
            }
            float elapsed = Mathf.Min(delta, _life), step = Mathf.Min(_range, _config.Speed * elapsed);
            Vector3 origin = transform.position;
            var contacts = new List<ContactData>();
            foreach (var collider in Physics.OverlapSphere(origin, _config.Radius, _mask, QueryTriggerInteraction.Ignore))
                if (!RocketWorldQueries.Ignored(collider, _source))
                    contacts.Add(new ContactData { Collider = collider, Point = collider.ClosestPoint(origin) });
            foreach (var hit in Physics.SphereCastAll(origin, _config.Radius, _direction, step, _mask, QueryTriggerInteraction.Ignore))
                if (!RocketWorldQueries.Ignored(hit.collider, _source))
                    contacts.Add(new ContactData { Collider = hit.collider, Point = hit.point, Distance = hit.distance });
            // On ties prefer a world barrier, never leak through a touching enemy/wall boundary.
            contacts.Sort((a, b) => {
                int distance = a.Distance.CompareTo(b.Distance);
                return distance != 0 ? distance : (RocketWorldQueries.Receiver(a.Collider) == null ? -1 : 1)
                    .CompareTo(RocketWorldQueries.Receiver(b.Collider) == null ? -1 : 1);
            });
            foreach (var contact in contacts)
            {
                if (!SourceAlive()) { Resolve(); return; }
                if (contact.Collider == null) continue;
                transform.position = origin + _direction * contact.Distance;
                Contact(contact.Collider, contact.Point);
                if (_resolved) return;
            }
            transform.position = origin + _direction * step;
            _range = Mathf.Max(0, _range - step); _life = Mathf.Max(0, _life - elapsed);
            if (_range <= 0 || _life <= 0) Resolve();
        }
        private bool SourceAlive() => _source != null && (_sourceHealth == null || !_sourceHealth.IsDead);
        private void Contact(Collider collider, Vector3 point)
        {
            var receiver = RocketWorldQueries.Receiver(collider);
            if (receiver == null) { Resolve(); return; }
            if (!_hit.Add(receiver)) return;
            var info = new DamageInfo(_damage, _source, point, _direction);
            var result = receiver.ApplyDamage(in info);
            if (result.WasApplied)
            {
                if (_config.Burn && !result.WasLethal && SourceAlive())
                    PlasmaBurn.Apply(_source, receiver, collider, _burnDamage, _burnVisualPrefab, _damageApplied);
                _damageApplied?.Invoke(HitscanShotResult.HitDamageable(collider, transform.position, _direction, point, result));
            }
            if (_hit.Count >= 1 + _config.PierceCount) Resolve();
        }
        private void Resolve() { _resolved = true; Destroy(gameObject); }
        private struct ContactData { public Collider Collider; public Vector3 Point; public float Distance; }
    }
}
