#if UNITY_EDITOR

using ProjectFirstRun.Builds;
using UnityEngine;

namespace ProjectFirstRun.Development.Weapons
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(PlayerStartingLoadoutInitializer))]
    public sealed class WeaponSwitchingDevelopmentBootstrap :
        MonoBehaviour
    {
        private PlayerStartingLoadoutInitializer
            _startingLoadoutInitializer;

        private void Awake()
        {
            _startingLoadoutInitializer =
                GetComponent<
                    PlayerStartingLoadoutInitializer>();

            if (_startingLoadoutInitializer.IsInitialized)
            {
                return;
            }

            _startingLoadoutInitializer.Initialize(
                new PlayerBuildCapacity(
                    PlayerBuildCapacity.MaximumWeaponSlots,
                    PlayerBuildCapacity.DefaultAbilitySlots,
                    PlayerBuildCapacity.DefaultUpgradeSlots));
        }
    }
}

#endif
