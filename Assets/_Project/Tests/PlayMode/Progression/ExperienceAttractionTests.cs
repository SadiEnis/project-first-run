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
    public sealed class ExperienceAttractionTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private PlayerExperienceCollector _collector;
        private float _previousTimeScale;

        [SetUp]
        public void SetUp()
        {
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 1f;
            _collector = NewObject("Collector").AddComponent<PlayerExperienceCollector>();
            _collector.Experience.Initialize(new ExperienceState(new ExperienceCurve(100, 50)));
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = _previousTimeScale;
            foreach (GameObject item in _objects)
                if (item != null) Object.DestroyImmediate(item);
            _objects.Clear();
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidRadius_DoesNotReplaceCurrentRadius(float radius)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _collector.SetAttractionRadius(radius));
            Assert.That(_collector.AttractionRadius, Is.EqualTo(3f));
        }

        [Test]
        public void Radius_UsesPickupCenterNotColliderEdge_AndCanChangeAtRuntime()
        {
            Vector3 center = _collector.CollectionPosition;
            Assert.That(_collector.CanAttract(center + Vector3.right * 3f), Is.True);
            Assert.That(_collector.CanAttract(center + Vector3.right * 3.01f), Is.False);
            Assert.That(_collector.CanAttract(center + Vector3.up * 3.01f), Is.False);
            _collector.SetAttractionRadius(5f);
            Assert.That(_collector.CanAttract(center + Vector3.right * 4f), Is.True);
            _collector.SetAttractionRadius(0f);
            Assert.That(_collector.CanAttract(center), Is.False);
        }

        [TestCase("dead")]
        [TestCase("paused")]
        [TestCase("collector_disabled")]
        [TestCase("xp_disabled")]
        [TestCase("pickup_disabled")]
        public void IneligibleAttraction_DoesNotMoveOrConsume(string reason)
        {
            ExperiencePickup pickup = NewPickup(2f);
            Vector3 before = pickup.transform.position;
            if (reason == "dead")
                _collector.GetComponent<HealthComponent>().ApplyDamage(new DamageInfo(1000f, null, Vector3.zero, Vector3.up));
            if (reason == "paused") Time.timeScale = 0f;
            if (reason == "collector_disabled") _collector.enabled = false;
            if (reason == "xp_disabled") _collector.Experience.enabled = false;
            if (reason == "pickup_disabled") pickup.enabled = false;
            Assert.That(pickup.AttractTowards(_collector, 1f), Is.False);
            Assert.That(pickup.transform.position, Is.EqualTo(before));
            Assert.That(pickup.IsCollected, Is.False);
            Assert.That(_collector.Experience.TotalExperience, Is.Zero);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidTimeStep_IsRejected(float deltaTime)
        {
            ExperiencePickup pickup = NewPickup(2f);
            Assert.Throws<ArgumentOutOfRangeException>(() => pickup.AttractTowards(_collector, deltaTime));
            Assert.That(pickup.IsCollected, Is.False);
        }

        [Test]
        public void ZeroTimeStepAndNullCollector_DoNotMove()
        {
            ExperiencePickup pickup = NewPickup(2f);
            Assert.That(pickup.AttractTowards(_collector, 0f), Is.False);
            Assert.That(pickup.AttractTowards(null, 1f), Is.False);
        }

        [Test]
        public void UninitializedPickupOrPlayer_CannotAttract()
        {
            ExperiencePickup uninitialized = NewObject("Uninitialized pickup").AddComponent<ExperiencePickup>();
            Assert.That(uninitialized.AttractTowards(_collector, 1f), Is.False);
            PlayerExperienceCollector uninitializedPlayer =
                NewObject("Uninitialized player").AddComponent<PlayerExperienceCollector>();
            Assert.That(NewPickup(2f).AttractTowards(uninitializedPlayer, 1f), Is.False);
        }

        [UnityTest]
        public IEnumerator PauseDuringFlight_StopsAndResumesWithoutAwardingEarly()
        {
            ExperiencePickup pickup = NewPickup(2.8f);
            yield return Steps(2);
            Time.timeScale = 0f;
            yield return null;
            Vector3 stopped = pickup.transform.position;
            yield return new WaitForSecondsRealtime(0.05f);
            Assert.That(pickup.transform.position, Is.EqualTo(stopped));
            Assert.That(_collector.Experience.TotalExperience, Is.Zero);
            Time.timeScale = 1f;
            yield return Steps(30);
            Assert.That(_collector.Experience.TotalExperience, Is.EqualTo(25));
        }

        [UnityTest]
        public IEnumerator InRange_MovesBeforeAwarding_ThenCollectsExactlyOnce()
        {
            ExperiencePickup pickup = NewPickup(2.5f);
            float before = Vector3.Distance(pickup.transform.position, _collector.CollectionPosition);
            yield return Steps(2);
            Assert.That(pickup != null, Is.True);
            Assert.That(Vector3.Distance(pickup.transform.position, _collector.CollectionPosition), Is.LessThan(before));
            Assert.That(_collector.Experience.TotalExperience, Is.Zero);
            yield return Steps(30);
            Assert.That(_collector.Experience.TotalExperience, Is.EqualTo(25));
            Assert.That(pickup == null, Is.True);
        }

        [UnityTest]
        public IEnumerator OutsideRange_StaysUntilPlayerApproaches()
        {
            ExperiencePickup pickup = NewPickup(3.2f); // Collider overlaps the scan, center is outside it.
            Vector3 before = pickup.transform.position;
            yield return Steps(3);
            Assert.That(pickup.transform.position, Is.EqualTo(before));
            _collector.transform.position = Vector3.right;
            Physics.SyncTransforms();
            yield return Steps(30);
            Assert.That(_collector.Experience.TotalExperience, Is.EqualTo(25));
        }

        [UnityTest]
        public IEnumerator RadiusIncrease_PicksUpAlreadySpawnedDrops()
        {
            ExperiencePickup pickup = NewPickup(4f);
            yield return Steps(2);
            Assert.That(pickup.IsCollected, Is.False);
            _collector.SetAttractionRadius(5f);
            yield return Steps(40);
            Assert.That(_collector.Experience.TotalExperience, Is.EqualTo(25));
        }

        [UnityTest]
        public IEnumerator LeavingRange_StopsFlight_AndReturningResumesIt()
        {
            ExperiencePickup pickup = NewPickup(2.8f);
            yield return Steps(2);
            _collector.transform.position = Vector3.left * 10f;
            Physics.SyncTransforms();
            yield return Steps(2);
            Vector3 stopped = pickup.transform.position;
            yield return Steps(3);
            Assert.That(pickup.transform.position, Is.EqualTo(stopped));
            Assert.That(_collector.Experience.TotalExperience, Is.Zero);
            _collector.transform.position = Vector3.zero;
            Physics.SyncTransforms();
            yield return Steps(30);
            Assert.That(_collector.Experience.TotalExperience, Is.EqualTo(25));
        }

        [UnityTest]
        public IEnumerator DensePile_GrowsQueryBufferWithoutLosingDrops()
        {
            for (int i = 0; i < 40; i++) NewPickup(2f);
            NewObject("Unrelated collider").AddComponent<SphereCollider>().gameObject.layer = 2;
            yield return Steps(35);
            Assert.That(_collector.Experience.TotalExperience, Is.EqualTo(1000));
        }

        [UnityTest]
        public IEnumerator DeathDuringFlight_StopsUntilHealthIsRestored()
        {
            ExperiencePickup pickup = NewPickup(2.8f);
            yield return Steps(2);
            HealthComponent health = _collector.GetComponent<HealthComponent>();
            health.ApplyDamage(new DamageInfo(1000f, null, Vector3.zero, Vector3.up));
            yield return Steps(2);
            Vector3 stopped = pickup.transform.position;
            yield return Steps(3);
            Assert.That(pickup.transform.position, Is.EqualTo(stopped));
            Assert.That(_collector.Experience.TotalExperience, Is.Zero);
            health.ResetHealth();
            yield return Steps(30);
            Assert.That(_collector.Experience.TotalExperience, Is.EqualTo(25));
        }

        private IEnumerator Steps(int count)
        {
            for (int i = 0; i < count; i++) yield return new WaitForFixedUpdate();
            yield return null;
        }

        private ExperiencePickup NewPickup(float distance)
        {
            ExperiencePickup pickup = NewObject("XP").AddComponent<ExperiencePickup>();
            pickup.gameObject.layer = 2;
            pickup.transform.position = _collector.CollectionPosition + Vector3.right * distance;
            pickup.GetComponent<SphereCollider>().isTrigger = true;
            pickup.GetComponent<Rigidbody>().isKinematic = true;
            pickup.GetComponent<Rigidbody>().useGravity = false;
            pickup.Initialize(25);
            Physics.SyncTransforms();
            return pickup;
        }

        private GameObject NewObject(string name)
        {
            var result = new GameObject(name);
            _objects.Add(result);
            return result;
        }
    }
}
