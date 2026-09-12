using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirstRun.Enemies
{
    [DisallowMultipleComponent]
    public sealed class EnemyRegistry : MonoBehaviour
    {
        private readonly HashSet<EnemyController> _activeEnemies =
            new HashSet<EnemyController>();

        public event Action<EnemyController> EnemyRegistered;
        public event Action<EnemyController> EnemyUnregistered;
        public event Action<int> ActiveCountChanged;

        public IReadOnlyCollection<EnemyController> ActiveEnemies =>
            _activeEnemies;

        public int ActiveCount =>
            _activeEnemies.Count;

        public bool Register(EnemyController enemy)
        {
            if (enemy == null)
            {
                throw new ArgumentNullException(nameof(enemy));
            }

            if (!_activeEnemies.Add(enemy))
            {
                return false;
            }

            EnemyRegistered?.Invoke(enemy);
            ActiveCountChanged?.Invoke(_activeEnemies.Count);

            return true;
        }

        public bool Unregister(EnemyController enemy)
        {
            if (enemy == null ||
                !_activeEnemies.Remove(enemy))
            {
                return false;
            }

            EnemyUnregistered?.Invoke(enemy);
            ActiveCountChanged?.Invoke(_activeEnemies.Count);

            return true;
        }
    }
}