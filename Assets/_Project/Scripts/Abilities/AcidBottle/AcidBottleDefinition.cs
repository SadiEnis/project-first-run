using System;
using UnityEngine;
namespace ProjectFirstRun.Abilities.AcidBottle
{
    [Serializable]
    public sealed class AcidLevelData
    {
        [SerializeField] private float _damage = 5, _duration = 3, _radius = 2, _slow;
        [SerializeField] private int _bottles = 1;
        public AcidConfig CreateConfig() => new AcidConfig(_damage, _duration, _radius, _bottles, _slow);
    }
    public readonly struct AcidConfig
    {
        public float Damage { get; }
        public float Duration { get; }
        public float Radius { get; }
        public float Slow { get; }
        public int Bottles { get; }
        public AcidConfig(float damage, float duration, float radius, int bottles, float slow)
        {
            if (!float.IsFinite(damage) || damage <= 0 || !float.IsFinite(duration) || duration <= 0 ||
                !float.IsFinite(radius) || radius <= 0 || !float.IsFinite(slow) || slow < 0 || slow >= 1 ||
                bottles < 1 || bottles > 2) throw new ArgumentException("Invalid acid configuration.");
            Damage = damage; Duration = duration; Radius = radius; Bottles = bottles; Slow = slow;
        }
    }
    [CreateAssetMenu(fileName = "AD_AcidBottle", menuName = "Project First Run/Items/Ability/Acid Bottle")]
    public sealed class AcidBottleDefinition : AbilityDefinition
    {
        [SerializeField] private AcidLevelData[] _levels = Array.Empty<AcidLevelData>();
        [SerializeField] private float _targetRange = 15;
        [SerializeField] private LayerMask _worldMask = 129;
        [SerializeField] private Material _material;
        public float TargetRange => _targetRange;
        public int WorldMask => _worldMask;
        public Material Material => _material;
        public AcidConfig[] CreateLevelConfigs()
        {
            ValidateLevelConfiguration();
            if (!float.IsFinite(_targetRange) || _targetRange <= 0 || _material == null ||
                _levels == null || _levels.Length != MaximumLevel)
                throw new InvalidOperationException("Assign acid range, material and every level.");
            var result = new AcidConfig[_levels.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = (_levels[i] ?? throw new InvalidOperationException("Missing acid level.")).CreateConfig();
            return result;
        }
    }
}
