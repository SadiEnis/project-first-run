using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirstRun.Arenas
{
    [DisallowMultipleComponent]
    public sealed class RegionPreparationTrigger : MonoBehaviour
    {
        public enum Purpose { Prepare, Activate }
        [SerializeField] private Collider _volume;
        [SerializeField] private Transform _player;
        [SerializeField] private PreparedRegionEncounter _encounter;
        [SerializeField] private Purpose _purpose;
        private readonly List<Collider> _bodies = new List<Collider>();

        public void Configure(Collider volume, Transform player, PreparedRegionEncounter encounter, Purpose purpose)
        {
            if (volume == null || !volume.isTrigger || player == null || encounter == null)
                throw new ArgumentException("A trigger volume, player and encounter are required.");
            if (!Enum.IsDefined(typeof(Purpose), purpose)) throw new ArgumentOutOfRangeException(nameof(purpose));
            _volume = volume;
            _player = player;
            _encounter = encounter;
            _purpose = purpose;
        }

        private void FixedUpdate()
        {
            if (_encounter == null || !_encounter.IsInitialized || _volume == null || _player == null) return;
            bool inside = false;
            _bodies.Clear();
            if (_volume.enabled && _volume.gameObject.activeInHierarchy)
            {
                _player.GetComponentsInChildren(false, _bodies);
                foreach (Collider body in _bodies)
                    if (body.enabled && !body.isTrigger && body.gameObject.activeInHierarchy &&
                        _volume.bounds.Intersects(body.bounds) &&
                        Physics.ComputePenetration(_volume, _volume.transform.position, _volume.transform.rotation,
                            body, body.transform.position, body.transform.rotation, out _, out _))
                    { inside = true; break; }
            }
            if (_purpose == Purpose.Activate) _encounter.SetPlayerInside(inside);
            else if (inside) _encounter.RequestPreparation();
        }
        private void OnDisable()
        {
            if (_purpose == Purpose.Activate && _encounter != null) _encounter.SetPlayerInside(false);
        }
    }
}
