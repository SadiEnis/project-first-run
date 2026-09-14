using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Enemies;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Enemies
{
    public sealed class EnemyChargeStateTests
    {
        private static EnemyChargeState Create() => new EnemyChargeState(new EnemyChargeConfig(7, .8f, 10, 8, .7f, 1.2f));

        [Test]
        public void Windup_LocksDirectionAndPauseDoesNotAdvance()
        {
            var state = Create();
            Assert.That(state.TryBegin(Vector3.right * 5), Is.True);
            Assert.That(state.TryBegin(Vector3.forward * 3), Is.False);
            state.Tick(0);
            Assert.That(state.Phase, Is.EqualTo(EnemyChargePhase.Windup));
            Assert.That(state.Direction, Is.EqualTo(Vector3.right));
            Assert.That(state.TryCommitHit(), Is.False);
            state.Tick(.8f);
            Assert.That(state.Phase, Is.EqualTo(EnemyChargePhase.Charging));
        }

        [Test]
        public void Charge_ClampsTravelAndCommitsOnlyOneHitUntilFreshAttack()
        {
            var state = Create();
            state.TryBegin(Vector3.forward * 5); state.Tick(.8f);
            Assert.That(state.StepDistance(100), Is.EqualTo(8));
            Assert.That(state.TryCommitHit(), Is.True);
            Assert.That(state.TryCommitHit(), Is.False);
            state.Advance(8, false);
            Assert.That(state.Phase, Is.EqualTo(EnemyChargePhase.Recovery));
            Assert.That(state.TryCommitHit(), Is.False);
            state.Tick(.6f);
            Assert.That(state.TryBegin(Vector3.right), Is.False);
            state.Tick(.6f);
            Assert.That(state.TryBegin(Vector3.right), Is.True);
            state.Tick(.8f);
            Assert.That(state.TryCommitHit(), Is.True);
        }

        [Test]
        public void BlockedCharge_RecoversWithoutSpendingRemainingDistance()
        {
            var state = Create();
            state.TryBegin(Vector3.forward); state.Tick(.8f); state.Advance(2, true);
            Assert.That(state.Phase, Is.EqualTo(EnemyChargePhase.Recovery));
            Assert.That(state.DistanceRemaining, Is.EqualTo(6));
            Assert.That(state.StepDistance(1), Is.Zero);
        }

        [Test]
        public void Cancel_DiscardsTheCommittedAttack()
        {
            var state = Create();
            state.TryBegin(Vector3.forward); state.Tick(.8f); state.TryCommitHit(); state.Cancel();
            Assert.That(state.Phase, Is.EqualTo(EnemyChargePhase.Pursuing));
            Assert.That(state.Direction, Is.EqualTo(Vector3.zero));
            Assert.That(state.TryCommitHit(), Is.False);
            Assert.That(state.DistanceRemaining, Is.Zero);
        }

        [Test]
        public void OutsideActivationRangeOrCoincidentTarget_DoesNotStart()
        {
            var state = Create();
            Assert.That(state.TryBegin(Vector3.forward * 8), Is.False);
            Assert.That(state.TryBegin(Vector3.zero), Is.False);
        }

        [TestCase(0f)] [TestCase(-1f)] [TestCase(float.NaN)] [TestCase(float.PositiveInfinity)]
        public void InvalidConfiguration_IsRejected(float value)
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new EnemyChargeConfig(value, 1, 1, 1, 1, 1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new EnemyChargeConfig(1, value, 1, 1, 1, 1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new EnemyChargeConfig(1, 1, value, 1, 1, 1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new EnemyChargeConfig(1, 1, 1, value, 1, 1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new EnemyChargeConfig(1, 1, 1, 1, value, 1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new EnemyChargeConfig(1, 1, 1, 1, 1, value));
        }

        [Test]
        public void DefaultConfigurationAndInvalidTime_AreRejected()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new EnemyChargeState(default));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => Create().Tick(-1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => Create().StepDistance(float.NaN));
        }

        [Test]
        public void RankAndBehavior_DefaultToNormalChaser_AndUnknownValuesAreRejected()
        {
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            try
            {
                Assert.That(definition.Rank, Is.EqualTo(EnemyRank.Normal));
                Assert.That(definition.Behavior, Is.EqualTo(EnemyBehavior.Chaser));
                definition.ValidateBehavior();
                typeof(EnemyDefinition).GetField("_rank", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(definition, (EnemyRank)99);
                Assert.Throws<System.InvalidOperationException>(() => definition.ValidateBehavior());
                typeof(EnemyDefinition).GetField("_rank", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(definition, EnemyRank.Boss);
                definition.ValidateBehavior();
                typeof(EnemyDefinition).GetField("_behavior", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(definition, (EnemyBehavior)99);
                Assert.Throws<System.InvalidOperationException>(() => definition.ValidateBehavior());
            }
            finally { Object.DestroyImmediate(definition); }
        }
    }
}
