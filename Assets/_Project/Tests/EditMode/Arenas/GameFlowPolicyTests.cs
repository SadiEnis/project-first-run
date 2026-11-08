using System;
using NUnit.Framework;
using ProjectFirstRun.Flow;

namespace ProjectFirstRun.Tests.EditMode.Arenas
{
    public sealed class GameFlowPolicyTests
    {
        private static GameFlowContext Context(GameFlowPhase phase = GameFlowPhase.Playing,
            bool ready = true, bool controls = true, bool closing = false) => new(phase, ready, controls, closing);

        private static GameFlowPolicy.Token Acquire(GameFlowPolicy policy, GameFlowRequest request, GameFlowContext context)
        {
            Assert.That(policy.TryAcquire(new object(), request, context, out var token), Is.True);
            return token;
        }

        [TestCase(GameFlowPhase.EntryMenu, GameFlowRequest.Pause)]
        [TestCase(GameFlowPhase.EntryApproach, GameFlowRequest.Reward)]
        [TestCase(GameFlowPhase.Ending, GameFlowRequest.Loading)]
        [TestCase(GameFlowPhase.Dead, GameFlowRequest.Pause)]
        [TestCase(GameFlowPhase.Completed, GameFlowRequest.Reward)]
        [TestCase(GameFlowPhase.Playing, GameFlowRequest.Death)]
        [TestCase(GameFlowPhase.Playing, GameFlowRequest.Loading)]
        [TestCase(GameFlowPhase.Playing, GameFlowRequest.Completed)]
        public void RejectedRequestDoesNotChangeOwnershipOrAccess(GameFlowPhase phase, GameFlowRequest request)
        {
            var policy = new GameFlowPolicy();
            var context = Context(phase);
            var before = policy.Evaluate(context);
            Assert.That(policy.TryAcquire(new object(), request, context, out var token), Is.False);
            Assert.That(token, Is.Null);
            Assert.That(policy.OwnerCount, Is.Zero);
            Assert.That(policy.Evaluate(context), Is.EqualTo(before));
        }

