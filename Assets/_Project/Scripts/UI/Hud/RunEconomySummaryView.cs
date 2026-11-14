using System.Globalization;
using ProjectFirstRun.Progression;
using UnityEngine;

namespace ProjectFirstRun.UI.Hud
{
    /// <summary>Read-only presentation shared by the existing terminal menus. Never settles a run.</summary>
    public static class RunEconomySummaryView
    {
        public static string Describe(RunEconomyResult result, bool isSettled, long? permanentBalance)
        {
            if (result == null) return "Preparing run summary...";
            bool confirmed = isSettled && permanentBalance.HasValue;
            string transfer = confirmed ? "Transferred" : "Pending transfer";
            string balance = confirmed ? Gold(permanentBalance.Value) : "Unavailable";
            string status = confirmed ? "Saved to your permanent wallet." :
                "Transfer not confirmed. Choose an option below to retry saving.";
            return $"Collected: {Gold(result.Collected)}\n" +
                $"Lost: {Gold(result.Lost)}\n" +
                $"Boss reward: {Gold(result.BossReward)}\n" +
                $"{transfer}: {Gold(result.Transferred)}\n" +
                $"Permanent balance: {balance}\n\n{status}";
        }

        private static string Gold(long amount) => amount.ToString("N0", CultureInfo.InvariantCulture) + " GOLD";

        public static void Draw(Rect area, RunEconomyController economy)
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true, alignment = TextAnchor.UpperLeft };
            GUI.Label(area, Describe(economy.Result, economy.IsSettled, economy.PermanentBalance), style);
        }
    }
}
