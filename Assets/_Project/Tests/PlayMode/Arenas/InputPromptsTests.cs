using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.UI.Hud;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ProjectFirstRun.Tests.PlayMode.Arenas
{
    public sealed class InputPromptsTests : InputTestFixture
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            typeof(InputPrompts).GetMethod("Reset", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        }

        [TestCase("buttonSouth", PromptDevice.Xbox, "xa")]
        [TestCase("buttonWest", PromptDevice.Xbox, "xx")]
        [TestCase("buttonNorth", PromptDevice.Xbox, "xy")]
        [TestCase("buttonWest", PromptDevice.DualShock, "ps")]
        [TestCase("buttonSouth", PromptDevice.DualSense, "px")]
        [TestCase("buttonSouth", PromptDevice.GenericGamepad, "fs")]
        public void FaceButtonsUseCorrectFamily(string control, PromptDevice family, string glyph)
        {
            Assert.That(InputPrompts.GlyphName("<Gamepad>/" + control, family, GamepadIconStyle.Automatic, out _), Is.EqualTo(glyph));
        }

        [Test]
        public void PlayStationMissingShoulderArtworkUsesCorrectTextAndOverrideWorks()
        {
            Assert.That(InputPrompts.GlyphName("<Gamepad>/rightTrigger", PromptDevice.DualSense, GamepadIconStyle.Automatic, out var text), Is.Null);
            Assert.That(text, Is.EqualTo("R2"));
            Assert.That(InputPrompts.GlyphName("<Gamepad>/buttonWest", PromptDevice.Xbox, GamepadIconStyle.PlayStation, out _), Is.EqualTo("ps"));
            Assert.That(InputPrompts.GlyphName("<Keyboard>/r", PromptDevice.KeyboardMouse, GamepadIconStyle.PlayStation, out _), Is.EqualTo("r"));
        }

        [Test]
        public void PhysicalLayoutsAreDistinguished()
        {
            Assert.That(InputPrompts.Classify(InputSystem.AddDevice<DualSenseGamepadHID>()), Is.EqualTo(PromptDevice.DualSense));
            Assert.That(InputPrompts.Classify(InputSystem.AddDevice<DualShock4GamepadHID>()), Is.EqualTo(PromptDevice.DualShock));
            Assert.That(InputPrompts.Classify(InputSystem.AddDevice<XInputController>()), Is.EqualTo(PromptDevice.Xbox));
        }

        [Test]
        public void MeaningfulInputSwitchesFamilyWhilePausedAndDisconnectFallsBack()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var pad = InputSystem.AddDevice<XInputController>();
            float previousScale = Time.timeScale;
            try
            {
                Time.timeScale = 0;
                Set(pad.leftStick, new Vector2(.1f, 0));
                Assert.That(InputPrompts.Device, Is.EqualTo(PromptDevice.KeyboardMouse), "Drift must not steal prompts.");
                Press(pad.buttonWest);
                Assert.That(InputPrompts.Device, Is.EqualTo(PromptDevice.Xbox));
                Release(pad.buttonWest);
                Press(keyboard.rKey);
                Assert.That(InputPrompts.Device, Is.EqualTo(PromptDevice.KeyboardMouse));
                Release(keyboard.rKey);
                Press(pad.buttonNorth);
                InputSystem.RemoveDevice(pad);
                Assert.That(InputPrompts.Device, Is.EqualTo(PromptDevice.KeyboardMouse));
            }
            finally { Time.timeScale = previousScale; }
        }

        [Test]
        public void SavedAtlasResolvesKeyboardMouseAndXboxIconsWithoutFallback()
        {
            Assert.That(InputPrompts.Settings, Is.Not.Null);
            var parts = InputPrompts.Parse("{Move} {Look} {Fire} {Reload} {Interact} {SwitchWeapon} {Jump}");
            Assert.That(parts.Count(p => p.IsIcon), Is.EqualTo(10));
            var pad = InputSystem.AddDevice<XInputController>();
            Press(pad.buttonSouth);
            parts = InputPrompts.Parse("{Move} {Look} {Fire} {Reload} {Interact} {SwitchWeapon} {Jump}");
            Assert.That(parts.Count(p => p.IsIcon), Is.EqualTo(7));
            Assert.That(parts.Where(p => p.IsIcon).All(p => p.UV.width > 0 && p.UV.height > 0), Is.True);
        }

        [UnityTest]
        public IEnumerator CanvasAdapterCreatesNonBlockingIconsAndReusesRuns()
        {
            var canvas = new GameObject("Prompt test canvas", typeof(Canvas));
            var label = new GameObject("Prompt test", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            try
            {
                canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                label.transform.SetParent(canvas.transform, false);
                var source = label.GetComponent<Text>();
                source.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                source.fontSize = 20; source.text = "{Reload} Reload\n{Fire} Fire";
                source.rectTransform.sizeDelta = new Vector2(380, 64);
                label.AddComponent<InputPromptLabel>();
                yield return null;
                Assert.That(source.enabled, Is.False);
                Assert.That(label.GetComponentsInChildren<RawImage>().Count(i => i.enabled && i.texture != null), Is.EqualTo(2));
                Assert.That(label.GetComponentsInChildren<Graphic>().Where(g => g.enabled).All(g => !g.raycastTarget), Is.True);
                int children = label.transform.childCount;
                yield return null;
                Assert.That(label.transform.childCount, Is.EqualTo(children));
            }
            finally { Object.DestroyImmediate(canvas); }
        }
    }
}
