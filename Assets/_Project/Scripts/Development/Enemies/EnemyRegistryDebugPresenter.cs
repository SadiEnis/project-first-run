#if UNITY_EDITOR

using ProjectFirstRun.Enemies;
using UnityEngine;

namespace ProjectFirstRun.Development.Enemies
{
    [DisallowMultipleComponent]
    public sealed class EnemyRegistryDebugPresenter : MonoBehaviour
    {
        [SerializeField]
        private EnemyRegistry _enemyRegistry;

        private int _activeEnemyCount;

        private void Awake()
        {
            if (_enemyRegistry == null)
            {
                Debug.LogError(
                    $"{nameof(EnemyRegistryDebugPresenter)} " +
                    "requires an EnemyRegistry.",
                    this);

                enabled = false;
                return;
            }

            _activeEnemyCount =
                _enemyRegistry.ActiveCount;
        }

        private void OnEnable()
        {
            if (_enemyRegistry != null)
            {
                _enemyRegistry.ActiveCountChanged +=
                    HandleActiveCountChanged;
            }
        }

        private void OnDisable()
        {
            if (_enemyRegistry != null)
            {
                _enemyRegistry.ActiveCountChanged -=
                    HandleActiveCountChanged;
            }
        }

        private void OnGUI()
        {
            GUI.Box(
                new Rect(20f, 180f, 260f, 65f),
                "Enemy Foundation Debug");

            GUI.Label(
                new Rect(35f, 210f, 220f, 25f),
                $"Active Enemies: {_activeEnemyCount}");
        }

        private void HandleActiveCountChanged(
            int activeEnemyCount)
        {
            _activeEnemyCount =
                activeEnemyCount;
        }
    }
}

#endif