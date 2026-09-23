#if UNITY_EDITOR

using ProjectFirstRun.Arenas;
using ProjectFirstRun.Development.Waves;
using ProjectFirstRun.Player;
using ProjectFirstRun.Waves;
using UnityEngine;

namespace ProjectFirstRun.Development.Arenas
{
    [DisallowMultipleComponent]
    public sealed class ArenaSessionDevelopmentBootstrap : MonoBehaviour
    {
        [SerializeField]
        private ArenaSessionController _arenaSessionController;

        [SerializeField]
        private WaveController _waveController;

        [SerializeField]
        private PlayerDeathController _playerDeathController;
        
        [SerializeField]
        private WaveDevelopmentBootstrap _waveBootstrap;

        [Header("Optional two-session transition showcase (same layout)")]
        [SerializeField] private ArenaWaveDefinition _secondArenaDefinition;
        [SerializeField] private Vector3 _exitPosition = new Vector3(0f, 1f, 7f);
        [SerializeField] private Vector3 _nextEntryPosition = new Vector3(0f, 0.1f, -5f);
        private RunSessionController _run;
        private ArenaTransitionTrigger _exit;
        private Renderer _exitRenderer;
        private Material _exitMaterial;

        private void Start()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            _waveBootstrap.InitializeWaveSystem();

            _arenaSessionController.Initialize(
                _waveController,
                _playerDeathController);

            if (_secondArenaDefinition == null) _arenaSessionController.Begin();
            else BeginTransitionShowcase();
        }

        private void BeginTransitionShowcase()
        {
            var nextRoot = new GameObject("Second Arena Session");
            nextRoot.transform.SetParent(transform);
            var nextWave = nextRoot.AddComponent<WaveController>();
            _waveBootstrap.InitializeAdditionalWaveSystem(nextWave, _secondArenaDefinition);
            var nextArena = nextRoot.AddComponent<ArenaSessionController>();
            nextArena.Initialize(nextWave, _playerDeathController);

            _run = gameObject.AddComponent<RunSessionController>();
            _run.Initialize(new IArenaSession[] { _arenaSessionController, nextArena }, _playerDeathController);
            var transitions = gameObject.AddComponent<ArenaTransitionController>();
            var entry = new GameObject("Second Arena Entry").transform;
            entry.SetParent(transform);
            entry.position = _nextEntryPosition;
            transitions.Configure(_run, _playerDeathController.transform,
                _playerDeathController.GetComponent<ProjectFirstRun.Combat.HealthComponent>(),
                new[] { transform, entry });

            var exitObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            exitObject.name = "Arena Exit (generic trigger showcase)";
            exitObject.transform.SetParent(transform);
            exitObject.transform.position = _exitPosition;
            exitObject.transform.localScale = new Vector3(2f, 2f, 2f);
            exitObject.GetComponent<Collider>().isTrigger = true;
            var body = exitObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            _exit = exitObject.AddComponent<ArenaTransitionTrigger>();
            _exit.Configure(transitions, 0);
            _exitRenderer = exitObject.GetComponent<Renderer>();
            _exitMaterial = _exitRenderer.material;
            _run.Begin();
        }

        private void Update()
        {
            if (_exitMaterial != null)
                _exitMaterial.color = _exit.IsAvailable ? Color.green : Color.gray;
        }

        private void OnGUI()
        {
            if (_run == null || !_run.IsInitialized) return;
            string hint = _exit.IsAvailable ? "Enter green exit to continue" : "Exit locked";
            GUI.Box(new Rect(20f, 485f, 300f, 70f),
                $"Run: {_run.Status} | Arena {_run.State.CurrentArenaIndex + 1}/{_run.State.TotalArenas}\n{hint}");
        }

        private void OnDestroy()
        {
            if (_exitMaterial != null) Destroy(_exitMaterial);
        }

        private bool ValidateReferences()
        {
            bool isValid = true;

            if (_arenaSessionController == null)
            {
                LogMissing(nameof(_arenaSessionController));
                isValid = false;
            }

            if (_waveController == null)
            {
                LogMissing(nameof(_waveController));
                isValid = false;
            }

            if (_playerDeathController == null)
            {
                LogMissing(nameof(_playerDeathController));
                isValid = false;
            }

            return isValid;
        }

        private void LogMissing(
            string fieldName)
        {
            Debug.LogError(
                $"{nameof(ArenaSessionDevelopmentBootstrap)} " +
                $"requires {fieldName}.",
                this);
        }
    }
}

#endif
