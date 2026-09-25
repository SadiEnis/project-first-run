using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Rewards;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Weapons
{
    public sealed class ShotgunContentTests
    {
        public const string AssetPath = "Assets/_Project/Data/Items/Weapons/WD_Shotgun.asset";
        private static void Advance(PlayerWeaponRuntimeEntry entry) => typeof(PlayerWeaponRuntimeEntry)
            .GetMethod("AdvanceLevel", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(entry, null);

        [TestCase(1, 8, 8, 6, 2f, .6f)]
        [TestCase(2, 10, 8, 6, 2f, .6f)]
        [TestCase(3, 10, 8, 8, 2f, .6f)]
        [TestCase(4, 10, 8, 8, 2f, 1f)]
        [TestCase(5, 12, 8, 8, 2f, 1f)]
        [TestCase(6, 12, 8, 8, 1.6f, 1f)]
        [TestCase(7, 12, 8, 10, 1.6f, 1f)]
        [TestCase(8, 12, 10, 10, 1.6f, 1f)]
        public void AssetMatchesEightLevelContract(int level, float damage, int pellets, int magazine, float reload, float push)
        {
            var definition = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(AssetPath);
            var entry = new PlayerWeaponRuntimeEntry(definition);
            for (int i = 1; i < level; i++) Advance(entry);
            Assert.That(definition.StableId, Is.EqualTo("weapon.shotgun"));
            Assert.That(definition.TriggerMode, Is.EqualTo(WeaponTriggerMode.SemiAutomatic));
            Assert.That(entry.MaximumLevel, Is.EqualTo(8));
            Assert.That(entry.BaseDamage, Is.EqualTo(damage));
            Assert.That(entry.Shot.PelletCount, Is.EqualTo(pellets));
            Assert.That(entry.Shot.HalfAngle, Is.EqualTo(6));
            Assert.That(entry.Shot.PushDistance, Is.EqualTo(push));
            Assert.That(entry.Fire.RecoilDegrees, Is.EqualTo(6f));
            Assert.That(entry.Fire.Recoil.Delay, Is.EqualTo(.08f));
            Assert.That(entry.Fire.Recoil.Duration, Is.EqualTo(.3f));
            Assert.That(entry.Fire.Recoil.MaximumOffset, Is.EqualTo(12));
            Assert.That(entry.Fire.PreparationDuration, Is.Zero);
            Assert.That(entry.Fire.CriticalChance, Is.Zero);
            Assert.That(entry.Range, Is.EqualTo(30));
            Assert.That(entry.RuntimeState.MagazineCapacity, Is.EqualTo(magazine));
            entry.RuntimeState.TryFire();
            Assert.That(entry.RuntimeState.FireCooldownRemaining, Is.EqualTo(1 / 1.2f).Within(.0001));
            entry.RuntimeState.TryStartReload();
            Assert.That(entry.RuntimeState.ReloadTimeRemaining, Is.EqualTo(reload));
        }

        [TestCase(0, 6f, 1f)]
        [TestCase(65, 6f, 1f)]
        [TestCase(8, -1f, 1f)]
        [TestCase(8, 90f, 1f)]
        [TestCase(8, float.NaN, 1f)]
        [TestCase(8, 6f, float.PositiveInfinity)]
        [TestCase(8, 6f, -1f)]
        public void InvalidShotConfigurationIsRejected(int pellets, float angle, float push)
            => Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponShotConfig(pellets, angle, push));

        [Test]
        public void SpreadIsDeterministicNormalizedCircularAndWithinCone()
        {
            Vector3 forward = new Vector3(1, .3f, 1).normalized;
            var random = new System.Random(728);
            var directions = Enumerable.Range(0, 500).Select(_ =>
                PelletSpread.Direction(forward, 6, random.NextDouble(), random.NextDouble())).ToArray();
            random = new System.Random(728);
            foreach (var direction in directions)
            {
                Assert.That(direction, Is.EqualTo(PelletSpread.Direction(forward, 6, random.NextDouble(), random.NextDouble())));
                Assert.That(direction.magnitude, Is.EqualTo(1).Within(.0001));
                Assert.That(Vector3.Angle(forward, direction), Is.LessThanOrEqualTo(6.001f));
            }
            Assert.That(directions.Distinct().Count(), Is.EqualTo(500));
            Assert.That(Vector3.Angle(forward, PelletSpread.Direction(forward, 0, .4, .8)), Is.LessThan(.01f));
            Assert.Throws<ArgumentOutOfRangeException>(() => PelletSpread.Direction(forward, 6, double.NaN, 0));
        }

        [TestCase("WD_PlasmaRifle")]
        [TestCase("WD_DevelopmentSecondaryWeapon")]
        public void ExistingAssetsRemainSingleRayAtEveryLevel(string name)
        {
            var entry = new PlayerWeaponRuntimeEntry(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(
                "Assets/_Project/Data/Items/Weapons/" + name + ".asset"));
            for (int level = 1; level <= 8; level++)
            {
                Assert.That(entry.Shot.PelletCount, Is.EqualTo(1));
                Assert.That(entry.Shot.HalfAngle, Is.Zero);
                Assert.That(entry.Shot.PushDistance, Is.Zero);
                if (level < 8) Advance(entry);
            }
        }

        [Test]
        public void AcquiredPatternAndRangeAreSnapshotsAndNewEntriesRejectMalformedLaterLevels()
        {
            var copy = Object.Instantiate(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(AssetPath));
            try
            {
                var first = new PlayerWeaponRuntimeEntry(copy);
                var second = new PlayerWeaponRuntimeEntry(copy);
                var data = new SerializedObject(copy);
                data.FindProperty("_range").floatValue = 10;
                data.FindProperty("_pelletCount").intValue = 2;
                data.FindProperty("_additionalLevels").GetArrayElementAtIndex(6).FindPropertyRelative("_pelletCount").intValue = 0;
                data.ApplyModifiedPropertiesWithoutUndo();
                for (int i = 1; i < 8; i++) Advance(first);
                Assert.That(first.Shot.PelletCount, Is.EqualTo(10));
                Assert.That(first.Range, Is.EqualTo(30));
                Assert.That(second.Level, Is.EqualTo(1));
                Assert.That(second.Shot.PelletCount, Is.EqualTo(8));
                Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerWeaponRuntimeEntry(copy));
            }
            finally { Object.DestroyImmediate(copy); }
        }

        [TestCase("RP_WeaponChest")]
        [TestCase("RP_DevelopmentRewardPool")]
        public void RewardPoolsContainShotgunExactlyOnce(string name)
        {
            var pool = AssetDatabase.LoadAssetAtPath<RewardItemPool>("Assets/_Project/Data/Reward/Dev/" + name + ".asset");
            Assert.That(pool.GetValidatedItems().Count(x => x.StableId == "weapon.shotgun"), Is.EqualTo(1));
        }
    }
}
