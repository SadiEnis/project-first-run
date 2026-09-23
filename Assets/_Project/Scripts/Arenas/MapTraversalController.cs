using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirstRun.Arenas
{
    [DisallowMultipleComponent]
    public sealed class MapTraversalController : MonoBehaviour
    {
        [SerializeField] private string _initialRegionId;
        [SerializeField] private string[] _regionIds = Array.Empty<string>();
        private MapTraversalSession _session;

        // Lazy construction avoids ordering dependencies between passage Start calls.
        public MapTraversalSession Session =>
            _session ?? (_session = new MapTraversalSession(_initialRegionId, _regionIds));

        public void Initialize(string initialRegionId, IEnumerable<string> regionIds)
        {
            if (_session != null) throw new InvalidOperationException("Map is already initialized.");
            _session = new MapTraversalSession(initialRegionId, regionIds);
        }
    }
}
