using System.Globalization;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Flow;
using UnityEngine;
namespace ProjectFirstRun.UI.Hud
{
    public sealed class RunHubView : MonoBehaviour
    {
        [SerializeField] private RunHubController _hub;
        private readonly DemoMenuInput _menu = new();
        private int _displayedLevel;
        private void Update()
        {
            int choice = _menu.Poll(_hub != null && !_hub.IsLoading, 3);
            if (choice == 0) ActivatePrimary();
            else if (choice == 1) _hub.PurchaseAmmo(_displayedLevel);
            else if (choice == 2) _hub.Quit();
        }
        private void ActivatePrimary()
        {
            if (_hub.IsReady) _hub.StartRun();
            else _hub.RefreshBalance();
        }
        private void OnDisable() => _menu.Tick(false, 0, false);
        private void OnGUI()
        {
            if (_hub == null) return;
            float width = Mathf.Min(560, Screen.width - 32);
            var panel = new Rect((Screen.width - width) / 2, Mathf.Max(8, (Screen.height - 540) / 2), width, 540);
            GUI.Box(panel, "PROJECT FIRST RUN — HUB");
            GUI.BeginGroup(panel);
            var style = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            string balance = _hub.PermanentBalance.HasValue ?
                _hub.PermanentBalance.Value.ToString("N0", CultureInfo.InvariantCulture) + " GOLD" : "Unavailable";
            GUI.Label(new Rect(20, 45, width - 40, 42), "Permanent balance: " + balance, style);
            string status = _hub.IsLoading ? "Starting a new run..." : _hub.LastError != null ?
                (_hub.IsReady ? "Could not start the run. Please try again." : "Could not confirm your save. Retry reading to continue.") :
                "Your earnings are safe here. Prepare for your next run.";
            GUI.Label(new Rect(24, 103, width - 48, 55), status, new GUIStyle(GUI.skin.label) { wordWrap = true, alignment = TextAnchor.MiddleCenter });
            _displayedLevel = _hub.AmmoLevel;
            string upgrade = _hub.IsReady ?
                _hub.AmmoUpgrade.DisplayName + " — Level " + _displayedLevel + "/" + _hub.AmmoUpgrade.MaxLevel +
                "\nCurrent bonus: +" + _hub.AmmoUpgrade.GetBonusMagazines(_displayedLevel) + " reserve magazines" : "Upgrade level unavailable";
            bool atMax = _hub.IsReady && _displayedLevel >= _hub.AmmoUpgrade.MaxLevel;
            if (_hub.IsReady && !atMax) upgrade += "\nNext: +" + _hub.AmmoUpgrade.GetBonusMagazines(_displayedLevel + 1) +
                " total reserve magazines — " + _hub.AmmoUpgrade.GetNextCost(_displayedLevel).ToString("N0", CultureInfo.InvariantCulture) + " GOLD";
            GUI.Label(new Rect(24, 160, width - 48, 100), upgrade,
                new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true, alignment = TextAnchor.MiddleCenter });
            GUI.Label(new Rect(24, 262, width - 48, 46), _hub.PurchaseMessage ?? "Bonus applies to the starting weapon of your next run.",
                new GUIStyle(GUI.skin.label) { wordWrap = true, alignment = TextAnchor.MiddleCenter });
            bool enabled = GUI.enabled;
            GUI.enabled = enabled && !_hub.IsLoading;
            if (_menu.Button(new Rect(40, 320, width - 80, 40), 0, _hub.IsReady ? "Start Run" : "Retry reading save")) ActivatePrimary();
            GUI.enabled = enabled && !_hub.IsLoading && _hub.IsReady && !atMax;
            if (_menu.Button(new Rect(40, 375, width - 80, 40), 1, atMax ? "Maximum level" : "Buy upgrade")) _hub.PurchaseAmmo(_displayedLevel);
            GUI.enabled = enabled && !_hub.IsLoading;
            if (_menu.Button(new Rect(40, 430, width - 80, 40), 2, "Quit")) _hub.Quit();
            GUI.enabled = enabled;
            InputPrompts.Draw(new Rect(12, 492, width - 24, 30), DemoMenuInput.Hint);
            GUI.EndGroup();
        }
    }
}
