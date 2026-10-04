using System;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Stats;
using UnityEngine;

namespace ProjectFirstRun.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent), typeof(PlayerStatsController))]
    public sealed class PlayerKillHealingController : MonoBehaviour
    {
        // Called only by EnemyController's guarded, initialized death path.
        // No registry subscriptions or retained enemy references survive map changes.
        internal void OnCreditedKill()
        {
            if (!isActiveAndEnabled) return;
            var health = GetComponent<HealthComponent>();
            if (health.IsDead) return;
            var stats = GetComponent<PlayerStatsController>();
            if (!stats.IsInitialized) return;
            float amount = stats.Evaluate(PlayerStatType.HealthOnKill, 0f);
            if (amount < 0f) throw new InvalidOperationException("Kill healing cannot be negative.");
            if (amount > 0f) health.Heal(amount);
        }
    }
}
