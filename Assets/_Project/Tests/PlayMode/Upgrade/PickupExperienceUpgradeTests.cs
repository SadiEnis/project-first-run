using System;
using NUnit.Framework;
using ProjectFirstRun.Progression;
using ProjectFirstRun.Stats;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectFirstRun.Tests.PlayMode.Upgrades
{
    public sealed class PickupExperienceUpgradeTests
    {
        private GameObject _player;
        private PlayerExperienceController _xp;
        private PlayerStatCollection _stats;
        private PlayerExperienceCollector _collector;
        private float _timeScale;

        [SetUp]
        public void SetUp()
        {
            _timeScale = Time.timeScale;
            Time.timeScale = 1;
            _player = new GameObject("XP upgrade test");
            _stats = _player.AddComponent<PlayerStatsController>().Stats;
            _xp = _player.AddComponent<PlayerExperienceController>();
            _xp.Initialize(new ExperienceState(new ExperienceCurve(100, 50)));
            _collector = _player.AddComponent<PlayerExperienceCollector>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_player);
            Time.timeScale = _timeScale;
        }

        private void Bonus(PlayerStatType stat, float amount, string source = "test") =>
            _stats.Add(new StatModifier(stat, StatModifierOperation.AdditivePercent, amount, source));

        [Test]
        public void RadiusUsesBaseAndMatchingBoundariesWithoutCompounding()
        {
            _collector.SetAttractionRadius(3);
            Bonus(PlayerStatType.PickupRadius, 1);
            for (int i = 0; i < 5; i++)
                Assert.That(_collector.AttractionRadius, Is.EqualTo(6));
            Assert.That(_collector.CanAttract(_collector.CollectionPosition + Vector3.right * 6), Is.True);
            Assert.That(_collector.CanAttract(_collector.CollectionPosition + Vector3.right * 6.01f), Is.False);
            _collector.SetAttractionRadius(4);
            Assert.That(_collector.AttractionRadius, Is.EqualTo(8));
            _stats.RemoveBySource("test");
            Assert.That(_collector.AttractionRadius, Is.EqualTo(4));
            Time.timeScale = 0;
            Assert.That(_collector.CanAttract(_collector.CollectionPosition), Is.False);
        }

        [Test]
        public void TenSmallAwardsProduceElevenAndMultiplierChangeKeepsRemainder()
        {
            Bonus(PlayerStatType.ExperienceGain, .1f);
            for (int i = 0; i < 10; i++) _xp.GainExperience(1);
            Assert.That(_xp.TotalExperience, Is.EqualTo(11));
            Assert.That(_xp.FractionalExperience, Is.Zero);
            _xp.GainExperience(1);
            Assert.That(_xp.FractionalExperience, Is.EqualTo(.1m));
            _stats.RemoveBySource("test");
            Bonus(PlayerStatType.ExperienceGain, .5f);
            _xp.GainExperience(1);
            Assert.That(_xp.FractionalExperience, Is.EqualTo(.6m));
            _xp.GainExperience(1);
            Assert.That(_xp.TotalExperience, Is.EqualTo(15));
            Assert.That(_xp.FractionalExperience, Is.EqualTo(.1m));
        }

        [Test]
        public void FailedAndZeroAwardsPreserveFractionAndNotificationsCannotReenter()
        {
            Bonus(PlayerStatType.ExperienceGain, .1f);
            _xp.GainExperience(1);
            Assert.Throws<ArgumentOutOfRangeException>(() => _xp.GainExperience(-1));
            Assert.Throws<OverflowException>(() => _xp.GainExperience(int.MaxValue));
            _xp.GainExperience(0);
            Assert.That(_xp.TotalExperience, Is.EqualTo(1));
            Assert.That(_xp.FractionalExperience, Is.EqualTo(.1m));
            _xp.ExperienceChanged += _ =>
            {
                Assert.Throws<InvalidOperationException>(() => _xp.GainExperience(1));
                throw new InvalidOperationException("observer");
            };
            Assert.Throws<InvalidOperationException>(() => _xp.GainExperience(1));
            Assert.That(_xp.TotalExperience, Is.EqualTo(2));
            Assert.That(_xp.FractionalExperience, Is.EqualTo(.2m));
        }

        [Test]
        public void PickupUsesCollectionTimeBonusAndCannotRepeatCommittedAward()
        {
            var pickupObject = new GameObject("XP pickup");
            try
            {
                var pickup = pickupObject.AddComponent<ExperiencePickup>();
                pickup.Initialize(10);
                float speed = pickup.AttractionSpeed;
                Bonus(PlayerStatType.ExperienceGain, .5f);
                Bonus(PlayerStatType.PickupRadius, 1, "radius");
                _xp.ExperienceChanged += _ => throw new InvalidOperationException("observer");
                Assert.Throws<InvalidOperationException>(() => pickup.TryCollect(_collector));
                Assert.That(pickup.IsCollected, Is.True);
                Assert.That(pickup.TryCollect(_collector), Is.False);
                Assert.That(_xp.TotalExperience, Is.EqualTo(15));
                Assert.That(pickup.AttractionSpeed, Is.EqualTo(speed));
            }
            finally { if (pickupObject != null) Object.DestroyImmediate(pickupObject); }
        }

        [Test]
        public void BonusAllowsMultipleLevelsThroughExistingNotifications()
        {
            Bonus(PlayerStatType.ExperienceGain, .5f);
            int levels = 0;
            _xp.LevelChanged += result => levels += result.LevelsGained;
            _xp.GainExperience(200);
            Assert.That(_xp.TotalExperience, Is.EqualTo(300));
            Assert.That(_xp.Level, Is.EqualTo(3));
            Assert.That(levels, Is.EqualTo(2));
            Assert.That(_xp.CurrentExperience, Is.EqualTo(50));
        }

        [Test]
        public void SceneRelocationRetainsFractionAndFreshRunStartsEmpty()
        {
            Bonus(PlayerStatType.ExperienceGain, .1f);
            _xp.GainExperience(1);
            var source = _player.scene;
            var destination = SceneManager.CreateScene("XP upgrade destination");
            try
            {
                SceneManager.MoveGameObjectToScene(_player, destination);
                for (int i = 0; i < 9; i++) _xp.GainExperience(1);
                Assert.That(_xp.TotalExperience, Is.EqualTo(11));
                var fresh = new GameObject("Fresh run");
                try
                {
                    var xp = fresh.AddComponent<PlayerExperienceController>();
                    xp.Initialize(new ExperienceState(new ExperienceCurve(100, 50)));
                    Assert.That(xp.FractionalExperience, Is.Zero);
                    xp.GainExperience(1);
                    Assert.That(xp.TotalExperience, Is.EqualTo(1));
                }
                finally { Object.DestroyImmediate(fresh); }
            }
            finally
            {
                SceneManager.MoveGameObjectToScene(_player, source);
                SceneManager.UnloadSceneAsync(destination);
            }
        }
    }
}
