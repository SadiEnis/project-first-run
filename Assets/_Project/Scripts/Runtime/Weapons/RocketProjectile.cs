using System;
using System.Collections.Generic;
using ProjectFirstRun.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Weapons
{
    [DisallowMultipleComponent]
    public sealed class RocketProjectile : MonoBehaviour
    {
        [SerializeField] private RocketProjectile _fragmentPrefab;
        [SerializeField] private RocketBlastVisual _blastPrefab;
        private GameObject _source;
        private HealthComponent _sourceHealth;
        private RocketConfig _config;
        private float _damage, _fragmentDamage, _remainingRange, _remainingLife, _speed, _radius;
        private int _mask;
        private bool _initialized, _resolved, _fragment;
        private Vector3 _direction;
        private HashSet<IDamageable> _fragmentHits;
        private Action<HitscanShotResult> _damageApplied;
        private Collider _launchBlocker;
        public bool IsResolved => _resolved;
        public bool IsFragment => _fragment;
        public float Damage => _damage;
        public float RemainingRange => _remainingRange;
        public RocketConfig Config => _config;
        public Vector3 Direction => _direction;
        public event Action<RocketProjectile> Exploded;

        public void ValidatePrefab(bool needsFragments)
        {
            if (!gameObject.activeSelf || !enabled || GetComponentInChildren<Renderer>(true) == null ||
                transform.localScale != Vector3.one)
                throw new InvalidOperationException("Rocket prefab requires enabled component, visible renderer and unit root scale.");
            if (_blastPrefab == null) throw new InvalidOperationException("Rocket blast visual prefab is required.");
            if (needsFragments && (_fragmentPrefab == null || _fragmentPrefab == this ||
                !_fragmentPrefab.enabled || !_fragmentPrefab.gameObject.activeSelf ||
                _fragmentPrefab.GetComponentInChildren<Renderer>(true) == null || _fragmentPrefab.transform.localScale != Vector3.one))
                throw new InvalidOperationException("A separate valid fragment prefab is required.");
        }

        public static RocketProjectile Launch(RocketProjectile prefab, Camera camera, Transform muzzle,
            GameObject source, RocketConfig config, float damage, float fragmentDamage, float range, int mask,
            Action<HitscanShotResult> damageApplied)
        {
            prefab.ValidatePrefab(config.FragmentCount > 0);
            Vector3 aim = RocketWorldQueries.AimPoint(camera, range, mask, source);
            Vector3 start = muzzle.position;
            Vector3 gap = start - camera.transform.position;
            Collider blocker = null;
            if (RocketWorldQueries.Sweep(camera.transform.position, config.CollisionRadius,
                gap.sqrMagnitude > .000001f ? gap.normalized : camera.transform.forward, gap.magnitude,
                mask, source, out blocker, out float travel, out _))
                start = camera.transform.position + gap.normalized * travel;
            Vector3 direction = aim - start;
            if (direction.sqrMagnitude < .000001f) direction = camera.transform.forward;
            var rocket = Instantiate(prefab, start, Quaternion.LookRotation(direction));
            // Explicit map ownership even if the firing player is persistent.
            SceneManager.MoveGameObjectToScene(rocket.gameObject, SceneManager.GetActiveScene());
            rocket.Initialize(source, config, damage, fragmentDamage, range, mask, direction, damageApplied);
            rocket._launchBlocker = blocker;
            return rocket;
        }

        public void Initialize(GameObject source, RocketConfig config, float damage, float fragmentDamage,
            float range, int mask, Vector3 direction, Action<HitscanShotResult> damageApplied = null)
        {
            if (_initialized) throw new InvalidOperationException("Rocket already initialized.");
            config.Validate(); RocketConfig.Positive(damage, nameof(damage));
            RocketConfig.Positive(fragmentDamage, nameof(fragmentDamage)); RocketConfig.Positive(range, nameof(range));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (!float.IsFinite(direction.x) || !float.IsFinite(direction.y) || !float.IsFinite(direction.z) ||
                !float.IsFinite(direction.sqrMagnitude) || direction.sqrMagnitude < .000001f)
                throw new ArgumentOutOfRangeException(nameof(direction));
            _source = source; _sourceHealth = source.GetComponent<HealthComponent>();
            _config = config; _damage = damage; _fragmentDamage = fragmentDamage; _mask = mask;
            _remainingRange = range; _remainingLife = config.Lifetime; _speed = config.Speed;
            _radius = config.CollisionRadius; _direction = direction.normalized; _damageApplied = damageApplied;
            _initialized = true;
        }

        private void Update() => Advance(Time.deltaTime);

        public void Advance(float deltaTime)
        {
            if (!float.IsFinite(deltaTime) || deltaTime < 0) throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (!_initialized || _resolved) return;
            if (_source == null || (_sourceHealth != null && _sourceHealth.IsDead)) { Resolve(); return; }
            if (deltaTime == 0) return;
            if (_launchBlocker != null) { Impact(_launchBlocker, _launchBlocker.ClosestPoint(transform.position)); return; }
            float elapsed = Mathf.Min(deltaTime, _remainingLife);
            float step = Mathf.Min(_remainingRange, _speed * elapsed);
            if (RocketWorldQueries.Sweep(transform.position, _radius, _direction, step, _mask, _source,
                out var collider, out float travel, out var contact))
            {
                transform.position += _direction * travel;
                Impact(collider, contact);
                return;
            }
            transform.position += _direction * step;
            _remainingRange = Mathf.Max(0, _remainingRange - step);
            _remainingLife = Mathf.Max(0, _remainingLife - elapsed);
            if (_remainingRange <= 0 || _remainingLife <= 0) Resolve();
        }

        private void Impact(Collider collider, Vector3 contact)
        {
            if (_resolved) return;
            _resolved = true;
            try
            {
                if (_fragment)
                {
                    var receiver = RocketWorldQueries.Receiver(collider);
                    if (receiver != null && _fragmentHits.Add(receiver)) Apply(receiver, collider, contact, _damage);
                    return;
                }
                Vector3 center = transform.position; // Swept sphere center stays on the near side of the surface.
                var targets = RocketWorldQueries.BlastTargets(center, _config.BlastRadius, _mask, _source);
                foreach (var target in targets)
                {
                    if (_source == null || (_sourceHealth != null && _sourceHealth.IsDead)) break;
                    if (target.Receiver is UnityEngine.Object value && value == null) continue;
                    Apply(target.Receiver, target.Collider, target.Point, _damage);
                }
                if (_source == null || (_sourceHealth != null && _sourceHealth.IsDead)) return;
                if (_blastPrefab != null)
                {
                    var effect = Instantiate(_blastPrefab, center, Quaternion.identity);
                    SceneManager.MoveGameObjectToScene(effect.gameObject, gameObject.scene);
                    effect.Initialize(_config.BlastRadius);
                }
                if (_config.FragmentCount > 0 && _fragmentPrefab != null)
                {
                    var hits = new HashSet<IDamageable>();
                    for (int i = 0; i < _config.FragmentCount; i++)
                    {
                        Vector3 direction = Quaternion.Euler(0, i * 360f / _config.FragmentCount, 0) * Vector3.forward;
                        var fragment = Instantiate(_fragmentPrefab, center, Quaternion.LookRotation(direction));
                        SceneManager.MoveGameObjectToScene(fragment.gameObject, gameObject.scene);
                        fragment.Initialize(_source, new RocketConfig(_config.FragmentSpeed, _config.FragmentRadius,
                            _config.FragmentLifetime), _fragmentDamage, _fragmentDamage, _config.FragmentRange,
                            _mask, direction, _damageApplied);
                        fragment._fragment = true;
                        fragment._fragmentHits = hits;
                    }
                }
                Exploded?.Invoke(this);
            }
            finally { Destroy(gameObject); }
        }

        private void Apply(IDamageable receiver, Collider collider, Vector3 point, float amount)
        {
            Vector3 direction = point - transform.position;
            if (direction.sqrMagnitude < .000001f) direction = _direction;
            var info = new DamageInfo(amount, _source, point, direction.normalized);
            var result = receiver.ApplyDamage(in info);
            if (result.WasApplied)
                _damageApplied?.Invoke(HitscanShotResult.HitDamageable(collider, transform.position, direction.normalized, point, result));
        }

        private void Resolve() { _resolved = true; Destroy(gameObject); }
    }
}
