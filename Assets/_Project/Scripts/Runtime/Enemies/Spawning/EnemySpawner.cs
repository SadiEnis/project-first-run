using System;
using UnityEngine;

namespace ProjectFirstRun.Enemies.Spawning
{
    [DisallowMultipleComponent]
    public sealed class EnemySpawner : MonoBehaviour
    {
        public EnemySpawnResult Spawn(
            in EnemySpawnRequest request)
        {
            // A previously valid request may contain Unity objects
            // that were destroyed after its construction.
            request.Validate();

            EnemyController spawnedEnemy = null;

            try
            {
                spawnedEnemy = Instantiate(
                    request.Prefab,
                    request.Position,
                    request.Rotation);

                spawnedEnemy.name =
                    $"{request.Prefab.name}_Instance";

                if (!spawnedEnemy.TryGetComponent(
                        out EnemyAttackController attackController))
                {
                    throw new InvalidOperationException(
                        $"Spawned enemy '{spawnedEnemy.name}' requires an " +
                        $"{nameof(EnemyAttackController)} component.");
                }

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