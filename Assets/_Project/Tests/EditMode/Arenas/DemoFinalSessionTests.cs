using NUnit.Framework;
using ProjectFirstRun.Arenas;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class DemoFinalSessionTests
    {
        [Test]
        public void ReplayRequiresCompletionAndAcceptsOnlyOneRequest()
        {
            var session = new DemoFinalSession();
            Assert.That(session.TryRequestReplay(), Is.False);
            session.TryBegin(true, true, true, true);
            Assert.That(session.TryRequestReplay(), Is.False);
            session.TryComplete();
            Assert.That(session.TryRequestReplay(), Is.True);
            Assert.That(session.TryRequestReplay(), Is.False);
            session.CancelFailedReplay();
            Assert.That(session.TryRequestReplay(), Is.True);
            Assert.That(new DemoFinalSession().ReplayRequested, Is.False);
        }

        [TestCase(false, true, true)]
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        public void EntryRequiresEligibilityControlAndInterior(bool eligible, bool allowed, bool inside)
        {
            var session = new DemoFinalSession();
            Assert.That(session.TryBegin(true, eligible, allowed, inside), Is.False);
            Assert.That(session.Phase, Is.EqualTo(DemoFinalPhase.Playing));
        }

        [Test]
        public void DeathBeforeEntryWinsAndCannotComplete()
        {
            var session = new DemoFinalSession();
            Assert.That(session.TryBegin(false, true, true, true), Is.False);
            Assert.That(session.Phase, Is.EqualTo(DemoFinalPhase.Dead));
            Assert.That(session.TryBegin(true, true, true, true), Is.False);
            Assert.That(session.TryComplete(), Is.False);
        }

        [Test]
        public void EndingIsOneShotAndDeathCannotOverwriteIt()
        {
            var session = new DemoFinalSession();
            Assert.That(session.TryComplete(), Is.False);
            Assert.That(session.TryBegin(true, true, true, true), Is.True);
            Assert.That(session.TryBegin(true, true, true, true), Is.False);
            Assert.That(session.TryDie(), Is.False);
            Assert.That(session.Phase, Is.EqualTo(DemoFinalPhase.Ending));
            Assert.That(session.TryComplete(), Is.True);
            Assert.That(session.TryComplete(), Is.False);
            Assert.That(session.TryDie(), Is.False);
            Assert.That(new DemoFinalSession().Phase, Is.EqualTo(DemoFinalPhase.Playing));
        }
    }
}
