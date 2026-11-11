using System;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Flow;
using ProjectFirstRun.Progression;
using UnityEngine;
namespace ProjectFirstRun.Tests.PlayMode.Progression
{
    public sealed class RunEndControllerTests
    {
        private sealed class Store : IMetaWalletStore
        {
            public MetaWalletSnapshot State = new(0, Array.Empty<string>());
            public MetaWalletSnapshot Load() => State;
            public void Save(MetaWalletSnapshot snapshot) => State = snapshot;
        }
        private GameObject _player, _services;
        private HealthComponent _health;
        private RunEndController _end;
        private RunEconomyController _economy;
        [SetUp] public void Setup()
        {
            _player = new GameObject("player");
            _health = _player.AddComponent<HealthComponent>();
            _services = new GameObject("run end without menus");
            _services.SetActive(false);
            _economy = _services.AddComponent<RunEconomyController>();
            _economy.ConfigureStore(new Store());
            _economy.Add(103);
            _end = _services.AddComponent<RunEndController>();
            Set("_health", _health); Set("_economy", _economy);
            _services.SetActive(true);
        }
        [TearDown] public void Cleanup()
        {
            UnityEngine.Object.DestroyImmediate(_services);
            UnityEngine.Object.DestroyImmediate(_player);
        }
        [Test] public void DeathSettlesImmediatelyWithoutDeathScreenOrRestart()
        {
            _health.ApplyDamage(new DamageInfo(10000, null, Vector3.zero, Vector3.forward));
            Assert.That(_economy.IsSettled, Is.True);
            Assert.That(_economy.PermanentBalance, Is.EqualTo(41));
            Assert.That(_end.TryEndForExit(), Is.True);
            Assert.That(_economy.PermanentBalance, Is.EqualTo(41));
        }
        [Test] public void VictoryThenExitPreservesVictoryAndPaysOnce()
        {
            _end.CompleteVictory();
            Assert.That(_end.TryEndForExit(), Is.True);
            Assert.That(_economy.Result.Outcome, Is.EqualTo(RunEconomyOutcome.Victory));
            Assert.That(_economy.PermanentBalance, Is.EqualTo(103));
        }
        private void Set(string field, object value) => typeof(RunEndController)
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_end, value);
    }
}
