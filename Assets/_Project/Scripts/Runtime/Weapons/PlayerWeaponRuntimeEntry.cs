using System;

namespace ProjectFirstRun.Weapons
{
    public sealed class PlayerWeaponRuntimeEntry
    {
        private readonly WeaponLevelConfig[] _levels;
        public int Level { get; private set; } = 1;
        public int MaximumLevel => _levels.Length;
        public float BaseDamage => _levels[Level - 1].Damage;

        internal void ValidateNextLevel()
        {
            if (Level >= MaximumLevel) throw new InvalidOperationException("Weapon is already at maximum level.");
            RuntimeState.ValidateConfiguration(_levels[Level].Runtime);
        }

        internal void AdvanceLevel()
        {
            RuntimeState.ApplyConfiguration(_levels[Level].Runtime);
            Level++;
        }

        public WeaponDefinition Definition
        {
            get;
        }

        public WeaponRuntimeState RuntimeState
        {
            get;
        }

        public PlayerWeaponRuntimeEntry(
            WeaponDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(
                    nameof(definition));
            }

            _levels = definition.CreateLevelConfigs();
            WeaponRuntimeConfig runtimeConfig = _levels[0].Runtime;

            Definition =
                definition;

            RuntimeState =
                new WeaponRuntimeState(
                    runtimeConfig);
        }
    }
}
