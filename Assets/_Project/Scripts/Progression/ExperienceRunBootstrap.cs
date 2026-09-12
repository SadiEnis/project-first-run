using System;
using UnityEngine;

namespace ProjectFirstRun.Progression
{
    [DisallowMultipleComponent]
    public sealed class ExperienceRunBootstrap : MonoBehaviour
    {
        [SerializeField] private PlayerExperienceController _playerExperience;
        [SerializeField] private ExperienceDefinition _definition;

        private void Awake()
        {
            if (_playerExperience == null || _definition == null)
                throw new InvalidOperationException("XP run bootstrap requires a player and definition.");

            _playerExperience.Initialize(new ExperienceState(_definition.CreateRuntimeCurve()));
        }
    }
}
