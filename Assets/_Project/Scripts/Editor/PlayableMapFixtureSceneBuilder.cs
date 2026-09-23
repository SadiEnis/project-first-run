#if UNITY_EDITOR
using System;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Abilities.Fireball;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Development.Abilities;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Spawning;
using ProjectFirstRun.Player;
using ProjectFirstRun.Progression;
using ProjectFirstRun.Rewards.Claims;
using ProjectFirstRun.Stats;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Waves;
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
    /// <summary>Explicit Editor authoring. Runtime consumes saved geometry, NavMesh and references.</summary>
    public static class PlayableMapFixtureSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Playable/PlayableMapFixture.unity";
        private const string Folder = "Assets/_Project/Scenes/Playable/";

        [MenuItem("Project First Run/Build Playable Map Fixture")]
        public static void Build()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildDestination();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = New("Playable Map Fixture (metadata)").AddComponent<MapSceneRoot>();
            var content = New("Playable Map Content");
            var map = New("Map traversal", content.transform).AddComponent<MapTraversalController>();
            Set(map, "_initialRegionId", "main");
            SetArray(map, "_regionIds", "main", "side-north", "side-south", "second-main");
            Set(root, "_content", content); Set(root, "_map", map);
            var player = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>("Assets/_Project/Prefabs/Player/Player.prefab"), scene);
            player.name = "Player (fixture)"; player.transform.position = new Vector3(0, .1f, 0);
            var traveller = player.AddComponent<SceneTravelController>();
            var registry = New("Map enemy registry", content.transform).AddComponent<EnemyRegistry>();
            Rewards(content.transform, player);
            var xp = New("Initial run experience", content.transform).AddComponent<ExperienceRunBootstrap>();
            Set(xp, "_playerExperience", player.GetComponent<PlayerExperienceController>());
            Set(xp, "_definition", Load<ExperienceDefinition>("Assets/_Project/Data/Progression/XP_DefaultRun.asset"));
            var abilities = New("Initial ability factory (no free ability)", content.transform).AddComponent<FireballDevelopmentBootstrap>();
            Set(abilities, "_fireballDefinition", Load<FireballDefinition>("Assets/_Project/Data/Abilities/Fireball/AD_Fireball.asset"));
            Set(abilities, "_grantStartingAbility", false);
            Set(abilities, "_playerAbilityAcquisitionController", player.GetComponent<PlayerAbilityAcquisitionController>());
            Set(abilities, "_playerStatsController", player.GetComponent<PlayerStatsController>());
            Set(abilities, "_damageSource", player); Set(abilities, "_enemyRegistry", registry);

            var geo = New("Authored geometry and baked navigation", content.transform).transform;
            Floor(geo, "Main floor", 0, 0, 20, 20, Color.gray);
            Floor(geo, "North floor", 0, 24, 16, 16, Color.cyan);
            Floor(geo, "South floor", 0, -24, 16, 16, Color.green);
            Floor(geo, "Second main floor", 30, 0, 20, 20, Color.yellow);
            Floor(geo, "North corridor", 0, 13, 4, 6, Color.gray);
            Floor(geo, "South corridor", 0, -13, 4, 6, Color.gray);
            Floor(geo, "East corridor", 15, 0, 10, 4, Color.gray);
            Wall(geo, -10, -10, -10, 10);
            Wall(geo, -10, 10, -2, 10); Wall(geo, 2, 10, 10, 10);
            Wall(geo, -10, -10, -2, -10); Wall(geo, 2, -10, 10, -10);
            Wall(geo, 10, -10, 10, -2); Wall(geo, 10, 2, 10, 10);
            foreach (int sign in new[] { -1, 1 })
            {
                Wall(geo, -2, sign*10, -2, sign*16); Wall(geo, 2, sign*10, 2, sign*16);
                Wall(geo, -8, sign*16, -2, sign*16); Wall(geo, 2, sign*16, 8, sign*16);
                Wall(geo, -8, sign*16, -8, sign*32); Wall(geo, 8, sign*16, 8, sign*32);
                Wall(geo, -8, sign*32, 8, sign*32);
                Wall(geo, -8, sign*21, 3, sign*21, "Spawn sight screen");
            }
            Wall(geo, 10, -2, 20, -2); Wall(geo, 10, 2, 20, 2);
            Wall(geo, 20, -10, 20, -2); Wall(geo, 20, 2, 20, 10);
            Wall(geo, 20, -10, 40, -10); Wall(geo, 20, 10, 40, 10); Wall(geo, 40, -10, 40, 10);
            Wall(geo, 24, -10, 24, 4, "Spawn sight screen");
            var north = Encounter(content.transform, player, registry, "side-north",
                new[] { new Vector3(-4,0,27), new Vector3(0,0,27), new Vector3(-4,0,30), new Vector3(0,0,30) });
            var south = Encounter(content.transform, player, registry, "side-south",
                new[] { new Vector3(-4,0,-27), new Vector3(0,0,-27), new Vector3(-4,0,-30), new Vector3(0,0,-30) });
            var second = Encounter(content.transform, player, registry, "second-main",
                new[] { new Vector3(30,0,-4), new Vector3(34,0,-4), new Vector3(30,0,-7), new Vector3(34,0,-7) });
            var passages = new[] {
                Passage(content.transform, new Vector3(0,0,13), Quaternion.identity, "side-north", false, player, map, north),
                Passage(content.transform, new Vector3(0,0,-13), Quaternion.Euler(0,180,0), "side-south", false, player, map, south),
                Passage(content.transform, new Vector3(15,0,0), Quaternion.Euler(0,90,0), "second-main", true, player, map, second)
            };
            Trigger(content.transform, "North preparation", new Vector3(0,1.5f,8), new Vector3(6,3,4), player, north, false);
            Trigger(content.transform, "South preparation", new Vector3(0,1.5f,-8), new Vector3(6,3,4), player, south, false);
            Trigger(content.transform, "East preparation", new Vector3(8,1.5f,0), new Vector3(4,3,6), player, second, false);
            Trigger(content.transform, "North activation", new Vector3(0,1.5f,27), new Vector3(16,3,10), player, north, true);
            Trigger(content.transform, "South activation", new Vector3(0,1.5f,-27), new Vector3(16,3,10), player, south, true);
            Trigger(content.transform, "East activation", new Vector3(32.5f,1.5f,0), new Vector3(15,3,20), player, second, true);
            var exitObject = New("Map exit — playable destination", content.transform);
            exitObject.transform.position = new Vector3(38,1.5f,7);
            var exitCollider = exitObject.AddComponent<BoxCollider>(); exitCollider.isTrigger = true;
            exitCollider.size = new Vector3(3,3,4);
            Label(exitObject.transform, "EXIT TO NEXT MAP", Vector3.up);
            var exit = exitObject.AddComponent<MapSceneExit>();
            Set(exit, "_traveller", traveller); Set(exit, "_source", root); Set(exit, "_sourceRegionId", "second-main");
            Set(exit, "_destination._scenePath", PlayableMapFixtureBootstrap.TargetScene); Set(exit, "_destination._entryId", "arrival");
            Lighting(content.transform); Bake(geo.gameObject, "FixtureNavigation.asset");
            var debug = root.gameObject.AddComponent<PlayableMapFixtureBootstrap>();
            Set(debug, "_mapRoot", root); Set(debug, "_encounter", second);
            SetArray(debug, "_encounters", north, south, second); SetArray(debug, "_passages", passages);
            Set(debug, "_playerExperience", player.GetComponent<PlayerExperienceController>());
            EditorSceneManager.SaveScene(scene, ScenePath); AssetDatabase.SaveAssets();
            Debug.Log("Authored stage-10 fixture scenes and navigation saved.");
        }

        private static void BuildDestination()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = New("Destination metadata").AddComponent<MapSceneRoot>();
            var content = New("Destination content");
            var map = content.AddComponent<MapTraversalController>();
            Set(map, "_initialRegionId", "arrival"); SetArray(map, "_regionIds", "arrival");
            content.AddComponent<EnemyRegistry>();
            var geo = New("Geometry", content.transform).transform;
            Floor(geo, "Arrival floor", 0,0,16,16,Color.gray);
            Wall(geo,-8,-8,8,-8); Wall(geo,-8,8,8,8); Wall(geo,-8,-8,-8,8); Wall(geo,8,-8,8,8);
            Label(content.transform, "ARRIVAL — PLAYER STATE PRESERVED", new Vector3(-6,2,5));
            Rewards(content.transform, null); Lighting(content.transform); Bake(geo.gameObject, "DestinationNavigation.asset");
            var entry = New("Arrival point", content.transform).transform; entry.position = new Vector3(0,.1f,0);
            Set(root, "_content", content); Set(root, "_map", map);
            var so = new SerializedObject(root); var entries = so.FindProperty("_entries"); entries.arraySize = 1;
            var item = entries.GetArrayElementAtIndex(0);
            item.FindPropertyRelative("Id").stringValue = "arrival";
            item.FindPropertyRelative("RegionId").stringValue = "arrival";
            item.FindPropertyRelative("Point").objectReferenceValue = entry;
            so.ApplyModifiedPropertiesWithoutUndo(); content.SetActive(false);
            EditorSceneManager.SaveScene(scene, PlayableMapFixtureBootstrap.TargetScene);
        }

        private static void Rewards(Transform parent, GameObject player)
        {
            var ui = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>("Assets/_Project/Prefabs/UI/RewardSelectionUI.prefab"), parent);
            var selection = ui.GetComponentInChildren<RewardSelectionController>(true);
            if (player != null)
            {
                Set(selection,"_claimController",player.GetComponent<PlayerRewardClaimController>());
                Set(selection,"_playerController",player.GetComponent<PlayerController>());
                Set(selection,"_playerDeathController",player.GetComponent<PlayerDeathController>());
            }
            var services = New("Map chest services",parent);
            var spawner = services.AddComponent<ChestSpawner>();
            var bootstrap = services.AddComponent<ChestSpawnerBootstrap>();
            Set(bootstrap,"_spawner",spawner); Set(bootstrap,"_selection",selection);
            if (player != null) Set(bootstrap,"_playerBuild",player.GetComponent<PlayerBuildController>());
            var placement = services.AddComponent<ChestSpawnPlacement>();
            var levelUp = services.AddComponent<LevelUpChestSource>();
            Set(levelUp,"_spawner",spawner); Set(levelUp,"_placement",placement);
            Set(levelUp,"_dropTable",Load<ChestDropTable>("Assets/_Project/Data/Chests/Dev/CDT_DevelopmentLevelUp.asset"));
            if (player != null)
            {
                Set(levelUp,"_playerExperience",player.GetComponent<PlayerExperienceController>());
                Set(levelUp,"_playerHealth",player.GetComponent<HealthComponent>());
            }
        }

        private static PreparedRegionEncounter Encounter(Transform parent, GameObject player, EnemyRegistry registry,
            string region, Vector3[] positions)
        {
            var encounter = New(region+" encounter",parent).AddComponent<PreparedRegionEncounter>();
            var spawner = encounter.gameObject.AddComponent<EnemySpawner>();
            var points = new Transform[positions.Length];
            for (int i=0;i<positions.Length;i++) { points[i]=New("Spawn "+i,encounter.transform).transform; points[i].position=positions[i]; }
            Set(encounter,"_regionId",region);
            Set(encounter,"_group",Load<EnemyWaveDefinition>("Assets/_Project/Data/Waves/Test/EW_PlayableFixture.asset"));
            SetArray(encounter,"_spawnPoints",points); Set(encounter,"_spawner",spawner); Set(encounter,"_registry",registry);
            Set(encounter,"_player",player.transform); Set(encounter,"_playerHealth",player.GetComponent<HealthComponent>());
            Set(encounter,"_maxEnemiesPerFrame",1); Set(encounter,"_millisecondsPerFrame",2f);
            return encounter;
        }

        private static RegionPassageController Passage(Transform parent, Vector3 position, Quaternion rotation,
            string destination, bool oneWay, GameObject player, MapTraversalController map, PreparedRegionEncounter encounter)
        {
            var go=New("Passage to "+destination,parent); go.transform.SetPositionAndRotation(position,rotation);
            var volume=go.AddComponent<BoxCollider>(); volume.isTrigger=true; volume.center=Vector3.up*2;
            volume.size=new Vector3(4.2f,5,4);
            var barrier=Box("Physical barrier",go.transform,Vector3.zero,new Vector3(4,4,.4f),Color.red);
            barrier.transform.localPosition=Vector3.up*2; barrier.transform.localRotation=Quaternion.identity;
            var obstacle=barrier.AddComponent<NavMeshObstacle>(); obstacle.shape=NavMeshObstacleShape.Box;
            obstacle.size=Vector3.one; obstacle.carving=true; barrier.AddComponent<FixtureBarrierView>();
            var controller=go.AddComponent<RegionPassageController>();
            Set(controller,"_clearanceVolume",volume); Set(controller,"_blocker",barrier.GetComponent<Collider>());
            Set(controller,"_player",player.transform); Set(controller,"_health",player.GetComponent<HealthComponent>());
            Set(controller,"_sourceId","main"); Set(controller,"_destinationId",destination);
            Set(controller,"_direction",oneWay?RegionTransitionDirection.OneWay:RegionTransitionDirection.Returnable);
            Set(controller,"_requirement",RegionTransitionRequirement.Free); Set(controller,"_map",map);
            Set(controller,"_destinationPreparation",encounter); return controller;
        }

        private static void Trigger(Transform parent,string name,Vector3 position,Vector3 size,GameObject player,
            PreparedRegionEncounter encounter,bool activate)
        {
            var go=New(name,parent); go.transform.position=position;
            var collider=go.AddComponent<BoxCollider>(); collider.isTrigger=true; collider.size=size;
            var trigger=go.AddComponent<RegionPreparationTrigger>();
            Set(trigger,"_volume",collider); Set(trigger,"_player",player.transform); Set(trigger,"_encounter",encounter);
            Set(trigger,"_purpose",activate?RegionPreparationTrigger.Purpose.Activate:RegionPreparationTrigger.Purpose.Prepare);
            Label(go.transform,name,Vector3.up);
        }
        private static void Floor(Transform parent,string name,float x,float z,float width,float depth,Color color)
        { Box(name,parent,new Vector3(x,-.5f,z),new Vector3(width,1,depth),color).layer=7; }
        private static void Wall(Transform parent,float x1,float z1,float x2,float z2,string name="Boundary wall")
        { Box(name,parent,new Vector3((x1+x2)/2,2,(z1+z2)/2),new Vector3(Mathf.Abs(x2-x1)+.5f,4,Mathf.Abs(z2-z1)+.5f),new Color(.18f,.22f,.28f)); }
        private static GameObject Box(string name,Transform parent,Vector3 position,Vector3 size,Color color)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name;
            go.transform.SetParent(parent,false); go.transform.position=position; go.transform.localScale=size;
            string path=Folder+"Fixture-"+ColorUtility.ToHtmlStringRGBA(color)+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material==null)
            {
                material=new Material(Shader.Find("Universal Render Pipeline/Lit")); material.color=color;
                AssetDatabase.CreateAsset(material,path);
            }
            go.GetComponent<Renderer>().sharedMaterial=material; return go;
        }
        private static void Bake(GameObject geometry,string file)
        {
            var surface=geometry.AddComponent<NavMeshSurface>(); surface.collectObjects=CollectObjects.Children;
            surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders; surface.BuildNavMesh();
            if(surface.navMeshData==null) throw new InvalidOperationException("NavMesh bake failed.");
            string path=Folder+file; var previous=AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
            if(previous==null) AssetDatabase.CreateAsset(surface.navMeshData,path);
            else
            {
                var generated=surface.navMeshData; surface.RemoveData();
                EditorUtility.CopySerialized(generated,previous); Object.DestroyImmediate(generated);
                surface.navMeshData=previous; EditorUtility.SetDirty(previous); surface.AddData();
            }
            EditorUtility.SetDirty(surface);
        }
        private static void Lighting(Transform parent)
        {
            var light=New("Directional light",parent).AddComponent<Light>(); light.type=LightType.Directional;
            light.intensity=1.3f; light.transform.rotation=Quaternion.Euler(50,-30,0); RenderSettings.ambientLight=Color.gray;
        }
        private static void Label(Transform parent,string text,Vector3 position)
        {
            var go=New(text,parent); go.transform.localPosition=position; var mesh=go.AddComponent<TextMesh>();
            mesh.text=text; mesh.fontSize=32; mesh.characterSize=.18f; mesh.color=Color.white;
        }
        private static GameObject New(string name,Transform parent=null)
        { var go=new GameObject(name); go.transform.SetParent(parent,false); return go; }
        private static T Load<T>(string path) where T:Object => AssetDatabase.LoadAssetAtPath<T>(path)
            ?? throw new InvalidOperationException("Missing fixture asset: "+path);
        private static void Set(Object target,string property,object value)
        {
            var so=new SerializedObject(target); var p=so.FindProperty(property)
                ?? throw new InvalidOperationException("Missing property "+property);
            if(value is Object obj) p.objectReferenceValue=obj;
            else if(value is string text) p.stringValue=text;
            else if(value is bool flag) p.boolValue=flag;
            else if(value is int number) p.intValue=number;
            else if(value is float real) p.floatValue=real;
            else if(value is Enum enumeration) p.enumValueIndex=Convert.ToInt32(enumeration);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void SetArray(Object target,string property,params object[] values)
        {
            var so=new SerializedObject(target); var p=so.FindProperty(property); p.arraySize=values.Length;
            for(int i=0;i<values.Length;i++)
            {
                var e=p.GetArrayElementAtIndex(i);
                if(values[i] is Object obj) e.objectReferenceValue=obj;
                else if(values[i] is string text) e.stringValue=text;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        public static void BuildValidationPlayer()
        {
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{ScenePath,PlayableMapFixtureBootstrap.TargetScene},
                locationPathName=".codex-build/Stage10/ProjectFirstRun.exe",
                target=BuildTarget.StandaloneWindows64,
                options=BuildOptions.Development | BuildOptions.CleanBuildCache });
            if(report.summary.result!=BuildResult.Succeeded) throw new InvalidOperationException("Fixture build failed: "+report.summary.result);
            Debug.Log("STAGE10_BUILD_PASS bytes="+report.summary.totalSize);
        }
    }
}
#endif
