using System;
using NUnit.Framework;
using ProjectFirstRun.Arenas;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class RegionEncounterSessionTests
    {
        private sealed class Encounter : IArenaSession
        {
            public bool IsInitialized { get; set; } = true;
            public ArenaSessionStatus Status { get; set; } = ArenaSessionStatus.Ready;
            public event Action Victory;
            public event Action Defeat;
            public int Starts;
            public bool RejectBegin;
            public Action OnBegin;
            public void Begin()
            {
                if (RejectBegin) throw new InvalidOperationException();
                Starts++;
                Status = ArenaSessionStatus.Running;
                OnBegin?.Invoke();
            }
            public void Complete() { Status = ArenaSessionStatus.Victory; Victory?.Invoke(); }
            public void Fail() { Status = ArenaSessionStatus.Defeat; Defeat?.Invoke(); }
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Identity_MustBeNonEmpty(string id)
        {
            Assert.Throws<ArgumentException>(() => new RegionEncounterSession(id));
        }

        [Test]
        public void Prepare_ValidatesBeforeBindingAndDoesNotStart()
        {
            using var region = new RegionEncounterSession("side_room");
            var encounter = new Encounter { IsInitialized = false };
            Assert.Throws<ArgumentNullException>(() => region.Prepare(null));
            Assert.Throws<InvalidOperationException>(() => region.Prepare(encounter));
            encounter.IsInitialized = true;
            encounter.Status = ArenaSessionStatus.Running;
            Assert.Throws<InvalidOperationException>(() => region.Prepare(encounter));
            encounter.Status = ArenaSessionStatus.Ready;
            region.Prepare(encounter);
            Assert.That(region.RegionId, Is.EqualTo("side_room"));
            Assert.That(region.Status, Is.EqualTo(RegionEncounterStatus.Prepared));
            Assert.That(region.IsPlayerInside, Is.False);
            Assert.That(encounter.Starts, Is.Zero);
            Assert.Throws<InvalidOperationException>(() => region.Prepare(encounter));
        }

        [Test]
        public void Enter_RequiresPreparation()
        {
            using var region = new RegionEncounterSession("a");
            Assert.Throws<InvalidOperationException>(region.Enter);
            Assert.That(region.IsPlayerInside, Is.False);
        }

        [Test]
        public void LeaveAndReenter_KeepActiveEncounterAndStartOnlyOnce()
        {
            using var region = new RegionEncounterSession("a");
            var encounter = new Encounter();
            region.Prepare(encounter);
            region.Enter();
            region.Enter();
            region.Leave();
            region.Leave();
            Assert.That(region.IsPlayerInside, Is.False);
            Assert.That(region.Status, Is.EqualTo(RegionEncounterStatus.Active));
            Assert.That(encounter.Status, Is.EqualTo(ArenaSessionStatus.Running));
            region.Enter();
            Assert.That(region.IsPlayerInside, Is.True);
            Assert.That(encounter.Starts, Is.EqualTo(1));
        }

        [Test]
        public void CompleteWhileOutside_IsLocalAndDoesNotRestartOnReentry()
        {
            using var region = new RegionEncounterSession("a");
            var encounter = new Encounter();
            var run = new RunSessionState(2);
            run.Begin();
            int completions = 0;
            region.Completed += () => completions++;
            region.Prepare(encounter);
            region.Enter();
            region.Leave();
            encounter.Complete();
            encounter.Complete();
            encounter.Fail();
            Assert.That(region.Status, Is.EqualTo(RegionEncounterStatus.Completed));
            Assert.That(region.IsPlayerInside, Is.False);
            region.Enter();
            Assert.That(completions, Is.EqualTo(1));
            Assert.That(encounter.Starts, Is.EqualTo(1));
            Assert.That(run.Status, Is.EqualTo(RunSessionStatus.Running));
            Assert.That(run.CompletedArenas, Is.Zero);
        }

        [Test]
        public void FailureWhileOutside_IsTerminalAndNotifiesOnce()
        {
            using var region = new RegionEncounterSession("a");
            var encounter = new Encounter();
            int failures = 0;
            region.Failed += () => failures++;
            region.Prepare(encounter);
            region.Enter();
            region.Leave();
            encounter.Fail();
            encounter.Fail();
            encounter.Complete();
            Assert.That(region.Status, Is.EqualTo(RegionEncounterStatus.Failed));
            Assert.That(failures, Is.EqualTo(1));
            Assert.Throws<InvalidOperationException>(region.Enter);
            Assert.That(region.IsPlayerInside, Is.False);
        }

        [Test]
        public void RejectedBegin_LeavesPreparationRetryable()
        {
            using var region = new RegionEncounterSession("a");
            var encounter = new Encounter { RejectBegin = true };
            region.Prepare(encounter);
            Assert.Throws<InvalidOperationException>(region.Enter);
            Assert.That(region.Status, Is.EqualTo(RegionEncounterStatus.Prepared));
            Assert.That(region.IsPlayerInside, Is.False);
            encounter.RejectBegin = false;
            region.Enter();
            Assert.That(encounter.Starts, Is.EqualTo(1));
        }

        [Test]
        public void SynchronousCompletionAndReentry_AreNotLost()
        {
            using var region = new RegionEncounterSession("a");
            var encounter = new Encounter();
            encounter.OnBegin = () => { region.Enter(); encounter.Complete(); };
            region.Prepare(encounter);
            region.Enter();
            Assert.That(region.Status, Is.EqualTo(RegionEncounterStatus.Completed));
            Assert.That(encounter.Starts, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_UnsubscribesAndRejectsFurtherOperations()
        {
            var region = new RegionEncounterSession("a");
            var encounter = new Encounter();
            int notifications = 0;
            region.Completed += () => notifications++;
            region.Failed += () => notifications++;
            region.Prepare(encounter);
            region.Enter();
            region.Dispose();
            region.Dispose();
            encounter.Complete();
            encounter.Fail();
            Assert.That(region.Status, Is.EqualTo(RegionEncounterStatus.Active));
            Assert.That(notifications, Is.Zero);
            Assert.Throws<ObjectDisposedException>(region.Enter);
            Assert.Throws<ObjectDisposedException>(region.Leave);
            Assert.Throws<ObjectDisposedException>(() => region.Prepare(encounter));
        }
    }
}
