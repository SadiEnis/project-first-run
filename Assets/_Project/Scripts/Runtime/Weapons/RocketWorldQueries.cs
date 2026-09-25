using System.Collections.Generic;
using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Weapons
{
    internal static class RocketWorldQueries
    {
        public static bool Ignored(Collider collider, GameObject source)
            => collider == null || collider.isTrigger ||
               (source != null && collider.transform.IsChildOf(source.transform)) ||
               collider.GetComponentInParent<RocketProjectile>() != null;

        public static IDamageable Receiver(Collider collider)
        {
            foreach (var component in collider.GetComponentsInParent<MonoBehaviour>(true))
                if (component is IDamageable receiver) return receiver;
            return null;
        }

        public static bool Sweep(Vector3 origin, float radius, Vector3 direction, float distance,
            int mask, GameObject source, out Collider collider, out float travel, out Vector3 contact)
        {
            collider = null; travel = distance; contact = origin;
            float nearestOverlap = float.PositiveInfinity;
            foreach (var overlap in Physics.OverlapSphere(origin, radius, mask, QueryTriggerInteraction.Ignore))
            {
                if (Ignored(overlap, source)) continue;
                float d = (overlap.ClosestPoint(origin) - origin).sqrMagnitude;
                if (d >= nearestOverlap) continue;
                nearestOverlap = d; collider = overlap; travel = 0; contact = overlap.ClosestPoint(origin);
            }
            if (collider != null) return true;
            if (distance <= 0) return false;
            foreach (var hit in Physics.SphereCastAll(origin, radius, direction, distance, mask, QueryTriggerInteraction.Ignore))
            {
                if (Ignored(hit.collider, source) || hit.distance > travel) continue;
                collider = hit.collider; travel = hit.distance; contact = hit.point;
            }
            return collider != null;
        }

        public static Vector3 AimPoint(Camera camera, float range, int mask, GameObject source)
        {
            var ray = camera.ViewportPointToRay(new Vector3(.5f, .5f));
            float nearest = range;
            foreach (var hit in Physics.RaycastAll(ray, range, mask, QueryTriggerInteraction.Ignore))
                if (!Ignored(hit.collider, source) && hit.distance < nearest) nearest = hit.distance;
            return ray.origin + ray.direction * nearest;
        }

        public static bool Covered(Vector3 origin, Vector3 target, int mask, GameObject source)
        {
            // Raycasts alone cannot detect that their origin starts inside a wall.
            foreach (var overlap in Physics.OverlapSphere(origin, .001f, mask, QueryTriggerInteraction.Ignore))
                if (!Ignored(overlap, source) && Receiver(overlap) == null) return true;
            Vector3 delta = target - origin;
            if (delta.sqrMagnitude < .000001f) return false;
            foreach (var hit in Physics.RaycastAll(origin, delta.normalized, delta.magnitude, mask, QueryTriggerInteraction.Ignore))
                if (!Ignored(hit.collider, source) && Receiver(hit.collider) == null) return true;
            return false;
        }

        public static List<BlastTarget> BlastTargets(Vector3 origin, float radius, int mask, GameObject source)
        {
            var targets = new List<BlastTarget>();
            foreach (var collider in Physics.OverlapSphere(origin, radius, mask, QueryTriggerInteraction.Ignore))
            {
                if (Ignored(collider, source)) continue;
                var receiver = Receiver(collider);
                if (receiver == null || targets.Exists(x => ReferenceEquals(x.Receiver, receiver))) continue;
                Vector3 point = collider.ClosestPoint(origin);
                if ((point - origin).sqrMagnitude > radius * radius || Covered(origin, point, mask, source)) continue;
                targets.Add(new BlastTarget { Receiver = receiver, Collider = collider, Point = point });
            }
            return targets;
        }

        internal sealed class BlastTarget
        {
            public IDamageable Receiver;
            public Collider Collider;
            public Vector3 Point;
        }
    }
}
