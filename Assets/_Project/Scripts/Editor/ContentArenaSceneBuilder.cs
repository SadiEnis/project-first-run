#if UNITY_EDITOR
using System;
using System.Linq;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.Fireball;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Development.Abilities;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Development.Weapons;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using ProjectFirstRun.Items;
using ProjectFirstRun.Player;
using ProjectFirstRun.Progression;
using ProjectFirstRun.Rewards.Claims;
using ProjectFirstRun.Stats;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Weapons;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Editor
{
    /// <summary>Explicit authoring tool for this fixture only. Never invoked on entering Play.</summary>
    public static class ContentArenaSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Tests/Test_ContentArena.unity";
        private const string NavigationPath = "Assets/_Project/Scenes/Tests/ContentArenaNavigation.asset";

        [MenuItem("Project First Run/Update Content Arena Shotgun Connections")]
        public static void UpdateShotgunConnections()
            => UpdateWeaponConnections("Shotgun");

        [MenuItem("Project First Run/Update Content Arena Minigun Connections")]
        public static void UpdateMinigunConnections()
            => UpdateWeaponConnections("Minigun");

        private static void UpdateWeaponConnections(string weaponName)
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var arena = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<ContentArenaController>(true)).Single();
            var shotgun = Load<ItemDefinition>($"Assets/_Project/Data/Items/Weapons/WD_{weaponName}.asset");
            var items = arena.Items.ToList();
            if (!items.Contains(shotgun)) items.Add(shotgun);
            SetArray(arena, "_items", items.ToArray());
            ConfigureTraces(arena.Player);
            arena.ValidateConfiguration();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"{weaponName.ToUpperInvariant()}_ARENA_UPDATED");
        }

        [MenuItem("Project First Run/Update Content Arena Rocket Launcher Connections")]
        public static void UpdateRocketConnections() => UpdateWeaponConnections("RocketLauncher");

        private static void ConfigureTraces(GameObject player)
        {
            var traces = player.GetComponent<VolleyTracePresenter>() ?? player.AddComponent<VolleyTracePresenter>();
            Set(traces, "_weapon", player.GetComponent<PlayerWeaponController>());
            const string tracePath = "Assets/_Project/Scenes/Tests/ContentArenaTrace.mat";
            var traceMaterial = AssetDatabase.LoadAssetAtPath<Material>(tracePath);
            if (traceMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) throw new InvalidOperationException("Missing URP unlit shader for test traces.");
                traceMaterial = new Material(shader);
                traceMaterial.SetColor("_BaseColor", new Color(1, .8f, .2f));
                AssetDatabase.CreateAsset(traceMaterial, tracePath);
            }
            Set(traces, "_material", traceMaterial);
        }

        [MenuItem("Project First Run/Build Content Test Arena")]
        public static void Build()
        {
            if (!Application.isBatchMode &&
                (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() ||
                 !EditorUtility.DisplayDialog("Rebuild content fixture", "Replace the saved Test_ContentArena scene and its NavMesh?", "Rebuild", "Cancel"))) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var services = New("Content arena services");
            var player = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>("Assets/_Project/Prefabs/Player/Player.prefab"), scene);
            player.name = "Player (content tests)";
            player.transform.position = new Vector3(0, .1f, -6);
            player.AddComponent<WeaponSwitchingDevelopmentBootstrap>();
            ConfigureTraces(player);
            foreach (var component in player.GetComponentsInChildren<MonoBehaviour>(true))
                if (component != null && component.GetType().Name.Contains("Debug"))
                {
                    component.enabled = false;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                }
            var registry = services.AddComponent<EnemyRegistry>();
            var health = player.GetComponent<HealthComponent>();
            var xp = services.AddComponent<ExperienceRunBootstrap>();
            Set(xp, "_playerExperience", player.GetComponent<PlayerExperienceController>());
            Set(xp, "_definition", Load<ExperienceDefinition>("Assets/_Project/Data/Progression/XP_DefaultRun.asset"));
            var abilities = services.AddComponent<FireballDevelopmentBootstrap>();
            Set(abilities, "_fireballDefinition", Load<FireballDefinition>("Assets/_Project/Data/Abilities/Fireball/AD_Fireball.asset"));
            Set(abilities, "_grantStartingAbility", false);
            Set(abilities, "_playerAbilityAcquisitionController", player.GetComponent<PlayerAbilityAcquisitionController>());
            Set(abilities, "_playerStatsController", player.GetComponent<PlayerStatsController>());
            Set(abilities, "_damageSource", player);
            Set(abilities, "_enemyRegistry", registry);

            var ui = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>("Assets/_Project/Prefabs/UI/RewardSelectionUI.prefab"), scene);
            var selection = ui.GetComponentInChildren<RewardSelectionController>(true);
            Set(selection, "_claimController", player.GetComponent<PlayerRewardClaimController>());
            Set(selection, "_playerController", player.GetComponent<PlayerController>());
            Set(selection, "_playerDeathController", player.GetComponent<PlayerDeathController>());
            var chests = services.AddComponent<ChestSpawner>();
            var chestBootstrap = services.AddComponent<ChestSpawnerBootstrap>();
            Set(chestBootstrap, "_spawner", chests);
            Set(chestBootstrap, "_selection", selection);
            Set(chestBootstrap, "_playerBuild", player.GetComponent<PlayerBuildController>());
            var placement = services.AddComponent<ChestSpawnPlacement>();
            var levelDrops = services.AddComponent<LevelUpChestSource>();
            Set(levelDrops, "_spawner", chests);
            Set(levelDrops, "_placement", placement);
            Set(levelDrops, "_playerExperience", player.GetComponent<PlayerExperienceController>());
            Set(levelDrops, "_playerHealth", health);
            Set(levelDrops, "_dropTable", Load<ChestDropTable>("Assets/_Project/Data/Chests/Dev/CDT_DevelopmentLevelUp.asset"));
            var enemyDrops = services.AddComponent<EnemyChestDropSource>();
            Set(enemyDrops, "_playerHealth", health);
            Set(enemyDrops, "_spawner", chests);
            Set(enemyDrops, "_placement", placement);
            var enemies = services.AddComponent<EnemySpawner>();
            Set(enemies, "_chestDropSource", enemyDrops);

            var geometry = New("Authored ground, cover and navigation");
            Box(geometry.transform, "Combat floor", new Vector3(0, -.5f, 20), new Vector3(40, 1, 60), "808080FF").layer = 7;
            Box(geometry.transform, "Left boundary", new Vector3(-20, 2, 20), new Vector3(1, 4, 60), "2E3847FF");
            Box(geometry.transform, "Right boundary", new Vector3(20, 2, 20), new Vector3(1, 4, 60), "2E3847FF");
            Box(geometry.transform, "Back boundary", new Vector3(0, 2, -10), new Vector3(40, 4, 1), "2E3847FF");
            Box(geometry.transform, "Far boundary", new Vector3(0, 2, 50), new Vector3(40, 4, 1), "2E3847FF");
            Box(geometry.transform, "Test bay cover left", new Vector3(-9, 1, 0), new Vector3(12, 2, 1), "2E3847FF");
            Box(geometry.transform, "Test bay cover right", new Vector3(9, 1, 0), new Vector3(12, 2, 1), "2E3847FF");
            foreach (int distance in new[] { 10, 20, 30, 40 })
            {
                var line = Box(geometry.transform, distance + "m marker", new Vector3(0, .015f, distance - 6), new Vector3(38, .02f, .15f), "00FFFFFF");
                Object.DestroyImmediate(line.GetComponent<Collider>());
                Label(geometry.transform, distance + " m", new Vector3(-18, 2, distance - 6));
            }
            Label(geometry.transform, "F1 - CONTENT TEST CONTROLS", new Vector3(-5, 3, 0));
            var points = new Transform[4];
            var positions = new[] { new Vector3(-5, 0, 14), new Vector3(5, 0, 22), new Vector3(-5, 0, 32), new Vector3(5, 0, 40) };
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = New("Enemy spawn " + (i + 1), services.transform).transform;
                points[i].position = positions[i];
            }
            var surface = geometry.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();
            if (surface.navMeshData == null) throw new InvalidOperationException("Content arena NavMesh bake failed.");
            var previous = AssetDatabase.LoadAssetAtPath<NavMeshData>(NavigationPath);
            if (previous == null) AssetDatabase.CreateAsset(surface.navMeshData, NavigationPath);
            else
            {
                var generated = surface.navMeshData;
                surface.RemoveData();
                EditorUtility.CopySerialized(generated, previous);
                Object.DestroyImmediate(generated);
                surface.navMeshData = previous;
                EditorUtility.SetDirty(previous);
                surface.AddData();
            }
            var light = New("Directional light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            RenderSettings.ambientLight = Color.gray;

            var arena = services.AddComponent<ContentArenaController>();
            Set(arena, "_player", player);
            Set(arena, "_registry", registry);
            Set(arena, "_enemySpawner", enemies);
            Set(arena, "_enemyPrefab", Load<GameObject>("Assets/_Project/Prefabs/Enemies/Enemy_ChaserBasic.prefab").GetComponent<EnemyController>());
            Set(arena, "_chestSpawner", chests);
            Set(arena, "_placement", placement);
            Set(arena, "_selection", selection);
            SetArray(arena, "_spawnPoints", points);
            SetArray(arena, "_enemyDefinitions",
                Load<EnemyDefinition>("Assets/_Project/Data/Enemies/ED_ChaserChestDropTest.asset"),
                Load<EnemyDefinition>("Assets/_Project/Data/Enemies/ED_Charger.asset"),
                Load<EnemyDefinition>("Assets/_Project/Data/Enemies/ED_Ranger.asset"));
            SetArray(arena, "_items",
                Load<ItemDefinition>("Assets/_Project/Data/Items/Weapons/WD_PlasmaRifle.asset"),
                Load<ItemDefinition>("Assets/_Project/Data/Items/Weapons/WD_DevelopmentSecondaryWeapon.asset"),
                Load<ItemDefinition>("Assets/_Project/Data/Abilities/Fireball/AD_Fireball.asset"),
                Load<ItemDefinition>("Assets/_Project/Data/Upgrade/Dev/UD_DevelopmentDamageBoost.asset"),
                Load<ItemDefinition>("Assets/_Project/Data/Items/Weapons/WD_Shotgun.asset"),
                Load<ItemDefinition>("Assets/_Project/Data/Items/Weapons/WD_Minigun.asset"),
                Load<ItemDefinition>("Assets/_Project/Data/Items/Weapons/WD_RocketLauncher.asset"));
            SetArray(arena, "_chests",
                Load<ChestDefinition>("Assets/_Project/Data/Chests/Dev/CD_WeaponChest.asset"),
                Load<ChestDefinition>("Assets/_Project/Data/Chests/Dev/CD_AbilityChest.asset"),
                Load<ChestDefinition>("Assets/_Project/Data/Chests/Dev/CD_UpgradeChest.asset"),
                Load<ChestDefinition>("Assets/_Project/Data/Chests/Dev/CD_GreenChest.asset"),
                Load<ChestDefinition>("Assets/_Project/Data/Chests/Dev/CD_PurpleChest.asset"),
                Load<ChestDefinition>("Assets/_Project/Data/Chests/Dev/CD_LegendaryChest.asset"),
                Load<ChestDefinition>("Assets/_Project/Data/Chests/Dev/CD_BossChest.asset"));
            Set(services.AddComponent<ContentArenaPanel>(), "_arena", arena);
            arena.ValidateConfiguration();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("CONTENT_ARENA_AUTHORED");
        }

        private static GameObject New(string name, Transform parent = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject Box(Transform parent, string name, Vector3 position, Vector3 size, string color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = Load<Material>("Assets/_Project/Scenes/Playable/Fixture-" + color + ".mat");
            return go;
        }

        private static void Label(Transform parent, string text, Vector3 position)
        {
            var label = New(text, parent).AddComponent<TextMesh>();
            label.transform.position = position;
            label.text = text;
            label.fontSize = 36;
            label.characterSize = .15f;
        }

        private static T Load<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<T>(path)
                ?? throw new InvalidOperationException("Missing or incompatible content arena asset: " + path
                    + "; expected " + typeof(T).FullName + "; loaded "
                    + AssetDatabase.LoadMainAssetAtPath(path)?.GetType().FullName);
        }

        private static void Set(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(field) ?? throw new InvalidOperationException("Missing field: " + field);
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Set(Object target, string field, bool value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).boolValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray(Object target, string field, params Object[] values)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(field);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
