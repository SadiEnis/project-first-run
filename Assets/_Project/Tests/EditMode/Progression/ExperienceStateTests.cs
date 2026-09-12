using System;
using NUnit.Framework;
using ProjectFirstRun.Progression;

namespace ProjectFirstRun.Tests.EditMode.Progression
{
    public sealed class ExperienceStateTests
    {
        [TestCase(0, 50)]
        [TestCase(-1, 50)]
        [TestCase(100, 0)]
        [TestCase(100, -1)]
        public void Curve_RejectsNonPositiveCosts(int first, int increase)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new ExperienceCurve(first, increase));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Curve_RejectsInvalidLevels(int level)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new ExperienceCurve(100, 50).GetRequiredExperience(level));
        }

        [Test]
        public void Curve_UsesWideArithmetic()
        {
            var curve = new ExperienceCurve(int.MaxValue, int.MaxValue);
            Assert.That(curve.GetRequiredExperience(int.MaxValue),
                Is.EqualTo((long)int.MaxValue * int.MaxValue));
        }

        [Test]
        public void State_RequiresCurve()
        {
            Assert.Throws<ArgumentNullException>(() => new ExperienceState(null));
        }

        [Test]
        public void NewRun_StartsAtLevelOneWithoutExperience()
        {
            var state = CreateState();
            Assert.That(state.Level, Is.EqualTo(1));
            Assert.That(state.CurrentExperience, Is.Zero);
            Assert.That(state.TotalExperience, Is.Zero);
            Assert.That(state.RequiredExperience, Is.EqualTo(100));
        }

        [TestCase(0, 1, 0, 100)]
        [TestCase(99, 1, 99, 100)]
        [TestCase(100, 2, 0, 150)]
        [TestCase(140, 2, 40, 150)]
        [TestCase(250, 3, 0, 200)]
        [TestCase(500, 4, 50, 250)]
        public void Gain_PreservesOverflowAcrossAllReachedLevels(
            int amount, int level, int remaining, int required)
        {
            var state = CreateState();
            ExperienceGainResult result = state.GainExperience(amount);
            Assert.That(state.Level, Is.EqualTo(level));
            Assert.That(state.CurrentExperience, Is.EqualTo(remaining));
            Assert.That(state.TotalExperience, Is.EqualTo(amount));
            Assert.That(state.RequiredExperience, Is.EqualTo(required));
            Assert.That(result.Amount, Is.EqualTo(amount));
            Assert.That(result.PreviousLevel, Is.EqualTo(1));
            Assert.That(result.CurrentLevel, Is.EqualTo(level));
            Assert.That(result.LevelsGained, Is.EqualTo(level - 1));
            Assert.That(result.CurrentExperience, Is.EqualTo(remaining));
            Assert.That(result.RequiredExperience, Is.EqualTo(required));
            Assert.That(result.TotalExperience, Is.EqualTo(amount));
        }

        [Test]
        public void NegativeGain_DoesNotChangeExistingProgress()
        {
            var state = CreateState();
            state.GainExperience(140);
            Assert.Throws<ArgumentOutOfRangeException>(() => state.GainExperience(-1));
            Assert.That(state.Level, Is.EqualTo(2));
            Assert.That(state.CurrentExperience, Is.EqualTo(40));
            Assert.That(state.TotalExperience, Is.EqualTo(140));
        }

        [Test]
        public void SplitAwards_HaveTheSameFinalProgressAsOneAward()
        {
            var split = CreateState();
            split.GainExperience(70);
            split.GainExperience(80);
            split.GainExperience(350);
            var single = CreateState();
            single.GainExperience(500);
            Assert.That(split.Level, Is.EqualTo(single.Level));
            Assert.That(split.CurrentExperience, Is.EqualTo(single.CurrentExperience));
            Assert.That(split.TotalExperience, Is.EqualTo(single.TotalExperience));
        }

        [Test]
        public void GainResult_IsSnapshotAndReportsPreviousLevel()
        {
            var state = CreateState();
            state.GainExperience(100);
            var result = state.GainExperience(170);
            state.GainExperience(1000);
            Assert.That(result.PreviousLevel, Is.EqualTo(2));
            Assert.That(result.CurrentLevel, Is.EqualTo(3));
            Assert.That(result.LevelsGained, Is.EqualTo(1));
            Assert.That(result.CurrentExperience, Is.EqualTo(20));
            Assert.That(result.TotalExperience, Is.EqualTo(270));
        }

        [Test]
        public void RunsSharingCurve_DoNotShareProgress()
        {
            var curve = new ExperienceCurve(100, 50);
            var first = new ExperienceState(curve);
            var second = new ExperienceState(curve);
            first.GainExperience(500);
            Assert.That(second.Level, Is.EqualTo(1));
            Assert.That(second.TotalExperience, Is.Zero);
        }

        [Test]
        public void LargeAwards_ConserveExperienceBeyondIntegerTotal()
        {
            var state = new ExperienceState(new ExperienceCurve(1, 1));
            state.GainExperience(int.MaxValue);
            state.GainExperience(int.MaxValue);
            long spent = (long)(state.Level - 1) * state.Level / 2;
            Assert.That(state.TotalExperience, Is.EqualTo(2L * int.MaxValue));
            Assert.That(spent + state.CurrentExperience, Is.EqualTo(state.TotalExperience));
            Assert.That(state.CurrentExperience, Is.LessThan(state.RequiredExperience));
        }

        private static ExperienceState CreateState()
        {
            return new ExperienceState(new ExperienceCurve(100, 50));
        }
    }
}
