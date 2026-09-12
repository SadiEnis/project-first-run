using System;
using UnityEngine;

namespace ProjectFirstRun.Progression
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public sealed class ExperiencePickup : MonoBehaviour
    {
        public int Amount { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsCollected { get; private set; }

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
