#if UNITY_EDITOR
using System.Linq;
using NUnit.Framework;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Presentation;
using ProjectFirstRun.Waves;
using UnityEditor;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Enemies
{
    public sealed class EnemyVarietyContentTests
    {
        [TestCase("ED_Charger", EnemyRank.Normal, 25, 2500)]
        [TestCase("ED_EliteCharger", EnemyRank.Elite, 50, 5000)]
        public void ChargerContent_HasExplicitRankStatsAndIndependentDropProfile(string name, EnemyRank rank, int xp, int chance)
        {
            var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>($"Assets/_Project/Data/Enemies/{name}.asset");
            Assert.That(definition, Is.Not.Null);
            definition.ValidateBehavior(); definition.ChestDropProfile.Validate();
            Assert.That(definition.Behavior, Is.EqualTo(EnemyBehavior.Charger));
            Assert.That(definition.Rank, Is.EqualTo(rank));
            Assert.That(definition.ExperienceReward, Is.EqualTo(xp));
            Assert.That(definition.ChestDropProfile.ChanceBasisPoints, Is.EqualTo(chance));
        }

        [Test]
        public void SecondWave_ContainsChaserNormalChargerAndEliteCharger_WithPresentation()
        {
            var wave = AssetDatabase.LoadAssetAtPath<EnemyWaveDefinition>("Assets/_Project/Data/Waves/Test/EW_Test_02.asset");
            wave.Validate();
            Assert.That(wave.TotalEnemyCount, Is.EqualTo(3));
            Assert.That(wave.Entries.Select(x => x.EnemyDefinition.Behavior), Is.EqualTo(new[] {
                EnemyBehavior.Chaser, EnemyBehavior.Charger, EnemyBehavior.Charger }));
            Assert.That(wave.Entries.Select(x => x.EnemyDefinition.Rank), Is.EqualTo(new[] {
                EnemyRank.Normal, EnemyRank.Normal, EnemyRank.Elite }));
            foreach (var entry in wave.Entries)
                Assert.That(entry.EnemyPrefab.GetComponent<EnemyChargeView>(), Is.Not.Null);
        }
    }
}
#endif
