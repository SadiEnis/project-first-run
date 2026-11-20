using System;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Progression;
using UnityEngine;
namespace ProjectFirstRun.Flow
{
    /// <summary>Hub scene boundary: reads the profile, submits purchases and starts a fresh run. Never settles or credits.</summary>
    public sealed class RunHubController : MonoBehaviour
    {
        [SerializeField] private RunSceneRoutes _routes;
        [SerializeField] private StartingAmmoUpgradeDefinition _ammoUpgrade;
        private PersistentMetaWallet _wallet;
        public StartingAmmoUpgradeDefinition AmmoUpgrade => _ammoUpgrade;
        public int AmmoLevel => _wallet != null ? _wallet.GetUpgradeLevel(_ammoUpgrade.StableId) : 0;
        public string PurchaseMessage { get; private set; }
        private IMetaWalletStore _store;
        private IRunSceneReloader _loader = new UnityRunSceneReloader();
        private readonly RunRestartOperation _operation = new();
        private string _readError;
        private bool _started;
        public long? PermanentBalance => _wallet?.Balance;
        public bool IsLoading => _operation.State == RunRestartState.Loading;
        public bool IsReady => PermanentBalance.HasValue && _readError == null;
        public string LastError => _readError ?? _operation.Error?.Message;
        public void ValidateConfiguration()
        {
            if (_routes == null || _ammoUpgrade == null) throw new InvalidOperationException("Assign hub scene routes.");
            _routes.ValidateConfiguration();
            _ammoUpgrade.Validate();
        }
        private void Start()
        {
            _started = true;
            // The old run has retired. This scene now owns its menu's global access state.
            Time.timeScale = 1;
            AudioListener.pause = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _store ??= new JsonMetaWalletStore(Application.persistentDataPath);
            RefreshBalance();
        }
        public bool RefreshBalance()
        {
            if (IsLoading) return false;
            try
            {
                ValidateConfiguration();
                if (_store == null) throw new InvalidOperationException("Hub storage has not initialized.");
                var wallet = new PersistentMetaWallet(_store);
                _ = _ammoUpgrade.GetBonusMagazines(wallet.GetUpgradeLevel(_ammoUpgrade.StableId));
                _wallet = wallet;
                _readError = null;
                PurchaseMessage = null;
                return true;
            }
            catch (Exception error)
            {
                _wallet = null;
                _readError = error.Message;
                return false;
            }
        }
        public bool PurchaseAmmo(int expectedLevel)
        {
            if (!isActiveAndEnabled || IsLoading) return false;
            PurchaseMessage = null;
            if (!RefreshBalance()) return false;
            if (AmmoLevel != expectedLevel) { PurchaseMessage = "Your upgrade level changed. Review the new offer."; return false; }
            if (AmmoLevel >= _ammoUpgrade.MaxLevel) { PurchaseMessage = "Maximum level reached."; return false; }
            int price = _ammoUpgrade.GetNextCost(AmmoLevel);
            if (_wallet.Balance < price) { PurchaseMessage = "Not enough GOLD."; return false; }
            try
            {
                bool purchased = _wallet.TryPurchase(_ammoUpgrade.StableId, expectedLevel, price);
                PurchaseMessage = purchased ? "Purchased. Applies to your next run." : "Offer changed. Please review it again.";
                return purchased;
            }
            catch (Exception error)
            {
                // Do not claim success or keep offering against an uncertain disk revision.
                _wallet = null;
                _readError = error.Message;
                PurchaseMessage = "Purchase not confirmed. Read your save again before continuing.";
                return false;
            }
        }
        public bool StartRun()
        {
            if (!isActiveAndEnabled || IsLoading || !RefreshBalance()) return false;
            DemoEntryController.SkipMenuOnNextLoad(_routes.RunScenePath);
            if (_operation.TryStart(true, _routes.RunScenePath, _loader)) return true;
            DemoEntryController.CancelSkip();
            return false;
        }
        public void Quit()
        {
            if (IsLoading) return;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        internal void ConfigureStore(IMetaWalletStore store)
        {
            if (_started) throw new InvalidOperationException("Configure hub storage before startup.");
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }
        internal void SetReloader(IRunSceneReloader loader)
        {
            if (IsLoading) throw new InvalidOperationException("A scene transition is already in progress.");
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        }
    }
}
