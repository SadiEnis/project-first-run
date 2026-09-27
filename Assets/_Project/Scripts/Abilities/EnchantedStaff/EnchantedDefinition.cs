using System;
using UnityEngine;
namespace ProjectFirstRun.Abilities.EnchantedStaff
{
    [Serializable]
    public sealed class EnchantedLevelData
    {
        [SerializeField] private float _damage = 20, _lifetime = 2, _cooldown = 5;
        [SerializeField] private int _count = 1;
        public EnchantedConfig CreateConfig() => new EnchantedConfig(_damage, _lifetime, _cooldown, _count);
    }
    public readonly struct EnchantedConfig
    {
        public float Damage { get; }
        public float Lifetime { get; }
        public int Count { get; }
        public AbilityRuntimeConfig Ability { get; }
        public EnchantedConfig(float damage, float lifetime, float cooldown, int count)
        {
            if (!float.IsFinite(damage) || damage <= 0 || !float.IsFinite(lifetime) || lifetime <= 0 || count < 1 || count > 2)
                throw new ArgumentException("Invalid enchanted beam configuration.");
            Damage = damage; Lifetime = lifetime; Count = count; Ability = new AbilityRuntimeConfig(cooldown);
        }
    }
    [CreateAssetMenu(fileName = "AD_EnchantedStaff", menuName = "Project First Run/Items/Ability/Enchanted Staff")]
    public sealed class EnchantedDefinition : AbilityDefinition
    {
        [SerializeField] private EnchantedLevelData[] _levels = Array.Empty<EnchantedLevelData>();
        [SerializeField] private float _speed = 8, _radius = .15f;
        [SerializeField] private LayerMask _worldMask = 129;
        [SerializeField] private Material _material;
        public float Speed => _speed;
        public float Radius => _radius;
        public int WorldMask => _worldMask;
        public Material Material => _material;
        public EnchantedConfig[] CreateLevelConfigs()
        {
            ValidateLevelConfiguration();
            if (_levels == null || _levels.Length != MaximumLevel || _material == null ||
                !float.IsFinite(_speed) || _speed <= 0 || !float.IsFinite(_radius) || _radius <= 0)
                throw new InvalidOperationException("Assign enchanted beam levels, speed, radius and material.");
            var result = new EnchantedConfig[_levels.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = (_levels[i] ?? throw new InvalidOperationException("Missing level.")).CreateConfig();
            return result;
        }
    }
}
