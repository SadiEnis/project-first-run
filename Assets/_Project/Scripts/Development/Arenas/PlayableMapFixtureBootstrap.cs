using System.Collections.Generic;
using ProjectFirstRun.Arenas;
using UnityEngine;

namespace ProjectFirstRun.Development.Arenas
{
    /// <summary>
    /// Debug presentation for the authored stage-9 scene. The scene owns all
    /// geometry and references; this component never creates gameplay objects.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayableMapFixtureBootstrap : MonoBehaviour
    {
        public const string Main = "main";
        public const string SideNorth = "side-north";
        public const string SideSouth = "side-south";
        public const string SecondMain = "second-main";
        public const string TargetScene = "Assets/_Project/Scenes/Tests/Test_SceneTravelTarget.unity";

        [SerializeField] private MapSceneRoot _mapRoot;
        [SerializeField] private PreparedRegionEncounter _encounter;
        [SerializeField] private List<RegionPassageController> _passages = new List<RegionPassageController>();
        private string _lastTransition = "Waiting for a passage";

        public MapSceneRoot MapRoot => _mapRoot;
        public MapTraversalController Map => _mapRoot != null ? _mapRoot.Map : null;
        public PreparedRegionEncounter Encounter => _encounter;

        private void Awake()
        {
            if (_mapRoot == null) _mapRoot = GetComponent<MapSceneRoot>();
            foreach (var passage in _passages)
                if (passage != null) passage.TransitionCompleted += HandleTransitionCompleted;
        }

        private void OnDestroy()
        {
            foreach (var passage in _passages)
                if (passage != null) passage.TransitionCompleted -= HandleTransitionCompleted;
        }

        private void HandleTransitionCompleted(string region) => _lastTransition = "Entered region: " + region;

        private void OnGUI()
        {
            if (Map == null) return;
            GUI.Box(new Rect(12f, 12f, 520f, 156f), "Playable Map Fixture Debug");
            GUI.Label(new Rect(26f, 42f, 330f, 22f), "Current region: " + Map.Session.CurrentRegionId);
            GUI.Label(new Rect(26f, 65f, 330f, 22f), _lastTransition);
            GUI.Label(new Rect(26f, 88f, 330f, 22f),
                "Encounter: " + (_encounter == null ? "not assigned" : _encounter.Status.ToString()));
            GUI.Label(new Rect(26f, 111f, 330f, 22f),
                "Preparation: " + (_encounter == null ? "not assigned" : _encounter.PreparationStatus.ToString()));
            GUI.Label(new Rect(26f, 134f, 490f, 22f),
                "Last error: " + (_encounter == null || _encounter.LastError == null
                    ? "none" : _encounter.LastError.Message));
        }
    }
}
