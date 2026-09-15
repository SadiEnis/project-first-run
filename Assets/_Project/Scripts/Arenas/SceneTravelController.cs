using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Chests.Interaction;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Input;
using ProjectFirstRun.Player;
using ProjectFirstRun.Weapons;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Arenas
{
    public enum SceneTravelStatus { Idle, Loading, Succeeded, Failed, Cancelled, CleanupFailed }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthComponent), typeof(PlayerMotor))]
    public sealed class SceneTravelController : MonoBehaviour
    {
        private IMapSceneLoader _loader = new UnityMapSceneLoader();
        private bool _busy, _cancel, _committed;
        private GameplayFreeze _freeze;
        private Scene _cleanupScene;
        private MapTraversalSession _sourceSession;
        private object _reservation;

        public bool IsBusy => _busy || (_cleanupScene.IsValid() && _cleanupScene.isLoaded);
        public SceneTravelStatus Status { get; private set; }
        public string LastError { get; private set; }
        public MapSceneRoot CurrentMap { get; private set; }
        public Task CurrentTravel { get; private set; } = Task.CompletedTask;

        public void ConfigureLoader(IMapSceneLoader loader)
        {
            if (IsBusy) throw new InvalidOperationException("Cannot replace a busy loader.");
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        }

        public bool TryTravel(MapSceneRoot source, SceneTravelDestination destination)
        {
            if (IsBusy || !isActiveAndEnabled || Time.timeScale <= 0 ||
                transform.parent != null || source == null || source.Map == null ||
                source.gameObject.scene != gameObject.scene || GetComponent<HealthComponent>().IsDead)
                return false;
            try
            {
                if (destination == null) throw new ArgumentNullException(nameof(destination));
                destination.Validate();
                if (!_loader.CanLoad(destination.ScenePath) ||
                    SceneManager.GetSceneByPath(destination.ScenePath).isLoaded ||
                    source.gameObject.scene.path == destination.ScenePath)
                    throw new InvalidOperationException("Target must be build-enabled and not already loaded.");
                _sourceSession = source.Map.Session;
                if (!_sourceSession.TryReserveDeparture(out _reservation)) return false;
            }
            catch (Exception error)
            {
                LastError = error.Message;
                Status = SceneTravelStatus.Failed;
                return false;
            }
            _busy = true;
            _cancel = _committed = false;
            LastError = null;
            Status = SceneTravelStatus.Loading;
            CurrentMap = source;
            _freeze = new GameplayFreeze(gameObject);
            CurrentTravel = TravelAsync(source.gameObject.scene, destination);
            return true;
        }

        public void Cancel() { if (!_committed) _cancel = true; }

        public bool RetryCleanup()
        {
            if (_busy || !_cleanupScene.IsValid() || !_cleanupScene.isLoaded) return false;
            _busy = true;
            CurrentTravel = RetryCleanupAsync();
            return true;
        }

        private async Task TravelAsync(Scene source, SceneTravelDestination destination)
        {
            Scene target = default;
            Scene previousActive = SceneManager.GetActiveScene();
            Vector3 previousPosition = transform.position;
            Quaternion previousRotation = transform.rotation;
            try
            {
                target = await _loader.LoadAsync(destination.ScenePath);
                EnsureCanCommit(source);
                if (target == source || !target.IsValid() || !target.isLoaded)
                    throw new InvalidOperationException("Loader returned an invalid target.");
                MapSceneRoot map = MapSceneRoot.FindIn(target);
                MapSceneEntry entry = map.ValidateDestination(destination.EntryId);
                map.BindPlayer(gameObject, entry);
                EnsureCanCommit(source);
                if (!SceneManager.SetActiveScene(target)) throw new InvalidOperationException("Cannot activate target scene.");
                SceneManager.MoveGameObjectToScene(gameObject, target);
                GetComponent<PlayerMotor>().Teleport(entry.Point.position, entry.Point.rotation);
                // From here onward old-map cleanup cannot safely be rolled back.
                _committed = true;
                CurrentMap = map;
                map.BindRuntimeTargets(gameObject);
                foreach (GameObject root in source.GetRootGameObjects()) root.SetActive(false);
                map.Activate();
                await _loader.UnloadAsync(source);
                if (source.IsValid() && source.isLoaded) throw new InvalidOperationException("Old map remains loaded.");
                Status = SceneTravelStatus.Succeeded;
            }
            catch (Exception error)
            {
                LastError = error.Message;
                Status = error is OperationCanceledException ? SceneTravelStatus.Cancelled : SceneTravelStatus.Failed;
                if (_committed)
                {
                    _cleanupScene = source;
                    Status = SceneTravelStatus.CleanupFailed;
                }
                else
                {
                    if (this != null && source.IsValid() && source.isLoaded)
                    {
                        if (gameObject.scene != source) SceneManager.MoveGameObjectToScene(gameObject, source);
                        GetComponent<PlayerMotor>().Teleport(previousPosition, previousRotation);
                    }
                    if (previousActive.IsValid() && previousActive.isLoaded) SceneManager.SetActiveScene(previousActive);
                    if (target.IsValid() && target != source && target.isLoaded)
                    {
                        try
                        {
                            foreach (var root in target.GetRootGameObjects()) root.SetActive(false);
                            await _loader.UnloadAsync(target);
                            if (target.IsValid() && target.isLoaded)
                                throw new InvalidOperationException("Rejected target remains loaded.");
                        }
                        catch (Exception cleanupError)
                        {
                            _cleanupScene = target;
                            Status = SceneTravelStatus.CleanupFailed;
                            LastError += " Cleanup: " + cleanupError.Message;
                        }
                    }
                }
            }
            finally
            {
                _sourceSession?.ReleaseDeparture(_reservation);
                _reservation = null;
                _freeze?.Dispose();
                _freeze = null;
                _busy = false;
            }
        }

        private async Task RetryCleanupAsync()
        {
            try
            {
                await _loader.UnloadAsync(_cleanupScene);
                if (_cleanupScene.IsValid() && _cleanupScene.isLoaded)
                    throw new InvalidOperationException("Cleanup scene remains loaded.");
                _cleanupScene = default;
                Status = _committed ? SceneTravelStatus.Succeeded : SceneTravelStatus.Cancelled;
                LastError = null;
            }
            catch (Exception error) { Status = SceneTravelStatus.CleanupFailed; LastError = error.Message; }
            finally { _busy = false; }
        }

        private void EnsureCanCommit(Scene source)
        {
            if (_cancel || this == null || !isActiveAndEnabled || GetComponent<HealthComponent>().IsDead ||
                !source.IsValid() || !source.isLoaded)
                throw new OperationCanceledException("Travel cancelled before arrival.");
        }

        private void OnDisable() => Cancel();
        private void OnDestroy()
        {
            Cancel();
            _freeze?.Dispose();
        }

        private sealed class GameplayFreeze : IDisposable
        {
            private readonly List<(Behaviour component, bool enabled)> _states = new List<(Behaviour, bool)>();
            private readonly float _timeScale;
            private readonly PlayerInputReader _input;
            private readonly HealthComponent _health;
            private readonly bool _inputWasEnabled;
            private bool _disposed;
            public GameplayFreeze(GameObject player)
            {
                _timeScale = Time.timeScale;
                _input = player.GetComponent<PlayerInputReader>();
                _health = player.GetComponent<HealthComponent>();
                _inputWasEnabled = _input != null && _input.IsGameplayInputEnabled;
                Time.timeScale = 0;
                Capture(player.GetComponent<PlayerController>());
                Capture(player.GetComponent<PlayerWeaponController>());
                Capture(player.GetComponent<PlayerAbilityController>());
                Capture(player.GetComponent<PlayerInputReader>());
                Capture(player.GetComponent<PlayerWeaponSwitcher>());
                Capture(player.GetComponent<PlayerChestInteractor>());
            }
            private void Capture(Behaviour component)
            {
                if (component == null) return;
                _states.Add((component, component.enabled));
                component.enabled = false;
            }
            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                foreach (var state in _states)
                    if (state.component != null) state.component.enabled = state.enabled;
                if (_input != null)
                    _input.SetGameplayInputEnabled(_inputWasEnabled && _health != null && !_health.IsDead);
                Time.timeScale = _timeScale;
            }
        }
    }
}
