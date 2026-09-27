using System;
using UnityEngine;
namespace ProjectFirstRun.Abilities.SniperBomb
{
    [Serializable]
    public sealed class SniperLevelData
    {
        [SerializeField] private float _damage = 50, _radius = 2, _cooldown = 6;
        [SerializeField] private int _count = 1;
        [SerializeField] private bool _retarget;
        public SniperConfig CreateConfig() => new SniperConfig(_damage, _radius, _cooldown, _count, _retarget);
    }
    public readonly struct SniperConfig
    {
        public float Damage { get; }
        public float Radius { get; }
        public int Count { get; }
        public bool Retarget { get; }
        public AbilityRuntimeConfig Ability { get; }
        public SniperConfig(float damage, float radius, float cooldown, int count, bool retarget)
        {
            if (!float.IsFinite(damage) || damage <= 0 || !float.IsFinite(radius) || radius <= 0 || count < 1 || count > 2)
                throw new ArgumentException("Invalid sniper bomb configuration.");
            Damage = damage; Radius = radius; Count = count; Retarget = retarget; Ability = new AbilityRuntimeConfig(cooldown);
        }
    }
    [CreateAssetMenu(fileName = "AD_SniperBomb", menuName = "Project First Run/Items/Ability/Sniper Bomb")]
    public sealed class SniperDefinition : AbilityDefinition
    {
        [SerializeField] private SniperLevelData[] _levels = Array.Empty<SniperLevelData>();
        [SerializeField] private float _range = 20, _speed = 10, _projectileRadius = .2f, _lifetime = 5;
        [SerializeField] private LayerMask _worldMask = 129;
        [SerializeField] private Material _material;
        public float Range => _range;
        public float Speed => _speed;
        public float ProjectileRadius => _projectileRadius;
        public float Lifetime => _lifetime;
        public int WorldMask => _worldMask;
        public Material Material => _material;
        public SniperConfig[] CreateLevelConfigs()
        {
            ValidateLevelConfiguration();
            if (_levels == null || _levels.Length != MaximumLevel || _material == null ||
                !Positive(_range) || !Positive(_speed) || !Positive(_projectileRadius) || !Positive(_lifetime))
                throw new InvalidOperationException("Assign sniper bomb levels, geometry and material.");
            var result = new SniperConfig[_levels.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = (_levels[i] ?? throw new InvalidOperationException("Missing level.")).CreateConfig();
            return result;
        }
        private static bool Positive(float value) => float.IsFinite(value) && value > 0;
    }
}
