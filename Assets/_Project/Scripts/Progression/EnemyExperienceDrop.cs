using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Progression
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyController))]
    public sealed class EnemyExperienceDrop : MonoBehaviour
    {
        [SerializeField] private ExperiencePickup _pickupPrefab;
        [SerializeField] private Vector3 _spawnOffset = new Vector3(0f, 0.35f, 0f);

        private EnemyController _enemy;
        private bool _hasDropped;

        private void Awake() => _enemy = GetComponent<EnemyController>();

        private void OnEnable()
        {
            _enemy.Died += HandleDied;
        }

        private void OnDisable()
        {
            if (_enemy != null)
                _enemy.Died -= HandleDied;
        }

        private void HandleDied(EnemyController enemy, DamageInfo info, DamageResult result)
        {
            if (_hasDropped || !enemy.IsInitialized || !enemy.IsDead || !result.WasLethal)
                return;

            _hasDropped = true;
            int amount = enemy.Definition.ExperienceReward;
            if (amount <= 0)
                return;

            if (_pickupPrefab == null)
            {
                Debug.LogError($"{nameof(EnemyExperienceDrop)} requires an XP pickup prefab.", this);
                return;
            }

            ExperiencePickup pickup = Instantiate(
                _pickupPrefab, transform.position + _spawnOffset, Quaternion.identity);
            // Loot outlives the enemy/region owner, but not its map scene.
            SceneManager.MoveGameObjectToScene(pickup.gameObject, gameObject.scene);
            pickup.Initialize(amount);
        }
    }
}
