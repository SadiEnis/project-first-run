using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirstRun.Waves
{
    public sealed class RoundRobinSpawnPointSelector
    {
        private readonly List<Transform> _spawnPoints;
        private int _nextIndex;

        public int Count =>
            _spawnPoints.Count;

        public int NextIndex =>
            _nextIndex;

        public RoundRobinSpawnPointSelector(
            IReadOnlyList<Transform> spawnPoints)
        {
            if (spawnPoints == null)
            {
                throw new ArgumentNullException(
                    nameof(spawnPoints));
            }

            if (spawnPoints.Count == 0)
            {
                throw new ArgumentException(
                    "At least one spawn point is required.",
                    nameof(spawnPoints));
            }

            _spawnPoints =
                new List<Transform>(spawnPoints.Count);

            for (int index = 0;
                 index < spawnPoints.Count;
                 index++)
            {
                Transform spawnPoint =
                    spawnPoints[index];

                if (spawnPoint == null)
                {
                    throw new ArgumentException(
                        $"The spawn-point collection contains " +
                        $"a null entry at index {index}.",
                        nameof(spawnPoints));
                }

                _spawnPoints.Add(spawnPoint);
            }
        }

        public Transform GetNext()
        {
            Transform spawnPoint =
                _spawnPoints[_nextIndex];

            if (spawnPoint == null)
            {
                throw new InvalidOperationException(
                    $"Spawn point at index {_nextIndex} " +
                    "is no longer available.");
            }

            _nextIndex =
                (_nextIndex + 1) %
                _spawnPoints.Count;

            return spawnPoint;
        }
    }
}