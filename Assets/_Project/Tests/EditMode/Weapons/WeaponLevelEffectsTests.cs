using System;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Items;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Weapons
{
    public sealed class WeaponLevelEffectsTests
    {
        [Test]
        public void ConfigurationChange_PreservesAmmoAndRunningTimers()
        {
            var state = new WeaponRuntimeState(new WeaponRuntimeConfig(6, 30, 2f, 2f));
            state.TryFire();
            state.TryStartReload();
            state.Tick(0.25f);
            state.ApplyConfiguration(new WeaponRuntimeConfig(10, 999, 4f, 1f));
            Assert.That(state.MagazineAmmo, Is.EqualTo(5));
            Assert.That(state.ReserveAmmo, Is.EqualTo(30));
            Assert.That(state.FireCooldownRemaining, Is.EqualTo(0.25f));
            Assert.That(state.ReloadTimeRemaining, Is.EqualTo(1.75f));
            Assert.That(state.IsReloading, Is.True);
            state.Tick(1.75f);
            Assert.That(state.MagazineAmmo, Is.EqualTo(10));
            Assert.That(state.ReserveAmmo, Is.EqualTo(25));
            state.TryFire();
            Assert.That(state.FireCooldownRemaining, Is.EqualTo(0.25f));
            state.TryStartReload();
            Assert.That(state.ReloadTimeRemaining, Is.EqualTo(1f));
        }

        [Test]
        public void ConfigurationChange_ReadyWeaponDoesNotGainAmmoOrCooldown()
        {
            var state = new WeaponRuntimeState(new WeaponRuntimeConfig(6, 0, 2f, 2f));
            state.ApplyConfiguration(new WeaponRuntimeConfig(10, 999, 4f, 1f));
            Assert.That(state.MagazineAmmo, Is.EqualTo(6));
            Assert.That(state.ReserveAmmo, Is.Zero);
            Assert.That(state.FireCooldownRemaining, Is.Zero);
            Assert.That(state.IsReloading, Is.False);
        }

        [Test]
        public void InvalidConfiguration_DoesNotMutateRuntime()
        {
            var state = new WeaponRuntimeState(new WeaponRuntimeConfig(6, 30, 2f, 2f));
            state.TryFire();
            Assert.Throws<ArgumentOutOfRangeException>(() => state.ApplyConfiguration(default));
            Assert.Throws<InvalidOperationException>(() => state.ApplyConfiguration(new WeaponRuntimeConfig(5, 30, 2f, 2f)));
            Assert.That(state.MagazineCapacity, Is.EqualTo(6));
            Assert.That(state.MagazineAmmo, Is.EqualTo(5));
            Assert.That(state.FireCooldownRemaining, Is.EqualTo(0.5f));
        }

        [TestCase("WD_PlasmaRifle", 25f)]
        [TestCase("WD_DevelopmentSecondaryWeapon", 40f)]
        public void SampleContent_HasEightValidLevelsAndUnchangedStartingDamage(string name, float startingDamage)
        {
            var definition = AssetDatabase.LoadAssetAtPath<WeaponDefinition>($"Assets/_Project/Data/Items/Weapons/{name}.asset");
            var entry = new PlayerWeaponRuntimeEntry(definition);
            Assert.That(entry.MaximumLevel, Is.EqualTo(8));
            Assert.That(entry.Level, Is.EqualTo(1));
            Assert.That(entry.BaseDamage, Is.EqualTo(startingDamage));
            Assert.That(entry.RuntimeState.MagazineCapacity, Is.EqualTo(definition.MagazineCapacity));
        }

        [TestCase(0)] // missing list entries
        [TestCase(1)] // null entry
        [TestCase(2)] // nonfinite damage
        [TestCase(3)] // shrinking capacity
        [TestCase(4)] // invalid rate
        [TestCase(5)] // null list
        public void MalformedProgression_IsRejectedBeforeRuntimeCreation(int scenario)
        {
            var definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            try
            {
                Set(definition, typeof(ItemDefinition), "_maximumLevel", 2);
                WeaponLevelData[] levels = scenario switch
                {
                    0 => Array.Empty<WeaponLevelData>(),
                    1 => new WeaponLevelData[] { null },
                    2 => new[] { new WeaponLevelData(float.NaN, 12, 5f, 1.5f) },
                    3 => new[] { new WeaponLevelData(30f, 11, 5f, 1.5f) },
                    4 => new[] { new WeaponLevelData(30f, 12, 0f, 1.5f) },
                    _ => null
                };
                Set(definition, typeof(WeaponDefinition), "_additionalLevels", levels);
                Assert.That(() => new PlayerWeaponRuntimeEntry(definition), Throws.Exception);
            }
            finally { Object.DestroyImmediate(definition); }
        }

        private static void Set(object target, Type type, string field, object value) =>
            type.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
