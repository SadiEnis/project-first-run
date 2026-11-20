using System.Globalization;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Flow;
using ProjectFirstRun.Progression;
using UnityEngine;
namespace ProjectFirstRun.UI.Hud
{
    public sealed class RunHubView : MonoBehaviour
    {
        [SerializeField] private RunHubController _hub;
        private readonly DemoMenuInput _menu = new();
        private readonly int[] _displayedLevels = new int[3];
        private void Update()
        {
            int choice = _menu.Poll(_hub != null && !_hub.IsLoading, 5);
            Activate(choice);
        }
        private void Activate(int choice)
        {
            if (_hub == null) return;
            if (choice == 0) { if (_hub.IsReady) _hub.StartRun(); else _hub.RefreshBalance(); }
            else if (choice == 1) _hub.PurchaseAmmo(_displayedLevels[0]);
            else if (choice == 2) _hub.PurchaseAbilitySlot(_displayedLevels[1]);
            else if (choice == 3) _hub.PurchaseUpgradeSlot(_displayedLevels[2]);
            else if (choice == 4) _hub.Quit();
        }
        private void OnDisable() => _menu.Tick(false, 0, false);
        private void OnGUI()
        {
            if (_hub == null) return;
            // Keep every offer and the controller selection visible on smaller windows.
            Matrix4x4 matrix = GUI.matrix;
            float scale = Mathf.Min(1, Mathf.Min(Screen.width / 640f, Screen.height / 700f));
            GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
            float width = 600;
            var panel = new Rect((Screen.width / scale - width) / 2, (Screen.height / scale - 680) / 2, width, 680);
            GUI.Box(panel, "PROJECT FIRST RUN — HUB");
            GUI.BeginGroup(panel);
            var label = new GUIStyle(GUI.skin.label) { wordWrap = true, alignment = TextAnchor.MiddleCenter };
            string balance = _hub.PermanentBalance.HasValue ?
                _hub.PermanentBalance.Value.ToString("N0", CultureInfo.InvariantCulture) + " GOLD" : "Unavailable";
            GUI.Label(new Rect(20, 30, width - 40, 35), "Permanent balance: " + balance,
                new GUIStyle(label) { fontSize = 20 });
            GUI.Label(new Rect(20, 68, width - 40, 38), _hub.IsLoading ? "Starting a new run..." :
                _hub.LastError != null ? _hub.LastError :
                "Purchases apply to your next run.", label);
            bool enabled = GUI.enabled;
            GUI.enabled = enabled && !_hub.IsLoading;
            if (_menu.Button(new Rect(40, 110, width - 80, 38), 0, _hub.IsReady ? "Start Run" : "Retry reading save")) Activate(0);
            for (int index = 0; index < 3; index++)
            {
                bool ready = _hub.IsReady;
                int level = index == 0 ? _hub.AmmoLevel : index == 1 ? _hub.AbilitySlotLevel : _hub.UpgradeSlotLevel;
                _displayedLevels[index] = level;
                string title = "Upgrade unavailable";
                bool max = false;
                if (ready)
                {
                    if (index == 0)
                    {
                        var def = _hub.AmmoUpgrade;
                        max = level == def.MaxLevel;
                        title = def.DisplayName + " — Level " + level + "/" + def.MaxLevel +
                            "\nCurrent: +" + def.GetBonusMagazines(level) + " reserve magazines" +
                            (max ? "" : " → +" + def.GetBonusMagazines(level + 1) + " | " + def.GetNextCost(level) + " GOLD");
                    }
                    else
                    {
                        SlotCapacityUpgradeDefinition def = index == 1 ? _hub.AbilitySlotUpgrade : _hub.UpgradeSlotUpgrade;
                        max = level == def.MaxLevel;
                        title = def.DisplayName + " — " + (index == 1 ? "Ability slots" : "Upgrade slots") +
                            "\nCurrent: " + def.GetCapacity(level) + (max ? "" : " → " + def.GetCapacity(level + 1) +
                            " | " + def.GetNextCost(level) + " GOLD");
                    }
                }
                float y = 165 + index * 125;
                GUI.enabled = enabled;
                GUI.Label(new Rect(24, y, width - 48, 60), title, new GUIStyle(label) { fontSize = 16 });
                GUI.enabled = enabled && ready && !_hub.IsLoading && !max;
                if (_menu.Button(new Rect(40, y + 65, width - 80, 36), index + 1, max ? "Maximum level" : "Buy upgrade"))
                    Activate(index + 1);
            }
            GUI.enabled = enabled;
            GUI.Label(new Rect(24, 540, width - 48, 42), _hub.PurchaseMessage ?? "", label);
            GUI.enabled = enabled && !_hub.IsLoading;
            if (_menu.Button(new Rect(40, 590, width - 80, 38), 4, "Quit")) Activate(4);
            GUI.enabled = enabled;
            InputPrompts.Draw(new Rect(12, 643, width - 24, 28), DemoMenuInput.Hint);
            GUI.EndGroup();
            GUI.matrix = matrix;
        }
    }
}
