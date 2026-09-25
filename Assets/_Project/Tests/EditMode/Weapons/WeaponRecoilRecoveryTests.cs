using System;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Weapons;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.EditMode.Weapons
{
    public sealed class WeaponRecoilRecoveryTests
    {
        private static WeaponRecoilConfig Shotgun => new WeaponRecoilConfig(6, .08f, .3f, 12);

        [Test]
        public void KickWaitsThenReturnsSmoothlyToExactZero()
        {
            var state = new WeaponRecoilState();
            state.Kick(Shotgun);
            Assert.That(state.Offset, Is.EqualTo(6));
            state.Tick(.08f);
            Assert.That(state.Offset, Is.EqualTo(6));
            state.Tick(.15f);
            Assert.That(state.Offset, Is.EqualTo(3).Within(.0001));
            state.Tick(.15f);
            Assert.That(state.Offset, Is.Zero);
            state.Tick(10);
            Assert.That(state.Offset, Is.Zero);
        }

        [Test]
        public void RefireAddsToCurrentRecoveryOffsetAndRestartsDelay()
        {
            var state = new WeaponRecoilState();
            state.Kick(Shotgun);
            state.Tick(.23f);
            Assert.That(state.Offset, Is.EqualTo(3).Within(.0001));
            state.Kick(Shotgun);
            Assert.That(state.Offset, Is.EqualTo(9).Within(.0001));
            state.Tick(.08f);
            Assert.That(state.Offset, Is.EqualTo(9).Within(.0001));
            state.Tick(.15f);
            Assert.That(state.Offset, Is.EqualTo(4.5f).Within(.0001));
            state.Kick(Shotgun);
            Assert.That(state.Offset, Is.EqualTo(10.5f).Within(.0001));
            state.Kick(Shotgun);
            Assert.That(state.Offset, Is.EqualTo(12));
        }

        [TestCase(30)] [TestCase(60)] [TestCase(120)]
        public void ReturnDoesNotDependOnFrameSubdivision(int fps)
        {
            var state = new WeaponRecoilState();
            var whole = new WeaponRecoilState();
            state.Kick(new WeaponRecoilConfig(6, .08f, 1));
            whole.Kick(new WeaponRecoilConfig(6, .08f, 1));
            for (int i = 0; i < fps / 2; i++) state.Tick(1f / fps);
            whole.Tick(.5f);
            Assert.That(state.Offset, Is.EqualTo(whole.Offset).Within(.0001));
            state.Tick(2);
            Assert.That(state.Offset, Is.Zero);
        }

        [Test]
        public void SustainedMinigunCapsAndZeroTimeFreezesRecovery()
        {
            var state = new WeaponRecoilState();
            var config = new WeaponRecoilConfig(.35f, .12f, .25f, 8);
            for (int i = 0; i < 100; i++) { state.Kick(config); state.Tick(1f / 12); }
            Assert.That(state.Offset, Is.EqualTo(8));
            float delay = state.DelayRemaining;
            state.Tick(0);
            Assert.That(state.DelayRemaining, Is.EqualTo(delay));
            Assert.That(state.Offset, Is.EqualTo(8));
            state.Tick(1);
            Assert.That(state.Offset, Is.Zero);
        }

        [Test]
        public void LowerCapProfileDoesNotSnapOldKickAndZeroKickDoesNotRestartRecovery()
        {
            var state = new WeaponRecoilState();
            state.Kick(Shotgun);
            state.Kick(Shotgun);
            state.Kick(new WeaponRecoilConfig(.35f, .12f, .25f, 8));
            Assert.That(state.Offset, Is.EqualTo(12));
            state.Tick(.245f);
            Assert.That(state.Offset, Is.EqualTo(6).Within(.0001));
            state.Kick(new WeaponRecoilConfig(0));
            state.Tick(.125f);
            Assert.That(state.Offset, Is.Zero);
        }

        [Test]
        public void PitchHeadroomTrimsHiddenOffsetAndResetClearsTimers()
        {
            var state = new WeaponRecoilState();
            state.Kick(Shotgun);
            state.Tick(.23f);
            state.Constrain(1);
            Assert.That(state.Offset, Is.EqualTo(1));
            state.Tick(.15f);
            Assert.That(state.Offset, Is.Zero);
            state.Kick(Shotgun);
            state.Constrain(0);
            Assert.That(state.Offset, Is.Zero);
            Assert.That(state.DelayRemaining, Is.Zero);
            state.Kick(Shotgun);
            state.Reset();
            Assert.That(state.Offset, Is.Zero);
        }

        [TestCase(-1, .08f, .3f, 12)]
        [TestCase(float.NaN, .08f, .3f, 12)]
        [TestCase(6, -1, .3f, 12)]
        [TestCase(6, float.PositiveInfinity, .3f, 12)]
        [TestCase(6, .08f, 0, 12)]
        [TestCase(6, .08f, float.NaN, 12)]
        [TestCase(6, .08f, .3f, 0)]
        [TestCase(6, .08f, .3f, 5)]
        [TestCase(6, .08f, .3f, float.PositiveInfinity)]
        public void InvalidConfigurationsAreRejected(float kick, float delay, float duration, float cap)
            => Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponRecoilConfig(kick, delay, duration, cap));

        [Test]
        public void InvalidTickDefaultConfigAndHeadroomAreRejectedWithoutMutation()
        {
            var state = new WeaponRecoilState();
            state.Kick(Shotgun);
            Assert.Throws<ArgumentOutOfRangeException>(() => state.Tick(float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => state.Tick(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => state.Constrain(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => state.Kick(default));
            Assert.That(state.Offset, Is.EqualTo(6));
        }

        [Test]
        public void ProfileSnapshotsAndValidationIncludeLaterLevelRecoverySettings()
        {
            var asset = Object.Instantiate(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(
                "Assets/_Project/Data/Items/Weapons/WD_Shotgun.asset"));
            try
            {
                var entry = new PlayerWeaponRuntimeEntry(asset);
                var data = new SerializedObject(asset);
                data.FindProperty("_recoilRecoveryDuration").floatValue = 4;
                data.FindProperty("_additionalLevels").GetArrayElementAtIndex(6)
                    .FindPropertyRelative("_recoilMaximumOffset").floatValue = 0;
                data.ApplyModifiedPropertiesWithoutUndo();
                for (int i = 1; i < 8; i++) typeof(PlayerWeaponRuntimeEntry)
                    .GetMethod("AdvanceLevel", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(entry, null);
                Assert.That(entry.Fire.Recoil.Duration, Is.EqualTo(.3f));
                Assert.That(entry.Fire.Recoil.MaximumOffset, Is.EqualTo(12));
                Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerWeaponRuntimeEntry(asset));
            }
            finally { Object.DestroyImmediate(asset); }
        }
    }
}
