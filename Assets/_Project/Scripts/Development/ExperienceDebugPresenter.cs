#if UNITY_EDITOR
using ProjectFirstRun.Progression;
using UnityEngine;

namespace ProjectFirstRun.Development
{
    [DisallowMultipleComponent]
    public sealed class ExperienceDebugPresenter : MonoBehaviour
    {
        [SerializeField] private PlayerExperienceController _playerExperience;

        private void OnGUI()
        {
            if (_playerExperience == null || !_playerExperience.IsInitialized)
                return;

            float x = (Screen.width - 300f) * 0.5f;
            float y = Screen.height - 100f;
            GUI.Box(new Rect(x, y, 300f, 85f), "Run Experience (Dev)");
            GUI.Label(new Rect(x + 15f, y + 22f, 270f, 22f),
                $"Level {_playerExperience.Level}  |  XP {_playerExperience.CurrentExperience} / {_playerExperience.RequiredExperience}");
            GUI.Label(new Rect(x + 15f, y + 43f, 270f, 22f),
                $"Total XP: {_playerExperience.TotalExperience}");

            float progress = (float)((double)_playerExperience.CurrentExperience /
                                     _playerExperience.RequiredExperience);
            Color previousColor = GUI.color;
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(x + 15f, y + 68f, 270f, 7f), Texture2D.whiteTexture);
            GUI.color = Color.cyan;
            GUI.DrawTexture(new Rect(x + 15f, y + 68f, 270f * progress, 7f), Texture2D.whiteTexture);
            GUI.color = previousColor;
        }
    }
}
#endif
