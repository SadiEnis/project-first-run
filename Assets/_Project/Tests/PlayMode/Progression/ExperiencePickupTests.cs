using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Progression;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Progression
{
    public sealed class ExperiencePickupTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private PlayerExperienceCollector _collector;
        private PlayerExperienceController _experience;
        private float _previousTimeScale;

        [SetUp]
        public void SetUp()
        {
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 1f;
            _collector = NewObject("Collector").AddComponent<PlayerExperienceCollector>();
            _collector.SetAttractionRadius(0f); // These tests isolate contact collection from attraction.
            _experience = _collector.Experience;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = _previousTimeScale;
            foreach (GameObject item in _objects)
                if (item != null) Object.DestroyImmediate(item);
            _objects.Clear();
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Initialize_RejectsNonPositiveAmount(int amount)
        {
            ExperiencePickup pickup = NewPickup();
            Assert.Throws<ArgumentOutOfRangeException>(() => pickup.Initialize(amount));
            Assert.That(pickup.IsInitialized, Is.False);
        }

        [Test]
        public void Initialize_RejectsReplacement()
        {
            ExperiencePickup pickup = NewPickup(25);
            Assert.Throws<InvalidOperationException>(() => pickup.Initialize(50));
            Assert.That(pickup.Amount, Is.EqualTo(25));
        }

        [Test]
        public void UninitializedPickupOrPlayer_RejectsWithoutConsuming()
        {
            ExperiencePickup pickup = NewPickup();
            Assert.That(pickup.TryCollect(_collector), Is.False);
            pickup.Initialize(25);
            Assert.That(pickup.TryCollect(_collector), Is.False);
            InitializePlayer();
            Assert.That(pickup.TryCollect(_collector), Is.True);
        }

        [Test]
        public void Collect_AwardsOnceAndReservesBeforeNotification()
        {
            InitializePlayer();
            ExperiencePickup pickup = NewPickup(25);
            int events = 0;
            _experience.ExperienceChanged += _ =>
            {
                events++;
                Assert.That(pickup.IsCollected, Is.True);
                Assert.That(pickup.TryCollect(_collector), Is.False);
            };
            Assert.That(pickup.TryCollect(_collector), Is.True);
            Assert.That(pickup.TryCollect(_collector), Is.False);
            Assert.That(_experience.TotalExperience, Is.EqualTo(25));
            Assert.That(events, Is.EqualTo(1));
            Assert.That(pickup.gameObject.activeSelf, Is.False);
            Assert.That(pickup.GetComponent<SphereCollider>().enabled, Is.False);
        }

        [TestCase("dead")]
        [TestCase("paused")]
        [TestCase("collector_disabled")]
        [TestCase("xp_disabled")]
        [TestCase("player_inactive")]
        [TestCase("pickup_disabled")]
        public void IneligibleAttempt_PreservesPickupForLater(string reason)
        {
            InitializePlayer();
            ExperiencePickup pickup = NewPickup(25);
            HealthComponent health = _collector.GetComponent<HealthComponent>();
            if (reason == "dead") health.ApplyDamage(new DamageInfo(1000f, null, Vector3.zero, Vector3.forward));
            if (reason == "paused") Time.timeScale = 0f;
            if (reason == "collector_disabled") _collector.enabled = false;
            if (reason == "xp_disabled") _experience.enabled = false;
            if (reason == "player_inactive") _collector.gameObject.SetActive(false);
            if (reason == "pickup_disabled") pickup.enabled = false;
            Assert.That(pickup.TryCollect(_collector), Is.False);
            Assert.That(pickup.IsCollected, Is.False);
            Assert.That(_experience.TotalExperience, Is.Zero);
            health.ResetHealth();
            Time.timeScale = 1f;
            _collector.enabled = true;
            _experience.enabled = true;
            _collector.gameObject.SetActive(true);
            pickup.enabled = true;
            Assert.That(pickup.TryCollect(_collector), Is.True);
        }

        [Test]
        public void NullCollector_DoesNotConsume()
        {
            Assert.That(NewPickup(25).TryCollect(null), Is.False);
        }

        [Test]
        public void RunBootstrap_InitializesPlayerBeforeCollection()
        {
            ExperienceDefinition definition = ScriptableObject.CreateInstance<ExperienceDefinition>();
            try
            {
                GameObject root = NewObject("Run bootstrap");
                root.SetActive(false);
                ExperienceRunBootstrap bootstrap = root.AddComponent<ExperienceRunBootstrap>();
                const System.Reflection.BindingFlags flags =
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                typeof(ExperienceRunBootstrap).GetField("_playerExperience", flags).SetValue(bootstrap, _experience);
                typeof(ExperienceRunBootstrap).GetField("_definition", flags).SetValue(bootstrap, definition);
                root.SetActive(true);
                Assert.That(_experience.IsInitialized, Is.True);
                Assert.That(_experience.Level, Is.EqualTo(1));
                Assert.That(NewPickup(100).TryCollect(_collector), Is.True);
                Assert.That(_experience.Level, Is.EqualTo(2));
                root.SetActive(false);
                root.SetActive(true);
                Assert.That(_experience.Level, Is.EqualTo(2), "Re-enabling must not reset the run.");
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void LargePickup_PreservesOverflowAndReportsEveryLevel()
        {
            InitializePlayer();
            int levels = 0;
            _experience.LevelChanged += result => levels += result.LevelsGained;
            NewPickup(500).TryCollect(_collector);
            Assert.That(_experience.Level, Is.EqualTo(4));
            Assert.That(_experience.CurrentExperience, Is.EqualTo(50));
            Assert.That(levels, Is.EqualTo(3));
        }

        [Test]
        public void NestedDifferentPickup_RejectsWithoutLosingItsXP()
        {
            InitializePlayer();
            ExperiencePickup first = NewPickup(25);
            ExperiencePickup second = NewPickup(50);
            Action<ExperienceGainResult> handler = _ =>
                Assert.Throws<InvalidOperationException>(() => second.TryCollect(_collector));
            _experience.ExperienceChanged += handler;
            first.TryCollect(_collector);
            _experience.ExperienceChanged -= handler;
            Assert.That(second.IsCollected, Is.False);
            Assert.That(second.TryCollect(_collector), Is.True);
            Assert.That(_experience.TotalExperience, Is.EqualTo(75));
        }

        [Test]
        public void ObserverThrowsAfterCommit_DoesNotDuplicateAward()
        {
            InitializePlayer();
            ExperiencePickup pickup = NewPickup(25);
            _experience.ExperienceChanged += _ => throw new InvalidOperationException("Test observer");
            Assert.Throws<InvalidOperationException>(() => pickup.TryCollect(_collector));
            Assert.That(pickup.IsCollected, Is.True);
            Assert.That(pickup.TryCollect(_collector), Is.False);
            Assert.That(_experience.TotalExperience, Is.EqualTo(25));
        }

        [UnityTest]
        public IEnumerator Trigger_IgnoresUnrelatedColliderAndCollectsCharacterController()
        {
            InitializePlayer();
            ExperiencePickup pickup = NewPickup(25);
            NewObject("Not a player").AddComponent<BoxCollider>();
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(pickup.IsCollected, Is.False);
            _collector.gameObject.AddComponent<CharacterController>();
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(_experience.TotalExperience, Is.EqualTo(25));
        }

        [UnityTest]
        public IEnumerator TriggerStay_CollectsAfterInitializationWhileAlreadyOverlapping()
        {
            _collector.gameObject.AddComponent<CharacterController>();
            ExperiencePickup pickup = NewPickup(25);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(pickup.IsCollected, Is.False);
            InitializePlayer();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(_experience.TotalExperience, Is.EqualTo(25));
        }

        private void InitializePlayer() =>
            _experience.Initialize(new ExperienceState(new ExperienceCurve(100, 50)));

        private GameObject NewObject(string name)
        {
            var result = new GameObject(name);
            _objects.Add(result);
            return result;
        }

        private ExperiencePickup NewPickup(int amount = 0)
        {
            ExperiencePickup result = NewObject("XP Pickup").AddComponent<ExperiencePickup>();
            result.gameObject.layer = 2;
            result.GetComponent<SphereCollider>().isTrigger = true;
            result.GetComponent<Rigidbody>().isKinematic = true;
            result.GetComponent<Rigidbody>().useGravity = false;
            if (amount > 0) result.Initialize(amount);
            return result;
        }
    }
}
