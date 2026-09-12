using System;
using NUnit.Framework;
using ProjectFirstRun.Enemies;

namespace ProjectFirstRun.Tests.EditMode.Enemies
{
    public sealed class EnemyAttackStateTests
    {
        private EnemyAttackConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = new EnemyAttackConfig(
                damage: 10f,
                range: 1.75f,
                cooldownDuration: 1f);
        }

        [Test]
        public void Constructor_StartsReady()
        {
            EnemyAttackState state =
                new EnemyAttackState(in _config);

            Assert.That(state.IsReady, Is.True);
            Assert.That(
                state.CooldownRemaining,
                Is.EqualTo(0f));

            Assert.That(state.Damage, Is.EqualTo(10f));
            Assert.That(state.Range, Is.EqualTo(1.75f));
        }

        [Test]
        public void TryCommitAttack_WhenTargetIsInRange_PerformsAttack()
        {
            EnemyAttackState state =
                new EnemyAttackState(in _config);

            EnemyAttackAttemptResult result =
                state.TryCommitAttack(true);

            Assert.That(
                result,
                Is.EqualTo(EnemyAttackAttemptResult.Performed));

            Assert.That(state.IsReady, Is.False);

            Assert.That(
                state.CooldownRemaining,
                Is.EqualTo(1f));
        }

        [Test]
        public void TryCommitAttack_DuringCooldown_IsRejected()
        {
            EnemyAttackState state =
                new EnemyAttackState(in _config);

            state.TryCommitAttack(true);

            EnemyAttackAttemptResult result =
                state.TryCommitAttack(true);

            Assert.That(
                result,
                Is.EqualTo(
                    EnemyAttackAttemptResult.BlockedByCooldown));

            Assert.That(
                state.CooldownRemaining,
                Is.EqualTo(1f));
        }

        [Test]
        public void TryCommitAttack_WhenTargetIsOutOfRange_DoesNotStartCooldown()
        {
            EnemyAttackState state =
                new EnemyAttackState(in _config);

            EnemyAttackAttemptResult result =
                state.TryCommitAttack(false);

            Assert.That(
                result,
                Is.EqualTo(
                    EnemyAttackAttemptResult.TargetOutOfRange));

            Assert.That(state.IsReady, Is.True);
            Assert.That(
                state.CooldownRemaining,
                Is.EqualTo(0f));
        }

        [Test]
        public void Tick_BeforeCooldownCompletes_RemainsBlocked()
        {
            EnemyAttackState state =
                new EnemyAttackState(in _config);

            state.TryCommitAttack(true);
            state.Tick(0.4f);

            Assert.That(state.IsReady, Is.False);

            Assert.That(
                state.CooldownRemaining,
                Is.EqualTo(0.6f).Within(0.0001f));

            Assert.That(
                state.TryCommitAttack(true),
                Is.EqualTo(
                    EnemyAttackAttemptResult.BlockedByCooldown));
        }

        [Test]
        public void Tick_WhenCooldownCompletes_AllowsAnotherAttack()
        {
            EnemyAttackState state =
                new EnemyAttackState(in _config);

            state.TryCommitAttack(true);
            state.Tick(1f);

            Assert.That(state.IsReady, Is.True);
            Assert.That(
                state.CooldownRemaining,
                Is.EqualTo(0f));

            Assert.That(
                state.TryCommitAttack(true),
                Is.EqualTo(
                    EnemyAttackAttemptResult.Performed));
        }

        [Test]
        public void Reset_ClearsCooldown()
        {
            EnemyAttackState state =
                new EnemyAttackState(in _config);

            state.TryCommitAttack(true);
            state.Reset();

            Assert.That(state.IsReady, Is.True);
            Assert.That(
                state.CooldownRemaining,
                Is.EqualTo(0f));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Tick_WithInvalidDeltaTime_Throws(
            float deltaTime)
        {
            EnemyAttackState state =
                new EnemyAttackState(in _config);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => state.Tick(deltaTime));
        }
    }
}