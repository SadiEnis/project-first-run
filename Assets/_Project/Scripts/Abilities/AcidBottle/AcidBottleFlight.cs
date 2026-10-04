using UnityEngine;
namespace ProjectFirstRun.Abilities.AcidBottle
{
    public sealed class AcidBottleFlight : MonoBehaviour
    {
        private AcidFieldSystem _system;
        private AcidConfig _config;
        private float _damage, _time;
        private Vector3 _start, _landing;
        private const float Duration = .65f;
        public static AcidBottleFlight Launch(AcidFieldSystem system, Vector3 start, Vector3 landing, AcidConfig config, float damage)
        {
            var go = system.Visual("Acid bottle", PrimitiveType.Cube, start, new Vector3(.18f, .3f, .18f));
            var flight = go.AddComponent<AcidBottleFlight>();
            flight._system = system; flight._config = config; flight._damage = damage;
            flight._start = start; flight._landing = landing + Vector3.up * .08f; return flight;
        }
        private Vector3 Point(float time)
        {
            float t = Mathf.Clamp01(time / Duration);
            return Vector3.Lerp(_start, _landing, t) + Vector3.up * (4 * 2 * t * (1 - t));
        }
        private void Update()
        {
            if (_system == null || !AcidWorld.SourceAlive(_system.Source)) { Destroy(gameObject); return; }
            float delta = Mathf.Min(Time.deltaTime, Duration - _time);
            while (delta > 0)
            {
                float step = Mathf.Min(delta, .025f);
                Vector3 from = transform.position, to = Point(_time + step), movement = to - from;
                RaycastHit closest = default; bool collided = false; float nearest = movement.magnitude;
                foreach (var overlap in Physics.OverlapSphere(from, .08f, _system.WorldMask, QueryTriggerInteraction.Ignore))
                    if (AcidWorld.World(overlap, _system.Source))
                    {
                        Vector3 point = overlap.ClosestPoint(from), away = from - point;
                        if (away.sqrMagnitude > .000001f && away.normalized.y >= .7f) Land(point);
                        Destroy(gameObject); return;
                    }
                foreach (var hit in Physics.SphereCastAll(from, .08f, movement.normalized, movement.magnitude,
                             _system.WorldMask, QueryTriggerInteraction.Ignore))
                    if (AcidWorld.World(hit.collider, _system.Source) && hit.distance <= nearest)
                    { closest = hit; nearest = hit.distance; collided = true; }
                if (collided)
                {
                    if (closest.normal.y >= .7f) Land(closest.point);
                    Destroy(gameObject); return;
                }
                _time += step; delta -= step; transform.position = to;
            }
            if (_time >= Duration)
            { Land(_landing); Destroy(gameObject); }
        }
        private void Land(Vector3 point)
        {
            if (AcidWorld.Ground(point, _system.WorldMask, _system.Source, out var ground, out var normal))
                _system.AddArea(ground, normal, _config, _damage);
        }
    }
}
