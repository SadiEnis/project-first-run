using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProjectFirstRun.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

namespace ProjectFirstRun.UI.Hud
{
    /// <summary>Shared, read-only presentation for Canvas and legacy IMGUI. Never enables gameplay actions.</summary>
    public static class InputPrompts
    {
        public readonly struct Part
        {
            public readonly string Text;
            public readonly Texture Texture;
            public readonly Rect UV;
            public bool IsIcon => Texture != null;
            public Part(string text, Texture texture = null, Rect uv = default)
            { Text = text; Texture = texture; UV = uv; }
        }

        public const string MenuHint = "{Navigate} Select     {Submit} Confirm";
        private static readonly Regex Tokens = new(@"\{([^}]+)\}|\n|[^\S\n]+|[^\s{]+", RegexOptions.Compiled);
        private static readonly Dictionary<int, bool> StickActive = new();
        private static readonly Dictionary<string, List<Part>> Parsed = new();
        private static int _cachedRevision = -1, _cachedStyle = -1;
        private static InputPromptSettings _settings;
        private static PlayerInputReader _reader;
        private static ProjectFirstRunInputActions _fallback;
        private static InputDevice _lastDevice;
        public static PromptDevice Device { get; private set; }
        public static int Revision { get; private set; }
        public static bool IsGamepad => Device != PromptDevice.KeyboardMouse;
        public static InputPromptSettings Settings => _settings != null ? _settings :
            _settings = Resources.Load<InputPromptSettings>("InputPromptSettings");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            InputSystem.onAfterUpdate -= Poll;
            InputSystem.onDeviceChange -= DeviceChanged;
            _fallback?.Dispose(); _fallback = null; _settings = null; _reader = null; _lastDevice = null;
            StickActive.Clear(); Device = PromptDevice.KeyboardMouse; Revision = 0;
            Parsed.Clear(); _cachedRevision = _cachedStyle = -1;
            InputSystem.onAfterUpdate += Poll;
            InputSystem.onDeviceChange += DeviceChanged;
        }

        public static PromptDevice Classify(InputDevice device) => device is DualSenseGamepadHID ? PromptDevice.DualSense :
            device is DualShockGamepad ? PromptDevice.DualShock : device is XInputController ? PromptDevice.Xbox :
            device is Gamepad ? PromptDevice.GenericGamepad : PromptDevice.KeyboardMouse;

        private static void Use(InputDevice device)
        {
            var kind = Classify(device);
            if (_lastDevice == device && Device == kind) return;
            _lastDevice = device; Device = kind; Revision++;
        }

        private static void DeviceChanged(InputDevice device, InputDeviceChange change)
        {
            if (change != InputDeviceChange.Removed && change != InputDeviceChange.Disconnected && change != InputDeviceChange.Disabled) return;
            StickActive.Remove(device.deviceId);
            if (_lastDevice == device) Use(Keyboard.current);
        }

