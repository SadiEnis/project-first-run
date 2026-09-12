using UnityEngine;

namespace ProjectFirstRun.Abilities.Execution
{
    public readonly struct AbilityExecutionContext
    {
        public Vector3 Origin { get; }
        public Transform Target { get; }

        public bool HasTarget =>
            Target != null;

        public AbilityExecutionContext(
            Vector3 origin,
            Transform target)
        {
            Origin = origin;
            Target = target;
        }
    }
}