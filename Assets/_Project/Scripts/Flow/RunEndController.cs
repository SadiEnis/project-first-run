using System;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Progression;
using UnityEngine;
namespace ProjectFirstRun.Flow
{
    /// <summary>Owns terminal run settlement independently of menus and scene reloads.</summary>
    [DisallowMultipleComponent]
    public sealed class RunEndController : MonoBehaviour
    {
        [SerializeField] private HealthComponent _health;
        [SerializeField] private RunEconomyController _economy;
        public bool HasEndedRun => _economy != null && _economy.Result != null;
        public string LastError => _economy != null ? _economy.LastError : null;
        private void OnEnable()
        {
            if (_health == null || _economy == null) throw new InvalidOperationException("Assign run end health and economy.");
            _health.Died += OnDied;
            if (_health.IsDead && !HasEndedRun) OnDied(default, default);
        }
        private void OnDisable() { if (_health != null) _health.Died -= OnDied; }
        private void OnDied(DamageInfo info, DamageResult result)
        {
            if (!HasEndedRun) _economy.Complete(RunEconomyOutcome.Death);
        }
        public void CompleteVictory()
        {
            if (!HasEndedRun) _economy.Complete(_health.IsDead ? RunEconomyOutcome.Death : RunEconomyOutcome.Victory);
        }
        // Until resumable runs exist, leaving an unfinished run is abandonment.
        // Complete preserves an already frozen death/victory result, including save retries.
        public bool TryEndForExit() => _economy.Complete(RunEconomyOutcome.Death);
    }
}
