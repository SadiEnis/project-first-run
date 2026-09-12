using System;
using System.Collections.Generic;
using ProjectFirstRun.Items;

namespace ProjectFirstRun.Rewards.Claims
{
    public sealed class RewardClaimHandlerRegistry
    {
        private readonly List<IRewardClaimHandler> _handlers =
            new List<IRewardClaimHandler>();

        public int HandlerCount =>
            _handlers.Count;

        public void Register(
            IRewardClaimHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(
                    nameof(handler));
            }

            if (_handlers.Contains(
                    handler))
            {
                throw new InvalidOperationException(
                    "The same reward claim handler " +
                    "has already been registered.");
            }

            _handlers.Add(
                handler);
        }

        public IRewardClaimHandler Resolve(
            ItemDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            IRewardClaimHandler matchedHandler =
                null;

            for (int index = 0;
                 index < _handlers.Count;
                 index++)
            {
                IRewardClaimHandler handler =
                    _handlers[index];

                if (!handler.Supports(
                        definition))
                {
                    continue;
                }

                if (matchedHandler != null)
                {
                    throw new InvalidOperationException(
                        $"Multiple reward claim handlers " +
                        $"support item " +
                        $"'{definition.Category}:{definition.StableId}'.");
                }

                matchedHandler =
                    handler;
            }

            if (matchedHandler == null)
            {
                throw new InvalidOperationException(
                    $"No reward claim handler supports item " +
                    $"'{definition.Category}:{definition.StableId}'.");
            }

            return matchedHandler;
        }
    }
}