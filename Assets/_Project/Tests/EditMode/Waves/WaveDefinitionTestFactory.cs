using System;
using System.Collections.Generic;
using System.Reflection;
using ProjectFirstRun.Waves;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Waves
{
    internal static class WaveDefinitionTestFactory
    {
        public static EnemyWaveDefinition CreateWave(
            string stableId,
            string displayName,
            params EnemyWaveEntry[] entries)
        {
            EnemyWaveDefinition wave =
                ScriptableObject.CreateInstance<EnemyWaveDefinition>();

            SetPrivateField(
                wave,
                "_stableId",
                stableId);

            SetPrivateField(
                wave,
                "_displayName",
                displayName);

            SetPrivateField(
                wave,
                "_entries",
                new List<EnemyWaveEntry>(
                    entries ?? Array.Empty<EnemyWaveEntry>()));

            return wave;
        }

        public static ArenaWaveDefinition CreateArena(
            string stableId,
            params EnemyWaveDefinition[] waves)
        {
            ArenaWaveDefinition arena =
                ScriptableObject.CreateInstance<ArenaWaveDefinition>();

            SetPrivateField(
                arena,
                "_stableId",
                stableId);

            SetPrivateField(
                arena,
                "_waves",
                new List<EnemyWaveDefinition>(
                    waves ?? Array.Empty<EnemyWaveDefinition>()));

            return arena;
        }

        public static void SetPrivateField(
            object target,
            string fieldName,
            object value)
        {
            if (target == null)
            {
                throw new ArgumentNullException(
                    nameof(target));
            }

            FieldInfo field =
                target.GetType().GetField(
                    fieldName,
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            if (field == null)
            {
                throw new MissingFieldException(
                    target.GetType().FullName,
                    fieldName);
            }

            field.SetValue(
                target,
                value);
        }
    }
}