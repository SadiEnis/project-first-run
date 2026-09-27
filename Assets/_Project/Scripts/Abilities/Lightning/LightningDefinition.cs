using System;
using UnityEngine;
namespace ProjectFirstRun.Abilities.Lightning
{
    [Serializable]
    public sealed class LightningLevelData
    {
        [SerializeField] private float _damage = 20, _cooldown = 4, _stun;
        [SerializeField] private int _strikes = 1, _chains;
        public LightningConfig CreateConfig() => new LightningConfig(_damage, _cooldown, _strikes, _stun, _chains);
    }
    public readonly struct LightningConfig
    {
        public float Damage { get; }
        public float Stun { get; }
        public int Strikes { get; }
        public int Chains { get; }
        public AbilityRuntimeConfig Ability { get; }
        public LightningConfig(float damage, float cooldown, int strikes, float stun, int chains)
        {
            if (!float.IsFinite(damage) || damage <= 0 || !float.IsFinite(stun) || stun < 0 ||
                strikes < 1 || strikes > 3 || chains < 0 || chains > 1)
                throw new ArgumentException("Invalid lightning configuration.");
            Damage = damage; Stun = stun; Strikes = strikes; Chains = chains;
            Ability = new AbilityRuntimeConfig(cooldown);
        }
    }
    [CreateAssetMenu(fileName = "AD_LightningStaff", menuName = "Project First Run/Items/Ability/Lightning Staff")]
    public sealed class LightningDefinition : AbilityDefinition
    {
        [SerializeField] private LightningLevelData[] _levels = Array.Empty<LightningLevelData>();
        [SerializeField] private float _range = 15, _radius = 2, _chainRange = 4;
        [SerializeField] private LayerMask _worldMask = 129;
        [SerializeField] private Material _material;
        public float Range => _range;
        public float Radius => _radius;
        public float ChainRange => _chainRange;
        public int WorldMask => _worldMask;
        public Material Material => _material;
        public LightningConfig[] CreateLevelConfigs()
        {
            ValidateLevelConfiguration();
            if (_levels == null || _levels.Length != MaximumLevel || _material == null ||
                !Positive(_range) || !Positive(_radius) || !Positive(_chainRange))
                throw new InvalidOperationException("Assign valid lightning geometry, material and every level.");
            var result = new LightningConfig[_levels.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = (_levels[i] ?? throw new InvalidOperationException("Missing lightning level.")).CreateConfig();
            return result;
        }
        private static bool Positive(float value) => float.IsFinite(value) && value > 0;
    }
}
