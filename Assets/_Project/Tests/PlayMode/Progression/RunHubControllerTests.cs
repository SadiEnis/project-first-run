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
        private SlotCapacityUpgradeDefinition _ability, _upgrade;
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
            _ability = ScriptableObject.CreateInstance<SlotCapacityUpgradeDefinition>();
            _upgrade = ScriptableObject.CreateInstance<SlotCapacityUpgradeDefinition>();
            JsonUtility.FromJsonOverwrite("{\"_stableId\":\"meta.ability-slots\",\"_displayName\":\"Ability\",\"_startingCapacity\":2,\"_costs\":[75,150,250]}", _ability);
            JsonUtility.FromJsonOverwrite("{\"_stableId\":\"meta.upgrade-slots\",\"_displayName\":\"Upgrade\",\"_startingCapacity\":3,\"_costs\":[50,100,175,275,400]}", _upgrade);
            Set(_hub, "_abilitySlotUpgrade", _ability); Set(_hub, "_upgradeSlotUpgrade", _upgrade);
            _store = new Store(); _loader = new Loader();
            Invoke("ConfigureStore", _store); Invoke("SetReloader", _loader);
            _go.SetActive(true);
        }
        [TearDown] public void Cleanup()
        {
            UnityEngine.Object.DestroyImmediate(_go); UnityEngine.Object.DestroyImmediate(_routes); UnityEngine.Object.DestroyImmediate(_ammo);
            UnityEngine.Object.DestroyImmediate(_ability); UnityEngine.Object.DestroyImmediate(_upgrade);
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
        [Test] public void SlotPurchasesRemainIndependentAndRejectRepeatedOffers()
        {
            Assert.That(_hub.PurchaseAbilitySlot(0), Is.True);
            Assert.That(_hub.PurchaseAbilitySlot(0), Is.False);
            Assert.That(_hub.PurchaseUpgradeSlot(0), Is.True);
            Assert.That(_hub.AbilitySlotLevel, Is.EqualTo(1));
            Assert.That(_hub.UpgradeSlotLevel, Is.EqualTo(1));
            Assert.That(_hub.AmmoLevel, Is.Zero);
            Assert.That(_hub.PermanentBalance, Is.EqualTo(125));
            Assert.That(_store.Saves, Is.EqualTo(2));
            Assert.That(_hub.RefreshBalance(), Is.True);
            Assert.That(_ability.GetCapacity(_hub.AbilitySlotLevel), Is.EqualTo(3));
            Assert.That(_upgrade.GetCapacity(_hub.UpgradeSlotLevel), Is.EqualTo(4));
        }
        [UnityTest] public IEnumerator MerchantBlocksOnlyHubMovementAndPreventsDepartureUntilClosed()
        {
            var rig = new GameObject("Hub test exploration"); rig.SetActive(false);
            var player = new GameObject("Hub movement"); player.transform.SetParent(rig.transform);
            var camera = new GameObject("Hub camera").AddComponent<Camera>(); camera.transform.SetParent(player.transform);
            camera.transform.localPosition = Vector3.up * 1.6f;
            player.AddComponent<CharacterController>();
            var input = player.AddComponent<ProjectFirstRun.Input.PlayerInputReader>();
            var motor = player.AddComponent<ProjectFirstRun.Player.PlayerMotor>();
            var look = player.AddComponent<ProjectFirstRun.Player.PlayerLook>(); Set(look, "_cameraPivot", camera.transform);
            var control = player.AddComponent<ProjectFirstRun.Player.PlayerController>();
            var vendor = GameObject.CreatePrimitive(PrimitiveType.Cube); vendor.transform.SetParent(rig.transform);
            vendor.transform.position = new Vector3(0, 1.6f, 2);
            var exit = new GameObject("Exit").AddComponent<BoxCollider>(); exit.transform.SetParent(rig.transform);
            exit.transform.position = Vector3.right * 20; exit.isTrigger = true;
            var interaction = rig.AddComponent<RunHubInteraction>();
            Set(interaction, "_hub", _hub); Set(interaction, "_player", control); Set(interaction, "_camera", camera);
            Set(interaction, "_vendor", vendor.GetComponent<Collider>()); Set(interaction, "_exit", exit);
            try
            {
                rig.SetActive(true); Physics.SyncTransforms();
                yield return null;
                yield return null;
                Assert.That(interaction.CanTrade, Is.True);
                Assert.That(interaction.OpenShop(), Is.True);
                Assert.That(input.IsGameplayInputEnabled, Is.False);
                var position = player.transform.position;
                motor.Tick(Vector2.up, false, .1f);
                Assert.That(player.transform.position, Is.EqualTo(position));
                Assert.That(interaction.TryEnterRun(), Is.False);
                Assert.That(_loader.Calls, Is.Zero);
                Assert.That(_store.Saves, Is.Zero, "Opening the vendor is read-only.");
                interaction.Close();
                Assert.That(input.IsGameplayInputEnabled, Is.True);
                _loader.Succeed = true;
                Assert.That(interaction.TryEnterRun(), Is.True);
                Assert.That(interaction.TryEnterRun(), Is.False);
                Assert.That(_loader.Calls, Is.EqualTo(1));
            }
            finally { UnityEngine.Object.DestroyImmediate(rig); }
        }
        private void Invoke(string method, object value) => typeof(RunHubController)
            .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_hub, new[] { value });
        private static void Set(object target, string field, object value) => target.GetType()
            .GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
    }
}
