using System;
using NUnit.Framework;
using ProjectFirstRun.Arenas;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class DemoEndingTimelineTests
    {
        [Test]
        public void ImpactOccursOnceAndFadeFollowsAftermath()
        {
            var timeline = new DemoEndingTimeline(1, 1, 1, 1, 1);
            Assert.That(timeline.Tick(0), Is.False);
            Assert.That(timeline.Tick(2), Is.False);
            Assert.That(timeline.Tick(1), Is.True);
            Assert.That(timeline.HasImpacted, Is.True);
            Assert.That(timeline.Fade, Is.Zero);
            Assert.That(timeline.Tick(1), Is.False);
            Assert.That(timeline.Fade, Is.Zero);
            timeline.Tick(.5f);
            Assert.That(timeline.Fade, Is.EqualTo(.5f).Within(.001f));
            Assert.That(timeline.IsComplete, Is.False);
            timeline.Tick(.5f);
            Assert.That(timeline.Fade, Is.EqualTo(1));
            Assert.That(timeline.IsComplete, Is.True);
            Assert.That(timeline.Tick(5), Is.False);
            Assert.That(timeline.Elapsed, Is.EqualTo(timeline.Duration));
        }

        [Test]
        public void LargeFrameStillImpactsOnceAndCompletes()
        {
            var timeline = new DemoEndingTimeline(1, 1, 1, 1, 1);
            Assert.That(timeline.Tick(100), Is.True);
            Assert.That(timeline.IsComplete, Is.True);
            Assert.That(timeline.Tick(100), Is.False);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(61)]
        public void InvalidDurationIsRejected(float duration)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DemoEndingTimeline(duration, 1, 1, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DemoEndingTimeline(1, 1, 1, 1, duration));
        }

        [Test]
        public void InvalidTickDoesNotAdvance()
        {
            var timeline = new DemoEndingTimeline(1, 1, 1, 1, 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => timeline.Tick(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => timeline.Tick(float.NaN));
            Assert.That(timeline.Elapsed, Is.Zero);
        }
    }
}