        [TestCase(GameFlowRequest.Pause, GameFlowRequest.Reward)]
        [TestCase(GameFlowRequest.Reward, GameFlowRequest.Pause)]
        public void PauseAndRewardCannotOverlap(GameFlowRequest first, GameFlowRequest second)
        {
            var policy = new GameFlowPolicy();
            var context = Context();
            using var token = Acquire(policy, first, context);
            Assert.That(policy.CanAcquire(second, context), Is.False);
            Assert.That(policy.TryAcquire(new object(), second, context, out _), Is.False);
            Assert.That(policy.OwnerCount, Is.EqualTo(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ReleasingOwnersInEitherOrderKeepsRemainingPause(bool reverse)
        {
            var policy = new GameFlowPolicy();
            var context = Context();
            var a = Acquire(policy, GameFlowRequest.Pause, context);
            var b = Acquire(policy, GameFlowRequest.Pause, context);
            var first = reverse ? b : a;
            var last = reverse ? a : b;
            first.Dispose(); first.Dispose();
            Assert.That(first.IsActive, Is.False);
            Assert.That(last.IsActive, Is.True);
            Assert.That(policy.OwnerCount, Is.EqualTo(1));
            var access = policy.Evaluate(context);
            Assert.That(access.FreezeWorld && access.MuteAudio && access.BlockGameplay, Is.True);
            Assert.That(access.Cursor, Is.EqualTo(GameFlowCursor.FreeVisible));
            last.Dispose();
            access = policy.Evaluate(context);
            Assert.That(access.FreezeWorld || access.MuteAudio || access.BlockGameplay, Is.False);
            Assert.That(access.AllowsGameplay, Is.True);
            Assert.That(access.Cursor, Is.EqualTo(GameFlowCursor.LockedHidden));
        }

        [Test]
        public void LoadingFailureReleasesOnlyLoadingAndCanRetry()
        {
            var policy = new GameFlowPolicy();
            var context = Context();
            using var pause = Acquire(policy, GameFlowRequest.Pause, context);
            var before = policy.Evaluate(context);
            var load = Acquire(policy, GameFlowRequest.Loading, context);
            var loading = policy.Evaluate(context);
            Assert.That(loading.BlockScreenInteraction, Is.True);
            Assert.That(loading.Cursor, Is.EqualTo(before.Cursor));
            foreach (GameFlowRequest request in Enum.GetValues(typeof(GameFlowRequest)))
                Assert.That(policy.CanAcquire(request, context), Is.False, request.ToString());
            load.Dispose();
            Assert.That(policy.Evaluate(context), Is.EqualTo(before));
            using var retry = Acquire(policy, GameFlowRequest.Loading, context);
            pause.Dispose();
            Assert.That(policy.Evaluate(context).FreezeWorld, Is.True);
            Assert.That(policy.Evaluate(context).MuteAudio, Is.True);
            Assert.That(policy.Evaluate(context).Cursor, Is.EqualTo(before.Cursor));
        }

        [Test]
        public void RewardClosingAfterDeathNeverEnablesPlayer()
        {
            var policy = new GameFlowPolicy();
            var reward = Acquire(policy, GameFlowRequest.Reward, Context());
            var dead = Context(GameFlowPhase.Dead, controls: false);
            using var death = Acquire(policy, GameFlowRequest.Death, dead);
            Assert.That(policy.Evaluate(dead).FreezeWorld, Is.True, "Reward still owns its freeze until dismissed.");
            reward.Dispose();
            var access = policy.Evaluate(dead);
            Assert.That(access.FreezeWorld || access.MuteAudio, Is.False, "Death preserves the current running-world behavior.");
            Assert.That(access.AllowsGameplay, Is.False);
            Assert.That(access.Cursor, Is.EqualTo(GameFlowCursor.FreeVisible));
            using var loading = Acquire(policy, GameFlowRequest.Loading, dead);
            Assert.That(policy.Evaluate(dead).FreezeWorld, Is.True);
        }

        [Test]
        public void ClosingUiDoesNotOverrideDisabledBaseControl()
        {
            var policy = new GameFlowPolicy();
            var context = Context(controls: false);
            var token = Acquire(policy, GameFlowRequest.Reward, context);
            token.Dispose();
            var access = policy.Evaluate(context);
            Assert.That(access.BlockGameplay, Is.False, "The UI contributes no remaining lock.");
            Assert.That(access.AllowsGameplay, Is.False, "The caller's base control remains disabled.");
        }

        [TestCase(GameFlowRequest.EntryMenu, GameFlowRequest.EntryApproach, GameFlowPhase.EntryMenu, GameFlowPhase.EntryApproach, GameFlowCursor.FreeHidden)]
        [TestCase(GameFlowRequest.Ending, GameFlowRequest.Completed, GameFlowPhase.Ending, GameFlowPhase.Completed, GameFlowCursor.FreeVisible)]
        public void CinematicChangesKeepOwnershipAndFreeze(GameFlowRequest from, GameFlowRequest to,
            GameFlowPhase before, GameFlowPhase after, GameFlowCursor cursor)
        {
            var policy = new GameFlowPolicy();
            using var token = Acquire(policy, from, Context(before));
            Assert.That(policy.TryChange(token, to, Context(before)), Is.False);
            Assert.That(policy.TryChange(token, to, Context(after)), Is.True);
            Assert.That(policy.OwnerCount, Is.EqualTo(1));
            var access = policy.Evaluate(Context(after));
            Assert.That(access.FreezeWorld && access.BlockGameplay, Is.True);
            Assert.That(access.MuteAudio, Is.False, "Cinematic audio must remain audible.");
            Assert.That(access.Cursor, Is.EqualTo(cursor));
        }

        [Test]
        public void ResetInvalidatesOldTokensWithoutAffectingNewRun()
        {
            var policy = new GameFlowPolicy();
            var owner = new object();
            Assert.That(policy.TryAcquire(owner, GameFlowRequest.Pause, Context(), out var old), Is.True);
            policy.Reset();
            Assert.That(old.IsActive, Is.False);
            Assert.That(policy.TryAcquire(owner, GameFlowRequest.Pause, Context(), out var current), Is.True);
            old.Dispose();
            Assert.That(policy.TryChange(old, GameFlowRequest.Loading, Context()), Is.False);
            Assert.That(current.IsActive, Is.True);
            Assert.That(new GameFlowPolicy().Release(current), Is.False);
            Assert.That(policy.OwnerCount, Is.EqualTo(1));
        }

        private sealed class EqualOwner
        {
            public override bool Equals(object obj) => obj is EqualOwner;
            public override int GetHashCode() => 0;
        }

        [TestCase(GameFlowPhase.Dead, GameFlowRequest.Death)]
        [TestCase(GameFlowPhase.Completed, GameFlowRequest.Completed)]
        [TestCase(GameFlowPhase.EncounterFailed, GameFlowRequest.Failure)]
        public void TerminalRestartRequiresItsVisibleScreenOwner(GameFlowPhase phase, GameFlowRequest screen)
        {
            var policy = new GameFlowPolicy();
            var context = Context(phase, controls: false);
            Assert.That(policy.CanAcquire(GameFlowRequest.Loading, context), Is.False);
            using var terminal = Acquire(policy, screen, context);
            using var load = Acquire(policy, GameFlowRequest.Loading, context);
            Assert.That(policy.Evaluate(context).BlockScreenInteraction, Is.True);
            Assert.That(policy.Evaluate(context).Cursor, Is.EqualTo(GameFlowCursor.FreeVisible));
        }

        [Test]
        public void EntryCanBlockBeforeReadyAndInvalidRequestsAreRejected()
        {
            var policy = new GameFlowPolicy();
            var context = Context(GameFlowPhase.EntryMenu, ready: false);
            using var menu = Acquire(policy, GameFlowRequest.EntryMenu, context);
            Assert.That(policy.Evaluate(context).FreezeWorld, Is.True);
            Assert.That(policy.Evaluate(context).MuteAudio, Is.False);
            Assert.That(policy.CanAcquire((GameFlowRequest)999, context), Is.False);
            Assert.That(policy.CanAcquire(GameFlowRequest.Pause, Context((GameFlowPhase)999)), Is.False);
            Assert.That(policy.TryChange(menu, GameFlowRequest.Pause, Context()), Is.False);
            Assert.That(policy.OwnerCount, Is.EqualTo(1));
        }

        [Test]
        public void RewardDoesNotMakePlayerImmortalButPauseAndFinaleProtectPlayer()
        {
            var policy = new GameFlowPolicy();
            var reward = Acquire(policy, GameFlowRequest.Reward, Context());
            Assert.That(policy.Evaluate(Context()).ProtectPlayer, Is.False);
            reward.Dispose();
            var pause = Acquire(policy, GameFlowRequest.Pause, Context());
            Assert.That(policy.Evaluate(Context()).ProtectPlayer, Is.True);
            pause.Dispose();
            using var ending = Acquire(policy, GameFlowRequest.Ending, Context(GameFlowPhase.Ending));
            Assert.That(policy.Evaluate(Context(GameFlowPhase.Ending)).ProtectPlayer, Is.True);
        }

        [Test]
        public void OwnershipUsesReferenceIdentityAndRejectsDuplicateOwner()
        {
            var policy = new GameFlowPolicy();
            var owner = new EqualOwner();
            Assert.That(policy.TryAcquire(owner, GameFlowRequest.Pause, Context(), out _), Is.True);
            Assert.That(policy.TryAcquire(owner, GameFlowRequest.Pause, Context(), out _), Is.False);
            Assert.That(policy.TryAcquire(new EqualOwner(), GameFlowRequest.Pause, Context(), out _), Is.True);
            Assert.That(policy.OwnerCount, Is.EqualTo(2));
            Assert.Throws<ArgumentNullException>(() => policy.TryAcquire(null, GameFlowRequest.Pause, Context(), out _));
        }

        [Test]
        public void NotReadyAndClosingPauseRejectGameplayModalsAndRestart()
        {
            var policy = new GameFlowPolicy();
            foreach (var request in new[] { GameFlowRequest.Pause, GameFlowRequest.Reward, GameFlowRequest.Loading })
                Assert.That(policy.CanAcquire(request, Context(ready: false)), Is.False);
            using var pause = Acquire(policy, GameFlowRequest.Pause, Context());
            Assert.That(policy.CanAcquire(GameFlowRequest.Loading, Context(closing: true)), Is.False);
            Assert.That(policy.CanAcquire(GameFlowRequest.Reward, Context(closing: true)), Is.False);
            Assert.That(policy.Evaluate(Context(closing: true)).FreezeWorld, Is.True);
        }
    }
}
