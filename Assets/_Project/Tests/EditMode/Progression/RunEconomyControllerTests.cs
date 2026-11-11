using System;
using System.IO;
using NUnit.Framework;
using ProjectFirstRun.Progression;
using UnityEditor;
using UnityEngine;

namespace ProjectFirstRun.Tests.EditMode.Progression
{
    public sealed class RunEconomyControllerTests
    {
        private GameObject _owner;
        private string _directory;
        private RunEconomyController _controller;
        [SetUp] public void Setup()
        {
            _directory = Path.Combine(Path.GetTempPath(), "PFR-controller-tests", Guid.NewGuid().ToString("N"));
            _owner = new GameObject("economy test");
            _controller = _owner.AddComponent<RunEconomyController>();
            _controller.ConfigureStore(new ProjectFirstRun.Progression.JsonMetaWalletStore(_directory));
        }
        [TearDown] public void Cleanup()
        {
            UnityEngine.Object.DestroyImmediate(_owner);
            if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
        }
        [Test]
        public void VictoryThenExitDoesNotApplyDeathDeductionOrPayTwice()
        {
            var data = new SerializedObject(_controller);
            data.FindProperty("_bossReward").intValue = 50;
            data.ApplyModifiedPropertiesWithoutUndo();
            _controller.Add(100);
            Assert.That(_controller.Complete(RunEconomyOutcome.Victory), Is.True);
            Assert.That(_controller.Complete(RunEconomyOutcome.Death), Is.True);
            Assert.That(new PersistentMetaWallet(new JsonMetaWalletStore(_directory)).Balance, Is.EqualTo(150));
            Assert.That(_controller.Result.Outcome, Is.EqualTo(RunEconomyOutcome.Victory));
        }
        [Test]
        public void FailedSaveKeepsOutcomeAndRetryPaysExactlyOnce()
        {
            var wallet = new PersistentMetaWallet(new JsonMetaWalletStore(_directory));
            Directory.CreateDirectory(System.IO.Path.Combine(_directory, "meta-wallet.json") + ".tmp");
            _controller.Add(103);
            Assert.That(_controller.Complete(RunEconomyOutcome.Death), Is.False);
            Assert.That(_controller.LastError, Is.Not.Null);
            Assert.That(_controller.IsSettled, Is.False);
            Assert.Throws<InvalidOperationException>(() => _controller.Add(1));
            Directory.Delete(System.IO.Path.Combine(_directory, "meta-wallet.json") + ".tmp");
            Assert.That(_controller.Complete(RunEconomyOutcome.Victory), Is.True);
            Assert.That(_controller.LastError, Is.Null);
            Assert.That(_controller.PermanentBalance, Is.EqualTo(41));
            Assert.That(_controller.Result.Outcome, Is.EqualTo(RunEconomyOutcome.Death));
            Assert.That(_controller.Complete(RunEconomyOutcome.Death), Is.True);
            Assert.That(new PersistentMetaWallet(new JsonMetaWalletStore(_directory)).Balance, Is.EqualTo(41));
        }
    }
}
