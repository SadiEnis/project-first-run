using System;
using UnityEngine;

namespace ProjectFirstRun.Stats
{
    [DisallowMultipleComponent]
    public sealed class PlayerStatsController :
        MonoBehaviour
    {
        private PlayerStatCollection _stats;

        public bool IsInitialized =>
            _stats != null;

        public PlayerStatCollection Stats
        {
            get
            {
                EnsureInitialized();
                return _stats;
            }
        }

        private void Awake()
        {
            Initialize();
        }

        public float Evaluate(
            PlayerStatType statType,
            float baseValue)
        {
            EnsureInitialized();

            return _stats.Evaluate(
                statType,
                baseValue);
        }

        private void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            _stats =
                new PlayerStatCollection();
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerStatsController)} " +
                    "has not been initialized.");
            }
        }
    }
}