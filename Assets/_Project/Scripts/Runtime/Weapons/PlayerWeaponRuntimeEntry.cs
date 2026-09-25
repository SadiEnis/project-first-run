using System;

namespace ProjectFirstRun.Weapons
{
    public sealed class PlayerWeaponRuntimeEntry
    {
        private readonly WeaponLevelConfig[] _levels;
        public int Level { get; private set; } = 1;
        public int MaximumLevel => _levels.Length;
        public float BaseDamage => _levels[Level - 1].Damage;
        public WeaponShotConfig Shot => _levels[Level - 1].Shot;
        public WeaponFireProfile Fire => _levels[Level - 1].Fire;
        public float ShotsPerSecond => _levels[Level - 1].Runtime.ShotsPerSecond;
        public WeaponTriggerMode TriggerMode { get; }
        public float Range { get; }
        public int DamageMask { get; }
        public WeaponDeliveryMode DeliveryMode { get; }
        public RocketProjectile RocketPrefab { get; }
        public RocketConfig? Rocket => _levels[Level - 1].Rocket;
        public PlasmaConfig? Plasma => _levels[Level - 1].Plasma;
        public PlasmaProjectile PlasmaPrefab { get; }

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
            if (!float.IsFinite(definition.Range) || definition.Range <= 0)
                throw new ArgumentOutOfRangeException(nameof(definition), "Weapon range must be finite and positive.");
            Range = definition.Range;
            DamageMask = definition.DamageMask;
            TriggerMode = definition.TriggerMode;
            DeliveryMode = definition.DeliveryMode;
            RocketPrefab = definition.RocketPrefab;
            PlasmaPrefab = definition.PlasmaPrefab;
            WeaponRuntimeConfig runtimeConfig = _levels[0].Runtime;

            Definition =
                definition;

            RuntimeState =
                new WeaponRuntimeState(
                    runtimeConfig);
        }
    }
}
