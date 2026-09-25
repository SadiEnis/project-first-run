using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Weapons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Weapons
{
    public sealed class RocketProjectileTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private readonly Vector3 _origin = new Vector3(2000, 2000, 2000);
        private GameObject _source;
        private GameObject New(string name, Vector3 offset)
        {
            var go = new GameObject(name); go.transform.position = _origin + offset;
            _objects.Add(go); return go;
        }
        private GameObject Box(Vector3 offset, Vector3 size)
        {
            var go = New("target", offset); go.AddComponent<BoxCollider>().size = size; return go;
        }
        private ShotgunDamageProbe Target(Vector3 offset, Vector3 size)
            => Box(offset, size).AddComponent<ShotgunDamageProbe>();
        private RocketProjectile Rocket(RocketConfig? config = null, float range = 60)
        {
            var rocket = New("rocket", Vector3.zero).AddComponent<RocketProjectile>();
            rocket.Initialize(_source, config ?? new RocketConfig(25), 80, 20, range, ~0, Vector3.forward);
            Physics.SyncTransforms(); return rocket;
        }
        [SetUp] public void Setup() => _source = New("source", Vector3.zero);
        [TearDown] public void Cleanup()
        {
            foreach (var go in _objects) if (go != null) Object.DestroyImmediate(go);
            _objects.Clear();
        }
        [Test]
        public void SweptImpactDamagesEachReceiverOnceWithoutDirectHitBonusOrSelfDamage()
        {
            var self = _source.AddComponent<ShotgunDamageProbe>(); _source.AddComponent<BoxCollider>();
            var front = Target(new Vector3(0, 0, 5), Vector3.one);
            var child = Box(new Vector3(0, 0, 5), Vector3.one * .8f); child.transform.SetParent(front.transform, true);
            var nearby = Target(new Vector3(2, 0, 5), Vector3.one);
            var outside = Target(new Vector3(7, 0, 5), Vector3.one);
            var rocket = Rocket(); int blasts = 0; rocket.Exploded += _ => blasts++;
            rocket.Advance(1); rocket.Advance(1);
            Assert.That(front.DamageCalls, Is.EqualTo(1)); Assert.That(front.Requested, Is.EqualTo(80));
            Assert.That(nearby.DamageCalls, Is.EqualTo(1)); Assert.That(outside.DamageCalls, Is.Zero);
            Assert.That(self.DamageCalls, Is.Zero); Assert.That(blasts, Is.EqualTo(1));
            Assert.That(front.PushCalls, Is.Zero);
        }
        [Test]
        public void ThinWorldCoverBlocksBlastButDamageableActorsDoNot()
        {
            Box(new Vector3(0, 0, 5), new Vector3(10, 10, .05f));
            var rear = Target(new Vector3(0, 0, 6), Vector3.one);
            var front = Target(new Vector3(2, 0, 4), Vector3.one);
            Rocket().Advance(3);
            Assert.That(rear.DamageCalls, Is.Zero); Assert.That(front.DamageCalls, Is.EqualTo(1));
        }
        [Test]
        public void InitialOverlapResolvesAndLargeTargetUsesSurfaceDistance()
        {
            var initial = Target(Vector3.zero, Vector3.one * .2f);
            var large = Target(new Vector3(5, 0, 0), new Vector3(5, 1, 1));
            Rocket().Advance(.01f);
            Assert.That(initial.DamageCalls, Is.EqualTo(1)); Assert.That(large.DamageCalls, Is.EqualTo(1));
        }
        [TestCase(true)] [TestCase(false)]
        public void RangeAndLifetimeExpireWithoutExplosion(bool byRange)
        {
            var target = Target(new Vector3(0, 0, 8), Vector3.one);
            var rocket = Rocket(new RocketConfig(25, lifetime: byRange ? 4 : .1f), byRange ? 2 : 60);
            int blasts = 0; rocket.Exploded += _ => blasts++;
            rocket.Advance(10);
            Assert.That(rocket.IsResolved, Is.True); Assert.That(blasts, Is.Zero);
            Assert.That(target.DamageCalls, Is.Zero);
        }
        [Test]
        public void PauseStopsTravelAndDeadSourceCancelsWithoutBlast()
        {
            var health = _source.AddComponent<HealthComponent>();
            var target = Target(new Vector3(0, 0, 3), Vector3.one);
            var rocket = Rocket(); rocket.Advance(0);
            Assert.That(rocket.transform.position, Is.EqualTo(_origin));
            health.ApplyDamage(new DamageInfo(100, _source, _origin, Vector3.forward));
            rocket.Advance(1);
            Assert.That(rocket.IsResolved, Is.True); Assert.That(target.DamageCalls, Is.Zero);
        }
        [Test]
        public void DestroyedSourceCancelsProjectile()
        {
            var rocket = Rocket(); Object.DestroyImmediate(_source); rocket.Advance(1);
            Assert.That(rocket.IsResolved, Is.True);
        }
        [Test]
        public void CameraToMuzzleObstructionCannotSpawnRocketBeyondWall()
        {
            var prefab = New("prefab", Vector3.left * 100).AddComponent<RocketProjectile>();
            prefab.gameObject.AddComponent<MeshRenderer>();
            var blast = New("blast", Vector3.left * 100).AddComponent<RocketBlastVisual>();
            typeof(RocketProjectile).GetField("_blastPrefab", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(prefab, blast);
            var camera = New("camera", Vector3.zero).AddComponent<Camera>(); camera.enabled = false;
            var muzzle = New("muzzle", Vector3.forward * 2).transform;
            Box(Vector3.forward, new Vector3(10, 10, .05f));
            var behind = Target(Vector3.forward * 2, Vector3.one * .2f);
            Physics.SyncTransforms();
            var rocket = RocketProjectile.Launch(prefab, camera, muzzle, _source, new RocketConfig(25), 80, 20, 60, ~0, null);
            _objects.Add(rocket.gameObject);
            Assert.That(rocket.transform.position.z, Is.LessThan(_origin.z + 1));
            rocket.Advance(.1f);
            Assert.That(rocket.IsResolved, Is.True); Assert.That(behind.DamageCalls, Is.Zero);
            foreach (var effect in Object.FindObjectsByType<RocketBlastVisual>(FindObjectsSortMode.None))
                if (effect != blast) _objects.Add(effect.gameObject);
        }
        [Test]
        public void EightFragmentsShareOneHitLimitAndNeverExplodeAgain()
        {
            var target = Target(Vector3.zero, Vector3.one);
            var fragmentTemplate = New("fragment template", Vector3.left * 100).AddComponent<RocketProjectile>();
            var rocket = Rocket(new RocketConfig(25, fragmentCount: 8));
            typeof(RocketProjectile).GetField("_fragmentPrefab", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(rocket, fragmentTemplate);
            rocket.Advance(.01f);
            var fragments = Object.FindObjectsByType<RocketProjectile>(FindObjectsSortMode.None).Where(x => x.IsFragment).ToArray();
            Assert.That(fragments.Length, Is.EqualTo(8));
            foreach (var fragment in fragments)
            {
                _objects.Add(fragment.gameObject);
                Assert.That(fragment.Damage, Is.EqualTo(20));
                Assert.That(fragment.RemainingRange, Is.EqualTo(8));
                Assert.That(fragment.Direction.y, Is.Zero.Within(.0001));
                fragment.Exploded += _ => Assert.Fail("Fragment must not explode");
                fragment.Advance(.1f);
            }
            Assert.That(target.DamageCalls, Is.EqualTo(2)); // One blast + one shared fragment hit.
            Assert.That(target.Health, Is.EqualTo(900));
        }
    }
}
