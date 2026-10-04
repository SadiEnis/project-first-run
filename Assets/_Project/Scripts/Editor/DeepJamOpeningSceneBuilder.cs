#if UNITY_EDITOR
using System;
using System.Linq;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Development.Weapons;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using ProjectFirstRun.Progression;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Waves;
using ProjectFirstRun.Weapons;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Editor
{
    /// <summary>One-time authoring, never runtime generation. Existing demo scenes are not overwritten.</summary>
    public static class DeepJamOpeningSceneBuilder
    {
        public const string Folder = "Assets/_Project/Scenes/Demo";
        public const string ScenePath = Folder + "/DeepJam_Opening.unity";
        public const string WavePath = Folder + "/EW_DemoOpening.asset";

        [MenuItem("Project First Run/Demo/Create Opening Greybox")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Create the demo outside Play Mode.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                EditorSceneManager.OpenScene(ScenePath);
                Debug.Log("Existing demo opened, not overwritten. Edit its saved objects directly.");
                return;
            }
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/Scenes", "Demo");
            // Copy first: all subsequent scene edits target the demo, never the source fixture.
            if (!AssetDatabase.CopyAsset(ContentArenaSceneBuilder.ScenePath, ScenePath))
                throw new InvalidOperationException("Could not copy the content arena into the demo folder.");
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var arena = Find<ContentArenaController>(scene).Single();
            arena.ValidateConfiguration();
            var player = arena.Player;
            var services = arena.gameObject;
            var selection = arena.Selection;
            var registry = arena.Registry;
            services.name = "Demo opening services";
            player.name = "Player (demo)";
            player.transform.SetPositionAndRotation(new Vector3(0, .1f, -3), Quaternion.identity);
            PrefabUtility.RecordPrefabInstancePropertyModifications(player.transform);
            var switchingFixture = player.GetComponent<WeaponSwitchingDevelopmentBootstrap>();
            if (switchingFixture != null) Object.DestroyImmediate(switchingFixture);
            foreach (var debug in player.GetComponentsInChildren<MonoBehaviour>(true)
                         .Where(x => x != null && x.GetType().Name.Contains("Debug")).ToArray())
                Object.DestroyImmediate(debug);
            foreach (var panel in Find<ContentArenaPanel>(scene)) Object.DestroyImmediate(panel);
            Object.DestroyImmediate(arena);
            foreach (Transform child in services.transform.Cast<Transform>().ToArray())
                if (child.name.StartsWith("Enemy spawn", StringComparison.Ordinal)) Object.DestroyImmediate(child.gameObject);
            foreach (var oldNavigation in Find<NavMeshSurface>(scene)) Object.DestroyImmediate(oldNavigation.gameObject);

            var geometry = New("Authored demo geometry").transform;
            Floor(geometry, "Start room", 0, 0, 12, 12);
            Floor(geometry, "Combat corridor", 0, 18, 10, 24);
            Floor(geometry, "First reward room", 0, 37, 16, 14);
            Wall(geometry, -6, -6, 6, -6); Wall(geometry, -6, -6, -6, 6); Wall(geometry, 6, -6, 6, 6);
            Wall(geometry, -6, 6, -3, 6); Wall(geometry, 3, 6, 6, 6);
            Wall(geometry, -5, 6, -5, 30); Wall(geometry, 5, 6, 5, 30);
            Wall(geometry, -8, 30, -3, 30); Wall(geometry, 3, 30, 8, 30);
            Wall(geometry, -8, 30, -8, 44); Wall(geometry, 8, 30, 8, 44); Wall(geometry, -8, 44, 8, 44);
            Wall(geometry, -5, 9, 1, 9, "Entrance sight screen — pass on right");
            Box(geometry, "Left cover", new Vector3(-3, .6f, 18), new Vector3(3, 1.2f, 2));
            Box(geometry, "Right cover", new Vector3(3, .6f, 25), new Vector3(3, 1.2f, 2));
            Label(geometry, "ENTER THE CORRIDOR >", new Vector3(-3, 2, 8.7f));
            Label(geometry, "FIRST REWARD", new Vector3(-3, 2.5f, 43.5f));

            if (AssetDatabase.LoadAssetAtPath<EnemyWaveDefinition>(WavePath) == null &&
                !AssetDatabase.CopyAsset("Assets/_Project/Data/Waves/Test/EW_PlayableFixture.asset", WavePath))
                throw new InvalidOperationException("Could not create demo encounter definition.");
            var wave = Load<EnemyWaveDefinition>(WavePath);
            Set(wave, "_stableId", "demo.opening"); Set(wave, "_displayName", "Demo opening — four Chasers");
            var waveProperties = new SerializedObject(wave);
            var entries = waveProperties.FindProperty("_entries"); entries.arraySize = 1;
            var entry = entries.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("_enemyDefinition").objectReferenceValue =
                Load<EnemyDefinition>("Assets/_Project/Data/Enemies/ED_ChaserChestDropTest.asset");
            entry.FindPropertyRelative("_count").intValue = 4;
            waveProperties.ApplyModifiedPropertiesWithoutUndo();
            wave.Validate();
            var encounter = New("Corridor encounter — four Chasers").AddComponent<PreparedRegionEncounter>();
            var points = new[] { new Vector3(-2, 0, 14), new Vector3(2, 0, 19), new Vector3(-2, 0, 24), new Vector3(2, 0, 28) }
                .Select((position, index) => {
                    var point = New("Spawn " + (index + 1), encounter.transform).transform;
                    point.position = position; return point;
                }).ToArray();
            Set(encounter, "_regionId", "demo.corridor"); Set(encounter, "_group", wave);
            Set(encounter, "_spawner", services.GetComponent<EnemySpawner>());
            Set(encounter, "_registry", registry); Set(encounter, "_player", player.transform);
            Set(encounter, "_playerHealth", player.GetComponent<HealthComponent>());
            SetArray(encounter, "_spawnPoints", points);
            var activation = New("Corridor activation volume");
            activation.transform.position = new Vector3(0, 2, 26.5f);
            var volume = activation.AddComponent<BoxCollider>();
            volume.isTrigger = true; volume.size = new Vector3(16, 5, 35);
            var trigger = activation.AddComponent<RegionPreparationTrigger>();
            Set(trigger, "_volume", volume); Set(trigger, "_player", player.transform);
            Set(trigger, "_encounter", encounter); Set(trigger, "_purpose", 1);

            var reward = Load<ChestDefinition>("Assets/_Project/Data/Chests/Dev/CD_AbilityChest.asset");
            var rewardPoint = New("Guaranteed ability chest spawn", services.transform).transform;
            var box = reward.WorldPrefab.GetComponent<BoxCollider>();
            // Serialized anchor is the chest root, lifted so the bottom sits just above the floor.
            float bottom = (box.center.y - box.size.y * .5f) * reward.WorldPrefab.transform.localScale.y;
            rewardPoint.position = new Vector3(0, .02f - bottom, 38);
            var demo = services.AddComponent<DemoOpeningController>();
            Set(demo, "_loadout", player.GetComponent<PlayerStartingLoadoutInitializer>());
            Set(demo, "_health", player.GetComponent<HealthComponent>());
            Set(demo, "_weapon", player.GetComponent<PlayerWeaponController>());
            Set(demo, "_experience", player.GetComponent<PlayerExperienceController>());
            Set(demo, "_encounter", encounter); Set(demo, "_chests", services.GetComponent<ChestSpawner>());
            Set(demo, "_firstReward", reward); Set(demo, "_rewardPoint", rewardPoint); Set(demo, "_selection", selection);
            demo.ValidateConfiguration();
            Bake(geometry.gameObject);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("Demo opening saved. Play: start room > corridor > guaranteed ability chest. Windows build remains unverified.");
        }

        [MenuItem("Project First Run/Demo/Build Windows Opening")]
        public static void BuildWindows()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
                throw new InvalidOperationException("Create Opening Greybox first.");
            // Explicit scenes; no changes to the project's existing Build Settings list.
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath }, target = BuildTarget.StandaloneWindows64,
                locationPathName = "Builds/DeepJamOpening/ProjectFirstRun.exe", options = BuildOptions.Development });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Demo Windows build: " + report.summary.result);
            Debug.Log("Windows build created. Launch Builds/DeepJamOpening/ProjectFirstRun.exe for gameplay validation.");
        }

        private static T[] Find<T>(Scene scene) where T : Component => scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();
        private static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path)
            ?? throw new InvalidOperationException("Missing demo dependency: " + path);
        private static GameObject New(string name, Transform parent = null)
        { var go = new GameObject(name); go.transform.SetParent(parent, false); return go; }
        private static void Floor(Transform parent, string name, float x, float z, float width, float depth)
        { Box(parent, name, new Vector3(x, -.5f, z), new Vector3(width, 1, depth), "808080FF").layer = 7; }
        private static void Wall(Transform parent, float x1, float z1, float x2, float z2, string name = "Boundary wall")
        { Box(parent, name, new Vector3((x1+x2)/2, 2, (z1+z2)/2), new Vector3(Mathf.Abs(x2-x1)+.5f, 4, Mathf.Abs(z2-z1)+.5f)); }
        private static GameObject Box(Transform parent, string name, Vector3 position, Vector3 size, string color = "2E3847FF")
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name;
            go.transform.SetParent(parent, false); go.transform.position = position; go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = Load<Material>("Assets/_Project/Scenes/Playable/Fixture-" + color + ".mat");
            return go;
        }
        private static void Label(Transform parent, string text, Vector3 position)
        {
            var label = New(text, parent).AddComponent<TextMesh>(); label.transform.position = position;
            label.text = text; label.fontSize = 36; label.characterSize = .12f;
        }
        private static void Bake(GameObject geometry)
        {
            var surface = geometry.AddComponent<NavMeshSurface>(); surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders; surface.BuildNavMesh();
            if (surface.navMeshData == null) throw new InvalidOperationException("Demo NavMesh bake failed.");
            AssetDatabase.CreateAsset(surface.navMeshData, AssetDatabase.GenerateUniqueAssetPath(Folder + "/OpeningNavigation.asset"));
            EditorUtility.SetDirty(surface);
        }
        private static void Set(Object target, string field, object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field) ?? throw new InvalidOperationException("Missing field: " + field);
            if (value is Object obj) p.objectReferenceValue = obj;
            else if (value is string text) p.stringValue = text;
            else if (value is int number) p.intValue = number;
            else throw new ArgumentException("Unsupported serialized value: " + field);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void SetArray(Object target, string field, Transform[] values)
        {
            var so = new SerializedObject(target); var p = so.FindProperty(field); p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
