using System;
using UnityEngine;

namespace ProjectFirstRun.Progression
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public sealed class ExperiencePickup : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float _attractionSpeed = 8f;
        private Rigidbody _body;

        public float AttractionSpeed => _attractionSpeed;
        public int Amount { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsCollected { get; private set; }

        public bool AttractTowards(PlayerExperienceCollector collector, float deltaTime)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (!isActiveAndEnabled || !IsInitialized || IsCollected || deltaTime == 0f ||
                collector == null || !collector.CanAttract(transform.position))
                return false;

            Vector3 target = collector.CollectionPosition;
            Vector3 nextPosition = Vector3.MoveTowards(transform.position, target, _attractionSpeed * deltaTime);
            if (nextPosition == target)
                return TryCollect(collector);

            if (_body == null)
                _body = GetComponent<Rigidbody>();
            _body.MovePosition(nextPosition);
            return true;
        }

        private void OnValidate()
        {
            if (float.IsNaN(_attractionSpeed) || float.IsInfinity(_attractionSpeed) || _attractionSpeed <= 0f)
                _attractionSpeed = 8f;
        }

        public void Initialize(int amount)
        {
            if (IsInitialized)
                throw new InvalidOperationException("An XP pickup can only be initialized once.");
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            Amount = amount;
            IsInitialized = true;
        }

        public bool TryCollect(PlayerExperienceCollector collector)
        {
            if (!isActiveAndEnabled || !IsInitialized || IsCollected ||
                collector == null || !collector.CanCollect)
                return false;

            PlayerExperienceController experience = collector.Experience;
            long previousTotal = experience.TotalExperience;
            IsCollected = true;
            try
            {
                experience.GainExperience(Amount);
            }
            catch
            {
                // An observer may fail AFTER state commits. Do not duplicate that award.
                IsCollected = experience.TotalExperience != previousTotal;
                throw;
            }
            finally
            {
                if (IsCollected)
                {
                    GetComponent<SphereCollider>().enabled = false;
                    gameObject.SetActive(false);
                    Destroy(gameObject);
                }
            }

            return true;
        }

        private void OnTriggerEnter(Collider other) => TryCollectFrom(other);
        private void OnTriggerStay(Collider other) => TryCollectFrom(other);

        private void TryCollectFrom(Collider other)
        {
            TryCollect(other.GetComponentInParent<PlayerExperienceCollector>());
        }
    }
}
