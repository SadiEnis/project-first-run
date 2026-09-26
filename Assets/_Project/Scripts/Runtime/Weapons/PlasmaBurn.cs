using System;
using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Weapons
{
    public sealed class PlasmaBurn : TimedBurn
    {
        public static void Apply(GameObject source, IDamageable receiver, Collider collider, float damage,
            GameObject visualPrefab, Action<HitscanShotResult> damageApplied) =>
            ApplyEffect<PlasmaBurn>(source, receiver, collider, damage, PlasmaBurnState.Duration, visualPrefab,
                (hit, point, result) => damageApplied?.Invoke(HitscanShotResult.HitDamageable(hit, point, Vector3.up, point, result)));
    }
}
