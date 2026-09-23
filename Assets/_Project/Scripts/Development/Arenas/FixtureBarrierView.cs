using UnityEngine;
using UnityEngine.AI;

namespace ProjectFirstRun.Development.Arenas
{
    /// <summary>Fixture presentation follows the passage's physical state, including deferred closure.</summary>
    [RequireComponent(typeof(BoxCollider), typeof(MeshRenderer), typeof(NavMeshObstacle))]
    public sealed class FixtureBarrierView : MonoBehaviour
    {
        private BoxCollider _blocker;
        private MeshRenderer _renderer;
        private NavMeshObstacle _obstacle;

        private void Awake()
        {
            _blocker = GetComponent<BoxCollider>();
            _renderer = GetComponent<MeshRenderer>();
            _obstacle = GetComponent<NavMeshObstacle>();
        }

        private void LateUpdate()
        {
            _renderer.enabled = _blocker.enabled;
            _obstacle.enabled = _blocker.enabled;
        }
    }
}
