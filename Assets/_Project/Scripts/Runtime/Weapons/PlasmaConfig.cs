using System;
using UnityEngine;

namespace ProjectFirstRun.Weapons
{
    [Serializable]
    public sealed class PlasmaLevelData
    {
        [SerializeField] private float _speed = 60;
        [SerializeField] private float _radius = .08f;
        [SerializeField] private float _lifetime = 2;
        [SerializeField] private int _pierceCount;
        [SerializeField] private bool _burn;
        [SerializeField] private float _burnDamage = 5;
        public PlasmaConfig CreateConfig() => new PlasmaConfig(_speed, _radius, _lifetime, _pierceCount, _burn, _burnDamage);
    }

    public readonly struct PlasmaConfig
    {
        public float Speed { get; }
        public float Radius { get; }
        public float Lifetime { get; }
        public int PierceCount { get; }
        public bool Burn { get; }
        public float BurnDamage { get; }
        public PlasmaConfig(float speed, float radius = .08f, float lifetime = 2,
            int pierceCount = 0, bool burn = false, float burnDamage = 5)
        {
            RocketConfig.Positive(speed, nameof(speed)); RocketConfig.Positive(radius, nameof(radius));
            RocketConfig.Positive(lifetime, nameof(lifetime)); RocketConfig.Positive(burnDamage, nameof(burnDamage));
            if (pierceCount < 0 || pierceCount > 2) throw new ArgumentOutOfRangeException(nameof(pierceCount));
            Speed = speed; Radius = radius; Lifetime = lifetime; PierceCount = pierceCount;
            Burn = burn; BurnDamage = burnDamage;
        }
        public void Validate() => _ = new PlasmaConfig(Speed, Radius, Lifetime, PierceCount, Burn, BurnDamage);
    }

    /// <summary>One non-stacking burn. Refresh preserves tick phase; catch-up is bounded by six ticks.</summary>
    public sealed class PlasmaBurnState : ProjectFirstRun.Combat.TimedBurnState
    {
        public const float Duration = 3;
        public void Refresh(float damage) => Refresh(damage, Duration);
    }
}
