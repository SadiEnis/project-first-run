using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Progression
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerExperienceController), typeof(HealthComponent))]
    public sealed class PlayerExperienceCollector : MonoBehaviour
    {
        private PlayerExperienceController _experience;
        private HealthComponent _health;

        public PlayerExperienceController Experience
        {
            get
            {
                EnsureReferences();
                return _experience;
            }
        }

        public bool CanCollect
        {
            get
            {
                EnsureReferences();
                return isActiveAndEnabled && _experience.isActiveAndEnabled &&
                       _experience.IsInitialized && !_health.IsDead && Time.timeScale > 0f;
            }
        }

        private void Awake() => EnsureReferences();

        private void EnsureReferences()
        {
            if (_experience == null)
                _experience = GetComponent<PlayerExperienceController>();
            if (_health == null)
                _health = GetComponent<HealthComponent>();
        }
    }
}
