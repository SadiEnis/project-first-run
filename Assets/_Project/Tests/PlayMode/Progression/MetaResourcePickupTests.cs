using System.Collections;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Progression;
using UnityEngine;
using UnityEngine.TestTools;

namespace ProjectFirstRun.Tests.PlayMode.Progression
{
    public sealed class MetaResourcePickupTests
    {
        private GameObject _player, _owner, _loot;
        private float _time;
        [SetUp] public void Setup()
        {
            _time = Time.timeScale;
            Time.timeScale = 1;
            _player = new GameObject("collector");
            _player.AddComponent<HealthComponent>();
            _owner = new GameObject("economy");
            var economy = _owner.AddComponent<RunEconomyController>();
            _player.AddComponent<PlayerMetaResourceCollector>().Configure(economy);
            _loot = new GameObject("resource");
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.timeScale = _time;
            Object.Destroy(_loot); Object.Destroy(_player); Object.Destroy(_owner);
            yield return null;
        }
        [UnityTest]
        public IEnumerator SpawningDoesNotAwardAndCollectionAwardsOnlyOnce()
        {
            var economy = _owner.GetComponent<RunEconomyController>();
            var health = _player.GetComponent<HealthComponent>();
            var pickup = _loot.AddComponent<MetaResourcePickup>();
            pickup.Initialize(5, _player.GetComponent<PlayerMetaResourceCollector>());
            Assert.That(economy.Collected, Is.Zero);
            Time.timeScale = 0;
            Assert.That(pickup.TryCollect(_player.GetComponent<PlayerMetaResourceCollector>()), Is.False);
            Time.timeScale = 1;
            Assert.That(pickup.TryCollect(null), Is.False);
            Assert.That(pickup.TryCollect(_player.GetComponent<PlayerMetaResourceCollector>()), Is.True);
            Assert.That(pickup.TryCollect(_player.GetComponent<PlayerMetaResourceCollector>()), Is.False);
            Assert.That(economy.Collected, Is.EqualTo(5));
            yield return null;
            Assert.That(_loot == null, Is.True);
        }
        [UnityTest]
        public IEnumerator DeadPlayerCannotCollect()
        {
            var health = _player.GetComponent<HealthComponent>();
            var economy = _owner.GetComponent<RunEconomyController>();
            var pickup = _loot.AddComponent<MetaResourcePickup>();
            pickup.Initialize(1, _player.GetComponent<PlayerMetaResourceCollector>());
            health.ApplyDamage(new DamageInfo(10000, _player, Vector3.zero, Vector3.forward));
            Assert.That(pickup.TryCollect(_player.GetComponent<PlayerMetaResourceCollector>()), Is.False);
            Assert.That(economy.Collected, Is.Zero);
            yield return null;
        }
    }
}
