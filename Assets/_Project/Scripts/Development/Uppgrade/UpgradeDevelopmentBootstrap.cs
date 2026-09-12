using System;
using System.Collections;
using ProjectFirstRun.Upgrades;
using UnityEngine;

namespace ProjectFirstRun.Development.Upgrades
{
    [DisallowMultipleComponent]
    public sealed class UpgradeDevelopmentBootstrap :
        MonoBehaviour
    {
        [Header("Content")]
        [SerializeField]
        private UpgradeDefinition _upgradeDefinition;

        [Header("Player")]
        [SerializeField]
        private PlayerUpgradeController _playerUpgradeController;

        public bool IsInstalled
        {
            get;
            private set;
        }

        private IEnumerator Start()
        {
            /*
             * PlayerStartingLoadoutInitializer initializes
             * PlayerBuild during Start.
             *
             * Development upgrade installation happens one
             * frame later so we do not depend on Start order.
             */
            yield return null;

            Install();
        }

        public void Install()
        {
            if (IsInstalled)
            {
                throw new InvalidOperationException(
                    "Development upgrade has already been installed.");
            }

            ValidateDependencies();

            UpgradeAcquireResult result =
                _playerUpgradeController.TryAcquire(
                    _upgradeDefinition);

            if (result != UpgradeAcquireResult.Acquired)
            {
                throw new InvalidOperationException(
                    $"Development upgrade could not be acquired. " +
                    $"Result: {result}.");
            }

            IsInstalled = true;
        }

        private void ValidateDependencies()
        {
            if (_upgradeDefinition == null)
            {
                throw new InvalidOperationException(
                    "Development upgrade definition is required.");
            }

            if (_playerUpgradeController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlayerUpgradeController)} is required.");
            }
        }
    }
}