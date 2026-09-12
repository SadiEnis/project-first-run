using System;
using ProjectFirstRun.Stats;
using UnityEngine;

namespace ProjectFirstRun.Development.Stats
{
    [DisallowMultipleComponent]
    public sealed class PlayerStatsDebugPresenter :
        MonoBehaviour
    {
        [Header("Source")]
        [SerializeField]
        private PlayerStatsController _statsController;

        [Header("Debug Panel")]
        [SerializeField]
        private Rect _panelRect =
            new Rect(
                1375f,
                150f,
                270f,
                220f);

        private void OnGUI()
        {
            GUI.Box(
                _panelRect,
                "Player Stats Debug");

            Rect contentRect =
                new Rect(
                    _panelRect.x + 12f,
                    _panelRect.y + 28f,
                    _panelRect.width - 24f,
                    _panelRect.height - 36f);

            GUILayout.BeginArea(
                contentRect);

            DrawContent();

            GUILayout.EndArea();
        }

        private void DrawContent()
        {
            if (_statsController == null)
            {
                GUILayout.Label(
                    "PlayerStatsController: Missing");

                return;
            }

            if (!_statsController.IsInitialized)
            {
                GUILayout.Label(
                    "Stats: Not Initialized");

                return;
            }

            PlayerStatCollection stats =
                _statsController.Stats;

            GUILayout.Label(
                $"Active Modifiers: {stats.ModifierCount}");

            bool hasAnyStat =
                false;

            Array statTypes =
                Enum.GetValues(
                    typeof(PlayerStatType));

            foreach (PlayerStatType statType
                     in statTypes)
            {
                if (!TryGetModifierSummary(
                        stats,
                        statType,
                        out string summary))
                {
                    continue;
                }

                hasAnyStat = true;

                GUILayout.Label(
                    $"{GetDisplayName(statType)}: {summary}");
            }

            if (!hasAnyStat)
            {
                GUILayout.Label(
                    "No active stat modifiers");
            }
        }

        private static bool TryGetModifierSummary(
            PlayerStatCollection stats,
            PlayerStatType statType,
            out string summary)
        {
            float flatTotal =
                0f;

            float additivePercent =
                0f;

            float multiplicativeFactor =
                1f;

            bool found =
                false;

            for (int index = 0;
                 index < stats.Modifiers.Count;
                 index++)
            {
                StatModifier modifier =
                    stats.Modifiers[index];

                if (modifier.StatType !=
                    statType)
                {
                    continue;
                }

                found = true;

                switch (modifier.Operation)
                {
                    case StatModifierOperation.Flat:
                        flatTotal +=
                            modifier.Value;
                        break;

                    case StatModifierOperation.AdditivePercent:
                        additivePercent +=
                            modifier.Value;
                        break;

                    case StatModifierOperation.MultiplicativePercent:
                        multiplicativeFactor *=
                            1f + modifier.Value;
                        break;

                    default:
                        throw new InvalidOperationException(
                            $"Unsupported stat modifier operation: " +
                            $"{modifier.Operation}.");
                }
            }

            if (!found)
            {
                summary = null;
                return false;
            }

            summary =
                BuildSummary(
                    flatTotal,
                    additivePercent,
                    multiplicativeFactor);

            return true;
        }

        private static string BuildSummary(
            float flat,
            float additivePercent,
            float multiplicativeFactor)
        {
            string result =
                string.Empty;

            if (!Mathf.Approximately(
                    flat,
                    0f))
            {
                result +=
                    $"{FormatSigned(flat)} flat";
            }

            if (!Mathf.Approximately(
                    additivePercent,
                    0f))
            {
                AppendSeparator(
                    ref result);

                result +=
                    $"{FormatSigned(additivePercent * 100f)}%";
            }

            if (!Mathf.Approximately(
                    multiplicativeFactor,
                    1f))
            {
                AppendSeparator(
                    ref result);

                result +=
                    $"x{multiplicativeFactor:0.##}";
            }

            if (string.IsNullOrEmpty(result))
            {
                result =
                    "No effective change";
            }

            return result;
        }

        private static void AppendSeparator(
            ref string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value += " | ";
            }
        }

        private static string FormatSigned(
            float value)
        {
            return value >= 0f
                ? $"+{value:0.##}"
                : $"{value:0.##}";
        }

        private static string GetDisplayName(
            PlayerStatType statType)
        {
            return statType switch
            {
                PlayerStatType.WeaponDamage =>
                    "Weapon Damage",

                PlayerStatType.AbilityDamage =>
                    "Ability Damage",

                PlayerStatType.MoveSpeed =>
                    "Move Speed",

                PlayerStatType.WeaponFireRate =>
                    "Weapon Fire Rate",

                PlayerStatType.ReloadSpeed =>
                    "Reload Speed",

                PlayerStatType.AbilityCooldown =>
                    "Ability Cooldown",

                PlayerStatType.MaxHealth =>
                    "Max Health",

                PlayerStatType.Luck =>
                    "Luck",

                _ =>
                    statType.ToString()
            };
        }
    }
}