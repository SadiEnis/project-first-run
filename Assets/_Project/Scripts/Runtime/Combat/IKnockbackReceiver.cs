using UnityEngine;

namespace ProjectFirstRun.Combat
{
    public interface IKnockbackReceiver
    {
        bool TryPush(Vector3 direction, float distance);
    }
}
