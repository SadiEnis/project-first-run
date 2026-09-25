#if UNITY_EDITOR
using System;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Editor
{
    public static class PlasmaContentAssetBuilder
    {
        private const string Folder = "Assets/_Project/Prefabs/Weapons/";
        [MenuItem("Project First Run/Apply Plasma Rifle Content Defaults")]
        public static void Build()
        {
            var visual = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "PlasmaBurnVisual.prefab");
            if (visual == null)
            {
                var body = Body("Plasma burn visual", new Color(1, .35f, .08f), new Vector3(.25f, .4f, .25f));
                visual = PrefabUtility.SaveAsPrefabAsset(body, Folder + "PlasmaBurnVisual.prefab");
                Object.DestroyImmediate(body);
            }
            var prefab = AssetDatabase.LoadAssetAtPath<PlasmaProjectile>(Folder + "PlasmaProjectile.prefab");
            if (prefab == null)
            {
                var body = Body("Plasma projectile", new Color(.05f, 1, .8f), new Vector3(.14f, .14f, .5f));
                var projectile = body.AddComponent<PlasmaProjectile>();
                var data = new SerializedObject(projectile);
                data.FindProperty("_burnVisualPrefab").objectReferenceValue = visual;
                data.ApplyModifiedPropertiesWithoutUndo();
                prefab = PrefabUtility.SaveAsPrefabAsset(body, Folder + "PlasmaProjectile.prefab").GetComponent<PlasmaProjectile>();
                Object.DestroyImmediate(body);
            }
            var definition = AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/_Project/Data/Items/Weapons/WD_PlasmaRifle.asset");
            if (definition == null) throw new InvalidOperationException("Existing Plasma Rifle asset is required.");
            var weapon = new SerializedObject(definition);
            weapon.FindProperty("_deliveryMode").intValue = 2;
            weapon.FindProperty("_plasmaPrefab").objectReferenceValue = prefab;
            weapon.FindProperty("_gameplayEffect").stringValue = "Automatic energy projectiles. Levels add damage, rate, piercing, capacity and target burn.";
            weapon.FindProperty("_triggerMode").intValue = 1;
            weapon.FindProperty("_maximumLevel").intValue = 8;
            weapon.FindProperty("_range").floatValue = 100;
            weapon.FindProperty("_startingReserveAmmo").intValue = 48;
            Level(weapon.FindProperty, 1, true);
            var levels = weapon.FindProperty("_additionalLevels"); levels.arraySize = 7;
            for (int i = 0; i < 7; i++) Level(levels.GetArrayElementAtIndex(i).FindPropertyRelative, i + 2, false);
            weapon.ApplyModifiedPropertiesWithoutUndo();
            _ = new PlayerWeaponRuntimeEntry(definition);
            AssetDatabase.SaveAssets();
            Debug.Log("PLASMA_CONTENT_CREATED");
        }
        private static void Level(Func<string, SerializedProperty> p, int level, bool first)
        {
            p(first ? "_baseDamage" : "_damage").floatValue = level >= 5 ? 35 : level >= 2 ? 30 : 25;
            p("_magazineCapacity").intValue = level >= 6 ? 20 : 12;
            p("_shotsPerSecond").floatValue = level >= 3 ? 6 : 5;
            p("_reloadDuration").floatValue = 1.5f;
            p("_pelletCount").intValue = 1; p("_spreadHalfAngle").floatValue = 0; p("_pushDistance").floatValue = 0;
            p("_criticalChance").floatValue = 0; p("_criticalMultiplier").floatValue = 2; p("_preparationDuration").floatValue = 0;
            p("_recoilDegrees").floatValue = .3f; p("_recoilRecoveryDelay").floatValue = .1f;
            p("_recoilRecoveryDuration").floatValue = .22f; p("_recoilMaximumOffset").floatValue = 6;
            var plasma = p("_plasma");
            plasma.FindPropertyRelative("_speed").floatValue = 60;
            plasma.FindPropertyRelative("_radius").floatValue = .08f;
            plasma.FindPropertyRelative("_lifetime").floatValue = 2;
            plasma.FindPropertyRelative("_pierceCount").intValue = level >= 7 ? 2 : level >= 4 ? 1 : 0;
            plasma.FindPropertyRelative("_burn").boolValue = level == 8;
            plasma.FindPropertyRelative("_burnDamage").floatValue = 5;
        }
        private static GameObject Body(string name, Color color, Vector3 scale)
        {
            string path = Folder + name.Replace(" ", "-") + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) throw new InvalidOperationException("URP Unlit shader is required.");
                material = new Material(shader); material.SetColor("_BaseColor", color);
                AssetDatabase.CreateAsset(material, path);
            }
            var root = new GameObject(name);
            var child = GameObject.CreatePrimitive(PrimitiveType.Sphere); child.name = "Visual";
            child.transform.SetParent(root.transform, false); child.transform.localScale = scale;
            Object.DestroyImmediate(child.GetComponent<Collider>());
            var renderer = child.GetComponent<Renderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows = false;
            return root;
        }
    }
}
#endif
