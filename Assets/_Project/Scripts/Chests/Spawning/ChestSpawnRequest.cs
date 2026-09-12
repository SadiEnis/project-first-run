using System;
using UnityEngine;

namespace ProjectFirstRun.Chests.Spawning
{
    public readonly struct ChestSpawnRequest
    {
        public ChestDefinition Definition
        {
            get;
        }

        public Vector3 Position
        {
            get;
        }

        public Quaternion Rotation
        {
            get;
        }

        public ChestSpawnRequest(
            ChestDefinition definition,
            Vector3 position,
            Quaternion rotation)
        {
            Definition = definition;
            Position = position;
            Rotation = rotation;

            Validate();
        }

        public void Validate()
        {
            if (Definition == null)
            {
                throw new ArgumentNullException(
                    nameof(Definition));
            }

            Definition.Validate();

            if (!IsFinite(
                    Position))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Position),
                    Position,
                    "The chest spawn position must contain finite values.");
            }

            if (!IsFinite(
                    Rotation))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Rotation),
                    Rotation,
                    "The chest spawn rotation must contain finite values.");
            }

            float rotationMagnitudeSquared =
                Rotation.x * Rotation.x +
                Rotation.y * Rotation.y +
                Rotation.z * Rotation.z +
                Rotation.w * Rotation.w;

            if (rotationMagnitudeSquared <=
                Mathf.Epsilon)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Rotation),
                    Rotation,
                    "The chest spawn rotation must not be a zero quaternion.");
            }
        }

        private static bool IsFinite(
            Vector3 value)
        {
            return IsFinite(value.x) &&
                   IsFinite(value.y) &&
                   IsFinite(value.z);
        }

        private static bool IsFinite(
            Quaternion value)
        {
            return IsFinite(value.x) &&
                   IsFinite(value.y) &&
                   IsFinite(value.z) &&
                   IsFinite(value.w);
        }

        private static bool IsFinite(
            float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value);
        }
    }
}
