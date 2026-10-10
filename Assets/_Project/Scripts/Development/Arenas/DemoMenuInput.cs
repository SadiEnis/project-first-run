using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectFirstRun.Development.Arenas
{
    /// <summary>Shared two-option menu input, independent of gameplay action maps and time scale.</summary>
    public sealed class DemoMenuInput
    {
        private bool _visible, _armed, _confirmHeld, _axisHeld;
        public int Selected { get; private set; }
        public bool IsArmed => _visible && _armed;

        // Returns the submitted option, or -1. One move per stick deflection / button press.
        public int Tick(bool visible, float vertical, bool confirm, bool pointerHeld = false)
        {
            if (!visible)
            {
                _visible = _armed = _confirmHeld = _axisHeld = false;
                Selected = 0;
                return -1;
            }
            if (!_visible)
            {
                _visible = true; _armed = false; Selected = 0;
                return -1; // Never accept the action that opened the menu.
            }
            if (!_armed)
            {
                if (Mathf.Abs(vertical) < .25f && !confirm && !pointerHeld)
                {
                    _armed = true; _confirmHeld = _axisHeld = false;
                }
                return -1;
            }
            if (Mathf.Abs(vertical) < .25f) _axisHeld = false;
            if (Mathf.Abs(vertical) >= .6f && !_axisHeld)
            {
                Selected = 1 - Selected;
                _axisHeld = true;
            }
            bool submit = confirm && !_confirmHeld;
            _confirmHeld = confirm;
            return submit ? Selected : -1;
        }

        public int Poll(bool visible)
        {
            var pad = Gamepad.current;
            var keyboard = Keyboard.current;
            float vertical = pad != null ? pad.leftStick.y.ReadValue() : 0;
            if (pad != null && pad.dpad.up.isPressed) vertical = 1;
            if (pad != null && pad.dpad.down.isPressed) vertical = -1;
            if (keyboard != null && keyboard.upArrowKey.isPressed) vertical = 1;
            if (keyboard != null && keyboard.downArrowKey.isPressed) vertical = -1;
            bool confirm = (pad != null && pad.buttonSouth.isPressed) ||
                (keyboard != null && (keyboard.enterKey.isPressed || keyboard.numpadEnterKey.isPressed || keyboard.spaceKey.isPressed));
            return Tick(visible, vertical, confirm, Mouse.current != null && Mouse.current.leftButton.isPressed);
        }

        public bool Button(Rect rect, int index, string label)
        {
            // Keyboard is handled in Update; prevent IMGUI focus from submitting a different button.
            if (Event.current.isKey) Event.current.Use();
            if (IsArmed && Event.current.type == EventType.MouseMove && rect.Contains(Event.current.mousePosition))
                Selected = index;
            bool enabled = GUI.enabled;
            Color background = GUI.backgroundColor;
            GUI.enabled = enabled && IsArmed;
            if (Selected == index) GUI.backgroundColor = new Color(1f, .8f, .3f);
            bool clicked = GUI.Button(rect, (Selected == index ? "> " : "   ") + label);
            GUI.enabled = enabled;
            GUI.backgroundColor = background;
            if (clicked) Selected = index;
            return clicked;
        }

        public const string Hint = "D-pad / Left stick / Up-Down: select    A / Cross / Enter: confirm";
    }
}
