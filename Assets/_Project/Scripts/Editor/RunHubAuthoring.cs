#if UNITY_EDITOR
using System;
using System.Linq;
using ProjectFirstRun.Flow;
using ProjectFirstRun.Input;
using ProjectFirstRun.Player;
using ProjectFirstRun.UI.Hud;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectFirstRun.Editor
{
    /// <summary>One-time saved Hub migration. Later runs preserve the authored scene.</summary>
    public static class RunHubAuthoring
    {
        public const string ScenePath = "Assets/_Project/Scenes/Demo/RunHub.unity";
        [MenuItem("Project First Run/Run/Prepare Walkable Hub")]
        public static void Prepare()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (!opened && scene.isDirty) throw new InvalidOperationException("Save or discard your Hub scene edits before preparing it.");
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            if (scene.GetRootGameObjects().Any(x => x.GetComponentInChildren<RunHubInteraction>(true) != null))
            {
                if (opened) EditorSceneManager.CloseScene(scene, true);
                return;
            }
            var previous = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(scene);
            try
            {
                var hub = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<RunHubController>(true)).Single();
                var view = hub.GetComponent<RunHubView>();
                var camera = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Camera>(true)).Single();
                var roots = scene.GetRootGameObjects();
                foreach (var renderer in roots.SelectMany(x => x.GetComponentsInChildren<MeshRenderer>()))
                    if (renderer.GetComponent<Collider>() == null) renderer.gameObject.AddComponent<BoxCollider>();
                var platform = roots.FirstOrDefault(x => x.name == "Future upgrade platform");
                if (platform != null) UnityEngine.Object.DestroyImmediate(platform);

                var world = new GameObject("Hub exploration");
                var player = new GameObject("Hub player — movement only");
                player.SetActive(false);
                player.transform.SetParent(world.transform);
                player.transform.position = new Vector3(0, .05f, -4);
                var character = player.AddComponent<CharacterController>();
                character.height = 1.8f; character.radius = .3f; character.center = new Vector3(0, .9f, 0); character.stepOffset = .3f;
                camera.transform.SetParent(player.transform);
                camera.transform.localPosition = new Vector3(0, 1.65f, 0);
                camera.transform.localRotation = Quaternion.identity;
                camera.fieldOfView = 75;
                player.AddComponent<PlayerInputReader>();
                player.AddComponent<PlayerMotor>();
                var look = player.AddComponent<PlayerLook>();
                Set(look, "_cameraPivot", camera.transform);
                var control = player.AddComponent<PlayerController>();

                var stone = Material("HubStone", new Color(.12f, .15f, .19f));
                var bronze = Material("HubBronze", new Color(.55f, .29f, .1f));
                var glow = Material("HubGate", new Color(.15f, .55f, .6f));
                Box(world.transform, "South wall", new Vector3(0, 3, -8), new Vector3(18, 6, 1), stone);
                Box(world.transform, "Merchant counter", new Vector3(-4, .55f, 3), new Vector3(3, 1.1f, 1), bronze);
                var vendor = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                vendor.name = "Merchant — interact target";
                vendor.transform.SetParent(world.transform);
                vendor.transform.position = new Vector3(-4, 1.2f, 4.1f);
                vendor.transform.localScale = new Vector3(.8f, 1.2f, .8f);
                vendor.GetComponent<Renderer>().sharedMaterial = stone;
                Box(world.transform, "Merchant canopy", new Vector3(-4, 3, 4), new Vector3(4, .2f, 3), stone);
                Label(world.transform, "Merchant sign", "THE KEEPER", new Vector3(-4, 2.8f, 2.3f), .15f);
                Box(world.transform, "Gate left", new Vector3(-2, 2.25f, 14), new Vector3(.8f, 4.5f, 1), bronze);
                Box(world.transform, "Gate right", new Vector3(2, 2.25f, 14), new Vector3(.8f, 4.5f, 1), bronze);
                Box(world.transform, "Gate lintel", new Vector3(0, 4.5f, 14), new Vector3(4.8f, .6f, 1), bronze);
                Box(world.transform, "Gate light", new Vector3(0, 2, 15.3f), new Vector3(3.2f, 4, .1f), glow);
                Label(world.transform, "Gate sign", "DESCEND", new Vector3(0, 4, 13.4f), .18f);
                var exit = new GameObject("Run departure threshold", typeof(BoxCollider));
                exit.transform.SetParent(world.transform);
                exit.transform.position = new Vector3(0, 1.5f, 14.4f);
                var bounds = exit.GetComponent<BoxCollider>();
                bounds.size = new Vector3(3.2f, 3, 1.2f); bounds.isTrigger = true;
                var interaction = hub.gameObject.AddComponent<RunHubInteraction>();
                Set(interaction, "_hub", hub); Set(interaction, "_player", control); Set(interaction, "_camera", camera);
                Set(interaction, "_vendor", vendor.GetComponent<Collider>()); Set(interaction, "_exit", bounds);
                player.SetActive(true);

                var events = new GameObject("Hub UI EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
                var canvas = new GameObject("Hub UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvas.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = .5f;
                var panel = Panel(canvas.transform, "Merchant panel", new Vector2(0, 0), new Vector2(1080, 580), new Color(.06f, .09f, .14f, .98f));
                var shop = panel.gameObject.AddComponent<CanvasGroup>();
                shop.alpha = 0; shop.interactable = shop.blocksRaycasts = false;
                TextAt(panel, "Title", "THE KEEPER / PERMANENT UPGRADES", new Vector2(30, -20), new Vector2(750, 45), 25);
                var balance = TextAt(panel, "Balance", "", new Vector2(790, -20), new Vector2(260, 45), 22);
                balance.alignment = TextAnchor.MiddleRight;
                TextAt(panel, "Subtitle", "Prepare for the descent", new Vector2(30, -66), new Vector2(500, 30), 16);
                var cards = new Button[3]; var labels = new Text[3]; var icons = new HudGlyphGraphic[3];
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = new Vector2(30 + i % 2 * 245, -112 - i / 2 * 170);
                    cards[i] = ButtonAt(panel, "Upgrade card " + i, "", pos, new Vector2(225, 150));
                    labels[i] = cards[i].GetComponentInChildren<Text>();
                    labels[i].rectTransform.offsetMin = new Vector2(8, 8);
                    labels[i].rectTransform.offsetMax = new Vector2(-8, -80);
                    labels[i].fontSize = 17;
                    var icon = Rect(cards[i].transform, "Upgrade icon", new Vector2(85, -12), new Vector2(55, 55));
                    icons[i] = icon.gameObject.AddComponent<HudGlyphGraphic>(); icons[i].raycastTarget = false;
                }
                var detailPanel = Panel(panel, "Selected upgrade details", new Vector2(250, -5), new Vector2(490, 410), new Color(.09f, .13f, .19f));
                var description = TextAt(detailPanel, "Description", "", new Vector2(24, -18), new Vector2(442, 300), 21);
                var buy = ButtonAt(detailPanel, "Purchase", "Purchase", new Vector2(24, -332), new Vector2(442, 52));
                var status = TextAt(panel, "Purchase status", "", new Vector2(30, -465), new Vector2(1020, 48), 17);
                var back = ButtonAt(panel, "Back to Hub", "Back", new Vector2(30, -524), new Vector2(180, 38));
                var hints = TextAt(panel, "Input hints", "{Navigate} Select    {Submit} Confirm    {Cancel} Back",
                    new Vector2(260, -524), new Vector2(790, 38), 17);
                hints.gameObject.AddComponent<InputPromptLabel>();
                Nav(cards[0], null, cards[1], null, cards[2]);
                Nav(cards[1], cards[0], buy, null, cards[2]);
                Nav(cards[2], null, buy, cards[0], back);
                Nav(buy, cards[0], null, cards[1], back);
                Nav(back, null, buy, cards[2], null);
                var menuPanel = Panel(canvas.transform, "Hub menu", Vector2.zero, new Vector2(430, 290), new Color(.06f, .09f, .14f, .98f));
                var menu = menuPanel.gameObject.AddComponent<CanvasGroup>();
                menu.alpha = 0; menu.interactable = menu.blocksRaycasts = false;
                TextAt(menuPanel, "Title", "PROJECT FIRST RUN / HUB", new Vector2(25, -20), new Vector2(380, 50), 23);
                var resume = ButtonAt(menuPanel, "Resume", "Resume", new Vector2(35, -100), new Vector2(360, 55));
                var quit = ButtonAt(menuPanel, "Quit", "Quit", new Vector2(35, -178), new Vector2(360, 55));
                Nav(resume, null, null, quit, quit); Nav(quit, null, null, resume, resume);
                var crosshair = TextAt(canvas.transform, "Crosshair", "·", Vector2.zero, new Vector2(24, 24), 24);
                crosshair.rectTransform.anchorMin = crosshair.rectTransform.anchorMax = new Vector2(.5f,.5f);
                crosshair.rectTransform.pivot = new Vector2(.5f,.5f); crosshair.alignment = TextAnchor.MiddleCenter;
                var prompt = TextAt(canvas.transform, "Interaction prompt", "", new Vector2(0, 38), new Vector2(900, 44), 20);
                prompt.rectTransform.anchorMin = prompt.rectTransform.anchorMax = new Vector2(.5f,0);
                prompt.rectTransform.pivot = new Vector2(.5f,0); prompt.alignment = TextAnchor.MiddleCenter;
                prompt.gameObject.AddComponent<InputPromptLabel>();
                Set(view, "_interaction", interaction); Set(view, "_shop", shop); Set(view, "_menu", menu);
                SetArray(view, "_cards", cards); SetArray(view, "_cardLabels", labels); SetArray(view, "_icons", icons);
                Set(view, "_buy", buy); Set(view, "_back", back); Set(view, "_resume", resume); Set(view, "_quit", quit);
                Set(view, "_balance", balance); Set(view, "_description", description);
                Set(view, "_buyLabel", buy.GetComponentInChildren<Text>()); Set(view, "_status", status);
                Set(view, "_prompt", prompt); Set(view, "_crosshair", crosshair); Set(view, "_events", events.GetComponent<EventSystem>());
                interaction.ValidateConfiguration(); view.ValidateConfiguration(); hub.ValidateConfiguration();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("Walkable Hub saved: merchant grid, exploration player and run exit.");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
        }
        private static Material Material(string name, Color color)
        {
            string path = "Assets/_Project/Scenes/Demo/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
        private static void Box(Transform parent, string name, Vector3 position, Vector3 size, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name; go.transform.SetParent(parent); go.transform.position = position; go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = material;
        }
        private static void Label(Transform parent, string name, string text, Vector3 position, float scale)
        {
            var go = new GameObject(name, typeof(TextMesh));
            go.transform.SetParent(parent); go.transform.position = position;
            var label = go.GetComponent<TextMesh>(); label.text = text; label.fontSize = 64;
            label.characterSize = scale; label.anchor = TextAnchor.MiddleCenter; label.color = new Color(1,.8f,.5f);
        }
        private static RectTransform Rect(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.layer = 5;
            var rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = pos; rect.sizeDelta = size; return rect;
        }
        private static RectTransform Panel(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            var rect = Rect(parent, name, pos, size);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f,.5f);
            rect.gameObject.AddComponent<Image>().color = color; return rect;
        }
        private static Text TextAt(Transform parent, string name, string text, Vector2 pos, Vector2 size, int fontSize)
        {
            var rect = Rect(parent, name, pos, size);
            var label = rect.gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = text; label.fontSize = fontSize; label.color = new Color(.87f,.92f,.96f);
            label.raycastTarget = false; label.verticalOverflow = VerticalWrapMode.Truncate; return label;
        }
        private static Button ButtonAt(Transform parent, string name, string text, Vector2 pos, Vector2 size)
        {
            var rect = Rect(parent, name, pos, size);
            var image = rect.gameObject.AddComponent<Image>(); image.color = Color.white;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            rect.gameObject.AddComponent<EventTrigger>();
            var colors = button.colors;
            colors.normalColor = new Color(.12f,.19f,.26f); colors.highlightedColor = colors.normalColor;
            colors.selectedColor = new Color(.22f,.45f,.52f); colors.pressedColor = new Color(.5f,.34f,.12f);
            colors.fadeDuration = .12f; button.colors = colors;
            var label = TextAt(rect, "Label", text, Vector2.zero, size, 21);
            label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(8,8); label.rectTransform.offsetMax = new Vector2(-8,-8);
            label.alignment = TextAnchor.MiddleCenter;
            return button;
        }
        private static void Nav(Selectable target, Selectable left, Selectable right, Selectable up, Selectable down)
        {
            target.navigation = new Navigation { mode = Navigation.Mode.Explicit,
                selectOnLeft = left, selectOnRight = right, selectOnUp = up, selectOnDown = down };
        }
        private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var data = new SerializedObject(target);
            data.FindProperty(field).objectReferenceValue = value; data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void SetArray(UnityEngine.Object target, string field, UnityEngine.Object[] values)
        {
            var data = new SerializedObject(target); var array = data.FindProperty(field); array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
