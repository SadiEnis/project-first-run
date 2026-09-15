#if UNITY_EDITOR
using System.Collections.Generic;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using ProjectFirstRun.Player;
using ProjectFirstRun.Waves;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Editor
{
    public static class PlayableMapFixtureSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Playable/PlayableMapFixture.unity";
        private const string PlayerPath = "Assets/_Project/Prefabs/Player/Player.prefab";
        private const string EnemyPath = "Assets/_Project/Prefabs/Enemies/Enemy_ChaserBasic.prefab";
        private const string DefinitionPath = "Assets/_Project/Data/Enemies/ED_ChaserBasic.asset";
        private const string WavePath = "Assets/_Project/Data/Waves/Test/EW_PlayableFixture.asset";

        [MenuItem("Project First Run/Build Playable Map Fixture")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var metadata = new GameObject("Playable Map Fixture (metadata)");
            var root = metadata.AddComponent<MapSceneRoot>();
            var debug = metadata.AddComponent<PlayableMapFixtureBootstrap>();
            var content = new GameObject("Playable Map Content");
            var mapObject = new GameObject("Map traversal");
            mapObject.transform.SetParent(content.transform, false);
            var map = mapObject.AddComponent<MapTraversalController>();
            Set(map, "_initialRegionId", PlayableMapFixtureBootstrap.Main);
            SetArray(map, "_regionIds", PlayableMapFixtureBootstrap.Main,
                PlayableMapFixtureBootstrap.SideNorth, PlayableMapFixtureBootstrap.SideSouth,
                PlayableMapFixtureBootstrap.SecondMain);
            Set(root, "_content", content);
            Set(root, "_map", map);
            SetArray(root, "_entries");

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPath);
            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            player.name = "Player (fixture)";
            player.transform.position = new Vector3(0f, 0.1f, -8f);
            var health = player.GetComponent<HealthComponent>();
            var traveller = player.AddComponent<SceneTravelController>();

            var registry = NewChild("Second main enemy registry", content.transform).AddComponent<EnemyRegistry>();
            var spawner = NewChild("Prepared enemy spawner", content.transform).AddComponent<EnemySpawner>();
            var encounterObject = NewChild("Second main prepared encounter", content.transform);
            var spawnPoint = NewChild("Prepared enemy spawn point", encounterObject.transform).transform;
            spawnPoint.position = new Vector3(24f, 0f, 6f);
            var encounter = encounterObject.AddComponent<PreparedRegionEncounter>();
            Set(encounter, "_regionId", PlayableMapFixtureBootstrap.SecondMain);
            Set(encounter, "_group", AssetDatabase.LoadAssetAtPath<EnemyWaveDefinition>(WavePath));
            SetArray(encounter, "_spawnPoints", spawnPoint);
            Set(encounter, "_spawner", spawner);
            Set(encounter, "_registry", registry);
            Set(encounter, "_player", player.transform);
            Set(encounter, "_playerHealth", health);
            Set(encounter, "_maxEnemiesPerFrame", 1);
            Set(encounter, "_millisecondsPerFrame", 2f);

            var passages = new List<RegionPassageController>
            {
                CreatePassage(content.transform, "Returnable passage — north side route", new Vector3(0f, 0f, 11f),
                    Quaternion.identity, PlayableMapFixtureBootstrap.Main, PlayableMapFixtureBootstrap.SideNorth,
                    RegionTransitionDirection.Returnable, Color.cyan, player, health, map),
                CreatePassage(content.transform, "Returnable passage — south side route", new Vector3(0f, 0f, -11f),
                    Quaternion.Euler(0f, 180f, 0f), PlayableMapFixtureBootstrap.Main, PlayableMapFixtureBootstrap.SideSouth,
                    RegionTransitionDirection.Returnable, Color.green, player, health, map),
                CreatePassage(content.transform, "One-way passage — second main area", new Vector3(12f, 0f, 0f),
                    Quaternion.Euler(0f, 90f, 0f), PlayableMapFixtureBootstrap.Main, PlayableMapFixtureBootstrap.SecondMain,
                    RegionTransitionDirection.OneWay, Color.yellow, player, health, map)
            };
            CreateBox("Main floor", content.transform, new Vector3(0f, -1f, 0f), new Vector3(18f, 1f, 22f), Color.gray);
            CreateBox("North side floor", content.transform, new Vector3(0f, -1f, 18f), new Vector3(10f, 1f, 12f), Color.cyan);
            CreateBox("South side floor", content.transform, new Vector3(0f, -1f, -18f), new Vector3(10f, 1f, 10f), Color.green);
            CreateBox("Second main floor", content.transform, new Vector3(24f, -1f, 0f), new Vector3(24f, 1f, 18f), Color.yellow);
            CreateBox("North passage corridor", content.transform, new Vector3(0f, -1f, 11.5f), new Vector3(6f, 1f, 3f), Color.gray);
            CreateBox("South passage corridor", content.transform, new Vector3(0f, -1f, -11.5f), new Vector3(6f, 1f, 3f), Color.gray);
            CreateBox("Second main passage corridor", content.transform, new Vector3(10.5f, -1f, 0f), new Vector3(3f, 1f, 6f), Color.gray);

            var prep = CreateMarker(content.transform, "Preparation corridor", new Vector3(20f, 0f, 0f), "PREPARE NEXT REGION");
            Set(prep.gameObject.AddComponent<RegionPreparationTrigger>(), "_volume", prep);
            var prepTrigger = prep.GetComponent<RegionPreparationTrigger>();
            Set(prepTrigger, "_player", player.transform);
            Set(prepTrigger, "_encounter", encounter);
            Set(prepTrigger, "_purpose", RegionPreparationTrigger.Purpose.Prepare);
            var activate = CreateMarker(content.transform, "Activation area", new Vector3(24f, 0f, 0f), "ACTIVATE ENCOUNTER");
            var activateTrigger = activate.gameObject.AddComponent<RegionPreparationTrigger>();
            Set(activateTrigger, "_volume", activate);
            Set(activateTrigger, "_player", player.transform);
            Set(activateTrigger, "_encounter", encounter);
            Set(activateTrigger, "_purpose", RegionPreparationTrigger.Purpose.Activate);

            var exitObject = NewChild("Map exit — target scene", content.transform);
            exitObject.transform.position = new Vector3(34f, 0f, 0f);
            var exitCollider = exitObject.AddComponent<BoxCollider>();
            exitCollider.isTrigger = true;
            exitCollider.size = new Vector3(3f, 3f, 5f);
            var exit = exitObject.AddComponent<MapSceneExit>();
            Set(exit, "_traveller", traveller);
            Set(exit, "_source", root);
            Set(exit, "_sourceRegionId", PlayableMapFixtureBootstrap.SecondMain);
            Set(exit, "_destination._scenePath", PlayableMapFixtureBootstrap.TargetScene);
            Set(exit, "_destination._entryId", "arrival");
            Set(debug, "_mapRoot", root);
            Set(debug, "_encounter", encounter);
            SetArray(debug, "_passages", passages.ToArray());
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = metadata;
            Debug.Log("Built authored playable map fixture: " + ScenePath);
        }

        private static RegionPassageController CreatePassage(Transform parent, string name, Vector3 position,
            Quaternion rotation, string source, string destination, RegionTransitionDirection direction,
            Color color, GameObject player, HealthComponent health, MapTraversalController map)
        {
            var passageObject = NewChild(name, parent);
            passageObject.transform.SetPositionAndRotation(position, rotation);
            var volume = passageObject.AddComponent<BoxCollider>();
            volume.isTrigger = true;
            volume.size = new Vector3(3f, 3f, 5f);
            var barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier.name = "Physical barrier";
            barrier.transform.SetParent(passageObject.transform, false);
            barrier.transform.localScale = new Vector3(2.5f, 2f, .35f);
            barrier.GetComponent<Renderer>().sharedMaterial.color = color;
            var controller = passageObject.AddComponent<RegionPassageController>();
            Set(controller, "_clearanceVolume", volume);
            Set(controller, "_blocker", barrier.GetComponent<Collider>());
            Set(controller, "_player", player.transform);
            Set(controller, "_health", health);
            Set(controller, "_sourceId", source);
            Set(controller, "_destinationId", destination);
            Set(controller, "_direction", direction);
            Set(controller, "_requirement", RegionTransitionRequirement.Free);
            Set(controller, "_singleUse", false);
            Set(controller, "_map", map);
            return controller;
        }

        private static BoxCollider CreateMarker(Transform parent, string name, Vector3 position, string label)
        {
            var marker = NewChild(name, parent);
            marker.transform.position = position;
            var volume = marker.AddComponent<BoxCollider>();
            volume.isTrigger = true;
            volume.size = new Vector3(4f, 2f, 8f);
            var text = new GameObject("Label — " + label);
            text.transform.SetParent(marker.transform, false);
            text.transform.localPosition = Vector3.up;
            var mesh = text.AddComponent<TextMesh>();
            mesh.text = label;
            mesh.characterSize = .25f;
            mesh.fontSize = 32;
            mesh.color = Color.magenta;
            return volume;
        }

        private static GameObject CreateBox(string name, Transform parent, Vector3 position, Vector3 size, Color color)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.SetPositionAndRotation(position, Quaternion.identity);
            box.transform.localScale = size;
            box.GetComponent<Renderer>().sharedMaterial.color = color;
            return box;
        }

        private static GameObject NewChild(string name, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }

        private static void Set(Object target, string property, object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(property);
            if (p == null && property.Contains("."))
            {
                var parts = property.Split('.');
                p = so.FindProperty(parts[0]);
                for (var i = 1; i < parts.Length && p != null; i++) p = p.FindPropertyRelative(parts[i]);
            }
            if (p == null) throw new System.InvalidOperationException("Missing property " + property);
            if (value is Object obj) p.objectReferenceValue = obj;
            else if (value is string text) p.stringValue = text;
            else if (value is bool flag) p.boolValue = flag;
            else if (value is int number) p.intValue = number;
            else if (value is float real) p.floatValue = real;
            else if (value is System.Enum enumeration) p.enumValueIndex = System.Convert.ToInt32(enumeration);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray(Object target, string property, params object[] values)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(property);
            p.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                var e = p.GetArrayElementAtIndex(i);
                if (values[i] is Object obj) e.objectReferenceValue = obj;
                else if (values[i] is string text) e.stringValue = text;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
