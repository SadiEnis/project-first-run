using System;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ProjectFirstRun.Progression
{
    [DisallowMultipleComponent]
    public sealed class MetaResourceSpawner : MonoBehaviour
    {
        [SerializeField] private PlayerMetaResourceCollector _collector;
        [SerializeField] private MetaResourcePickup _pickupPrefab;
        public void ValidateConfiguration()
        {
            if (_collector == null || _pickupPrefab == null) throw new InvalidOperationException("Assign resource collector and pickup prefab.");
        }
        public void Spawn(int amount, Vector3 position)
        {
            if (!isActiveAndEnabled || amount <= 0 || !_collector.CanReceive) return;
            var pickup = Instantiate(_pickupPrefab, position, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(pickup.gameObject, gameObject.scene);
            pickup.Initialize(amount, _collector);
        }
    }
}
