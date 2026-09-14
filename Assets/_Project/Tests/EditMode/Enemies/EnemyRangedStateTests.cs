using NUnit.Framework;
using ProjectFirstRun.Enemies;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Enemies
{
    public sealed class EnemyRangedStateTests
    {
        private static EnemyRangedState Create() => new EnemyRangedState(
            new EnemyRangedConfig(5f, 9f, 12f, .6f, 1.5f, 7f, 4f, .15f));

        [Test]
        public void BandAndLineOfSightControlWindup()
        {
            var state = Create();
            Assert.That(state.TryBeginWindup(Vector3.forward * 7f, true), Is.False);
            state.SetHolding();
            Assert.That(state.TryBeginWindup(Vector3.forward * 7f, false), Is.False);
            Assert.That(state.TryBeginWindup(Vector3.forward * 7f, true), Is.True);
            Assert.That(state.Direction, Is.EqualTo(Vector3.forward));
            Assert.That(state.TryBeginWindup(Vector3.right * 7f, true), Is.False);
        }

        [Test]
        public void WindupStaysReadyUntilReleaseAndCooldownReturnsToHolding()
        {
            var state = Create(); state.SetHolding(); state.TryBeginWindup(Vector3.forward * 7f, true);
            state.Tick(.6f);
            Assert.That(state.IsReadyToRelease, Is.True);
            Assert.That(state.TryRelease(), Is.True);
            state.CommitRelease();
            Assert.That(state.Phase, Is.EqualTo(EnemyRangedPhase.Cooldown));
            state.Tick(1.5f);
            Assert.That(state.Phase, Is.EqualTo(EnemyRangedPhase.Holding));
        }

        [Test]
        public void CancelAndPauseDoNotAdvanceAttack()
        {
            var state = Create(); state.SetHolding(); state.TryBeginWindup(Vector3.forward * 7f, true);
            state.Tick(0f); Assert.That(state.TimeRemaining, Is.EqualTo(.6f));
            state.CancelWindup(); Assert.That(state.Phase, Is.EqualTo(EnemyRangedPhase.Holding));
            state.Cancel(); Assert.That(state.Phase, Is.EqualTo(EnemyRangedPhase.Approaching));
        }

        [Test]
        public void InvalidRangedConfigurationIsRejected()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new EnemyRangedConfig(9, 5, 12, .6f, 1, 7, 4, .15f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new EnemyRangedConfig(5, 9, 8, .6f, 1, 7, 4, .15f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new EnemyRangedConfig(5, 9, 12, 0, 1, 7, 4, .15f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => Create().Tick(-1));
        }
    }
}
