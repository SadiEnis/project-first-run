using UnityEngine;

namespace ProjectFirstRun.Arenas
{
    [DisallowMultipleComponent]
    public sealed class ArenaTransitionTrigger : MonoBehaviour
    {
        [SerializeField] private ArenaTransitionController _transitions;
        [SerializeField, Min(0)] private int _sourceArenaIndex;

        public bool IsAvailable => isActiveAndEnabled && _transitions != null &&
            _transitions.CanTransition(_sourceArenaIndex);

        public void Configure(ArenaTransitionController transitions, int sourceArenaIndex)
        {
            if (transitions == null) throw new System.ArgumentNullException(nameof(transitions));
            if (sourceArenaIndex < 0) throw new System.ArgumentOutOfRangeException(nameof(sourceArenaIndex));
            _transitions = transitions;
            _sourceArenaIndex = sourceArenaIndex;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsAvailable) _transitions.TryTransition(_sourceArenaIndex, other);
        }

        private void Reset()
        {
            // Supply a default volume; designers can replace it with any trigger shape.
            Collider volume = GetComponent<Collider>();
            if (volume == null) volume = gameObject.AddComponent<BoxCollider>();
            volume.isTrigger = true;
        }
    }
}
