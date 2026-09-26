using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Abilities.Fireball
{
    public sealed class FireballBurn : TimedBurn
    {
        public static void Apply(GameObject source, HealthComponent target, Collider collider,
            float damage, float duration, GameObject visual = null) =>
            ApplyEffect<FireballBurn>(source, target, collider, damage, duration, visual, null);
    }
}
