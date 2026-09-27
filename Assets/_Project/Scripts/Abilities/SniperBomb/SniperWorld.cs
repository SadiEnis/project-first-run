using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using UnityEngine;
namespace ProjectFirstRun.Abilities.SniperBomb
{
    internal static class SniperWorld
    {
        public static bool Alive(EnemyController enemy) => enemy != null && enemy.IsInitialized &&
            enemy.isActiveAndEnabled && !enemy.IsDead && !enemy.Health.IsDead;
        public static bool SourceAlive(GameObject source) => source != null && source.activeInHierarchy &&
            (source.GetComponent<HealthComponent>() == null || !source.GetComponent<HealthComponent>().IsDead);
        public static bool Registered(EnemyRegistry registry, EnemyController enemy)
        {
            if (registry == null || !Alive(enemy)) return false;
            foreach (var candidate in registry.ActiveEnemies) if (candidate == enemy) return true;
            return false;
        }
        public static Vector3 Aim(EnemyController enemy)
        {
            foreach (var collider in enemy.GetComponentsInChildren<Collider>())
                if (collider.enabled && !collider.isTrigger) return collider.bounds.center;
            return enemy.transform.position + Vector3.up;
        }
        public static bool World(Collider collider, GameObject source, int mask) => collider != null && !collider.isTrigger &&
            (mask & (1 << collider.gameObject.layer)) != 0 &&
            (source == null || !collider.transform.IsChildOf(source.transform)) &&
            collider.GetComponentInParent<HealthComponent>() == null &&
            collider.GetComponentInParent<EnemyController>() == null;
        public static bool Covered(Vector3 from, Vector3 to, GameObject source, int mask)
        {
            foreach (var collider in Physics.OverlapSphere(from, .001f, mask, QueryTriggerInteraction.Ignore))
                if (World(collider, source, mask)) return true;
            Vector3 delta = to - from;
            if (delta.sqrMagnitude < .000001f) return false;
            foreach (var hit in Physics.RaycastAll(from, delta.normalized, delta.magnitude, mask, QueryTriggerInteraction.Ignore))
                if (World(hit.collider, source, mask)) return true;
            return false;
        }
        public static EnemyController Select(EnemyRegistry registry, GameObject source, Vector3 origin, float range, int mask)
        {
            if (registry == null || !SourceAlive(source)) return null;
            EnemyController best = null; int rank = -1; float distance = float.PositiveInfinity;
            foreach (var enemy in registry.ActiveEnemies)
            {
                if (!Alive(enemy)) continue;
                Vector3 aim = Aim(enemy); float squared = (aim - origin).sqrMagnitude;
                int priority = enemy.Definition.Rank == EnemyRank.Boss ? 2 : enemy.Definition.Rank == EnemyRank.Elite ? 1 : 0;
                if (squared > range * range || priority < rank || (priority == rank && squared >= distance) ||
                    Covered(origin, aim, source, mask)) continue;
                best = enemy; rank = priority; distance = squared;
            }
            return best;
        }
    }
}
