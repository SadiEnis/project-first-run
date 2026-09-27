using System;
using UnityEngine;
namespace ProjectFirstRun.Abilities.Shuriken
{
    [Serializable]
    public sealed class ShurikenLevelData
    {
        [SerializeField] private float _damage = 15, _turnSeconds = 1.5f, _cooldown = 3, _radius = 2.5f, _bleedDamage;
        [SerializeField] private int _count = 1;
        public ShurikenConfig CreateConfig() => new ShurikenConfig(_damage, _turnSeconds, _cooldown, _radius, _count, _bleedDamage);
    }
    public readonly struct ShurikenConfig
    {
        public float Damage { get; }
        public float TurnSeconds { get; }
        public float Cooldown { get; }
        public float Radius { get; }
        public int Count { get; }
        public float BleedDamage { get; }
        public ShurikenConfig(float damage, float turnSeconds, float cooldown, float radius, int count, float bleedDamage)
        {
            if (!Positive(damage) || !Positive(turnSeconds) || !Positive(cooldown) || !Positive(radius) ||
                count < 1 || count > 2 || !float.IsFinite(bleedDamage) || bleedDamage < 0)
                throw new ArgumentException("Invalid shuriken configuration.");
            Damage = damage; TurnSeconds = turnSeconds; Cooldown = cooldown; Radius = radius;
            Count = count; BleedDamage = bleedDamage;
        }
        private static bool Positive(float value) => float.IsFinite(value) && value > 0;
    }
    [CreateAssetMenu(fileName = "AD_Shuriken", menuName = "Project First Run/Items/Ability/Shuriken")]
    public sealed class ShurikenDefinition : AbilityDefinition
    {
        [SerializeField] private ShurikenLevelData[] _levels = Array.Empty<ShurikenLevelData>();
        [SerializeField] private Material _material;
        [SerializeField] private LayerMask _worldMask = 129;
        [SerializeField, Min(.01f)] private float _contactRadius = .3f;
        public Material Material => _material;
        public int WorldMask => _worldMask;
        public float ContactRadius => _contactRadius;
        public ShurikenConfig[] CreateLevelConfigs()
        {
            ValidateLevelConfiguration();
            if (_levels == null || _levels.Length != MaximumLevel || _material == null ||
                !float.IsFinite(_contactRadius) || _contactRadius <= 0)
                throw new InvalidOperationException("Assign valid shuriken levels, material and contact radius.");
            var result = new ShurikenConfig[_levels.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = (_levels[i] ?? throw new InvalidOperationException("Missing shuriken level.")).CreateConfig();
            return result;
        }
    }
}
