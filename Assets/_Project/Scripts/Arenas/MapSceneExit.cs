using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirstRun.Arenas
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class MapSceneExit : MonoBehaviour
    {
        [SerializeField] private SceneTravelController _traveller;
        [SerializeField] private MapSceneRoot _source;
        [SerializeField] private SceneTravelDestination _destination;
        [SerializeField] private string _sourceRegionId;
        private bool _requested;
        private readonly HashSet<int> _contacts = new HashSet<int>();

        public void Configure(SceneTravelController traveller, MapSceneRoot source,
            SceneTravelDestination destination, string sourceRegionId)
        {
            if (!GetComponent<Collider>().isTrigger) throw new ArgumentException("Scene exit requires a trigger.");
            destination.Validate();
            if (traveller == null || source == null || !source.Map.Session.ContainsRegion(sourceRegionId))
                throw new ArgumentException("Assign a traveller, source map and known source region.");
            _traveller = traveller;
            _source = source;
            _destination = destination;
            _sourceRegionId = sourceRegionId;
        }

        public void BindScenePlayer(SceneTravelController traveller, MapSceneRoot source) =>
            Configure(traveller, source, _destination, _sourceRegionId);

        private void OnTriggerEnter(Collider other)
        {
            if (_traveller == null || _source == null || other.isTrigger ||
                !other.transform.IsChildOf(_traveller.transform)) return;
            _contacts.Add(other.GetInstanceID());
            if (_requested || _source.Map.Session.CurrentRegionId != _sourceRegionId) return;
            // One attempt per entry. A failed load does not retry every physics tick.
            _requested = _traveller.TryTravel(_source, _destination);
        }

        private void OnTriggerExit(Collider other)
        {
            _contacts.Remove(other.GetInstanceID());
            if (_contacts.Count == 0) _requested = false;
        }

        private void OnDisable() { _contacts.Clear(); _requested = false; }
    }
}
