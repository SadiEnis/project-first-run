using System;
using ProjectFirstRun.Abilities.Execution;
using ProjectFirstRun.Abilities.Targeting;
using UnityEngine;

namespace ProjectFirstRun.Abilities
{
    public sealed class AbilityRuntimeEntry
    {
        private readonly IAbilityTargetSelector _targetSelector;
        private readonly IAbilityExecutor _executor;

        public AbilityDefinition Definition { get; }

        public AbilityRuntimeState State { get; }

        public bool IsReady =>
            State.IsReady;

        public AbilityRuntimeEntry(
            AbilityDefinition definition,
            IAbilityTargetSelector targetSelector,
            IAbilityExecutor executor)
        {
            Definition =
                definition ??
                throw new ArgumentNullException(
                    nameof(definition));

            _executor =
                executor ??
                throw new ArgumentNullException(
                    nameof(executor));

            /*
             * A null target selector is valid.
             *
             * It represents an ability that does not require
             * an external target.
             */
            _targetSelector =
                targetSelector;

            AbilityRuntimeConfig config =
                definition.CreateRuntimeConfig();

            State =
                new AbilityRuntimeState(
                    in config);
        }

        public void Tick(
            float deltaTime)
        {
            State.Tick(
                deltaTime);
        }

        public AbilityAutoCastResult TryAutoCast(
            Vector3 origin)
        {
            ValidateOrigin(
                origin);

            if (!State.IsReady)
            {
                return AbilityAutoCastResult.OnCooldown;
            }

            Transform target = null;

            if (_targetSelector != null)
            {
                bool targetFound =
                    _targetSelector.TrySelectTarget(
                        origin,
                        out target);

                if (!targetFound)
                {
                    return AbilityAutoCastResult.NoTarget;
                }
            }

            AbilityExecutionContext context =
                new AbilityExecutionContext(
                    origin,
                    target);

            AbilityExecutionResult executionResult =
                _executor.TryExecute(
                    in context);

            if (executionResult ==
                AbilityExecutionResult.Failed)
            {
                return AbilityAutoCastResult.ExecutionFailed;
            }

            if (executionResult !=
                AbilityExecutionResult.Performed)
            {
                throw new InvalidOperationException(
                    $"Unsupported ability execution result: " +
                    $"{executionResult}.");
            }

            AbilityCastResult castResult =
                State.TryCommitCast();

            if (castResult !=
                AbilityCastResult.Performed)
            {
                throw new InvalidOperationException(
                    "Ability execution succeeded but its " +
                    "cooldown could not be committed.");
            }

            return AbilityAutoCastResult.Performed;
        }

        private static void ValidateOrigin(
            Vector3 origin)
        {
            if (!IsFinite(origin.x) ||
                !IsFinite(origin.y) ||
                !IsFinite(origin.z))
            {
                throw new ArgumentException(
                    "Ability origin must contain finite values.",
                    nameof(origin));
            }
        }

        private static bool IsFinite(
            float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value);
        }
    }
}