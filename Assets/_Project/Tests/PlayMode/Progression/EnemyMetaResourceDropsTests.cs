using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Progression;
using UnityEngine;

namespace ProjectFirstRun.Tests.PlayMode.Progression
{
    public sealed class EnemyMetaResourceDropsTests
    {
        private GameObject _services, _player, _enemyObject;
        private EnemyController _enemy;
        private EnemyDefinition _definition;
        private MetaResourcePickup _template;

        [SetUp]
        public void Setup()
        {
            _player = new GameObject("resource test player");
            var health = _player.AddComponent<HealthComponent>();
            _services = new GameObject("resource test services");
            _services.SetActive(false);
            var registry = _services.AddComponent<EnemyRegistry>();
            var economy = _services.AddComponent<RunEconomyController>();
            _template = new GameObject("resource template").AddComponent<MetaResourcePickup>();
            _template.transform.position = Vector3.one * 100;
            _template.GetComponent<Rigidbody>().isKinematic = true;
            _template.GetComponent<SphereCollider>().isTrigger = true;
            var collector = _player.AddComponent<PlayerMetaResourceCollector>();
            collector.Configure(economy);
            var spawner = _services.AddComponent<MetaResourceSpawner>();
            Set(spawner, "_collector", collector);
            Set(spawner, "_pickupPrefab", _template);
            var drops = _services.AddComponent<EnemyMetaResourceDrops>();
            Set(drops, "_registry", registry);
            Set(drops, "_spawner", spawner);
            _services.SetActive(true);
            _definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            _enemyObject = new GameObject("resource enemy");
            _enemyObject.SetActive(false);
            _enemy = _enemyObject.AddComponent<EnemyController>();
            _enemy.Initialize(_definition, _player.transform, registry);
            _enemyObject.SetActive(true);
        }

        [TearDown]
        public void Cleanup()
        {
            foreach (var pickup in Drops()) Object.DestroyImmediate(pickup.gameObject);
            Object.DestroyImmediate(_template.gameObject);
            Object.DestroyImmediate(_services);
            Object.DestroyImmediate(_enemyObject);
            Object.DestroyImmediate(_player);
            Object.DestroyImmediate(_definition);
        }

        [TestCase(EnemyRank.Normal, 7)]
        [TestCase(EnemyRank.Normal, 13)]
        [TestCase(EnemyRank.Elite, 2)]
        [TestCase(EnemyRank.Boss, 9)]
        public void AssetAmountControlsDropRegardlessOfRank(EnemyRank rank, int amount)
        {
            Set(_definition, "_rank", rank);
            Set(_definition, "_metaResourceReward", amount);
            Kill(); Kill();
            var drops = Drops();
            Assert.That(drops.Length, Is.EqualTo(1));
            Assert.That(drops[0].Amount, Is.EqualTo(amount));
            Set(_definition, "_metaResourceReward", amount + 10);
            Assert.That(drops[0].Amount, Is.EqualTo(amount), "Spawned loot snapshots the reward.");
            Assert.That(_services.GetComponent<RunEconomyController>().Collected, Is.Zero);
        }

        [Test]
        public void ZeroAmountDoesNotSpawn()
        {
            Kill();
            Assert.That(Drops(), Is.Empty);
        }

        [Test]
        public void LivingDespawnDoesNotSpawn()
        {
            Set(_definition, "_metaResourceReward", 10);
            _enemyObject.SetActive(false);
            Assert.That(Drops(), Is.Empty);
        }

        [Test]
        public void ReenabledListenerObservesExistingEnemyExactlyOnce()
        {
            var listener = _services.GetComponent<EnemyMetaResourceDrops>();
            listener.enabled = false;
            listener.enabled = true;
            Set(_definition, "_metaResourceReward", 7);
            Kill();
            Assert.That(Drops().Length, Is.EqualTo(1));
        }

        [Test]
        public void DisabledListenerDoesNotAwardAndDoesNotReplayPastDeath()
        {
            var listener = _services.GetComponent<EnemyMetaResourceDrops>();
            listener.enabled = false;
            Set(_definition, "_metaResourceReward", 7);
            Kill();
            listener.enabled = true;
            Assert.That(Drops(), Is.Empty);
        }

        private void Kill() => _enemy.Health.ApplyDamage(new DamageInfo(10000, null, Vector3.zero, Vector3.forward));
        private MetaResourcePickup[] Drops() => Object.FindObjectsByType<MetaResourcePickup>(FindObjectsSortMode.None)
            .Where(x => x != _template).ToArray();
        private static void Set(object target, string field, object value) => target.GetType()
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
