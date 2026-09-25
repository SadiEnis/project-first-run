using System;
using UnityEngine;

namespace ProjectFirstRun.Weapons
{
    public enum WeaponDeliveryMode { Hitscan = 0, Rocket = 1 }

    [Serializable]
    public sealed class RocketLevelData
    {
        [SerializeField] private float _speed = 25;
        [SerializeField] private float _collisionRadius = .15f;
        [SerializeField] private float _lifetime = 4;
        [SerializeField] private float _blastRadius = 3;
        [SerializeField] private int _fragmentCount;
        [SerializeField] private float _fragmentDamage = 20;
        [SerializeField] private float _fragmentSpeed = 20;
        [SerializeField] private float _fragmentRadius = .05f;
        [SerializeField] private float _fragmentRange = 8;
        [SerializeField] private float _fragmentLifetime = .4f;

        public RocketConfig CreateConfig() => new RocketConfig(_speed, _collisionRadius, _lifetime,
            _blastRadius, _fragmentCount, _fragmentDamage, _fragmentSpeed, _fragmentRadius, _fragmentRange, _fragmentLifetime);
    }

    public readonly struct RocketConfig
    {
        public float Speed { get; }
        public float CollisionRadius { get; }
        public float Lifetime { get; }
        public float BlastRadius { get; }
        public int FragmentCount { get; }
        public float FragmentDamage { get; }
        public float FragmentSpeed { get; }
        public float FragmentRadius { get; }
        public float FragmentRange { get; }
        public float FragmentLifetime { get; }

        public RocketConfig(float speed, float collisionRadius = .15f, float lifetime = 4,
            float blastRadius = 3, int fragmentCount = 0, float fragmentDamage = 20,
            float fragmentSpeed = 20, float fragmentRadius = .05f, float fragmentRange = 8, float fragmentLifetime = .4f)
        {
            Positive(speed, nameof(speed)); Positive(collisionRadius, nameof(collisionRadius));
            Positive(lifetime, nameof(lifetime)); Positive(blastRadius, nameof(blastRadius));
            Positive(fragmentDamage, nameof(fragmentDamage)); Positive(fragmentSpeed, nameof(fragmentSpeed));
            Positive(fragmentRadius, nameof(fragmentRadius)); Positive(fragmentRange, nameof(fragmentRange));
            Positive(fragmentLifetime, nameof(fragmentLifetime));
            if (fragmentCount < 0 || fragmentCount > 8) throw new ArgumentOutOfRangeException(nameof(fragmentCount));
            Speed = speed; CollisionRadius = collisionRadius; Lifetime = lifetime; BlastRadius = blastRadius;
            FragmentCount = fragmentCount; FragmentDamage = fragmentDamage; FragmentSpeed = fragmentSpeed;
            FragmentRadius = fragmentRadius; FragmentRange = fragmentRange; FragmentLifetime = fragmentLifetime;
        }

        public void Validate() => _ = new RocketConfig(Speed, CollisionRadius, Lifetime, BlastRadius,
            FragmentCount, FragmentDamage, FragmentSpeed, FragmentRadius, FragmentRange, FragmentLifetime);

        internal static void Positive(float value, string name)
        {
            if (!float.IsFinite(value) || value <= 0) throw new ArgumentOutOfRangeException(name);
        }
    }
}
