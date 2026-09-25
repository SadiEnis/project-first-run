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
    public sealed class MinigunContentTests
    {
        private const string Path = "Assets/_Project/Data/Items/Weapons/WD_Minigun.asset";
        private static void Advance(PlayerWeaponRuntimeEntry entry) => typeof(PlayerWeaponRuntimeEntry)
            .GetMethod("AdvanceLevel", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(entry, null);

        [TestCase(1, 8, .05f, 80, 12, 3f, .35f)]
        [TestCase(2, 10, .05f, 80, 12, 3f, .35f)]
        [TestCase(3, 10, .1f, 80, 12, 3f, .35f)]
        [TestCase(4, 10, .1f, 100, 12, 3f, .35f)]
        [TestCase(5, 10, .1f, 100, 15, 3f, .35f)]
        [TestCase(6, 12, .1f, 100, 15, 3f, .35f)]
        [TestCase(7, 12, .1f, 100, 15, 2.4f, .35f)]
        [TestCase(8, 12, .1f, 100, 15, 2.4f, .2f)]
        public void AssetMatchesAllLevels(int level, float damage, float crit, int magazine, float rate, float reload, float recoil)
        {
            var definition = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(Path);
            var entry = new PlayerWeaponRuntimeEntry(definition);
            for (int i = 1; i < level; i++) Advance(entry);
            Assert.That(definition.StableId, Is.EqualTo("weapon.minigun"));
            Assert.That(entry.TriggerMode, Is.EqualTo(WeaponTriggerMode.Automatic));
            Assert.That(entry.BaseDamage, Is.EqualTo(damage));
            Assert.That(entry.Fire.CriticalChance, Is.EqualTo(crit));
            Assert.That(entry.Fire.CriticalMultiplier, Is.EqualTo(2));
            Assert.That(entry.Fire.RecoilDegrees, Is.EqualTo(recoil));
            Assert.That(entry.Fire.Recoil.Delay, Is.EqualTo(.12f));
            Assert.That(entry.Fire.Recoil.Duration, Is.EqualTo(.25f));
            Assert.That(entry.Fire.Recoil.MaximumOffset, Is.EqualTo(8));
            Assert.That(entry.Fire.PreparationDuration, Is.EqualTo(.35f));
            Assert.That(entry.Shot.PelletCount, Is.EqualTo(1));
            Assert.That(entry.Shot.HalfAngle, Is.EqualTo(1));
            Assert.That(entry.Shot.PushDistance, Is.Zero);
            Assert.That(entry.Range, Is.EqualTo(40));
            Assert.That(entry.RuntimeState.MagazineCapacity, Is.EqualTo(magazine));
            Assert.That(entry.RuntimeState.ReserveAmmo, Is.EqualTo(240));
            Assert.That(entry.ShotsPerSecond, Is.EqualTo(rate));
            entry.RuntimeState.TryFire();
            entry.RuntimeState.TryStartReload();
            Assert.That(entry.RuntimeState.ReloadTimeRemaining, Is.EqualTo(reload));
        }

        [TestCase(-1, 0, 2, 0)]
        [TestCase(float.NaN, 0, 2, 0)]
        [TestCase(0, -1, 2, 0)]
        [TestCase(0, 1.1f, 2, 0)]
        [TestCase(0, float.NaN, 2, 0)]
        [TestCase(0, 0, .9f, 0)]
        [TestCase(0, 0, float.PositiveInfinity, 0)]
        [TestCase(0, 0, 2, -1)]
        [TestCase(0, 0, 2, float.NaN)]
        public void InvalidProfileIsRejected(float prepare, float chance, float multiplier, float recoil)
            => Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponFireProfile(prepare, chance, multiplier, recoil));

        [Test]
        public void CriticalRollBoundariesAndGlobalRandomIsolation()
        {
            var before = UnityEngine.Random.state;
            Assert.That(new WeaponFireProfile(0, 0).RollCritical(null), Is.False);
            Assert.That(new WeaponFireProfile(0, 1).RollCritical(null), Is.True);
            var profile = new WeaponFireProfile(0, .1f);
            Assert.That(profile.RollCritical(() => .099), Is.True);
            Assert.That(profile.RollCritical(() => .1001), Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(() => profile.RollCritical(() => double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => profile.RollCritical(() => 1));
            Assert.That(UnityEngine.Random.state, Is.EqualTo(before));
        }

        [TestCase(30, 12)] [TestCase(60, 12)] [TestCase(120, 12)]
        [TestCase(30, 15)] [TestCase(60, 15)] [TestCase(120, 15)]
        public void SustainedCadencePreservesRemainder(int fps, float rate)
        {
            var weapon = new WeaponRuntimeState(new WeaponRuntimeConfig(1000, 0, rate, 3));
            var fire = new PreparedAutomaticFire();
            int count = 0;
            for (int i = 0; i < fps * 5; i++)
                fire.Tick(weapon, 1f / fps, .35f, () => { count++; return weapon.TryFire() == WeaponFireResult.Fired; });
            Assert.That(count, Is.EqualTo(1 + (int)Math.Floor((5 - .35) * rate)).Within(1));
            Assert.That(weapon.MagazineAmmo, Is.EqualTo(1000 - count));
        }

        [Test]
        public void PreparationResetPauseAndStallHaveNoFreeShotsOrDeferredBurst()
        {
            var weapon = new WeaponRuntimeState(new WeaponRuntimeConfig(80, 240, 12, 3));
            var fire = new PreparedAutomaticFire();
            int count = 0;
            Func<bool> shoot = () => { count++; return weapon.TryFire() == WeaponFireResult.Fired; };
            fire.Tick(weapon, .2f, .35f, shoot);
            Assert.That(count, Is.Zero);
            fire.Tick(weapon, 0, .35f, shoot);
            Assert.That(fire.PreparationElapsed, Is.EqualTo(.2f));
            fire.Reset();
            fire.Tick(weapon, .2f, .35f, shoot);
            Assert.That(count, Is.Zero);
            fire.Tick(weapon, 10, .35f, shoot);
            Assert.That(count, Is.EqualTo(PreparedAutomaticFire.MaximumShotsPerTick));
            fire.Tick(weapon, .001f, .35f, shoot);
            Assert.That(count, Is.EqualTo(3));
            Assert.Throws<ArgumentOutOfRangeException>(() => fire.Tick(weapon, float.NaN, .35f, shoot));
        }

        [TestCase("WD_PlasmaRifle")]
        [TestCase("WD_DevelopmentSecondaryWeapon")]
        [TestCase("WD_Shotgun")]
        public void ExistingAssetsRetainTheirExplicitFireProfiles(string name)
        {
            var entry = new PlayerWeaponRuntimeEntry(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(
                "Assets/_Project/Data/Items/Weapons/" + name + ".asset"));
            for (int i = 1; i <= entry.MaximumLevel; i++)
            {
                Assert.That(entry.Fire.PreparationDuration, Is.Zero);
                Assert.That(entry.Fire.CriticalChance, Is.Zero);
                Assert.That(entry.Fire.RecoilDegrees, Is.EqualTo(name == "WD_Shotgun" ? 6f : name == "WD_PlasmaRifle" ? .3f : 0f));
                Assert.That(entry.Fire.Recoil.Duration, Is.GreaterThan(0));
                Assert.That(entry.Fire.Recoil.MaximumOffset, Is.GreaterThan(0));
                Assert.That(entry.Fire.CriticalMultiplier, Is.GreaterThanOrEqualTo(1));
                if (i < entry.MaximumLevel) Advance(entry);
            }
        }

        [Test]
        public void LevelProfilesAreSnapshotsAndMalformedLaterLevelIsRejected()
        {
            var asset = Object.Instantiate(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(Path));
            try
            {
                var first = new PlayerWeaponRuntimeEntry(asset);
                var second = new PlayerWeaponRuntimeEntry(asset);
                first.RuntimeState.TryFire();
                first.RuntimeState.TryStartReload();
                float cooldown = first.RuntimeState.FireCooldownRemaining;
                var data = new SerializedObject(asset);
                data.FindProperty("_triggerMode").enumValueIndex = 0;
                data.FindProperty("_additionalLevels").GetArrayElementAtIndex(6).FindPropertyRelative("_criticalChance").floatValue = 2;
                data.ApplyModifiedPropertiesWithoutUndo();
                for (int i = 1; i < 8; i++) Advance(first);
                Assert.That(first.Fire.RecoilDegrees, Is.EqualTo(.2f));
                Assert.That(first.TriggerMode, Is.EqualTo(WeaponTriggerMode.Automatic));
                Assert.That(first.RuntimeState.MagazineAmmo, Is.EqualTo(79));
                Assert.That(first.RuntimeState.ReloadTimeRemaining, Is.EqualTo(3));
                Assert.That(first.RuntimeState.FireCooldownRemaining, Is.EqualTo(cooldown));
                Assert.That(second.Level, Is.EqualTo(1));
                Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerWeaponRuntimeEntry(asset));
            }
            finally { Object.DestroyImmediate(asset); }
        }

        [TestCase("RP_WeaponChest")] [TestCase("RP_DevelopmentRewardPool")]
        public void PoolsIncludeMinigunOnce(string name)
        {
            var pool = AssetDatabase.LoadAssetAtPath<RewardItemPool>("Assets/_Project/Data/Reward/Dev/" + name + ".asset");
            Assert.That(pool.GetValidatedItems().Count(x => x.StableId == "weapon.minigun"), Is.EqualTo(1));
        }
    }
}
