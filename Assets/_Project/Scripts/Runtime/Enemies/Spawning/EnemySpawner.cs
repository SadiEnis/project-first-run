using System;
using ProjectFirstRun.Chests.Spawning;
using UnityEngine;

namespace ProjectFirstRun.Enemies.Spawning
{
    [DisallowMultipleComponent]
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyChestDropSource _chestDropSource;

        public EnemySpawnResult Spawn(
            in EnemySpawnRequest request)
        {
            return SpawnInternal(in request, null);
        }

        // Inactive hierarchy prevents OnEnable/registry/attack side effects before
        // the preparation owner can put the instance into its dormant state.
        public EnemySpawnResult SpawnInactive(in EnemySpawnRequest request, Transform inactiveParent)
        {
            if (inactiveParent == null || inactiveParent.gameObject.activeInHierarchy)
                throw new ArgumentException("An inactive preparation parent is required.", nameof(inactiveParent));
            return SpawnInternal(in request, inactiveParent);
        }

        private EnemySpawnResult SpawnInternal(in EnemySpawnRequest request, Transform parent)
        {
            // A previously valid request may contain Unity objects
            // that were destroyed after its construction.
            request.Validate();

            EnemyChestDropProfile dropProfile = request.Definition.ChestDropProfile;
            if (dropProfile != null)
            {
                dropProfile.Validate();
                if (dropProfile.ChanceBasisPoints > 0 && (_chestDropSource == null ||
                    !request.Prefab.TryGetComponent<EnemyChestDrop>(out _)))
                    throw new InvalidOperationException("Enabled enemy chest drops require a scene source and prefab drop component.");
            }

            EnemyController spawnedEnemy = null;

            try
            {
                spawnedEnemy = Instantiate(
                    request.Prefab,
                    request.Position,
                    request.Rotation,
                    parent);

                spawnedEnemy.name =
                    $"{request.Prefab.name}_Instance";

                if (!spawnedEnemy.TryGetComponent(
                        out EnemyAttackController attackController))
                {
                    throw new InvalidOperationException(
                        $"Spawned enemy '{spawnedEnemy.name}' requires an " +
                        $"{nameof(EnemyAttackController)} component.");
                }

                if (_chestDropSource != null && spawnedEnemy.TryGetComponent(out EnemyChestDrop chestDrop))
                    chestDrop.Initialize(_chestDropSource);

                spawnedEnemy.Initialize(
                    request.Definition,
                    request.Target,
                    request.Registry);

                attackController.Initialize(
                    request.Definition,
                    request.Target,
                    request.TargetDamageable);

                return new EnemySpawnResult(
                    spawnedEnemy,
                    attackController);
            }
            catch
            {
                if (spawnedEnemy != null)
                {
                    CleanupFailedSpawn(
                        spawnedEnemy.gameObject);
                }

                throw;
            }
        }

        private static void CleanupFailedSpawn(
            GameObject spawnedInstance)
        {
            if (spawnedInstance == null)
            {
                return;
            }

            // Immediately stops movement, attack updates and registry
            // membership before destruction is completed.
            spawnedInstance.SetActive(false);

            if (Application.isPlaying)
            {
                Destroy(spawnedInstance);
                return;
            }

            DestroyImmediate(spawnedInstance);
        }
    }
}
