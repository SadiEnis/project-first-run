using UnityEngine;

namespace ProjectFirstRun.Abilities.Targeting
{
    public interface IAbilityTargetSelector
    {
        bool TrySelectTarget(
            Vector3 origin,
            out Transform target);
    }
}