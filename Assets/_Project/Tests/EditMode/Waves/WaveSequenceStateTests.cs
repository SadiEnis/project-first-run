using System;
using NUnit.Framework;
using ProjectFirstRun.Waves;

namespace ProjectFirstRun.Tests.EditMode.Waves
{
    public sealed class WaveSequenceStateTests
    {
        [Test]
        public void Constructor_WithPositiveTotal_InitializesReadyState()
        {
            WaveSequenceState state =
                new WaveSequenceState(3);

            Assert.That(
                state.TotalWaves,
                Is.EqualTo(3));

            Assert.That(
                state.CurrentWaveIndex,
                Is.EqualTo(-1));

            Assert.That(
                state.CompletedWaves,
                Is.EqualTo(0));

            Assert.That(
                state.Status,
                Is.EqualTo(WaveSequenceStatus.Ready));

            Assert.That(
                state.HasActiveWave,
                Is.False);

            Assert.That(
                state.HasRemainingWaves,
                Is.True);

            Assert.That(
                state.IsCompleted,
                Is.False);

            Assert.That(
                state.IsFailed,
                Is.False);
        }

        [Test]
        public void Constructor_WithZeroTotal_Throws()
        {
            Assert.That(
                () => new WaveSequenceState(0),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_WithNegativeTotal_Throws()
        {
            Assert.That(
                () => new WaveSequenceState(-1),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void BeginNextWave_FromReady_StartsFirstWave()
        {
            WaveSequenceState state =
                new WaveSequenceState(3);

            state.BeginNextWave();

            Assert.That(
                state.Status,
                Is.EqualTo(WaveSequenceStatus.Running));

            Assert.That(
                state.CurrentWaveIndex,
                Is.EqualTo(0));

            Assert.That(
                state.CompletedWaves,
                Is.EqualTo(0));

            Assert.That(
                state.HasActiveWave,
                Is.True);

            Assert.That(
                state.HasRemainingWaves,
                Is.True);
        }

        [Test]
        public void BeginNextWave_WhileRunning_Throws()
        {
            WaveSequenceState state =
                new WaveSequenceState(2);

            state.BeginNextWave();

            Assert.That(
                state.BeginNextWave,
                Throws.InvalidOperationException);

            Assert.That(
                state.Status,
                Is.EqualTo(WaveSequenceStatus.Running));

            Assert.That(
                state.CurrentWaveIndex,
                Is.EqualTo(0));
        }

        [Test]
        public void CompleteCurrentWave_WhileReady_Throws()
        {
            WaveSequenceState state =
                new WaveSequenceState(2);

            Assert.That(
                state.CompleteCurrentWave,
                Throws.InvalidOperationException);

            Assert.That(
                state.Status,
                Is.EqualTo(WaveSequenceStatus.Ready));

            Assert.That(
                state.CompletedWaves,
                Is.EqualTo(0));
        }

        [Test]
        public void CompleteCurrentWave_WithRemainingWaves_ReturnsToReady()
        {
            WaveSequenceState state =
                new WaveSequenceState(3);

            state.BeginNextWave();
            state.CompleteCurrentWave();

            Assert.That(
                state.Status,
                Is.EqualTo(WaveSequenceStatus.Ready));

            Assert.That(
                state.CurrentWaveIndex,
                Is.EqualTo(0));

            Assert.That(
                state.CompletedWaves,
                Is.EqualTo(1));

            Assert.That(
                state.HasActiveWave,
                Is.False);

            Assert.That(
                state.HasRemainingWaves,
                Is.True);

            Assert.That(
                state.IsCompleted,
                Is.False);
        }

        [Test]
        public void BeginNextWave_AfterCompletionOfPreviousWave_UsesNextIndex()
        {
            WaveSequenceState state =
                new WaveSequenceState(3);

            state.BeginNextWave();
            state.CompleteCurrentWave();

            state.BeginNextWave();

            Assert.That(
                state.Status,
                Is.EqualTo(WaveSequenceStatus.Running));

            Assert.That(
                state.CurrentWaveIndex,
                Is.EqualTo(1));

            Assert.That(
                state.CompletedWaves,
                Is.EqualTo(1));
        }

        [Test]
        public void CompleteCurrentWave_OnFinalWave_CompletesSequence()
        {
            WaveSequenceState state =
                new WaveSequenceState(2);

            state.BeginNextWave();
            state.CompleteCurrentWave();

            state.BeginNextWave();
            state.CompleteCurrentWave();

            Assert.That(
                state.Status,
                Is.EqualTo(WaveSequenceStatus.Completed));

            Assert.That(
                state.CurrentWaveIndex,
                Is.EqualTo(1));

            Assert.That(
                state.CompletedWaves,
                Is.EqualTo(2));

            Assert.That(
                state.HasActiveWave,
                Is.False);

            Assert.That(
                state.HasRemainingWaves,
                Is.False);

            Assert.That(
                state.IsCompleted,
                Is.True);

            Assert.That(
                state.IsFailed,
                Is.False);
        }

        [Test]
        public void BeginNextWave_AfterSequenceCompleted_Throws()
        {
            WaveSequenceState state =
                CreateCompletedState();

            Assert.That(
                state.BeginNextWave,
                Throws.InvalidOperationException);
        }

        [Test]
        public void CompleteCurrentWave_AfterSequenceCompleted_Throws()
        {
            WaveSequenceState state =
                CreateCompletedState();

            Assert.That(
                state.CompleteCurrentWave,
                Throws.InvalidOperationException);
        }

        [Test]
        public void MarkFailed_FromReady_SetsFailedState()
        {
            WaveSequenceState state =
                new WaveSequenceState(2);

            state.MarkFailed();

            Assert.That(
                state.Status,
                Is.EqualTo(WaveSequenceStatus.Failed));

            Assert.That(
                state.IsFailed,
                Is.True);

            Assert.That(
                state.IsCompleted,
                Is.False);

            Assert.That(
                state.HasActiveWave,
                Is.False);

            Assert.That(
                state.CompletedWaves,
                Is.EqualTo(0));
        }

        [Test]
        public void MarkFailed_WhileRunning_SetsFailedState()
        {
            WaveSequenceState state =
                new WaveSequenceState(2);

            state.BeginNextWave();
            state.MarkFailed();

            Assert.That(
                state.Status,
                Is.EqualTo(WaveSequenceStatus.Failed));

            Assert.That(
                state.IsFailed,
                Is.True);

            Assert.That(
                state.HasActiveWave,
                Is.False);

            Assert.That(
                state.CurrentWaveIndex,
                Is.EqualTo(0));

            Assert.That(
                state.CompletedWaves,
                Is.EqualTo(0));
        }

        [Test]
        public void MarkFailed_Twice_Throws()
        {
            WaveSequenceState state =
                new WaveSequenceState(2);

            state.MarkFailed();

            Assert.That(
                state.MarkFailed,
                Throws.InvalidOperationException);
        }

        [Test]
        public void MarkFailed_AfterSequenceCompleted_Throws()
        {
            WaveSequenceState state =
                CreateCompletedState();

            Assert.That(
                state.MarkFailed,
                Throws.InvalidOperationException);

            Assert.That(
                state.Status,
                Is.EqualTo(WaveSequenceStatus.Completed));
        }

        [Test]
        public void BeginNextWave_AfterSequenceFailed_Throws()
        {
            WaveSequenceState state =
                new WaveSequenceState(2);

            state.MarkFailed();

            Assert.That(
                state.BeginNextWave,
                Throws.InvalidOperationException);
        }

        [Test]
        public void CompleteCurrentWave_AfterSequenceFailed_Throws()
        {
            WaveSequenceState state =
                new WaveSequenceState(2);

            state.BeginNextWave();
            state.MarkFailed();

            Assert.That(
                state.CompleteCurrentWave,
                Throws.InvalidOperationException);
        }

        private static WaveSequenceState CreateCompletedState()
        {
            WaveSequenceState state =
                new WaveSequenceState(1);

            state.BeginNextWave();
            state.CompleteCurrentWave();

            return state;
        }
    }
}