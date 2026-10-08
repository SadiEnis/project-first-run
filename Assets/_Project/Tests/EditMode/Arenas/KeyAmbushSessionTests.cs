using System;
using NUnit.Framework;
using ProjectFirstRun.Arenas;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class KeyAmbushSessionTests
    {
        [TestCase(false, true, true)]
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        public void UnsafePickupDoesNotConsumeKeyOrCloseEntrance(bool alive, bool allowed, bool clear)
        {
            var session = new KeyAmbushSession();
            Assert.That(session.TryCollect(alive, allowed, clear), Is.False);
            Assert.That(session.HasKey, Is.False);
            Assert.That(session.EntranceClosed, Is.False);
            Assert.That(session.ExitOpen, Is.False);
        }

        [Test]
        public void CollectOnceClosesEntranceAndRequestsOnlyFirstGroup()
        {
            var session = new KeyAmbushSession();
            Assert.That(session.TryCollect(true, true, true), Is.True);
            Assert.That(session.TryCollect(true, true, true), Is.False);
            Assert.That(session.EntranceClosed, Is.True);
            Assert.That(session.CanEnterFinal, Is.False);
            Assert.That(session.RequestedGroup, Is.Zero);
            Assert.That(session.TryStartGroup(1), Is.False);
        }

        [Test]
        public void BothGroupsMustFinishInOrderAndIntermissionDoesNotOpenExit()
        {
            var session = StartFirst();
            session.CompleteGroup(1);
            Assert.That(session.Phase, Is.EqualTo(KeyAmbushPhase.FightingFirst));
            session.CompleteGroup(0);
            Assert.That(session.Phase, Is.EqualTo(KeyAmbushPhase.Intermission));
            Assert.That(session.ExitOpen, Is.False);
            session.Tick(.5f);
            session.CompleteGroup(0); // Duplicate completion cannot restart the timer.
            session.Tick(.5f);
            Assert.That(session.RequestedGroup, Is.EqualTo(1));
            Assert.That(session.TryStartGroup(1), Is.True);
            Assert.That(session.TryStartGroup(1), Is.False);
            session.CompleteGroup(0);
            Assert.That(session.ExitOpen, Is.False);
            session.CompleteGroup(1);
            Assert.That(session.CanEnterFinal, Is.True);
            Assert.That(session.EntranceClosed, Is.True);
            Assert.That(session.ExitOpen, Is.True);
        }

        [Test]
        public void PauseDoesNotAdvanceIntermission()
        {
            var session = StartFirst();
            session.CompleteGroup(0);
            for (int i = 0; i < 100; i++) session.Tick(0);
            Assert.That(session.Phase, Is.EqualTo(KeyAmbushPhase.Intermission));
        }

        [Test]
        public void FailedPreparationNeverBecomesVictoryOrRetriesFromEvents()
        {
            var session = StartFirst();
            session.Fail("Missing spawn point");
            session.CompleteGroup(0); session.CompleteGroup(1); session.Tick(10);
            Assert.That(session.TryStartGroup(1), Is.False);
            Assert.That(session.TryCollect(true, true, true), Is.False);
            Assert.That(session.Phase, Is.EqualTo(KeyAmbushPhase.Failed));
            Assert.That(session.Error, Is.EqualTo("Missing spawn point"));
            Assert.That(session.CanEnterFinal, Is.False);
        }

        [Test]
        public void DeathClearsKeyAndCannotBeOverwrittenByLateVictory()
        {
            var session = StartFirst();
            session.Cancel(); session.CompleteGroup(0); session.CompleteGroup(1); session.Fail("late error");
            Assert.That(session.Phase, Is.EqualTo(KeyAmbushPhase.Cancelled));
            Assert.That(session.HasKey, Is.False);
            Assert.That(session.CanEnterFinal, Is.False);
        }

        [Test]
        public void FreshRunHasInitialGateAndKeyState()
        {
            var old = StartFirst(); old.Cancel();
            var fresh = new KeyAmbushSession();
            Assert.That(fresh.Phase, Is.EqualTo(KeyAmbushPhase.AwaitingKey));
            Assert.That(fresh.EntranceClosed, Is.False);
            Assert.That(fresh.ExitOpen, Is.False);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidDurationIsRejected(float seconds)
            => Assert.Throws<ArgumentOutOfRangeException>(() => new KeyAmbushSession(seconds));

        private static KeyAmbushSession StartFirst()
        {
            var session = new KeyAmbushSession();
            session.TryCollect(true, true, true);
            Assert.That(session.TryStartGroup(0), Is.True);
            return session;
        }
    }
}
