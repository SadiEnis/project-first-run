using System;
using UnityEngine;

namespace ProjectFirstRun.Chests.Spawning
{
    public readonly struct ChestSpawnResult
    {
        public ChestController ChestController
        {
            get;
        }

        public GameObject Instance
        {
            get;
        }

        public ChestSpawnResult(
            ChestController chestController)
        {
            ChestController =
                chestController ??
                throw new ArgumentNullException(
                    nameof(chestController));

            Instance =
                chestController.gameObject;
        }
    }
}
