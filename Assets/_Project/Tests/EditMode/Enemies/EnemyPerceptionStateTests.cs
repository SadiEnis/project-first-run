using System;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Enemies;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Enemies
{
    public sealed class EnemyPerceptionStateTests
    {
        [TestCase(0f, true)]
        [TestCase(60f, true)]
        [TestCase(61f, false)]
        [TestCase(180f, false)]
        public void AcquisitionUsesTotalHorizontalCone(float angle, bool expected)
        {
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * 10;
            Assert.That(EnemyPerceptionState.InView(offset, Vector3.forward, 18, 120), Is.EqualTo(expected));
        }

        [Test]
        public void RangeIsThreeDimensionalAndVerticalAlignmentStillChecksRange()
        {
            Assert.That(EnemyPerceptionState.InView(Vector3.up * 18, Vector3.forward, 18, 120), Is.True);
            Assert.That(EnemyPerceptionState.InView(Vector3.up * 19, Vector3.forward, 18, 120), Is.False);
            Assert.That(EnemyPerceptionState.InView(new Vector3(0, 15, 15), Vector3.forward, 18, 120), Is.False);
            Assert.That(EnemyPerceptionState.InView(Vector3.back * 24, Vector3.forward, 24, 360), Is.True);
        }

        [Test]
        public void HiddenDamageRenewsMemoryWithoutTrackingThePlayer()
        {
            var state = new EnemyPerceptionState(3);
            state.Alarm(Vector3.forward * 30);
            state.Tick(2);
            state.Alarm(Vector3.right * 50);
            Assert.That(state.Awareness, Is.EqualTo(EnemyAwareness.Investigating));
            Assert.That(state.LastKnownPosition, Is.EqualTo(Vector3.forward * 30));
            Assert.That(state.MemoryRemaining, Is.EqualTo(3));
            state.Tick(3);
            Assert.That(state.IsAlerted, Is.False);
            state.Alarm(Vector3.right);
            Assert.That(state.LastKnownPosition, Is.EqualTo(Vector3.right));
        }

        [Test]
        public void SightRefreshesSnapshotAndPausePreservesMemory()
        {
            var state = new EnemyPerceptionState(3);
            state.Observe(true, Vector3.one);
            state.Observe(false, Vector3.right * 99);
            state.Tick(0);
            Assert.That(state.LastKnownPosition, Is.EqualTo(Vector3.one));
            Assert.That(state.MemoryRemaining, Is.EqualTo(3));
            state.Observe(true, Vector3.up);
            Assert.That(state.Awareness, Is.EqualTo(EnemyAwareness.Visible));
            Assert.That(state.LastKnownPosition, Is.EqualTo(Vector3.up));
            state.Reset();
            Assert.That(state.MemoryRemaining, Is.Zero);
        }

        [TestCase(0f)] [TestCase(-1f)] [TestCase(float.NaN)]
        public void InvalidMemoryIsRejected(float value) =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyPerceptionState(value));

        [TestCase("_acquisitionRange", 0f)]
        [TestCase("_trackingRange", 10f)]
        [TestCase("_coneDegrees", 361f)]
        [TestCase("_sampleInterval", float.NaN)]
        [TestCase("_memorySeconds", -1f)]
        public void ProfileRejectsInvalidSettings(string field, float value)
        {
            var profile = ScriptableObject.CreateInstance<EnemyPerceptionProfile>();
            try
            {
                Assert.DoesNotThrow(profile.Validate);
                typeof(EnemyPerceptionProfile).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(profile, value);
                Assert.Throws<InvalidOperationException>(profile.Validate);
            }
            finally { UnityEngine.Object.DestroyImmediate(profile); }
        }
    }
}
