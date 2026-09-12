using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectFirstRun.Progression;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Progression
{
    public sealed class PlayerExperienceControllerTests
    {
        private GameObject _owner;
        private PlayerExperienceController _controller;

        [SetUp]
        public void SetUp()
        {
            _owner = new GameObject("Experience_Test");
            _controller = _owner.AddComponent<PlayerExperienceController>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_owner);
        }

        [Test]
        public void BeforeInitialization_ReadAndGainFail()
        {
            Assert.That(_controller.IsInitialized, Is.False);
            Assert.Throws<InvalidOperationException>(() => _controller.GainExperience(10));
            Assert.Throws<InvalidOperationException>(() => { _ = _controller.Level; });
        }

        [Test]
        public void Initialization_RejectsNullAndReplacementWithoutLosingState()
        {
            Assert.Throws<ArgumentNullException>(() => _controller.Initialize(null));
            Assert.That(_controller.IsInitialized, Is.False);
            Initialize();
            _controller.GainExperience(140);
            Assert.Throws<InvalidOperationException>(() => _controller.Initialize(CreateState()));
            Assert.That(_controller.Level, Is.EqualTo(2));
            Assert.That(_controller.CurrentExperience, Is.EqualTo(40));
        }

        [Test]
        public void ExistingRun_CanBeBoundAfterSceneAdapterIsRecreated()
        {
            var state = CreateState();
            _controller.Initialize(state);
            _controller.GainExperience(140);
            UnityEngine.Object.DestroyImmediate(_owner);
            _owner = new GameObject("Experience_Recreated");
            _controller = _owner.AddComponent<PlayerExperienceController>();
            _controller.Initialize(state);
            Assert.That(_controller.Level, Is.EqualTo(2));
            Assert.That(_controller.CurrentExperience, Is.EqualTo(40));
            Assert.That(_controller.TotalExperience, Is.EqualTo(140));
            Assert.That(_controller.RequiredExperience, Is.EqualTo(150));
        }

        [Test]
        public void MultiLevelAward_PublishesOneCommittedResultWithExactLevelCount()
        {
            Initialize();
            var order = new List<string>();
            _controller.ExperienceChanged += result =>
            {
                order.Add("experience");
                Assert.That(_controller.Level, Is.EqualTo(4));
                Assert.That(_controller.CurrentExperience, Is.EqualTo(50));
                Assert.That(result.TotalExperience, Is.EqualTo(500));
            };
            _controller.LevelChanged += result =>
            {
                order.Add("level");
                Assert.That(result.LevelsGained, Is.EqualTo(3));
                Assert.That(result.PreviousLevel, Is.EqualTo(1));
                Assert.That(result.CurrentLevel, Is.EqualTo(4));
            };
            _controller.GainExperience(500);
            Assert.That(order, Is.EqualTo(new[] { "experience", "level" }));
        }

        [Test]
        public void SubThresholdAward_OnlyPublishesExperience()
        {
            Initialize();
            int experienceEvents = 0;
            int levelEvents = 0;
            _controller.ExperienceChanged += _ => experienceEvents++;
            _controller.LevelChanged += _ => levelEvents++;
            _controller.GainExperience(25);
            Assert.That(experienceEvents, Is.EqualTo(1));
            Assert.That(levelEvents, Is.Zero);
        }

        [Test]
        public void ZeroAndNegativeAwards_DoNotPublishEvents()
        {
            Initialize();
            _controller.ExperienceChanged += _ => Assert.Fail("Unexpected XP event.");
            _controller.LevelChanged += _ => Assert.Fail("Unexpected level event.");
            _controller.GainExperience(0);
            Assert.Throws<ArgumentOutOfRangeException>(() => _controller.GainExperience(-1));
            Assert.That(_controller.TotalExperience, Is.Zero);
        }

        [Test]
        public void NestedAward_IsRejectedAndLaterAwardsRemainPossible()
        {
            Initialize();
            _controller.ExperienceChanged += _ =>
                Assert.Throws<InvalidOperationException>(() => _controller.GainExperience(100));
            _controller.GainExperience(100);
            _controller.GainExperience(150);
            Assert.That(_controller.Level, Is.EqualTo(3));
            Assert.That(_controller.TotalExperience, Is.EqualTo(250));
        }

        private void Initialize()
        {
            _controller.Initialize(CreateState());
        }

        private static ExperienceState CreateState()
        {
            return new ExperienceState(new ExperienceCurve(100, 50));
        }
    }
}
