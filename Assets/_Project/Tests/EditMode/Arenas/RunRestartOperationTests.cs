using System;
using NUnit.Framework;
using ProjectFirstRun.Flow;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class RunRestartOperationTests
    {
        private sealed class Loader : IRunSceneReloader
        {
            public int Calls;
            public Func<bool> Load = () => true;
            public bool Begin(string path) { Calls++; return Load(); }
        }

        [Test]
        public void IneligibleAndDuplicateRequestsNeverLoad()
        {
            var operation = new RunRestartOperation();
            var loader = new Loader();
            Assert.That(operation.TryStart(false, "run", loader), Is.False);
            Assert.That(loader.Calls, Is.Zero);
            Assert.That(operation.State, Is.EqualTo(RunRestartState.Idle));
            Assert.That(operation.TryStart(true, "run", loader), Is.True);
            Assert.That(operation.TryStart(true, "run", loader), Is.False);
            Assert.That(loader.Calls, Is.EqualTo(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void StartupFailureAllowsRetry(bool throws)
        {
            var operation = new RunRestartOperation();
            var loader = new Loader { Load = () => throws ? throw new InvalidOperationException("test") : false };
            Assert.That(operation.TryStart(true, "run", loader), Is.False);
            Assert.That(operation.State, Is.EqualTo(RunRestartState.Failed));
            Assert.That(operation.Error, Is.Not.Null);
            loader.Load = () => true;
            Assert.That(operation.TryStart(true, "run", loader), Is.True);
            Assert.That(operation.Error, Is.Null);
            Assert.That(loader.Calls, Is.EqualTo(2));
        }

        [Test]
        public void ReentrantRequestCannotStartAnotherLoad()
        {
            var operation = new RunRestartOperation();
            var loader = new Loader();
            loader.Load = () => { Assert.That(operation.TryStart(true, "run", loader), Is.False); return true; };
            Assert.That(operation.TryStart(true, "run", loader), Is.True);
            Assert.That(loader.Calls, Is.EqualTo(1));
        }

        [Test]
        public void UnsavedSceneFailsBeforeInvokingLoader()
        {
            var operation = new RunRestartOperation();
            var loader = new Loader();
            Assert.That(operation.TryStart(true, "", loader), Is.False);
            Assert.That(loader.Calls, Is.Zero);
            Assert.That(operation.State, Is.EqualTo(RunRestartState.Failed));
        }
    }
}
