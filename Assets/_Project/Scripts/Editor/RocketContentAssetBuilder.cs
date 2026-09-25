#if UNITY_EDITOR
using System;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Editor
{
    public static class RocketContentAssetBuilder
    {
        public const string WeaponPath = "Assets/_Project/Data/Items/Weapons/WD_RocketLauncher.asset";
        private const string Folder = "Assets/_Project/Prefabs/Weapons/";

        [MenuItem("Project First Run/Create Missing Rocket Launcher Content")]
        public static void Build()
        {
            var rocketMaterial = Material("Rocket-Body.mat", new Color(1, .32f, .06f, 1), false);
            var fragmentMaterial = Material("Rocket-Fragment.mat", new Color(1, .85f, .15f, 1), false);
            var blastMaterial = Material("Rocket-Blast.mat", new Color(1, .5f, .05f, .22f), true);
            var blast = AssetDatabase.LoadAssetAtPath<RocketBlastVisual>(Folder + "RocketBlast.prefab");
            if (blast == null)
            {
                var go = Body("Rocket blast", blastMaterial, Vector3.one);
                go.AddComponent<RocketBlastVisual>();
                blast = PrefabUtility.SaveAsPrefabAsset(go, Folder + "RocketBlast.prefab").GetComponent<RocketBlastVisual>();
                Object.DestroyImmediate(go);
            }
            var fragment = Projectile("RocketFragment", fragmentMaterial, new Vector3(.1f, .1f, .14f), null, null);
            var rocket = Projectile("RocketProjectile", rocketMaterial, new Vector3(.25f, .25f, .65f), fragment, blast);
            var definition = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(WeaponPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<WeaponDefinition>();
                var data = new SerializedObject(definition);
                data.FindProperty("_stableId").stringValue = "weapon.rocket-launcher";
                data.FindProperty("_displayName").stringValue = "Rocket Launcher";
                data.FindProperty("_gameplayEffect").stringValue = "Launches an explosive rocket. Level up for damage, speed, capacity, reload, blast radius and level-eight fragments.";
                data.FindProperty("_maximumLevel").intValue = 8;
                data.FindProperty("_triggerMode").enumValueIndex = 0;
                data.FindProperty("_deliveryMode").enumValueIndex = 1;
                data.FindProperty("_rocketPrefab").objectReferenceValue = rocket;
                data.FindProperty("_range").floatValue = 60;
                data.FindProperty("_damageMask").intValue = 247;
                data.FindProperty("_startingReserveAmmo").intValue = 12;
                Level(data.FindProperty, 1, true);
                var levels = data.FindProperty("_additionalLevels");
                levels.arraySize = 7;
                for (int i = 0; i < 7; i++) Level(levels.GetArrayElementAtIndex(i).FindPropertyRelative, i + 2, false);
                data.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(definition, WeaponPath);
            }
            _ = new PlayerWeaponRuntimeEntry(definition);
            AddToPool("RP_WeaponChest", definition);
            AddToPool("RP_DevelopmentRewardPool", definition);
            AssetDatabase.SaveAssets();
            ContentArenaSceneBuilder.UpdateRocketConnections();
            Debug.Log("ROCKET_CONTENT_CREATED");
        }

        private static void Level(Func<string, SerializedProperty> p, int level, bool first)
        {
            p(first ? "_baseDamage" : "_damage").floatValue = level >= 6 ? 120 : level >= 2 ? 100 : 80;
            p("_magazineCapacity").intValue = level >= 4 ? 4 : 3;
            p("_shotsPerSecond").floatValue = .8f;
            p("_reloadDuration").floatValue = level >= 5 ? 2.4f : 3;
            p("_pelletCount").intValue = 1;
            p("_spreadHalfAngle").floatValue = 0; p("_pushDistance").floatValue = 0;
            p("_preparationDuration").floatValue = 0; p("_criticalChance").floatValue = 0;
            p("_criticalMultiplier").floatValue = 2;
            p("_recoilDegrees").floatValue = 5;
            p("_recoilRecoveryDelay").floatValue = .1f;
            p("_recoilRecoveryDuration").floatValue = .35f;
            p("_recoilMaximumOffset").floatValue = 10;
            var r = p("_rocket");
            r.FindPropertyRelative("_speed").floatValue = level >= 3 ? 35 : 25;
            r.FindPropertyRelative("_collisionRadius").floatValue = .15f;
            r.FindPropertyRelative("_lifetime").floatValue = 4;
            r.FindPropertyRelative("_blastRadius").floatValue = level >= 7 ? 4 : 3;
            r.FindPropertyRelative("_fragmentCount").intValue = level == 8 ? 8 : 0;
            r.FindPropertyRelative("_fragmentDamage").floatValue = 20;
            r.FindPropertyRelative("_fragmentSpeed").floatValue = 20;
            r.FindPropertyRelative("_fragmentRadius").floatValue = .05f;
            r.FindPropertyRelative("_fragmentRange").floatValue = 8;
            r.FindPropertyRelative("_fragmentLifetime").floatValue = .4f;
        }

        private static RocketProjectile Projectile(string name, Material material, Vector3 scale,
            RocketProjectile fragment, RocketBlastVisual blast)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<RocketProjectile>(Folder + name + ".prefab");
            if (prefab != null) return prefab;
            var go = Body(name, material, scale);
            var component = go.AddComponent<RocketProjectile>();
            var data = new SerializedObject(component);
            data.FindProperty("_fragmentPrefab").objectReferenceValue = fragment;
            data.FindProperty("_blastPrefab").objectReferenceValue = blast;
            data.ApplyModifiedPropertiesWithoutUndo();
            prefab = PrefabUtility.SaveAsPrefabAsset(go, Folder + name + ".prefab").GetComponent<RocketProjectile>();
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject Body(string name, Material material, Vector3 scale)
        {
            var root = new GameObject(name);
            var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = scale;
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            var renderer = visual.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return root;
        }

        private static Material Material(string name, Color color, bool transparent)
        {
            string path = Folder + name;
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) throw new InvalidOperationException("Missing URP unlit shader.");
            material = new Material(shader);
            material.SetColor("_BaseColor", color);
            if (transparent)
            {
                material.SetFloat("_Surface", 1); material.SetFloat("_ZWrite", 0);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)RenderQueue.Transparent;
            }
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void AddToPool(string name, WeaponDefinition definition)
        {
            var pool = AssetDatabase.LoadAssetAtPath<RewardItemPool>("Assets/_Project/Data/Reward/Dev/" + name + ".asset");
            var data = new SerializedObject(pool);
            var items = data.FindProperty("_items");
            for (int i = 0; i < items.arraySize; i++) if (items.GetArrayElementAtIndex(i).objectReferenceValue == definition) return;
            items.arraySize++;
            items.GetArrayElementAtIndex(items.arraySize - 1).objectReferenceValue = definition;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
