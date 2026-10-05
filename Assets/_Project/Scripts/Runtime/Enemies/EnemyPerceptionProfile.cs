using System;
using UnityEngine;

namespace ProjectFirstRun.Enemies
{
    [CreateAssetMenu(menuName = "Project First Run/Enemies/Perception Profile")]
    public sealed class EnemyPerceptionProfile : ScriptableObject
    {
        [SerializeField, Min(.01f)] private float _acquisitionRange = 18f;
        [SerializeField, Range(1f, 360f)] private float _coneDegrees = 120f;
        [SerializeField, Min(.01f)] private float _trackingRange = 24f;
        [SerializeField, Min(.01f)] private float _sampleInterval = .1f;
        [SerializeField, Min(.01f)] private float _memorySeconds = 3f;
        [SerializeField] private Vector3 _eyeOffset = Vector3.up;
        [SerializeField] private Vector3 _targetOffset = Vector3.up;
        [SerializeField] private LayerMask _obstructionMask = Physics.DefaultRaycastLayers;

        public float AcquisitionRange => _acquisitionRange;
        public float ConeDegrees => _coneDegrees;
        public float TrackingRange => _trackingRange;
        public float SampleInterval => _sampleInterval;
        public float MemorySeconds => _memorySeconds;
        public Vector3 EyeOffset => _eyeOffset;
        public Vector3 TargetOffset => _targetOffset;
        public int ObstructionMask => _obstructionMask;

        public void Validate()
        {
            if (!Positive(_acquisitionRange) || !Positive(_trackingRange) ||
                _trackingRange < _acquisitionRange || !Positive(_sampleInterval) ||
                !Positive(_memorySeconds) || !Positive(_coneDegrees) || _coneDegrees > 360f ||
                !Finite(_eyeOffset) || !Finite(_targetOffset))
                throw new InvalidOperationException("Invalid enemy perception profile.");
        }

        private static bool Positive(float value) => float.IsFinite(value) && value > 0f;
        private static bool Finite(Vector3 value) =>
            float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
    }
}
