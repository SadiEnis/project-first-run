using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Weapons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Weapons
{
    public sealed class PlasmaProjectileTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private readonly Vector3 _origin = new Vector3(3000, 3000, 3000);
        private GameObject _source;
        private GameObject New(string name, Vector3 offset)
        {
            var go = new GameObject(name); go.transform.position = _origin + offset; _objects.Add(go); return go;
        }
        private GameObject Box(Vector3 offset, Vector3 size)
        { var go = New("box", offset); go.AddComponent<BoxCollider>().size = size; return go; }
        private ShotgunDamageProbe Target(float z)
            => Box(Vector3.forward * z, Vector3.one).AddComponent<ShotgunDamageProbe>();
        private PlasmaProjectile Fire(int pierce = 0, bool burn = false, float range = 100, float lifetime = 2)
        {
            var projectile = New("plasma", Vector3.zero).AddComponent<PlasmaProjectile>();
            projectile.Initialize(_source, new PlasmaConfig(60, lifetime: lifetime, pierceCount: pierce, burn: burn), 25, 5, range, ~0, Vector3.forward);
            Physics.SyncTransforms(); return projectile;
        }
        [SetUp] public void Setup() => _source = New("source", Vector3.zero);
        [TearDown] public void Cleanup()
        { foreach (var go in _objects) if (go != null) Object.DestroyImmediate(go); _objects.Clear(); }

        [TestCase(0, 1)] [TestCase(1, 2)] [TestCase(2, 3)]
        public void OneLongStepHonorsDistinctTargetBudget(int pierce, int expected)
        {
            var targets = new[] { Target(3), Target(5), Target(7), Target(9) };
            var duplicate = Box(Vector3.forward * 3, Vector3.one * .8f);
            duplicate.transform.SetParent(targets[0].transform, true);
            Fire(pierce).Advance(1);
            for (int i = 0; i < 4; i++) Assert.That(targets[i].DamageCalls, Is.EqualTo(i < expected ? 1 : 0));
        }
        [Test]
        public void PiercingNeverBypassesThinWall()
        {
            var first = Target(3); var rear = Target(6);
            Box(Vector3.forward * 4, new Vector3(10, 10, .01f));
            Fire(2).Advance(1);
            Assert.That(first.DamageCalls, Is.EqualTo(1)); Assert.That(rear.DamageCalls, Is.Zero);
        }
        [Test]
        public void MuzzleBeyondWallSpawnsOnNearSideAndDoesNotHitRearTarget()
        {
            var prefab = New("prefab", Vector3.left * 100).AddComponent<PlasmaProjectile>();
            prefab.gameObject.AddComponent<MeshRenderer>();
            var visual = New("burn visual", Vector3.left * 100); visual.AddComponent<MeshRenderer>();
            typeof(PlasmaProjectile).GetField("_burnVisualPrefab", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(prefab, visual);
            var camera = New("camera", Vector3.zero).AddComponent<Camera>(); camera.enabled = false;
            var muzzle = New("muzzle", Vector3.forward * 2).transform;
            Box(Vector3.forward, new Vector3(10, 10, .01f)); var rear = Target(3);
            Physics.SyncTransforms();
            var projectile = PlasmaProjectile.Launch(prefab, camera, muzzle, _source,
                new PlasmaConfig(60, pierceCount: 2), 25, 5, 100, ~0, null);
            _objects.Add(projectile.gameObject);
            Assert.That(projectile.transform.position.z, Is.LessThan(_origin.z + 1));
            projectile.Advance(1); Assert.That(projectile.IsResolved, Is.True); Assert.That(rear.DamageCalls, Is.Zero);
        }
        [Test]
        public void InitialOverlapAndRepeatedFramesDoNotRehitOrTrapProjectile()
        {
            var first = Target(0); var second = Target(3);
            var projectile = Fire(1); projectile.Advance(.001f); projectile.Advance(.001f); projectile.Advance(.1f);
            Assert.That(first.DamageCalls, Is.EqualTo(1)); Assert.That(second.DamageCalls, Is.EqualTo(1));
            Assert.That(projectile.IsResolved, Is.True);
        }
        [Test]
        public void RejectedTargetConsumesBudgetAndDoesNotBurn()
        {
            var first = Target(3); first.Reject = true; var second = Target(6);
            Fire(0, true).Advance(1);
            Assert.That(first.GetComponent<PlasmaBurn>(), Is.Null);
            Assert.That(second.DamageCalls, Is.Zero);
        }
        [Test]
        public void BurnRefreshDoesNotStackOrDelayTickAndExpiryIsBounded()
        {
            var target = Target(3); var collider = target.GetComponent<Collider>();
            PlasmaBurn.Apply(_source, target, collider, 5, null, null);
            var burn = target.GetComponent<PlasmaBurn>(); burn.Advance(.4f);
            PlasmaBurn.Apply(_source, target, collider, 10, null, null);
            Assert.That(target.GetComponents<PlasmaBurn>().Length, Is.EqualTo(1));
            burn.Advance(.1f); Assert.That(target.DamageCalls, Is.EqualTo(1));
            burn.Advance(100); Assert.That(target.DamageCalls, Is.EqualTo(6));
            Assert.That(target.Health, Is.EqualTo(940));
            burn.Advance(100); Assert.That(target.DamageCalls, Is.EqualTo(6));
        }
        [Test]
        public void BurnPauseDisableReuseAndSourceDeathPreventStaleDamage()
        {
            var sourceHealth = _source.AddComponent<HealthComponent>();
            var target = Target(3); var collider = target.GetComponent<Collider>();
            PlasmaBurn.Apply(_source, target, collider, 5, null, null);
            var burn = target.GetComponent<PlasmaBurn>(); burn.Advance(0);
            Assert.That(target.DamageCalls, Is.Zero);
            target.gameObject.SetActive(false); target.gameObject.SetActive(true); burn.Advance(3);
            Assert.That(target.DamageCalls, Is.Zero);
            PlasmaBurn.Apply(_source, target, collider, 5, null, null);
            sourceHealth.ApplyDamage(new DamageInfo(100, null, Vector3.zero, Vector3.forward));
            foreach (var component in target.GetComponents<PlasmaBurn>()) component.Advance(3);
            Assert.That(target.DamageCalls, Is.Zero);
        }
        [Test]
        public void DirectHitAndSixBurnTicksUseIndependentDamageWithoutExtraImpacts()
        {
            var target = Target(3); Fire(0, true).Advance(.1f);
            Assert.That(target.Health, Is.EqualTo(975));
            target.GetComponent<PlasmaBurn>().Advance(3);
            Assert.That(target.Health, Is.EqualTo(945)); Assert.That(target.DamageCalls, Is.EqualTo(7));
        }
        [TestCase(true)] [TestCase(false)]
        public void RangeOrLifeExpiryHasNoDamage(bool range)
        {
            var target = Target(5); var projectile = Fire(range: range ? 1 : 100, lifetime: range ? 2 : .01f);
            projectile.Advance(10); Assert.That(projectile.IsResolved, Is.True); Assert.That(target.DamageCalls, Is.Zero);
        }
        [Test]
        public void SourceDeathDisarmsAndPauseFreezesTravel()
        {
            var health = _source.AddComponent<HealthComponent>(); var target = Target(3); var projectile = Fire();
            projectile.Advance(0); Assert.That(projectile.transform.position, Is.EqualTo(_origin));
            health.ApplyDamage(new DamageInfo(100, null, Vector3.zero, Vector3.forward));
            projectile.Advance(1); Assert.That(projectile.IsResolved, Is.True); Assert.That(target.DamageCalls, Is.Zero);
        }
        [Test]
        public void LethalDirectHitDoesNotAttachBurnAndBurnDeathOccursOnce()
        {
            var target = Target(3); target.Health = 10; Fire(0, true).Advance(1);
            Assert.That(target.GetComponent<PlasmaBurn>(), Is.Null);
            var living = Box(Vector3.right * 10, Vector3.one).AddComponent<HealthComponent>(); living.Initialize(8);
            int deaths = 0; living.Died += (_, __) => deaths++;
            PlasmaBurn.Apply(_source, living, living.GetComponent<Collider>(), 5, null, null);
            living.GetComponent<PlasmaBurn>().Advance(3);
            Assert.That(deaths, Is.EqualTo(1)); Assert.That(living.IsDead, Is.True);
        }
    }
}