        private static void Poll()
        {
            foreach (var pad in Gamepad.all)
            {
                if (!pad.enabled) continue;
                bool stick = pad.leftStick.ReadValue().sqrMagnitude > .25f || pad.rightStick.ReadValue().sqrMagnitude > .25f;
                StickActive.TryGetValue(pad.deviceId, out bool previous);
                bool pressed = stick && !previous;
                StickActive[pad.deviceId] = stick;
                foreach (var control in pad.allControls)
                    if (control is ButtonControl button && button.wasPressedThisFrame) { pressed = true; break; }
                if (pressed) Use(pad);
            }
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard != null && keyboard.anyKey.wasPressedThisFrame) Use(keyboard);
            else if (mouse != null && (mouse.delta.ReadValue().sqrMagnitude > 9 || mouse.scroll.ReadValue().sqrMagnitude > 0 ||
                mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame)) Use(mouse);
        }

        // Public deterministic mapping also allows tests without connected physical controllers.
        public static string GlyphName(string path, PromptDevice device, GamepadIconStyle style, out string fallback)
        {
            int slash = path.LastIndexOf('/');
            string control = slash >= 0 ? path.Substring(slash + 1) : path;
            fallback = control;
            if (path.StartsWith("<Keyboard>"))
            {
                fallback = control.ToUpperInvariant();
                return control switch { "escape" => "esc", "leftShift" or "rightShift" => "shift",
                    "upArrow" => "ku", "downArrow" => "kd", "leftArrow" => "kl", "rightArrow" => "kr", _ => control.ToLowerInvariant() };
            }
            if (path.StartsWith("<Mouse>"))
            {
                fallback = control == "delta" ? "Mouse" : control;
                return control switch { "leftButton" => "ml", "rightButton" => "mr", "middleButton" => "mm", "delta" => "mouse", _ => null };
            }
            bool ps = style == GamepadIconStyle.PlayStation || (style == GamepadIconStyle.Automatic &&
                (device == PromptDevice.DualShock || device == PromptDevice.DualSense));
            bool xbox = style == GamepadIconStyle.Xbox || (style == GamepadIconStyle.Automatic && device == PromptDevice.Xbox);
            switch (control)
            {
                case "buttonSouth": fallback = ps ? "Cross" : xbox ? "A" : "South"; return ps ? "px" : xbox ? "xa" : "fs";
                case "buttonEast": fallback = ps ? "Circle" : xbox ? "B" : "East"; return ps ? "po" : xbox ? "xb" : "fe";
                case "buttonWest": fallback = ps ? "Square" : xbox ? "X" : "West"; return ps ? "ps" : xbox ? "xx" : "fw";
                case "buttonNorth": fallback = ps ? "Triangle" : xbox ? "Y" : "North"; return ps ? "pt" : xbox ? "xy" : "fn";
                case "leftShoulder": fallback = ps ? "L1" : "LB"; return ps ? null : "lb";
                case "rightShoulder": fallback = ps ? "R1" : "RB"; return ps ? null : "rb";
                case "leftTrigger": fallback = ps ? "L2" : "LT"; return ps ? null : "lt";
                case "rightTrigger": fallback = ps ? "R2" : "RT"; return ps ? null : "rt";
                case "leftStick": fallback = "Left stick"; return "jl";
                case "rightStick": fallback = "Right stick"; return "jr";
                case "leftStickPress": fallback = ps ? "L3" : "LS"; return "jlclick";
                case "rightStickPress": fallback = ps ? "R3" : "RS"; return "jrclick";
                case "dpad": fallback = "D-pad"; return "dpad";
                case "start": fallback = ps ? "Options" : "Menu"; return ps ? "pmenu" : null;
                default: return null;
            }
        }

        private static Part Control(string path)
        {
            string name = GlyphName(path, Device, Settings != null ? Settings.GamepadStyle : GamepadIconStyle.Automatic, out string fallback);
            var atlas = Settings != null ? Settings.Atlas : null;
            if (atlas != null && name != null)
            {
                int index = atlas.GetSpriteIndexFromName(name);
                if (index >= 0)
                {
                    var rect = atlas.spriteCharacterTable[index].glyph.glyphRect;
                    var texture = atlas.spriteSheet;
                    if (texture != null) return new Part(fallback, texture,
                        new Rect((float)rect.x / texture.width, (float)rect.y / texture.height,
                            (float)rect.width / texture.width, (float)rect.height / texture.height));
                }
            }
            return new Part("[" + fallback + "]");
        }

        private static IEnumerable<Part> Action(string name)
        {
            // These match DemoMenuInput and the shared EventSystem's menu bindings.
            if (name == "Navigate" || name == "NavigateGrid")
            {
                if (IsGamepad) yield return Control("<Gamepad>/dpad");
                else
                {
                    yield return Control("<Keyboard>/upArrow"); yield return Control("<Keyboard>/downArrow");
                    if (name == "NavigateGrid")
                    { yield return Control("<Keyboard>/leftArrow"); yield return Control("<Keyboard>/rightArrow"); }
                }
                yield break;
            }
            if (name == "Submit") { yield return Control(IsGamepad ? "<Gamepad>/buttonSouth" : "<Keyboard>/enter"); yield break; }
            if (_reader == null) _reader = Object.FindFirstObjectByType<PlayerInputReader>();
            var action = _reader != null ? _reader.FindGameplayAction(name) : null;
            if (action == null) { _fallback ??= new ProjectFirstRunInputActions(); action = _fallback.asset.FindAction("Gameplay/" + name); }
            bool found = false;
            if (action != null) foreach (var binding in action.bindings)
            {
                string path = binding.effectivePath;
                if (binding.isComposite || string.IsNullOrEmpty(path)) continue;
                bool pad = path.StartsWith("<Gamepad>") || path.StartsWith("<DualShock") || path.StartsWith("<XInput");
                if (pad != IsGamepad) continue;
                yield return Control(path); found = true;
                if (!binding.isPartOfComposite) break;
            }
            if (!found) yield return new Part("[" + name + "]");
        }

        public static List<Part> Parse(string template)
        {
            template ??= string.Empty;
            int style = Settings != null ? (int)Settings.GamepadStyle : 0;
            if (_cachedRevision != Revision || _cachedStyle != style)
            {
                Parsed.Clear(); _cachedRevision = Revision; _cachedStyle = style;
            }
            if (Parsed.TryGetValue(template, out var cached)) return cached;
            var parts = new List<Part>();
            foreach (Match match in Tokens.Matches(template))
                if (match.Groups[1].Success) parts.AddRange(Action(match.Groups[1].Value));
                else parts.Add(new Part(match.Value));
            if (Parsed.Count >= 64) Parsed.Clear();
            Parsed[template] = parts;
            return parts;
        }

        public static void RefreshBindings() => Revision++;

        public static float IconWidth(Part part, float height) => height * part.UV.width * part.Texture.width /
            (part.UV.height * part.Texture.height);

        public static void Draw(Rect rect, string template, bool box = false)
        {
            if (box) GUI.Box(rect, GUIContent.none);
            var parts = Parse(template);
            var style = GUI.skin.label;
            float height = Mathf.Min(24, rect.height - 4), total = 0;
            foreach (var part in parts) total += part.IsIcon ? IconWidth(part, height) + 3 : style.CalcSize(new GUIContent(part.Text)).x;
            // Fit long prompts rather than drawing beyond the panel on smaller windows.
            float scale = Mathf.Min(1, (rect.width - 12) / Mathf.Max(1, total));
            GUI.BeginGroup(rect);
            var scaledStyle = new GUIStyle(style) { fontSize = Mathf.Max(8, Mathf.RoundToInt((style.fontSize > 0 ? style.fontSize : 12) * scale)), alignment = TextAnchor.MiddleLeft };
            float x = Mathf.Max(6, (rect.width - total * scale) / 2), y = (rect.height - height * scale) / 2;
            foreach (var part in parts)
            {
                float width = part.IsIcon ? IconWidth(part, height) : style.CalcSize(new GUIContent(part.Text)).x;
                var target = new Rect(x, y, width * scale, height * scale);
                if (part.IsIcon) GUI.DrawTextureWithTexCoords(target, part.Texture, part.UV, true);
                else GUI.Label(target, part.Text, scaledStyle);
                x += (width + (part.IsIcon ? 3 : 0)) * scale;
            }
            GUI.EndGroup();
        }
    }
}
