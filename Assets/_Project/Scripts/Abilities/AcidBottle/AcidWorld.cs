using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using UnityEngine;
using UnityEngine.AI;
namespace ProjectFirstRun.Abilities.AcidBottle
{
    internal static class AcidWorld
    {
        public static bool Alive(EnemyController enemy) => enemy != null && enemy.IsInitialized &&
            enemy.isActiveAndEnabled && !enemy.IsDead && !enemy.Health.IsDead;
        public static bool SourceAlive(GameObject source) => source != null && source.activeInHierarchy &&
            (source.GetComponent<HealthComponent>() == null || !source.GetComponent<HealthComponent>().IsDead);
        public static bool World(Collider collider, GameObject source) => collider != null && !collider.isTrigger &&
            (source == null || !collider.transform.IsChildOf(source.transform)) &&
            collider.GetComponentInParent<HealthComponent>() == null &&
            collider.GetComponentInParent<EnemyController>() == null &&
            collider.GetComponentInParent<AcidFieldSystem>() == null;
        public static bool Visible(Vector3 from, Vector3 to, int mask, GameObject source)
        {
            foreach (var overlap in Physics.OverlapSphere(from, .02f, mask, QueryTriggerInteraction.Ignore))
                if (World(overlap, source)) return false;
            Vector3 delta = to - from;
            foreach (var hit in Physics.RaycastAll(from, delta.normalized, delta.magnitude, mask, QueryTriggerInteraction.Ignore))
                if (World(hit.collider, source)) return false;
            return true;
        }
        public static bool Ground(Vector3 point, int mask, GameObject source, out Vector3 ground, out Vector3 normal)
        {
            ground = default; normal = Vector3.up;
            float nearest = float.PositiveInfinity; bool found = false;
            foreach (var hit in Physics.RaycastAll(point + Vector3.up, Vector3.down, 4, mask, QueryTriggerInteraction.Ignore))
            {
                if (!World(hit.collider, source) || hit.distance >= nearest) continue;
                nearest = hit.distance; ground = hit.point; normal = hit.normal; found = true;
            }
            return found && normal.y >= .7f && NavMesh.SamplePosition(ground, out var nav, .6f, NavMesh.AllAreas) &&
                Mathf.Abs(nav.position.y - ground.y) <= .3f;
        }
    }
}
