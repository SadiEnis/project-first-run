using System;
using System.Collections.Generic;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using UnityEngine;
namespace ProjectFirstRun.Progression
{
    /// <summary>Translates enemy death into the reward authored on that enemy's definition.</summary>
    [DisallowMultipleComponent]
    public sealed class EnemyMetaResourceDrops : MonoBehaviour
    {
        [SerializeField] private EnemyRegistry _registry;
        [SerializeField] private MetaResourceSpawner _spawner;
        private readonly HashSet<EnemyController> _observed = new();
        public void ValidateConfiguration()
        {
            if (_registry == null || _spawner == null) throw new InvalidOperationException("Assign meta drop registry and spawner.");
            _spawner.ValidateConfiguration();
        }
        private void OnEnable()
        {
            ValidateConfiguration();
            _registry.EnemyRegistered += Observe;
            _registry.EnemyUnregistered += OnUnregistered;
            foreach (var enemy in _registry.ActiveEnemies) Observe(enemy);
        }
        private void OnDisable()
        {
            if (_registry != null)
            {
                _registry.EnemyRegistered -= Observe;
                _registry.EnemyUnregistered -= OnUnregistered;
            }
            foreach (var enemy in _observed) if (enemy != null) enemy.Died -= OnDied;
            _observed.Clear();
        }
        private void Observe(EnemyController enemy)
        {
            if (enemy != null && !enemy.IsDead && _observed.Add(enemy)) enemy.Died += OnDied;
        }
        private void Forget(EnemyController enemy)
        {
            if (_observed.Remove(enemy) && enemy != null) enemy.Died -= OnDied;
        }
        private void OnUnregistered(EnemyController enemy)
        {
            // Dead enemies unregister before publishing Died; retain that one pending callback.
            // Living despawns detach immediately and never create a reward.
            if (!enemy.IsDead) Forget(enemy);
        }
        private void OnDied(EnemyController enemy, DamageInfo info, DamageResult result)
        {
            if (!_observed.Contains(enemy)) return;
            Forget(enemy);
            _spawner.Spawn(enemy.Definition.MetaResourceReward, enemy.transform.position + new Vector3(.4f, .35f, 0));
        }
    }
}
