using NUnit.Framework;
using ProjectFirstRun.Abilities;

namespace ProjectFirstRun.Tests.EditMode.Abilities
{
    public sealed class AbilityRuntimeStateTests
    {
        [Test]
        public void Constructor_CreatesReadyState()
        {
            AbilityRuntimeState state =
                CreateState(3f);

            Assert.That(
                state.Cooldown,
                Is.EqualTo(3f));

            Assert.That(
                state.CooldownRemaining,
                Is.EqualTo(0f));

            Assert.That(
                state.IsReady,
                Is.True);
        }

        [Test]
        public void TryCommitCast_WhenReady_PerformsCast()
        {
            AbilityRuntimeState state =
                CreateState(3f);

            AbilityCastResult result =
                state.TryCommitCast();

            Assert.That(
                result,
                Is.EqualTo(
                    AbilityCastResult.Performed));

            Assert.That(
                state.IsReady,
                Is.False);

            Assert.That(
                state.CooldownRemaining,
                Is.EqualTo(3f));
        }

        [Test]
        public void TryCommitCast_WhileCoolingDown_ReturnsOnCooldown()
        {
            AbilityRuntimeState state =
                CreateState(3f);

            state.TryCommitCast();

            state.Tick(1f);

            AbilityCastResult result =
                state.TryCommitCast();

            Assert.That(
                result,
                Is.EqualTo(
                    AbilityCastResult.OnCooldown));

            Assert.That(
                state.CooldownRemaining,
                Is.EqualTo(2f));
        }

        [Test]
        public void Tick_WhileCoolingDown_ReducesRemainingCooldown()
        {
            AbilityRuntimeState state =
                CreateState(3f);

            state.TryCommitCast();

            state.Tick(1.25f);

            Assert.That(
                state.CooldownRemaining,
                Is.EqualTo(1.75f).Within(0.0001f));

            Assert.That(
                state.IsReady,
                Is.False);
        }

        [Test]
        public void Tick_WhenCooldownExpires_ClampsToZero()
        {
            AbilityRuntimeState state =
                CreateState(2f);

            state.TryCommitCast();

            state.Tick(5f);

            Assert.That(
                state.CooldownRemaining,
                Is.EqualTo(0f));

            Assert.That(
                state.IsReady,
                Is.True);
        }

        [Test]
        public void Tick_WhenAlreadyReady_RemainsReady()
        {
            AbilityRuntimeState state =
                CreateState(2f);

            state.Tick(1f);

            Assert.That(
                state.CooldownRemaining,
                Is.EqualTo(0f));

            Assert.That(
                state.IsReady,
                Is.True);
        }

        [Test]
        public void Tick_WithZeroDeltaTime_DoesNotChangeState()
        {
            AbilityRuntimeState state =
                CreateState(2f);

            state.TryCommitCast();

            state.Tick(0f);

            Assert.That(
                state.CooldownRemaining,
                Is.EqualTo(2f));
        }

        [Test]
        public void Tick_WithNegativeDeltaTime_Throws()
        {
            AbilityRuntimeState state =
                CreateState(2f);

            Assert.That(
                () => state.Tick(-0.1f),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Tick_WithNaNDeltaTime_Throws()
        {
            AbilityRuntimeState state =
                CreateState(2f);

            Assert.That(
                () => state.Tick(float.NaN),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Tick_WithInfiniteDeltaTime_Throws()
        {
            AbilityRuntimeState state =
                CreateState(2f);

            Assert.That(
                () =>
                    state.Tick(
                        float.PositiveInfinity),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void CompletedCooldown_AllowsAnotherCast()
        {
            AbilityRuntimeState state =
                CreateState(2f);

            AbilityCastResult firstCast =
                state.TryCommitCast();

            state.Tick(2f);

            AbilityCastResult secondCast =
                state.TryCommitCast();

            Assert.That(
                firstCast,
                Is.EqualTo(
                    AbilityCastResult.Performed));

            Assert.That(
                secondCast,
                Is.EqualTo(
                    AbilityCastResult.Performed));

            Assert.That(
                state.CooldownRemaining,
                Is.EqualTo(2f));
        }

        private static AbilityRuntimeState CreateState(
            float cooldown)
        {
            AbilityRuntimeConfig config =
                new AbilityRuntimeConfig(
                    cooldown);

            return new AbilityRuntimeState(
                in config);
        }
    }
}