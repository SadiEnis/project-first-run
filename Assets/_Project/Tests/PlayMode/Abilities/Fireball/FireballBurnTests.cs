using System.Collections;
using NUnit.Framework;
using ProjectFirstRun.Abilities.Fireball;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Weapons;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ProjectFirstRun.Tests.PlayMode.Abilities.Fireball
{
    public sealed class FireballBurnTests
    {
        GameObject source, target, projectile;
        HealthComponent health;
        Collider collider;
        [SetUp] public void Setup()
        {
            source = new GameObject("burn source");
            target = new GameObject("burn target"); target.transform.position = new Vector3(5000, 5000, 5000);
            health = target.AddComponent<HealthComponent>(); health.Initialize(100);
            collider = target.AddComponent<BoxCollider>();
        }
        [TearDown] public void Cleanup()
        {
            Time.timeScale = 1;
            if (projectile != null) Object.DestroyImmediate(projectile);
            if (target != null) Object.DestroyImmediate(target);
            if (source != null) Object.DestroyImmediate(source);
        }
        void Apply(float damage = 5, float duration = 1.5f) => FireballBurn.Apply(source, health, collider, damage, duration);
        [TestCase(1.5f, 85)] [TestCase(3f, 70)]
        public void DurationProducesThreeOrSixTicks(float duration, float remaining)
        {
            Apply(duration: duration); var burn = target.GetComponent<FireballBurn>(); burn.Advance(10);
            Assert.That(health.CurrentHealth, Is.EqualTo(remaining)); burn.Advance(10);
            Assert.That(health.CurrentHealth, Is.EqualTo(remaining));
        }
        [Test] public void RefreshPreservesTickPhaseAndReplacesSnapshotWithoutStacking()
        {
            Apply(); var burn = target.GetComponent<FireballBurn>(); burn.Advance(.4f); Apply(10);
            Assert.That(target.GetComponents<FireballBurn>().Length, Is.EqualTo(1));
            burn.Advance(.1f); Assert.That(health.CurrentHealth, Is.EqualTo(90));
        }
        [Test] public void PlasmaAndFireballCoexistIndependently()
        {
            Apply(); PlasmaBurn.Apply(source, health, collider, 7, null, null);
            target.GetComponent<FireballBurn>().Advance(.5f); target.GetComponent<PlasmaBurn>().Advance(.5f);
            Assert.That(health.CurrentHealth, Is.EqualTo(88));
        }
        [Test] public void SourceDeathPreventsPendingTicks()
        {
            var hp = source.AddComponent<HealthComponent>(); hp.Initialize(1); Apply();
            var hit = new DamageInfo(1, target, Vector3.zero, Vector3.up); hp.ApplyDamage(in hit);
            target.GetComponent<FireballBurn>().Advance(3); Assert.That(health.CurrentHealth, Is.EqualTo(100));
        }
        [Test] public void DisableAndReuseDoesNotResurrectOldBurn()
        {
            Apply(); var old = target.GetComponent<FireballBurn>(); target.SetActive(false); target.SetActive(true);
            old.Advance(3); Assert.That(health.CurrentHealth, Is.EqualTo(100)); Apply(10);
            foreach (var burn in target.GetComponents<FireballBurn>()) burn.Advance(.5f);
            Assert.That(health.CurrentHealth, Is.EqualTo(90));
        }
        [UnityTest] public IEnumerator ActualPauseFreezesBurn()
        {
            Apply(); Time.timeScale = 0; yield return new WaitForSecondsRealtime(.6f);
            Assert.That(health.CurrentHealth, Is.EqualTo(100));
            Assert.That(target.GetComponent<FireballBurn>().State.Remaining, Is.EqualTo(1.5f));
        }
        [Test] public void DirectHitStartsBurnOnlyOnSurvivingReceiver()
        {
            projectile = new GameObject("fireball"); projectile.transform.position = target.transform.position;
            projectile.AddComponent<SphereCollider>(); projectile.AddComponent<Rigidbody>();
            var ball = projectile.AddComponent<FireballProjectile>(); Physics.SyncTransforms();
            ball.Initialize(Vector3.forward, 25, 12, 5, source, burnDamage: 5);
            ball.Advance(.01f); Assert.That(health.CurrentHealth, Is.EqualTo(75));
            target.GetComponent<FireballBurn>().Advance(.5f); Assert.That(health.CurrentHealth, Is.EqualTo(70));
        }
        [UnityTest] public IEnumerator OwningMapUnloadRemovesBurnAndProjectile()
        {
            var map = SceneManager.CreateScene("Fireball cleanup test");
            SceneManager.MoveGameObjectToScene(target, map); Apply();
            var burn = target.GetComponent<FireballBurn>();
            projectile = new GameObject("map projectile"); projectile.AddComponent<SphereCollider>(); projectile.AddComponent<Rigidbody>();
            var ball = projectile.AddComponent<FireballProjectile>(); ball.Initialize(Vector3.forward, 25, 12, 5, source);
            SceneManager.MoveGameObjectToScene(projectile, map);
            yield return SceneManager.UnloadSceneAsync(map);
            Assert.That(burn == null, Is.True); Assert.That(ball == null, Is.True); Assert.That(source != null, Is.True);
        }
    }
}
