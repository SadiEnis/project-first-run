using System;
using System.Collections.Generic;

namespace ProjectFirstRun.Flow
{
    /// <summary>Scene-local ownership and admission rules. No Unity APIs or persisted health/run state.</summary>
    public sealed class GameFlowPolicy
    {
        public sealed class Token : IDisposable
        {
            private readonly GameFlowPolicy _policy;
            internal readonly object Owner;
            internal GameFlowRequest Request;
            internal readonly GameFlowCursor LoadingCursor;

            internal Token(GameFlowPolicy policy, object owner, GameFlowRequest request, GameFlowCursor cursor)
            {
                _policy = policy;
                Owner = owner;
                Request = request;
                LoadingCursor = cursor;
            }

            public bool IsActive => _policy._tokens.Contains(this);
            public void Dispose() => _policy.Release(this);
        }

        private readonly List<Token> _tokens = new();
        public int OwnerCount => _tokens.Count;

        public bool CanAcquire(GameFlowRequest request, in GameFlowContext context)
        {
            if (!Enum.IsDefined(typeof(GameFlowPhase), context.Phase) || Has(GameFlowRequest.Loading)) return false;
            switch (request)
            {
                case GameFlowRequest.EntryMenu: return context.Phase == GameFlowPhase.EntryMenu;
                case GameFlowRequest.EntryApproach: return context.Phase == GameFlowPhase.EntryApproach;
                case GameFlowRequest.Death: return context.Phase == GameFlowPhase.Dead;
                case GameFlowRequest.Ending: return context.Phase == GameFlowPhase.Ending;
                case GameFlowRequest.Completed: return context.Phase == GameFlowPhase.Completed;
                case GameFlowRequest.Failure: return context.Phase == GameFlowPhase.EncounterFailed;
                case GameFlowRequest.Pause:
                case GameFlowRequest.Reward:
                    return context.IsReady && context.Phase == GameFlowPhase.Playing && !context.PauseClosing &&
                        !HasTerminalOrEntry() && !Has(request == GameFlowRequest.Pause ? GameFlowRequest.Reward : GameFlowRequest.Pause);
                case GameFlowRequest.Loading:
                    if (!context.IsReady || context.PauseClosing || Has(GameFlowRequest.Reward) ||
                        Has(GameFlowRequest.EntryMenu) || Has(GameFlowRequest.EntryApproach)) return false;
                    return (context.Phase == GameFlowPhase.Dead && Has(GameFlowRequest.Death)) ||
                        (context.Phase == GameFlowPhase.Completed && Has(GameFlowRequest.Completed)) ||
                        (context.Phase == GameFlowPhase.EncounterFailed && Has(GameFlowRequest.Failure)) ||
                        (context.Phase == GameFlowPhase.Playing && Has(GameFlowRequest.Pause) && !HasTerminalOrEntry());
                default: return false;
            }
        }

        public bool TryAcquire(object owner, GameFlowRequest request, in GameFlowContext context, out Token token)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            token = null;
            foreach (var active in _tokens)
                if (ReferenceEquals(active.Owner, owner)) return false;
            if (!CanAcquire(request, context)) return false;
            token = new Token(this, owner, request, Evaluate(context).Cursor);
            _tokens.Add(token);
            return true;
        }

        // Keep ownership through cinematic transitions; no release/acquire gap can resume gameplay.
        public bool TryChange(Token token, GameFlowRequest request, in GameFlowContext context)
        {
            if (token == null || !_tokens.Contains(token)) return false;
            bool transition = (token.Request == GameFlowRequest.EntryMenu && request == GameFlowRequest.EntryApproach) ||
                (token.Request == GameFlowRequest.Ending && request == GameFlowRequest.Completed);
            if (!transition || !CanAcquire(request, context)) return false;
            token.Request = request;
            return true;
        }

        public bool Release(Token token) => token != null && _tokens.Remove(token);

        // Tokens are unique references: delayed releases from a previous run cannot remove new owners.
        public void Reset() => _tokens.Clear();

        public GameFlowAccess Evaluate(in GameFlowContext context)
        {
            bool freeze = false, mute = false, loading = false, protect = false;
            bool blocked = !context.IsReady || context.Phase != GameFlowPhase.Playing;
            var cursor = context.Phase == GameFlowPhase.Dead || !context.BaseControlEnabled
                ? GameFlowCursor.FreeVisible : GameFlowCursor.LockedHidden;
            int priority = 0;
            foreach (var token in _tokens)
            {
                blocked = true;
                protect |= token.Request != GameFlowRequest.Death && token.Request != GameFlowRequest.Reward;
                freeze |= token.Request != GameFlowRequest.Death;
                mute |= token.Request == GameFlowRequest.Pause || token.Request == GameFlowRequest.Loading;
                loading |= token.Request == GameFlowRequest.Loading;
                int nextPriority = Priority(token.Request);
                if (nextPriority <= priority) continue;
                priority = nextPriority;
                cursor = token.Request == GameFlowRequest.Loading ? token.LoadingCursor :
                    token.Request == GameFlowRequest.Ending ? GameFlowCursor.LockedHidden :
                    token.Request == GameFlowRequest.EntryApproach ? GameFlowCursor.FreeHidden : GameFlowCursor.FreeVisible;
            }
            return new GameFlowAccess(freeze, mute, blocked, context.BaseControlEnabled, loading, cursor, protect);
        }

        private bool Has(GameFlowRequest request)
        {
            foreach (var token in _tokens) if (token.Request == request) return true;
            return false;
        }

        private bool HasTerminalOrEntry() => Has(GameFlowRequest.EntryMenu) || Has(GameFlowRequest.EntryApproach) ||
            Has(GameFlowRequest.Death) || Has(GameFlowRequest.Ending) || Has(GameFlowRequest.Completed) || Has(GameFlowRequest.Failure);

        private static int Priority(GameFlowRequest request) => request switch
        {
            GameFlowRequest.Loading => 80,
            GameFlowRequest.Death => 70,
            GameFlowRequest.Failure => 65,
            GameFlowRequest.Completed => 60,
            GameFlowRequest.Ending => 50,
            GameFlowRequest.EntryApproach => 40,
            GameFlowRequest.EntryMenu => 30,
            GameFlowRequest.Reward => 20,
            GameFlowRequest.Pause => 10,
            _ => 0
        };
    }
}
