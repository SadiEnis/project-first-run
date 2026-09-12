using System;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Combat
{
    public sealed class HealthComponentTests
    {
        private GameObject _gameObject;
        private HealthComponent _healthComponent;

        [SetUp]
        public void SetUp()
        {
            _gameObject =
                new GameObject("HealthComponentTestObject");

            _healthComponent =
                _gameObject.AddComponent<HealthComponent>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    _gameObject);
            }
        }

        [Test]
        public void Initialize_WithValidMaximumHealth_StartsAtMaximum()
        {
            _healthComponent.Initialize(150f);

            Assert.That(
                _healthComponent.MaximumHealth,
                Is.EqualTo(150f));

            Assert.That(
                _healthComponent.CurrentHealth,
                Is.EqualTo(150f));

            Assert.That(
                _healthComponent.IsDead,
                Is.False);
        }

        [Test]
        public void Initialize_AfterReceivingDamage_RecreatesHealthState()
        {
            _healthComponent.Initialize(100f);

            DamageInfo damageInfo = new DamageInfo(
                amount: 40f,
                source: null,
                hitPoint: Vector3.zero,
                hitDirection: Vector3.forward);

            _healthComponent.ApplyDamage(in damageInfo);

            Assert.That(
                _healthComponent.CurrentHealth,
                Is.EqualTo(60f));

            _healthComponent.Initialize(200f);

            Assert.That(
                _healthComponent.MaximumHealth,
                Is.EqualTo(200f));

            Assert.That(
                _healthComponent.CurrentHealth,
                Is.EqualTo(200f));

            Assert.That(
                _healthComponent.IsDead,
                Is.False);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Initialize_WithInvalidMaximumHealth_Throws(
            float maximumHealth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _healthComponent.Initialize(maximumHealth));
        }

        [Test]
        public void ResetHealth_AfterInitialization_RestoresConfiguredMaximum()
        {
            _healthComponent.Initialize(175f);

            DamageInfo damageInfo = new DamageInfo(
                amount: 75f,
                source: null,
                hitPoint: Vector3.zero,
                hitDirection: Vector3.forward);

            _healthComponent.ApplyDamage(in damageInfo);

            Assert.That(
                _healthComponent.CurrentHealth,
                Is.EqualTo(100f));

            _healthComponent.ResetHealth();

            Assert.That(
                _healthComponent.CurrentHealth,
                Is.EqualTo(175f));

            Assert.That(
                _healthComponent.MaximumHealth,
                Is.EqualTo(175f));
        }
    }
}