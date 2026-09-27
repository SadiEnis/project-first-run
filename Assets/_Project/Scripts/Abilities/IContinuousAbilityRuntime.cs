using UnityEngine;
namespace ProjectFirstRun.Abilities
{
    public interface IContinuousAbilityRuntime
    {
        void TickContinuous(float deltaTime, Vector3 origin, bool controlEnabled);
    }
}
