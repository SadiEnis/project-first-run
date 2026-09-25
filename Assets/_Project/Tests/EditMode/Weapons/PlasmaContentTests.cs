using System;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Weapons
{
    public sealed class PlasmaContentTests
    {
        [TestCase(1, 25, 5, 12, 0, false)]
        [TestCase(2, 30, 5, 12, 0, false)]
        [TestCase(3, 30, 6, 12, 0, false)]
        [TestCase(4, 30, 6, 12, 1, false)]
        [TestCase(5, 35, 6, 12, 1, false)]
        [TestCase(6, 35, 6, 20, 1, false)]
        [TestCase(7, 35, 6, 20, 2, false)]
        [TestCase(8, 35, 6, 20, 2, true)]
        public void LevelsMatchGdd(int level, float damage, float rate, int capacity, int pierce, bool burn)
        {
            var entry = new PlayerWeaponRuntimeEntry(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(
                "Assets/_Project/Data/Items/Weapons/WD_PlasmaRifle.asset"));
            entry.RuntimeState.TryFire(); entry.RuntimeState.TryStartReload();
            for (int i = 1; i < level; i++) typeof(PlayerWeaponRuntimeEntry)
                .GetMethod("AdvanceLevel", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(entry, null);
            Assert.That(entry.DeliveryMode, Is.EqualTo(WeaponDeliveryMode.Plasma));
            Assert.That(entry.Definition.StableId, Is.EqualTo("weapon.plasma-rifle"));
            Assert.That(entry.BaseDamage, Is.EqualTo(damage)); Assert.That(entry.ShotsPerSecond, Is.EqualTo(rate));
            Assert.That(entry.RuntimeState.MagazineCapacity, Is.EqualTo(capacity));
            Assert.That(entry.RuntimeState.MagazineAmmo, Is.EqualTo(11));
            Assert.That(entry.RuntimeState.ReloadTimeRemaining, Is.EqualTo(1.5f));
            Assert.That(entry.RuntimeState.ReserveAmmo, Is.EqualTo(48));
            Assert.That(entry.Plasma.Value.PierceCount, Is.EqualTo(pierce));
            Assert.That(entry.Plasma.Value.Burn, Is.EqualTo(burn));
            Assert.That(entry.Plasma.Value.Speed, Is.EqualTo(60));
            Assert.That(entry.Fire.RecoilDegrees, Is.EqualTo(.3f));
            Assert.DoesNotThrow(() => entry.PlasmaPrefab.ValidatePrefab());
        }
        [TestCase(30)] [TestCase(60)] [TestCase(144)]
        public void BurnExpiresAfterSixTicksAtDifferentFrameRates(int fps)
        {
            var state = new PlasmaBurnState(); state.Refresh(5); int ticks = 0;
            for (int i = 0; i < fps * 4; i++) ticks += state.Advance(1f / fps);
            Assert.That(ticks, Is.EqualTo(6)); Assert.That(state.Remaining, Is.Zero);
        }
        [Test]
        public void BurnRefreshPreservesPhaseAndBoundsStallCatchup()
        {
            var state = new PlasmaBurnState(); state.Refresh(5);
            Assert.That(state.Advance(.4f), Is.Zero);
            state.Refresh(10);
            Assert.That(state.Advance(.1f), Is.EqualTo(1)); Assert.That(state.Damage, Is.EqualTo(10));
            Assert.That(state.Advance(0), Is.Zero);
            Assert.That(state.Advance(100), Is.EqualTo(5));
            Assert.That(state.Advance(100), Is.Zero);
        }
        [TestCase(-1)] [TestCase(3)]
        public void InvalidPierceRejected(int count) => Assert.Throws<ArgumentOutOfRangeException>(() => new PlasmaConfig(60, pierceCount: count));
        [TestCase(0)] [TestCase(float.NaN)] [TestCase(float.PositiveInfinity)]
        public void InvalidSpeedRejected(float speed) => Assert.Throws<ArgumentOutOfRangeException>(() => new PlasmaConfig(speed));
        [TestCase("_plasmaPrefab")] [TestCase("_triggerMode")] [TestCase("_criticalChance")]
        public void IncompatibleContentRejectedBeforeRuntimeCreation(string field)
        {
            var copy = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(
                "Assets/_Project/Data/Items/Weapons/WD_PlasmaRifle.asset"));
            try
            {
                var data = new SerializedObject(copy);
                if (field == "_plasmaPrefab") data.FindProperty(field).objectReferenceValue = null;
                else if (field == "_triggerMode") data.FindProperty(field).intValue = 0;
                else data.FindProperty(field).floatValue = .1f;
                data.ApplyModifiedPropertiesWithoutUndo();
                Assert.Throws<InvalidOperationException>(() => new PlayerWeaponRuntimeEntry(copy));
            }
            finally { UnityEngine.Object.DestroyImmediate(copy); }
        }
        [Test]
        public void AcquiredProfilesAreImmutableAndMalformedFutureLevelRejectsNewEntry()
        {
            var copy = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(
                "Assets/_Project/Data/Items/Weapons/WD_PlasmaRifle.asset"));
            try
            {
                var entry = new PlayerWeaponRuntimeEntry(copy);
                var data = new SerializedObject(copy);
                data.FindProperty("_additionalLevels").GetArrayElementAtIndex(6).FindPropertyRelative("_plasma")
                    .FindPropertyRelative("_pierceCount").intValue = 99;
                data.ApplyModifiedPropertiesWithoutUndo();
                for (int i = 1; i < 8; i++) typeof(PlayerWeaponRuntimeEntry)
                    .GetMethod("AdvanceLevel", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(entry, null);
                Assert.That(entry.Plasma.Value.PierceCount, Is.EqualTo(2));
                Assert.That(entry.Plasma.Value.Burn, Is.True);
                Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerWeaponRuntimeEntry(copy));
            }
            finally { UnityEngine.Object.DestroyImmediate(copy); }
        }
    }
}
