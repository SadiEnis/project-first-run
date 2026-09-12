using System;
using NUnit.Framework;
using ProjectFirstRun.Progression;
using UnityEditor;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Progression
{
    public sealed class ExperienceDefinitionTests
    {
        [Test]
        public void RuntimeCurve_IsIndependentFromLaterAssetEdits()
        {
            var definition = ScriptableObject.CreateInstance<ExperienceDefinition>();
            try
            {
                var original = definition.CreateRuntimeCurve();
                var serialized = new SerializedObject(definition);
                serialized.FindProperty("_firstLevelCost").intValue = 200;
                serialized.FindProperty("_costIncreasePerLevel").intValue = 75;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                var updated = definition.CreateRuntimeCurve();
                Assert.That(original.GetRequiredExperience(2), Is.EqualTo(150));
                Assert.That(updated.GetRequiredExperience(1), Is.EqualTo(200));
                Assert.That(updated.GetRequiredExperience(2), Is.EqualTo(275));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }

        [TestCase("_firstLevelCost")]
        [TestCase("_costIncreasePerLevel")]
        public void InvalidAssetValues_FailAtRuntimeBoundary(string field)
        {
            var definition = ScriptableObject.CreateInstance<ExperienceDefinition>();
            try
            {
                var serialized = new SerializedObject(definition);
                serialized.FindProperty(field).intValue = 0;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => definition.CreateRuntimeCurve());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(definition);
            }
        }
    }
}
