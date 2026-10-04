using System;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Stats;
using UnityEngine;

namespace ProjectFirstRun.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent), typeof(PlayerStatsController))]
    public sealed class PlayerLastStandController : MonoBehaviour
    {
        private HealthComponent _health;
        private PlayerStatsController _stats;
        private StatModifier[] _installed = Array.Empty<StatModifier>();
        private bool _bound;
        private float _bonus;
        public bool IsBonusActive => _bonus > 0f;

        private void Start() => Bind();
        private void OnEnable()
        {
            var stats = GetComponent<PlayerStatsController>();
            if (stats != null && stats.IsInitialized) Bind();
        }

        private void Bind()
        {
            if (_bound) return;
            _health = GetComponent<HealthComponent>();
            _stats = GetComponent<PlayerStatsController>();
            _bound = true;
            _health.HealthChanged += HealthChanged;
            _stats.Stats.Changed += Refresh;
            Refresh();
        }

        private void HealthChanged(float current, float maximum) => Refresh();

        private void Refresh()
        {
            float bonus = _stats.Evaluate(PlayerStatType.LowHealthDamageBonus, 0f);
            if (bonus < 0f) throw new InvalidOperationException("Low-health damage bonus cannot be negative.");
            if (_health.IsDead || _health.CurrentHealth / _health.MaximumHealth > .30f) bonus = 0f;
            ReplaceBonus(bonus);
        }

        private void ReplaceBonus(float bonus)
        {
            if (bonus == _bonus) return;
            var next = bonus == 0f ? Array.Empty<StatModifier>() : new[]
            {
                new StatModifier(PlayerStatType.WeaponDamage, StatModifierOperation.AdditivePercent, bonus, "runtime.last-stand"),
                new StatModifier(PlayerStatType.AbilityDamage, StatModifierOperation.AdditivePercent, bonus, "runtime.last-stand")
            };
            // Commit both effects and our local state before notifying (which can re-enter Refresh).
            _stats.Stats.ReplaceWithoutNotification(_installed, next);
            _installed = next;
            _bonus = bonus;
            _stats.Stats.NotifyChanged();
        }

        private void OnDisable()
        {
            if (!_bound) return;
            _bound = false;
            _health.HealthChanged -= HealthChanged;
            _stats.Stats.Changed -= Refresh;
            ReplaceBonus(0f);
        }
    }
}
