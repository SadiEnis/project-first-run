using System;
using UnityEngine;

namespace ProjectFirstRun.Progression
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public sealed class MetaResourcePickup : MonoBehaviour
    {
        [SerializeField, Min(.01f)] private float _attractionSpeed = 8;
        private PlayerMetaResourceCollector _player;
        public int Amount { get; private set; }
        public bool IsCollected { get; private set; }

        public void Initialize(int amount, PlayerMetaResourceCollector player)
        {
            if (Amount != 0) throw new InvalidOperationException("Pickup already initialized.");
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            _player = player != null ? player : throw new ArgumentNullException(nameof(player));
            Amount = amount;
        }

        private bool CanCollect => isActiveAndEnabled && Amount > 0 && !IsCollected &&
            _player != null && _player.CanCollect;

        public bool TryCollect(PlayerMetaResourceCollector player)
        {
            if (!CanCollect || player != _player || !player.TryReceive(Amount)) return false;
            IsCollected = true;
            GetComponent<SphereCollider>().enabled = false;
            gameObject.SetActive(false);
            Destroy(gameObject);
            return true;
        }

        private void FixedUpdate()
        {
            if (!CanCollect) return;
            Vector3 target = _player.CollectionPoint;
            if ((target - transform.position).sqrMagnitude > _player.AttractionRadius * _player.AttractionRadius) return;
            Vector3 next = Vector3.MoveTowards(transform.position, target, _attractionSpeed * Time.fixedDeltaTime);
            if (next == target) TryCollect(_player);
            else GetComponent<Rigidbody>().MovePosition(next);
        }

        private void OnTriggerEnter(Collider other) => TryCollect(other.GetComponentInParent<PlayerMetaResourceCollector>());
        private void OnTriggerStay(Collider other) => TryCollect(other.GetComponentInParent<PlayerMetaResourceCollector>());
    }
}
