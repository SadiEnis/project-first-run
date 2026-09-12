using System;
using UnityEngine;

namespace ProjectFirstRun.Enemies.Spawning
{
    public readonly struct EnemySpawnResult
    {
        public EnemyController EnemyController { get; }
        public EnemyAttackController AttackController { get; }
        public GameObject Instance { get; }

        public EnemySpawnResult(
            EnemyController enemyController,
            EnemyAttackController attackController)
        {
            if (enemyController == null)
            {
                throw new ArgumentNullException(
                    nameof(enemyController));
            }

            if (attackController == null)
            {
                throw new ArgumentNullException(
                    nameof(attackController));
            }

            if (enemyController.gameObject !=
                attackController.gameObject)
            {
                throw new ArgumentException(
                    "EnemyController and EnemyAttackController " +
                    "must belong to the same GameObject.",
                    nameof(attackController));
            }

            EnemyController = enemyController;
            AttackController = attackController;
            Instance = enemyController.gameObject;
        }
    }
}