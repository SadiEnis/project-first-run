using System;
using ProjectFirstRun.Combat;
using UnityEngine;
namespace ProjectFirstRun.Progression
{
    [DisallowMultipleComponent, RequireComponent(typeof(HealthComponent))]
    public sealed class PlayerMetaResourceCollector : MonoBehaviour
    {
        [SerializeField] private RunEconomyController _economy;
        [SerializeField, Min(0)] private float _attractionRadius = 2;
        private HealthComponent _health;
        private HealthComponent Health => _health != null ? _health : (_health = GetComponent<HealthComponent>());
        public bool CanReceive => isActiveAndEnabled && _economy != null && _economy.isActiveAndEnabled &&
            _economy.Result == null && !Health.IsDead;
        public bool CanCollect => CanReceive && Time.timeScale > 0;
        public float AttractionRadius => _attractionRadius;
        public Vector3 CollectionPoint => transform.position + Vector3.up * .75f;
        public void Configure(RunEconomyController economy) => _economy = economy != null ? economy : throw new ArgumentNullException(nameof(economy));
        public bool TryReceive(int amount)
        {
            if (amount <= 0 || !CanCollect) return false;
            _economy.Add(amount);
            return true;
        }
    }
}
