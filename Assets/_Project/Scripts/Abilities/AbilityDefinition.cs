using ProjectFirstRun.Items;
using UnityEngine;

namespace ProjectFirstRun.Abilities
{
    public abstract class AbilityDefinition :
        ItemDefinition
    {
        [Header("Runtime")]
        [SerializeField, Min(0.01f)]
        private float _cooldown = 5f;

        public sealed override ItemCategory Category =>
            ItemCategory.Ability;

        public float Cooldown =>
            _cooldown;

        public AbilityRuntimeConfig CreateRuntimeConfig()
        {
            return new AbilityRuntimeConfig(
                _cooldown);
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (float.IsNaN(_cooldown) ||
                float.IsInfinity(_cooldown) ||
                _cooldown <= 0f)
            {
                _cooldown = 5f;
            }
        }
    }
}