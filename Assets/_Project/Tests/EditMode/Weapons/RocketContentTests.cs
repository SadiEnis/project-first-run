using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Weapons
{
    public sealed class RocketContentTests
    {
        private const string Path = "Assets/_Project/Data/Items/Weapons/WD_RocketLauncher.asset";
        [TestCase(1, 80, 25, 3, 3, 3, 0)]
        [TestCase(2, 100, 25, 3, 3, 3, 0)]
        [TestCase(3, 100, 35, 3, 3, 3, 0)]
        [TestCase(4, 100, 35, 4, 3, 3, 0)]
        [TestCase(5, 100, 35, 4, 2.4f, 3, 0)]
        [TestCase(6, 120, 35, 4, 2.4f, 3, 0)]
        [TestCase(7, 120, 35, 4, 2.4f, 4, 0)]
        [TestCase(8, 120, 35, 4, 2.4f, 4, 8)]
        public void AuthoredLevelsMatchDesign(int level, float damage, float speed, int magazine,
            float reload, float radius, int fragments)
        {
            var definition = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(Path);
            Assert.That(definition, Is.Not.Null);
            var entry = new PlayerWeaponRuntimeEntry(definition);
            for (int i = 1; i < level; i++) typeof(PlayerWeaponRuntimeEntry)
                .GetMethod("AdvanceLevel", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(entry, null);
            Assert.That(definition.StableId, Is.EqualTo("weapon.rocket-launcher"));
            Assert.That(entry.DeliveryMode, Is.EqualTo(WeaponDeliveryMode.Rocket));
            Assert.That(entry.TriggerMode, Is.EqualTo(WeaponTriggerMode.SemiAutomatic));
            Assert.That(entry.BaseDamage, Is.EqualTo(damage));
            var config = entry.Rocket.Value;
            Assert.That(config.Speed, Is.EqualTo(speed));
            Assert.That(config.BlastRadius, Is.EqualTo(radius));
            Assert.That(config.FragmentCount, Is.EqualTo(fragments));
            Assert.That(config.FragmentDamage, Is.EqualTo(20));
            Assert.That(config.FragmentRange, Is.EqualTo(8));
            Assert.That(config.FragmentLifetime, Is.EqualTo(.4f));
            Assert.That(config.FragmentSpeed, Is.EqualTo(20));
            Assert.That(config.CollisionRadius, Is.EqualTo(.15f));
            Assert.That(config.Lifetime, Is.EqualTo(4));
            Assert.That(entry.Range, Is.EqualTo(60));
            Assert.That(entry.Fire.RecoilDegrees, Is.EqualTo(5));
            Assert.That(entry.Fire.PreparationDuration, Is.Zero);
            Assert.That(entry.Fire.CriticalChance, Is.Zero);
            Assert.That(entry.Shot.PelletCount, Is.EqualTo(1));
            Assert.That(entry.RuntimeState.MagazineCapacity, Is.EqualTo(magazine));
            Assert.That(entry.RuntimeState.ReserveAmmo, Is.EqualTo(12));
            Assert.That(entry.ShotsPerSecond, Is.EqualTo(.8f));
            entry.RuntimeState.TryFire(); entry.RuntimeState.TryStartReload();
            Assert.That(entry.RuntimeState.ReloadTimeRemaining, Is.EqualTo(reload));
            Assert.DoesNotThrow(() => entry.RocketPrefab.ValidatePrefab(fragments > 0));
        }
        [TestCase(0)] [TestCase(-1)] [TestCase(float.NaN)] [TestCase(float.PositiveInfinity)]
        public void InvalidSpeedRejected(float speed)
            => Assert.Throws<ArgumentOutOfRangeException>(() => new RocketConfig(speed));

        [TestCase("_deliveryMode", 99)]
        [TestCase("_triggerMode", 1)]
        [TestCase("_pelletCount", 2)]
        public void IncompatibleModesRejected(string property, int value)
        {
            var asset = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(Path));
            try
            {
                var data = new SerializedObject(asset); data.FindProperty(property).intValue = value;
                data.ApplyModifiedPropertiesWithoutUndo();
                Assert.Throws<InvalidOperationException>(() => new PlayerWeaponRuntimeEntry(asset));
            }
            finally { UnityEngine.Object.DestroyImmediate(asset); }
        }
        [Test]
        public void RuntimeProfilesAreSnapshotsAndMalformedFinalLevelRejectsNewAcquisition()
        {
            var asset = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(Path));
            try
            {
                var entry = new PlayerWeaponRuntimeEntry(asset);
                var data = new SerializedObject(asset);
                data.FindProperty("_additionalLevels").GetArrayElementAtIndex(6).FindPropertyRelative("_rocket")
                    .FindPropertyRelative("_fragmentCount").intValue = 9;
                data.ApplyModifiedPropertiesWithoutUndo();
                for (int i = 1; i < 8; i++) typeof(PlayerWeaponRuntimeEntry)
                    .GetMethod("AdvanceLevel", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(entry, null);
                Assert.That(entry.Rocket.Value.FragmentCount, Is.EqualTo(8));
                Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerWeaponRuntimeEntry(asset));
            }
            finally { UnityEngine.Object.DestroyImmediate(asset); }
        }

        [TestCase("RP_WeaponChest")] [TestCase("RP_DevelopmentRewardPool")]
        public void PoolsContainRocketOnce(string name)
        {
            var pool = AssetDatabase.LoadAssetAtPath<RewardItemPool>("Assets/_Project/Data/Reward/Dev/" + name + ".asset");
            Assert.That(pool.GetValidatedItems().Count(x => x.StableId == "weapon.rocket-launcher"), Is.EqualTo(1));
        }
        [Test]
        public void OtherWeaponsRetainTheirDeliveryModes()
        {
            foreach (var name in new[] { "PlasmaRifle", "DevelopmentSecondaryWeapon", "Shotgun", "Minigun" })
            {
                var entry = new PlayerWeaponRuntimeEntry(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(
                    "Assets/_Project/Data/Items/Weapons/WD_" + name + ".asset"));
                Assert.That(entry.DeliveryMode, Is.EqualTo(name == "PlasmaRifle" ? WeaponDeliveryMode.Plasma : WeaponDeliveryMode.Hitscan));
                Assert.That(entry.Rocket.HasValue, Is.False);
            }
        }
    }
}
