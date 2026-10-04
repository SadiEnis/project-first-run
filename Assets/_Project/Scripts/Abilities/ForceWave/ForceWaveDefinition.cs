using System;
using UnityEngine;

namespace ProjectFirstRun.Abilities.ForceWave
{
    [Serializable]
    public sealed class ForceWaveLevelData
    {
        [SerializeField] private float _damage = 20, _reach = 4, _cooldown = 5, _push = 2;
        public ForceWaveConfig CreateConfig() => new ForceWaveConfig(_damage, _reach, _cooldown, _push);
    }

    public readonly struct ForceWaveConfig
    {
        public float Damage { get; }
        public float Reach { get; }
        public float Push { get; }
        public AbilityRuntimeConfig Ability { get; }
        public ForceWaveConfig(float damage, float reach, float cooldown, float push)
        {
            Positive(damage); Positive(reach); Positive(push);
            Damage = damage; Reach = reach; Push = push; Ability = new AbilityRuntimeConfig(cooldown);
        }
        private static void Positive(float value)
        {
            if (!float.IsFinite(value) || value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    [CreateAssetMenu(fileName = "AD_ForceWave", menuName = "Project First Run/Items/Ability/Force Wave")]
    public sealed class ForceWaveDefinition : AbilityDefinition
    {
        [SerializeField] private float _damage = 20, _reach = 4, _push = 2;
        [SerializeField, Range(1, 180)] private float _angle = 100;
        [SerializeField, Min(.01f)] private float _heightTolerance = 2;
        [SerializeField] private LayerMask _worldMask = 129;
        [SerializeField] private Material _visualMaterial;
        [SerializeField] private ForceWaveLevelData[] _additionalLevels = Array.Empty<ForceWaveLevelData>();
        public float Angle => _angle;
        public float HeightTolerance => _heightTolerance;
        public int WorldMask => _worldMask;
        public Material VisualMaterial => _visualMaterial;
        public ForceWaveConfig[] CreateLevelConfigs()
        {
            ValidateLevelConfiguration();
            if (!float.IsFinite(_angle) || _angle <= 0 || _angle > 180 ||
                !float.IsFinite(_heightTolerance) || _heightTolerance <= 0)
                throw new InvalidOperationException("Invalid Force Wave sector.");
            if (_additionalLevels == null || _additionalLevels.Length != MaximumLevel - 1)
                throw new InvalidOperationException("Force Wave level data must match maximum level.");
            var levels = new ForceWaveConfig[MaximumLevel];
            levels[0] = new ForceWaveConfig(_damage, _reach, Cooldown, _push);
            for (int i = 1; i < levels.Length; i++)
                levels[i] = (_additionalLevels[i - 1] ?? throw new InvalidOperationException("Missing Force Wave level.")).CreateConfig();
            return levels;
        }
    }
}
