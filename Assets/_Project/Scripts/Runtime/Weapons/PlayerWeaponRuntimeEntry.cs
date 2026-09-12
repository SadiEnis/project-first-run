using System;

namespace ProjectFirstRun.Weapons
{
    public sealed class PlayerWeaponRuntimeEntry
    {
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

            WeaponRuntimeConfig runtimeConfig =
                definition.CreateRuntimeConfig();

            Definition =
                definition;

            RuntimeState =
                new WeaponRuntimeState(
                    runtimeConfig);
        }
    }
}