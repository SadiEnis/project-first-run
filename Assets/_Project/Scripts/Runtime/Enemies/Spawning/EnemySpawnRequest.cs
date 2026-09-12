using System;
using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Enemies.Spawning
{
    public readonly struct EnemySpawnRequest
    {
        public EnemyController Prefab { get; }
        public EnemyDefinition Definition { get; }
        public Transform Target { get; }
        public IDamageable TargetDamageable { get; }
        public EnemyRegistry Registry { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        public EnemySpawnRequest(
            EnemyController prefab,
            EnemyDefinition definition,
            Transform target,
            IDamageable targetDamageable,
            EnemyRegistry registry,
            Vector3 position,
            Quaternion rotation)
        {
            Prefab = prefab;
            Definition = definition;
            Target = target;
            TargetDamageable = targetDamageable;
            Registry = registry;
            Position = position;
            Rotation = rotation;

            Validate();
        }

        public void Validate()
        {
            if (Prefab == null)
            {
                throw new ArgumentNullException(
                    nameof(Prefab));
            }

            if (Definition == null)
            {
                throw new ArgumentNullException(
                    nameof(Definition));
            }

            if (Target == null)
            {
                throw new ArgumentNullException(
                    nameof(Target));
            }

            if (TargetDamageable == null)
            {
                throw new ArgumentNullException(
                    nameof(TargetDamageable));
            }

            if (TargetDamageable is not UnityEngine.Object
                targetDamageableObject)
            {
                throw new ArgumentException(
                    "The target damageable must be a Unity object.",
                    nameof(TargetDamageable));
            }

            if (targetDamageableObject == null)
            {
                throw new ArgumentException(
                    "The target damageable Unity object has been destroyed.",
                    nameof(TargetDamageable));
            }

            if (Registry == null)
            {
                throw new ArgumentNullException(
                    nameof(Registry));
            }

            if (!IsFinite(Position))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Position),
                    Position,
                    "The spawn position must contain finite values.");
            }

            if (!IsFinite(Rotation))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Rotation),
                    Rotation,
                    "The spawn rotation must contain finite values.");
            }

            float rotationMagnitudeSquared =
                Rotation.x * Rotation.x +
                Rotation.y * Rotation.y +
                Rotation.z * Rotation.z +
                Rotation.w * Rotation.w;

            if (rotationMagnitudeSquared <= Mathf.Epsilon)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Rotation),
                    Rotation,
                    "The spawn rotation must not be a zero quaternion.");
            }
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) &&
                   IsFinite(value.y) &&
                   IsFinite(value.z);
        }

        private static bool IsFinite(Quaternion value)
        {
            return IsFinite(value.x) &&
                   IsFinite(value.y) &&
                   IsFinite(value.z) &&
                   IsFinite(value.w);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value);
        }
    }
}