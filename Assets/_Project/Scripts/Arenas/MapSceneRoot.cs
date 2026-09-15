using System;
using System.Collections.Generic;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Player;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Enemies;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Arenas
{
    [Serializable]
    public sealed class MapSceneEntry
    {
        public string Id;
        public string RegionId;
        public Transform Point;
        public MapSceneEntry(string id, string regionId, Transform point)
        { Id = id; RegionId = regionId; Point = point; }
    }

    /// <summary>Scene metadata stays active; all destination gameplay is authored beneath inactive content.</summary>
    [DisallowMultipleComponent]
    public sealed class MapSceneRoot : MonoBehaviour
    {
        [SerializeField] private GameObject _content;
        [SerializeField] private MapTraversalController _map;
        [SerializeField] private MapSceneEntry[] _entries = Array.Empty<MapSceneEntry>();
        public MapTraversalController Map => _map;
        public GameObject Content => _content;
        public bool HasArrived { get; private set; }
        private EnemyRegistry _incomingRegistry;

        public void Configure(GameObject content, MapTraversalController map, MapSceneEntry[] entries)
        {
            if (HasArrived) throw new InvalidOperationException("Map has already been entered.");
            _content = content;
            _map = map;
            _entries = entries;
        }

        public MapSceneEntry ValidateDestination(string entryId)
        {
            if (HasArrived || _content == null || _content.activeSelf ||
                _content.transform.parent != null || _content == gameObject ||
                _content.scene != gameObject.scene || transform.parent != null ||
                _map == null || !_map.transform.IsChildOf(_content.transform))
                throw new InvalidOperationException("Destination requires fresh, inactive, scene-root content and its map.");
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
                if (root != gameObject && root != _content)
                    throw new InvalidOperationException("Destination gameplay must be under its content root.");
            if (_content.GetComponentInChildren<PlayerMotor>(true) != null)
                throw new InvalidOperationException("A destination must not contain another player.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            MapSceneEntry selected = null;
            if (_entries == null) throw new InvalidOperationException("Assign scene entries.");
            foreach (MapSceneEntry entry in _entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.Id) || !ids.Add(entry.Id) ||
                    !_map.Session.ContainsRegion(entry.RegionId) || entry.Point == null ||
                    !entry.Point.IsChildOf(_content.transform))
                    throw new InvalidOperationException("Entries require unique IDs, known regions and owned transforms.");
                if (entry.Id == entryId) selected = entry;
            }
            return selected ?? throw new InvalidOperationException("Destination entry was not found: " + entryId);
        }

        public void BindPlayer(GameObject player, MapSceneEntry entry)
        {
            if (!ReferenceEquals(ValidateDestination(entry?.Id), entry))
                throw new InvalidOperationException("Entry does not belong to this map.");
            var health = player.GetComponent<HealthComponent>();
            if (health == null || health.IsDead) throw new InvalidOperationException("A living player is required.");
            var abilities = player.GetComponent<PlayerAbilityController>();
            var acquisition = player.GetComponent<PlayerAbilityAcquisitionController>();
            if ((abilities != null && abilities.AbilityCount > 0) || (acquisition != null && acquisition.IsInitialized))
            {
                var registries = _content.GetComponentsInChildren<EnemyRegistry>(true);
                if (registries.Length != 1) throw new InvalidOperationException("Abilities require one destination enemy registry.");
                _incomingRegistry = registries[0];
            }
            _map.Session.EnterSceneAt(entry.RegionId);
            foreach (var encounter in _content.GetComponentsInChildren<PreparedRegionEncounter>(true))
                encounter.BindScenePlayer(player.transform, health);
            foreach (var passage in _content.GetComponentsInChildren<RegionPassageController>(true))
                passage.BindScenePlayer(player.transform, health, _map);
            foreach (var trigger in _content.GetComponentsInChildren<RegionPreparationTrigger>(true))
                trigger.BindScenePlayer(player.transform);
            foreach (var selection in _content.GetComponentsInChildren<RewardSelectionController>(true))
                selection.BindScenePlayer(player);
            foreach (var bootstrap in _content.GetComponentsInChildren<ChestSpawnerBootstrap>(true))
                bootstrap.BindScenePlayer(player);
            foreach (var source in _content.GetComponentsInChildren<LevelUpChestSource>(true))
                source.BindScenePlayer(player);
            foreach (var exit in _content.GetComponentsInChildren<MapSceneExit>(true))
                exit.BindScenePlayer(player.GetComponent<SceneTravelController>(), this);
        }

        public void Activate()
        {
            HasArrived = true;
            _content.SetActive(true);
        }

        public void BindRuntimeTargets(GameObject player)
        {
            if (_incomingRegistry == null) return;
            player.GetComponent<PlayerAbilityAcquisitionController>()?.BindEnemyRegistry(_incomingRegistry);
            var abilities = player.GetComponent<PlayerAbilityController>();
            if (abilities != null)
                foreach (var entry in abilities.Entries) entry.BindEnemyRegistry(_incomingRegistry);
        }

        public static MapSceneRoot FindIn(Scene scene)
        {
            MapSceneRoot found = null;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var candidate in root.GetComponentsInChildren<MapSceneRoot>(true))
                {
                    if (found != null) throw new InvalidOperationException("Scene has multiple map roots.");
                    found = candidate;
                }
            return found != null ? found : throw new InvalidOperationException("Scene has no map root.");
        }
    }
}
