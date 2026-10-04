using System;
using UnityEngine;
namespace ProjectFirstRun.Abilities.Drone
{
    [Serializable]
    public sealed class DroneLevelData
    {
        [SerializeField] private float _damage = 8, _rate = 2, _range = 12, _bossMultiplier = 1;
        [SerializeField] private int _count = 1;
        public DroneConfig CreateConfig() => new DroneConfig(_damage, _rate, _range, _count, _bossMultiplier);
    }
    public readonly struct DroneConfig
    {
        public float Damage { get; }
        public float Rate { get; }
        public float Range { get; }
        public float BossMultiplier { get; }
        public int Count { get; }
        public DroneConfig(float damage, float rate, float range, int count, float bossMultiplier)
        {
            if (!float.IsFinite(damage) || damage <= 0 || !float.IsFinite(rate) || rate <= 0 ||
                !float.IsFinite(range) || range <= 0 || !float.IsFinite(bossMultiplier) || bossMultiplier < 1 ||
                count < 1 || count > 2) throw new ArgumentException("Invalid drone configuration.");
            Damage = damage; Rate = rate; Range = range; Count = count; BossMultiplier = bossMultiplier;
        }
    }
    [CreateAssetMenu(fileName = "AD_Drone", menuName = "Project First Run/Items/Ability/Drone")]
    public sealed class DroneDefinition : AbilityDefinition
    {
        [SerializeField] private DroneLevelData[] _levels = Array.Empty<DroneLevelData>();
        [SerializeField] private Material _material;
        [SerializeField] private LayerMask _worldMask = 129, _hitMask = 247;
        public Material Material => _material;
        public int WorldMask => _worldMask;
        public int HitMask => _hitMask;
        public DroneConfig[] CreateLevelConfigs()
        {
            ValidateLevelConfiguration();
            if (_levels == null || _levels.Length != MaximumLevel) throw new InvalidOperationException("Drone requires one config per level.");
            var result = new DroneConfig[_levels.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = (_levels[i] ?? throw new InvalidOperationException("Missing drone level.")).CreateConfig();
            if (_material == null) throw new InvalidOperationException("Drone requires a presentation material.");
            return result;
        }
    }
}
