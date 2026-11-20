using System;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Progression;
using UnityEngine;
namespace ProjectFirstRun.Flow
{
    /// <summary>Hub scene boundary: reads the saved wallet and starts a fresh run. Never settles or credits.</summary>
    public sealed class RunHubController : MonoBehaviour
    {
        [SerializeField] private RunSceneRoutes _routes;
        private IMetaWalletStore _store;
        private IRunSceneReloader _loader = new UnityRunSceneReloader();
        private readonly RunRestartOperation _operation = new();
        private string _readError;
        private bool _started;
        public long? PermanentBalance { get; private set; }
        public bool IsLoading => _operation.State == RunRestartState.Loading;
        public bool IsReady => PermanentBalance.HasValue && _readError == null;
        public string LastError => _readError ?? _operation.Error?.Message;
        public void ValidateConfiguration()
        {
            if (_routes == null) throw new InvalidOperationException("Assign hub scene routes.");
            _routes.ValidateConfiguration();
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
                PermanentBalance = _store.Load().Balance;
                _readError = null;
                return true;
            }
            catch (Exception error)
            {
                PermanentBalance = null;
                _readError = error.Message;
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
