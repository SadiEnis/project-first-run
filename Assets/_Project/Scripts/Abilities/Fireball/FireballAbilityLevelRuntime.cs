using System;
using ProjectFirstRun.Abilities;

namespace ProjectFirstRun.Abilities.Fireball
{
    internal sealed class FireballAbilityLevelRuntime : IAbilityLevelRuntime
    {
        private readonly FireballRuntimeConfig[] _levels;
        private readonly FireballAbilityExecutor _executor;

        public int Level { get; private set; } = 1;
        public int MaximumLevel => _levels.Length;
        public AbilityRuntimeConfig InitialConfig => _levels[0].Ability;

        public FireballAbilityLevelRuntime(FireballRuntimeConfig[] levels, FireballAbilityExecutor executor)
        {
            _levels = levels ?? throw new ArgumentNullException(nameof(levels));
            if (_levels.Length == 0) throw new InvalidOperationException("Fireball requires at least one level.");
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        }

        public void ValidateNextLevel(AbilityRuntimeState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (Level >= MaximumLevel) throw new InvalidOperationException("Fireball is already at maximum level.");
            state.ValidateConfiguration(_levels[Level].Ability);
        }

        public void AdvanceLevel(AbilityRuntimeState state)
        {
            ValidateNextLevel(state);
            FireballRuntimeConfig config = _levels[Level];
            state.ApplyConfiguration(config.Ability);
            _executor.ApplyConfiguration(config);
            Level++;
        }
    }
}
