using System.Collections;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Enemies;
using ProjectFirstRun.Enemies.Lifecycle;
using ProjectFirstRun.Progression;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Progression
{
    public sealed class EnemyExperienceDropTests
    {
        private GameObject _enemyObject;
        private GameObject _target;
        private EnemyDefinition _definition;
        private EnemyRegistry _registry;
        private EnemyController _enemy;
        private ExperiencePickup _template;

        [SetUp]
        public void SetUp()
        {
            _target = new GameObject("Target");
            _registry = new GameObject("Registry").AddComponent<EnemyRegistry>();
            _definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            SetField(_definition, "_experienceReward", 25);
            _template = new GameObject("PickupTemplate").AddComponent<ExperiencePickup>();
            _template.transform.position = Vector3.one * 100f;
            _template.GetComponent<SphereCollider>().isTrigger = true;
            _template.GetComponent<Rigidbody>().isKinematic = true;
            _enemyObject = new GameObject("Enemy");
            _enemyObject.SetActive(false);
            _enemy = _enemyObject.AddComponent<EnemyController>();
            EnemyDeathLifecycle lifecycle = _enemyObject.AddComponent<EnemyDeathLifecycle>();
            SetField(lifecycle, "_destroyDelay", 0f);
            EnemyExperienceDrop drop = _enemyObject.AddComponent<EnemyExperienceDrop>();
            SetField(drop, "_pickupPrefab", _template);
            _enemy.Initialize(_definition, _target.transform, _registry);
            _enemyObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (ExperiencePickup pickup in Object.FindObjectsByType<ExperiencePickup>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
                Object.DestroyImmediate(pickup.gameObject);
            if (_enemyObject != null) Object.DestroyImmediate(_enemyObject);
            Object.DestroyImmediate(_target);
            Object.DestroyImmediate(_registry.gameObject);
            Object.DestroyImmediate(_definition);
        }

        [Test]
        public void NonLethalDamage_DoesNotDrop()
        {
            Damage(1f);
            Assert.That(Drops(), Is.Empty);
        }

        [Test]
        public void LethalDamage_SpawnsOneInitializedPickupAtDeathPosition()
        {
            _enemyObject.transform.position = new Vector3(2f, 0f, 3f);
            Damage(1000f);
            Damage(1000f);
            ExperiencePickup[] drops = Drops();
            Assert.That(drops, Has.Length.EqualTo(1));
            Assert.That(drops[0].IsInitialized, Is.True);
            Assert.That(drops[0].Amount, Is.EqualTo(25));
            Assert.That(drops[0].transform.position, Is.EqualTo(new Vector3(2f, 0.35f, 3f)));
            SetField(_definition, "_experienceReward", 999);
            Assert.That(drops[0].Amount, Is.EqualTo(25), "Pickup snapshots its award.");
        }

        [Test]
        public void ZeroReward_DoesNotSpawn()
        {
            SetField(_definition, "_experienceReward", 0);
            Damage(1000f);
            Assert.That(Drops(), Is.Empty);
        }

        [Test]
        public void DisableAndDestroyLivingEnemy_DoesNotSpawn()
        {
            _enemyObject.SetActive(false);
            Object.DestroyImmediate(_enemyObject);
            Assert.That(Drops(), Is.Empty);
        }

        [UnityTest]
        public IEnumerator ZeroDelayCleanup_DoesNotDestroyTheDroppedPickup()
        {
            Damage(1000f);
            yield return null;
            Assert.That(_enemyObject == null, Is.True);
            Assert.That(Drops(), Has.Length.EqualTo(1));
        }

        [Test]
        public void DisableReenableBeforeDeath_DoesNotDuplicateSubscription()
        {
            _enemyObject.SetActive(false);
            _enemyObject.SetActive(true);
            Damage(1000f);
            Assert.That(Drops(), Has.Length.EqualTo(1));
        }

        private void Damage(float amount) =>
            _enemy.Health.ApplyDamage(new DamageInfo(amount, null, Vector3.zero, Vector3.forward));

        private ExperiencePickup[] Drops() => System.Array.FindAll(
            Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None),
            pickup => pickup != _template);

        private static void SetField(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(target, value);
    }
}
