using System;
using System.Collections.Generic;
using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Weapons
{
    /// <summary>Queries the whole volley before damage can remove or move any target.</summary>
    public sealed class HitscanVolleyResolver
    {
        private readonly Camera _camera;
        private readonly Transform _muzzle;
        private readonly GameObject _source;
        private readonly Func<double> _random;

        public HitscanVolleyResolver(Camera camera, Transform muzzle, GameObject source, Func<double> random = null)
        {
            _camera = camera != null ? camera : throw new ArgumentNullException(nameof(camera));
            _muzzle = muzzle != null ? muzzle : throw new ArgumentNullException(nameof(muzzle));
            _source = source != null ? source : throw new ArgumentNullException(nameof(source));
            _random = random ?? new System.Random().NextDouble;
        }

        public HitscanVolleyResult Resolve(float pelletDamage, float range, int mask, WeaponShotConfig config, bool isCritical = false)
        {
            if (!float.IsFinite(pelletDamage) || pelletDamage <= 0 || !float.IsFinite(pelletDamage * config.PelletCount))
                throw new ArgumentOutOfRangeException(nameof(pelletDamage));
            if (!float.IsFinite(range) || range <= 0) throw new ArgumentOutOfRangeException(nameof(range));
            _ = new WeaponShotConfig(config.PelletCount, config.HalfAngle, config.PushDistance);
            var traces = new HitscanShotResult[config.PelletCount];
            var groups = new List<TargetHit>();
            Ray aim = _camera.ViewportPointToRay(new Vector3(.5f, .5f));
            Vector3 origin = _muzzle.position;
            for (int i = 0; i < traces.Length; i++)
            {
                Vector3 direction = config.HalfAngle == 0 ? aim.direction :
                    PelletSpread.Direction(aim.direction, config.HalfAngle, _random(), _random());
                Vector3 point = aim.origin + direction * range;
                if (FirstHit(aim.origin, direction, range, mask, out var aimHit)) point = aimHit.point;
                Vector3 delta = point - origin;
                float distance = Mathf.Min(range, delta.magnitude);
                Vector3 muzzleDirection = distance > Mathf.Epsilon ? delta.normalized : direction;
                if (distance <= Mathf.Epsilon) distance = range;
                if (!FirstHit(origin, muzzleDirection, distance + .001f, mask, out var hit))
                {
                    traces[i] = HitscanShotResult.Miss(origin, muzzleDirection, origin + muzzleDirection * distance);
                    continue;
                }
                traces[i] = HitscanShotResult.HitWithoutDamageable(hit.collider, origin, muzzleDirection, hit.point);
                var receiver = FindReceiver<IDamageable>(hit.collider);
                if (receiver == null) continue;
                var group = groups.Find(x => ReferenceEquals(x.Receiver, receiver));
                if (group == null)
                {
                    group = new TargetHit { Receiver = receiver, Collider = hit.collider,
                        Point = hit.point, Direction = muzzleDirection,
                        Push = FindReceiver<IKnockbackReceiver>(hit.collider) };
                    groups.Add(group);
                }
                group.Count++;
            }
            var targets = new List<HitscanShotResult>(groups.Count);
            foreach (var group in groups)
            {
                if (group.Receiver is UnityEngine.Object unityReceiver && unityReceiver == null) continue;
                var info = new DamageInfo(pelletDamage * group.Count, _source, group.Point, group.Direction);
                var damage = group.Receiver.ApplyDamage(in info);
                targets.Add(HitscanShotResult.HitDamageable(group.Collider, origin, group.Direction, group.Point, damage));
                if (damage.WasApplied && !damage.WasLethal && damage.CurrentHealth > 0 && config.PushDistance > 0)
                    group.Push?.TryPush(group.Point - origin, config.PushDistance);
            }
            return new HitscanVolleyResult(traces, targets.ToArray(), isCritical);
        }

        private bool FirstHit(Vector3 origin, Vector3 direction, float distance, int mask, out RaycastHit first)
        {
            first = default;
            float nearest = float.PositiveInfinity;
            foreach (var hit in Physics.RaycastAll(origin, direction, distance, mask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.IsChildOf(_source.transform) || hit.distance >= nearest) continue;
                first = hit;
                nearest = hit.distance;
            }
            return !float.IsPositiveInfinity(nearest);
        }

        private static T FindReceiver<T>(Collider collider) where T : class
        {
            foreach (var component in collider.GetComponentsInParent<MonoBehaviour>(true))
                if (component is T receiver) return receiver;
            return null;
        }

        private sealed class TargetHit
        {
            public IDamageable Receiver;
            public IKnockbackReceiver Push;
            public Collider Collider;
            public Vector3 Point, Direction;
            public int Count;
        }
    }
}
