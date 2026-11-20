using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using ProjectFirstRun.Flow;
using ProjectFirstRun.Progression;
using ProjectFirstRun.Development.Arenas;
using UnityEngine;
using UnityEngine.TestTools;
namespace ProjectFirstRun.Tests.PlayMode.Progression
{
    public sealed class RunHubControllerTests
    {
        private sealed class Store : IMetaWalletStore
        {
            public bool Fail;
            public bool FailSave;
            public int Saves;
            public MetaWalletSnapshot State = new(250, Array.Empty<string>());
            public MetaWalletSnapshot Load() => Fail ? throw new InvalidOperationException("Read failed") : State;
            public void Save(MetaWalletSnapshot snapshot) { if (FailSave) throw new InvalidOperationException("Save failed"); Saves++; State = snapshot; }
        }
        private sealed class Loader : IRunSceneReloader
        {
            public int Calls; public bool Succeed; public string Path;
            public bool Begin(string path) { Calls++; Path = path; return Succeed; }
        }
        private GameObject _go;
        private RunHubController _hub;
        private RunSceneRoutes _routes;
        private StartingAmmoUpgradeDefinition _ammo;
        private Store _store;
        private Loader _loader;
        private float _time;
        private bool _audio, _cursor;
        private CursorLockMode _lock;
        [SetUp] public void Setup()
        {
            _time = Time.timeScale; _audio = AudioListener.pause; _cursor = Cursor.visible; _lock = Cursor.lockState;
            _routes = ScriptableObject.CreateInstance<RunSceneRoutes>();
            Set(_routes, "_hubScenePath", "Assets/Hub.unity"); Set(_routes, "_runScenePath", "Assets/Run.unity");
            _go = new GameObject("hub test"); _go.SetActive(false);
            _hub = _go.AddComponent<RunHubController>();
            Set(_hub, "_routes", _routes);
            _ammo = ScriptableObject.CreateInstance<StartingAmmoUpgradeDefinition>();
            JsonUtility.FromJsonOverwrite("{\"_levels\":[{\"cost\":50,\"totalBonusMagazines\":1},{\"cost\":100,\"totalBonusMagazines\":2}]}", _ammo);
            Set(_hub, "_ammoUpgrade", _ammo);
            _store = new Store(); _loader = new Loader();
            Invoke("ConfigureStore", _store); Invoke("SetReloader", _loader);
            _go.SetActive(true);
        }
        [TearDown] public void Cleanup()
        {
            UnityEngine.Object.DestroyImmediate(_go); UnityEngine.Object.DestroyImmediate(_routes); UnityEngine.Object.DestroyImmediate(_ammo);
            DemoEntryController.CancelSkip();
            Time.timeScale = _time; AudioListener.pause = _audio; Cursor.visible = _cursor; Cursor.lockState = _lock;
        }
        [UnityTest] public IEnumerator ArrivalRestoresMenuAccessAndReadsWalletWithoutPayingAgain()
        {
            var wallet = new PersistentMetaWallet(_store);
            wallet.Settle(RunEconomyResult.Calculate(Guid.NewGuid().ToString("N"), 100, RunEconomyOutcome.Death));
            int writes = _store.Saves;
            Time.timeScale = 0; AudioListener.pause = true;
            yield return null;
            Assert.That(_hub.PermanentBalance, Is.EqualTo(290));
            Assert.That(Time.timeScale, Is.EqualTo(1)); Assert.That(AudioListener.pause, Is.False);
            Assert.That(Cursor.lockState, Is.EqualTo(CursorLockMode.None));
            _hub.RefreshBalance();
            Assert.That(_store.Saves, Is.EqualTo(writes), "Hub reads must not settle or credit.");
        }
        [Test] public void FailedReadBlocksRunAndDoesNotInventZeroBalance()
        {
            _store.Fail = true;
            Assert.That(_hub.StartRun(), Is.False);
            Assert.That(_hub.PermanentBalance, Is.Null);
            Assert.That(_hub.IsReady, Is.False); Assert.That(_loader.Calls, Is.Zero);
            _store.Fail = false;
            Assert.That(_hub.RefreshBalance(), Is.True);
            Assert.That(_hub.PermanentBalance, Is.EqualTo(250));
            Assert.That(_store.Saves, Is.Zero);
        }
        [Test] public void FailedLoadCanRetryAndSuccessfulLoadRejectsDoubleSubmit()
        {
            Assert.That(_hub.StartRun(), Is.False);
            Assert.That(_hub.LastError, Is.Not.Null);
            Assert.That(_hub.PermanentBalance, Is.EqualTo(250));
            _loader.Succeed = true;
            Assert.That(_hub.StartRun(), Is.True);
            Assert.That(_hub.StartRun(), Is.False);
            Assert.That(_loader.Calls, Is.EqualTo(2));
            Assert.That(_loader.Path, Is.EqualTo(_routes.RunScenePath));
            Assert.That(_hub.LastError, Is.Null); Assert.That(_store.Saves, Is.Zero);
        }
        [Test] public void PurchaseUsesQuotedLevelAndStopsAtMaximum()
        {
            Assert.That(_hub.PurchaseAmmo(0), Is.True);
            Assert.That(_hub.PermanentBalance, Is.EqualTo(200));
            Assert.That(_hub.PurchaseAmmo(0), Is.False);
            Assert.That(_hub.PurchaseAmmo(1), Is.True);
            Assert.That(_hub.PermanentBalance, Is.EqualTo(100));
            Assert.That(_hub.PurchaseAmmo(2), Is.False);
            Assert.That(_store.Saves, Is.EqualTo(2));
            Assert.That(_hub.AmmoLevel, Is.EqualTo(2));
        }
        [Test] public void FailedPurchaseRequiresRefreshAndPreservesOriginalProfile()
        {
            _store.FailSave = true;
            Assert.That(_hub.PurchaseAmmo(0), Is.False);
            Assert.That(_store.State.Balance, Is.EqualTo(250));
            Assert.That(_store.State.GetUpgradeLevel(_ammo.StableId), Is.Zero);
            Assert.That(_hub.IsReady, Is.False);
            _store.FailSave = false;
            Assert.That(_hub.RefreshBalance(), Is.True);
            Assert.That(_hub.AmmoLevel, Is.Zero);
            Assert.That(_hub.PurchaseAmmo(0), Is.True);
        }
        [Test] public void InsufficientFundsNeverWritesAndLoadingDisallowsPurchase()
        {
            _store.State = new MetaWalletSnapshot(1, Array.Empty<string>());
            Assert.That(_hub.PurchaseAmmo(0), Is.False);
            Assert.That(_store.Saves, Is.Zero);
            _loader.Succeed = true;
            Assert.That(_hub.StartRun(), Is.True);
            Assert.That(_hub.PurchaseAmmo(0), Is.False);
            Assert.That(_store.Saves, Is.Zero);
        }
        private void Invoke(string method, object value) => typeof(RunHubController)
            .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_hub, new[] { value });
        private static void Set(object target, string field, object value) => target.GetType()
            .GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
    }
}
