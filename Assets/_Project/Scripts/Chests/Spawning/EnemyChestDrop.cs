using System;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using UnityEngine;

namespace ProjectFirstRun.Chests.Spawning
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyController))]
    public sealed class EnemyChestDrop : MonoBehaviour
    {
        private EnemyController _enemy;
        private EnemyChestDropSource _source;
        private bool _hasAttempted;
        private bool _isInitialized;

        public void Initialize(EnemyChestDropSource source)
        {
            if (_isInitialized) throw new InvalidOperationException("Enemy chest drop is already initialized.");
            if (source == null) throw new ArgumentNullException(nameof(source));
            _source = source;
            _isInitialized = true;
        }

        private void Awake() => _enemy = GetComponent<EnemyController>();
        private void OnEnable()
        {
            if (_enemy == null) _enemy = GetComponent<EnemyController>();
            _enemy.Died += HandleDied;
        }
        private void OnDisable()
        {
            if (_enemy != null) _enemy.Died -= HandleDied;
        }

        private void HandleDied(EnemyController enemy, DamageInfo info, DamageResult result)
        {
            if (_hasAttempted || !enemy.IsInitialized || !enemy.IsDead || !result.WasLethal) return;
            _hasAttempted = true;
            EnemyChestDropProfile profile = enemy.Definition.ChestDropProfile;
            if (profile == null || profile.ChanceBasisPoints == 0) return;
            try
            {
                if (!_isInitialized || _source == null)
                    throw new InvalidOperationException("Enemy chest drop requires an injected scene source.");
                _source.TryQueueDrop(profile, transform.position, transform.forward);
            }
            catch (Exception exception)
            {
                // A content failure must not prevent XP or other death observers from running.
                Debug.LogException(exception, this);
            }
        }
    }
}
