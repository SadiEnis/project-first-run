using TMPro;
using UnityEngine;

namespace ProjectFirstRun.UI.Hud
{
    public enum PromptDevice { KeyboardMouse, Xbox, DualShock, DualSense, GenericGamepad }
    public enum GamepadIconStyle { Automatic, Xbox, PlayStation }

    [CreateAssetMenu(menuName = "Project First Run/UI/Input prompt settings")]
    public sealed class InputPromptSettings : ScriptableObject
    {
        public TMP_SpriteAsset Atlas;
        [Tooltip("Only overrides gamepad artwork, not keyboard/mouse detection. Useful for Steam Input emulation.")]
        public GamepadIconStyle GamepadStyle;
    }
}
