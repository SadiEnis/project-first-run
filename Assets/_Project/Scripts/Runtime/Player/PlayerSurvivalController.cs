using System;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Stats;
using ProjectFirstRun.Arenas;
using UnityEngine;

namespace ProjectFirstRun.Player
{
    /// <summary>Player-only stat bridge; the shared health model remains usable by enemies.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent), typeof(PlayerStatsController))]
    public sealed class PlayerSurvivalController : MonoBehaviour
    {
        [SerializeField] private RunSessionController _run;
        public void BindRun(RunSessionController run) { _run = run; ResetTimer(); }
        private HealthComponent _health;
        private PlayerStatsController _stats;
        private PlayerController _control;
        private bool _bound;
        private float _elapsed;
        public float RegenerationPerSecond { get; private set; }

        private void Start() => Bind();
        private void OnEnable()
        {
            // Awake order is unspecified; Start/Update handles components not yet initialized.
            var stats = GetComponent<PlayerStatsController>();
            if (stats != null && stats.IsInitialized) Bind();
        }

        private void Bind()
        {
            if (_bound) return;
            _health = GetComponent<HealthComponent>();
            _stats = GetComponent<PlayerStatsController>();
            _control = GetComponent<PlayerController>();
            RefreshStats();
            _stats.Stats.Changed += RefreshStats;
            _health.HealthReset += ResetTimer;
            _bound = true;
        }

        private void OnDisable()
        {
            if (_bound)
            {
                _stats.Stats.Changed -= RefreshStats;
                _health.HealthReset -= ResetTimer;
                _bound = false;
            }
            ResetTimer();
        }

        private void ResetTimer() => _elapsed = 0f;

        private void RefreshStats()
        {
            float maximum = _stats.Evaluate(PlayerStatType.MaxHealth, _health.BaseMaximumHealth);
            float reduction = _stats.Evaluate(PlayerStatType.DamageReduction, 0f);
            float regeneration = _stats.Evaluate(PlayerStatType.HealthRegeneration, 0f);
            float incomingDamage = _stats.Evaluate(PlayerStatType.IncomingDamage, 1f);
            if (maximum <= 0f || regeneration < 0f || incomingDamage <= 0f)
                throw new InvalidOperationException("Invalid survival stat configuration.");
            _health.SetDamageReduction(reduction);
            _health.SetIncomingDamageMultiplier(incomingDamage);
            RegenerationPerSecond = regeneration;
            _health.SetMaximumHealth(maximum);
        }

        private void Update()
        {
            TickRegeneration(Time.deltaTime, Time.timeScale > 0f &&
                (_control == null || (_control.isActiveAndEnabled && _control.IsControlEnabled)));
        }

        public void TickRegeneration(float deltaTime, bool gameplayActive)
        {
            if (!float.IsFinite(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            Bind();
            if (!isActiveAndEnabled || !gameplayActive || _health.IsDead ||
                (_run != null && (_run.Status == RunSessionStatus.Victory || _run.Status == RunSessionStatus.Defeat)) ||
                RegenerationPerSecond <= 0f || _health.CurrentHealth >= _health.MaximumHealth)
            {
                ResetTimer();
                return;
            }
            _elapsed += deltaTime;
            if (_elapsed < 1f) return;
            float ticks = Mathf.Floor(_elapsed);
            _elapsed -= ticks;
            _health.Heal(ticks * RegenerationPerSecond);
            if (_health.CurrentHealth >= _health.MaximumHealth) ResetTimer();
        }
    }
}
