using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Weapons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Weapons
{
    public sealed class ShotgunDamageProbe : MonoBehaviour, IDamageable, IKnockbackReceiver
    {
        public float Health = 1000, Requested, PushDistance;
        public int DamageCalls, PushCalls;
        public bool Reject;
        public DamageResult ApplyDamage(in DamageInfo damage)
        {
            DamageCalls++;
            Requested = damage.Amount;
            if (Reject) return DamageResult.Rejected(damage.Amount, Health);
            float previous = Health;
            Health = Mathf.Max(0, Health - damage.Amount);
            return new DamageResult(damage.Amount, previous - Health, previous, Health, Health == 0);
        }
        public bool TryPush(Vector3 direction, float distance)
        {
            PushCalls++;
            PushDistance = distance;
            transform.position += direction.normalized * distance;
            return true;
        }
    }

    public sealed class ShotgunVolleyTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private Camera _camera;
        private Transform _muzzle;
        private GameObject _source;
        private Vector3 _origin = new Vector3(1000, 1000, 1000);
        [SetUp]
        public void Setup()
        {
            _source = New("source");
            _camera = New("camera").AddComponent<Camera>();
            _camera.enabled = false;
            _muzzle = New("muzzle").transform;
        }
        [TearDown]
        public void Cleanup()
        {
            foreach (var go in _objects) if (go != null) Object.DestroyImmediate(go);
            _objects.Clear();
        }
        private GameObject New(string name)
        {
            var go = new GameObject(name);
            go.transform.position = _origin;
            _objects.Add(go);
            return go;
        }
        private GameObject Box(Vector3 offset, Vector3 size)
        {
            var go = New("box");
            go.transform.position += offset;
            go.AddComponent<BoxCollider>().size = size;
            return go;
        }
        private HitscanVolleyResult Fire(WeaponShotConfig config, Func<double> random = null)
        {
            Physics.SyncTransforms();
            return new HitscanVolleyResolver(_camera, _muzzle, _source, random).Resolve(8, 30, ~0, config);
        }

        [Test]
        public void EightPelletsAggregateDamageAndPushOnceBeforeGeometryCanChange()
        {
            var target = Box(new Vector3(0, 0, 5), new Vector3(3, 3, 1)).AddComponent<ShotgunDamageProbe>();
            var result = Fire(new WeaponShotConfig(8, 6, .6f), new System.Random(2).NextDouble);
            Assert.That(result.Pellets.Count, Is.EqualTo(8));
            Assert.That(result.Targets.Count, Is.EqualTo(1));
            Assert.That(target.Requested, Is.EqualTo(64));
            Assert.That(target.DamageCalls, Is.EqualTo(1));
            Assert.That(target.PushCalls, Is.EqualTo(1));
            Assert.That(target.PushDistance, Is.EqualTo(.6f));
        }
        [TestCase(false, 10)]
        [TestCase(true, 1000)]
        public void LethalOrRejectedDamageDoesNotPush(bool reject, float health)
        {
            var target = Box(new Vector3(0, 0, 5), Vector3.one * 3).AddComponent<ShotgunDamageProbe>();
            target.Health = health;
            target.Reject = reject;
            Fire(new WeaponShotConfig(8, 0, 1));
            Assert.That(target.DamageCalls, Is.EqualTo(1));
            Assert.That(target.PushCalls, Is.Zero);
        }
        [Test]
        public void MultipleCollidersDoNotMultiplyOnePelletAndFirstTargetBlocksRearTarget()
        {
            var front = Box(new Vector3(0, 0, 5), Vector3.one * 2).AddComponent<ShotgunDamageProbe>();
            var child = Box(new Vector3(0, 0, 4), Vector3.one);
            child.transform.SetParent(front.transform, true);
            var rear = Box(new Vector3(0, 0, 10), Vector3.one * 3).AddComponent<ShotgunDamageProbe>();
            Fire(new WeaponShotConfig(8, 0, 0));
            Assert.That(front.Requested, Is.EqualTo(64));
            Assert.That(front.DamageCalls, Is.EqualTo(1));
            Assert.That(rear.DamageCalls, Is.Zero);
        }
        [Test]
        public void ConeCanHitDistinctTargetsInOneVolley()
        {
            var left = Box(new Vector3(-.52f, 0, 5), new Vector3(.5f, 2, .5f)).AddComponent<ShotgunDamageProbe>();
            var right = Box(new Vector3(.52f, 0, 5), new Vector3(.5f, 2, .5f)).AddComponent<ShotgunDamageProbe>();
            double[] values = { 1, 0, 1, .5 };
            int n = 0;
            var result = Fire(new WeaponShotConfig(2, 6, 0), () => values[n++]);
            Assert.That(result.Targets.Count, Is.EqualTo(2));
            Assert.That(left.Requested, Is.EqualTo(8));
            Assert.That(right.Requested, Is.EqualTo(8));
        }
        [Test]
        public void SpreadDoesNotConsumeUnityGlobalRandomState()
        {
            var state = UnityEngine.Random.state;
            Fire(new WeaponShotConfig(8, 6, 0));
            Assert.That(UnityEngine.Random.state, Is.EqualTo(state));
        }

        [Test]
        public void SolidWallBlocksEvenWithoutDamageReceiver()
        {
            Box(new Vector3(0, 0, 3), new Vector3(6, 6, .3f));
            var target = Box(new Vector3(0, 0, 5), Vector3.one * 3).AddComponent<ShotgunDamageProbe>();
            Assert.That(Fire(new WeaponShotConfig(8, 6, 1)).Targets, Is.Empty);
            Assert.That(target.DamageCalls, Is.Zero);
        }
        [Test]
        public void MuzzleObstructionBlocksCameraVisibleTarget()
        {
            _muzzle.position += Vector3.right;
            Box(new Vector3(.8f, 0, 1), new Vector3(.45f, 2, .2f));
            var target = Box(new Vector3(0, 0, 5), Vector3.one).AddComponent<ShotgunDamageProbe>();
            Fire(new WeaponShotConfig(8, 0, 0));
            Assert.That(target.DamageCalls, Is.Zero);
        }
        [Test]
        public void TriggerAndSelfCollidersAreIgnoredButRangeIsRespected()
        {
            var trigger = Box(new Vector3(0, 0, 2), Vector3.one * 3);
            trigger.GetComponent<BoxCollider>().isTrigger = true;
            var self = Box(new Vector3(0, 0, 3), Vector3.one * 2);
            self.transform.SetParent(_source.transform, true);
            var target = Box(new Vector3(0, 0, 8), Vector3.one * 2).AddComponent<ShotgunDamageProbe>();
            Fire(new WeaponShotConfig(1, 0, 0));
            Assert.That(target.Requested, Is.EqualTo(8));
            target.transform.position = _origin + Vector3.forward * 40;
            Assert.That(Fire(new WeaponShotConfig(8, 0, 0)).Targets, Is.Empty);
        }
        [Test]
        public void NonPushReceiverStillTakesDamageAndSingleRayMatchesLegacy()
        {
            var health = Box(new Vector3(0, 0, 5), Vector3.one * 2).AddComponent<HealthComponent>();
            health.Initialize(100);
            Physics.SyncTransforms();
            var legacy = new HitscanShotResolver(_camera, _muzzle, _source).Resolve(8, 30, ~0);
            var volley = Fire(new WeaponShotConfig(1, 0, 1));
            Assert.That(volley.Targets[0].DamageResult.AppliedDamage, Is.EqualTo(legacy.DamageResult.AppliedDamage));
            Assert.That(Vector3.Distance(volley.Pellets[0].HitPoint, legacy.HitPoint), Is.LessThan(.001f));
            Assert.That(health.CurrentHealth, Is.EqualTo(84));
        }
    }
}
