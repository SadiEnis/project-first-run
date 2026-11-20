using System;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Progression;
using ProjectFirstRun.Builds;
using UnityEngine;
namespace ProjectFirstRun.Flow
{
    /// <summary>Hub scene boundary: reads the profile, submits purchases and starts a fresh run. Never settles or credits.</summary>
    public sealed class RunHubController : MonoBehaviour
    {
        [SerializeField] private RunSceneRoutes _routes;
        [SerializeField] private StartingAmmoUpgradeDefinition _ammoUpgrade;
        [SerializeField] private SlotCapacityUpgradeDefinition _abilitySlotUpgrade;
        [SerializeField] private SlotCapacityUpgradeDefinition _upgradeSlotUpgrade;
        public SlotCapacityUpgradeDefinition AbilitySlotUpgrade => _abilitySlotUpgrade;
        public SlotCapacityUpgradeDefinition UpgradeSlotUpgrade => _upgradeSlotUpgrade;
        public int AbilitySlotLevel => _wallet?.GetUpgradeLevel(_abilitySlotUpgrade.StableId) ?? 0;
        public int UpgradeSlotLevel => _wallet?.GetUpgradeLevel(_upgradeSlotUpgrade.StableId) ?? 0;
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
            if (_routes == null) throw new InvalidOperationException("Hub scene routes are not assigned.");
            if (_ammoUpgrade == null) throw new InvalidOperationException("Hub starting ammo upgrade is not assigned.");
            if (_abilitySlotUpgrade == null) throw new InvalidOperationException("Hub ability slot upgrade is not assigned. Reopen the saved Hub scene.");
            if (_upgradeSlotUpgrade == null) throw new InvalidOperationException("Hub upgrade slot upgrade is not assigned. Reopen the saved Hub scene.");
            _routes.ValidateConfiguration();
            _ammoUpgrade.Validate();
            _abilitySlotUpgrade.Validate(PlayerBuildCapacity.MaximumAbilitySlots);
            _upgradeSlotUpgrade.Validate(PlayerBuildCapacity.MaximumUpgradeSlots);
            if (_abilitySlotUpgrade.StableId == _upgradeSlotUpgrade.StableId ||
                _ammoUpgrade.StableId == _abilitySlotUpgrade.StableId || _ammoUpgrade.StableId == _upgradeSlotUpgrade.StableId)
                throw new InvalidOperationException("Meta upgrade IDs must be distinct.");
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
                _ = _abilitySlotUpgrade.GetCapacity(wallet.GetUpgradeLevel(_abilitySlotUpgrade.StableId));
                _ = _upgradeSlotUpgrade.GetCapacity(wallet.GetUpgradeLevel(_upgradeSlotUpgrade.StableId));
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
        public bool PurchaseAmmo(int expectedLevel) =>
            Purchase(_ammoUpgrade.StableId, expectedLevel, _ammoUpgrade.MaxLevel, _ammoUpgrade.GetNextCost);
        public bool PurchaseAbilitySlot(int expectedLevel) =>
            Purchase(_abilitySlotUpgrade.StableId, expectedLevel, _abilitySlotUpgrade.MaxLevel, _abilitySlotUpgrade.GetNextCost);
        public bool PurchaseUpgradeSlot(int expectedLevel) =>
            Purchase(_upgradeSlotUpgrade.StableId, expectedLevel, _upgradeSlotUpgrade.MaxLevel, _upgradeSlotUpgrade.GetNextCost);
        private bool Purchase(string id, int expectedLevel, int maxLevel, Func<int, int> getCost)
        {
            if (!isActiveAndEnabled || IsLoading) return false;
            PurchaseMessage = null;
            if (!RefreshBalance()) return false;
            int level = _wallet.GetUpgradeLevel(id);
            if (level != expectedLevel) { PurchaseMessage = "Your upgrade level changed. Review the new offer."; return false; }
            if (level >= maxLevel) { PurchaseMessage = "Maximum level reached."; return false; }
            int price = getCost(level);
            if (_wallet.Balance < price) { PurchaseMessage = "Not enough GOLD."; return false; }
            try
            {
                bool purchased = _wallet.TryPurchase(id, expectedLevel, price);
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
