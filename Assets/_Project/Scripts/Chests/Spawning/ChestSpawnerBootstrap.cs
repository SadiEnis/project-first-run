using System;
using System.Collections;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.UI.Rewards;
using UnityEngine;

namespace ProjectFirstRun.Chests.Spawning
{
    [DisallowMultipleComponent]
    public sealed class ChestSpawnerBootstrap : MonoBehaviour
    {
        [SerializeField] private ChestSpawner _spawner;
        [SerializeField] private PlayerBuildController _playerBuild;
        [SerializeField] private RewardSelectionController _selection;

        private IEnumerator Start()
        {
            if (_spawner == null || _playerBuild == null || _selection == null)
                throw new InvalidOperationException("Chest spawner bootstrap references are incomplete.");
            while (!_playerBuild.IsInitialized) yield return null;
            if (!_spawner.IsInitialized)
                _spawner.Initialize(_playerBuild, _selection,
                    new RewardOfferGenerator(new RewardCandidateFilter(), new UnityRandomSource()));
        }
    }
}
