using System;
using System.Collections;
using ProjectFirstRun.Arenas;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Chests;
using ProjectFirstRun.Chests.Spawning;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Progression;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Development.Arenas
{
    /// <summary>First demo slice only; geometry and dependencies are authored in the saved scene.</summary>
    public sealed class DemoOpeningController : MonoBehaviour
    {
        [SerializeField] private PlayerStartingLoadoutInitializer _loadout;
        [SerializeField] private HealthComponent _health;
        [SerializeField] private PlayerExperienceController _experience;
        [SerializeField] private PlayerWeaponController _weapon;
        [SerializeField] private PreparedRegionEncounter _encounter;
        [SerializeField] private ChestSpawner _chests;
        [SerializeField] private ChestDefinition _firstReward;
        [SerializeField] private Transform _rewardPoint;
        [SerializeField] private RewardSelectionController _selection;
        [SerializeField] private PreparedRegionEncounter[] _roomEncounters = Array.Empty<PreparedRegionEncounter>();
        [SerializeField] private EncounterChestReward _optionalReward;
        [SerializeField] private DemoKeyAmbushController _keyAmbush;
        [SerializeField] private DemoFinalController _final;
        [SerializeField] private ProjectFirstRun.UI.Hud.DemoHudController _hud;
        private bool _restarting;
        private bool _rewardClaimed;
        public bool IsReady { get; private set; }
        public ChestController Reward { get; private set; }
        public PreparedRegionEncounter Encounter => _encounter;
        public HealthComponent Health => _health;
        public bool RewardClaimed => _rewardClaimed;

        public void ValidateConfiguration()
        {
            if (_loadout == null || _health == null || _experience == null || _weapon == null ||
                _encounter == null || _chests == null || _firstReward == null ||
                _rewardPoint == null || _selection == null)
                throw new InvalidOperationException("Assign all demo opening scene references.");
            _firstReward.Validate();
            foreach (var room in _roomEncounters)
                if (room == null) throw new InvalidOperationException("Missing demo room encounter reference.");
            if (_roomEncounters.Length > 0 && _optionalReward == null)
                throw new InvalidOperationException("Assign the optional room reward.");
            if (_optionalReward != null) _optionalReward.ValidateConfiguration();
        }

        private void Awake()
        {
            ValidateConfiguration();
            // Unlike the Editor-only weapon-switching fixture, this also executes in a player build.
            if (!_loadout.IsInitialized)
                _loadout.Initialize(new PlayerBuildCapacity(PlayerBuildCapacity.MaximumWeaponSlots,
                    PlayerBuildCapacity.DefaultAbilitySlots, PlayerBuildCapacity.DefaultUpgradeSlots));
            _encounter.Victory += HandleVictory;
        }

        private IEnumerator Start()
        {
            float deadline = Time.realtimeSinceStartup + 10;
            while (!_encounter.IsInitialized || !_chests.IsInitialized || !_experience.IsInitialized)
            {
                if (Time.realtimeSinceStartup > deadline)
                    throw new InvalidOperationException("Demo opening services did not initialize.");
                yield return null;
            }
            _encounter.RequestPreparation();
            IsReady = true;
        }

        private void HandleVictory()
        {
            if (Reward != null || _rewardClaimed || _health.IsDead) return;
            var request = new ChestSpawnRequest(_firstReward, _rewardPoint.position, _rewardPoint.rotation);
            Reward = _chests.Spawn(in request).ChestController;
            Reward.ChestOpened += HandleRewardClaimed;
        }

        private void HandleRewardClaimed(ChestController chest) => _rewardClaimed = true;

        private void Update()
        {
            // Reward UI retains its own input/cursor authority. Retry never interrupts a live run.
            if ((_final == null || !_final.IsEnding) && _health.IsDead && Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
                Restart();
        }

        public void Restart()
        {
            if (_restarting || !_health.IsDead || (_final != null && _final.IsEnding)) return;
            _restarting = true;
            Time.timeScale = 1;
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                gameObject.scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
            SceneManager.LoadSceneAsync(gameObject.scene.path, LoadSceneMode.Single);
#endif
        }

        private void OnGUI()
        {
            if (!IsReady || _selection.IsOpen || (_final != null && _final.IsEnding)) return;
            bool canvasHud = _hud != null && _hud.ReplacesDebugHud;
            if (!canvasHud)
            {
                GUI.Box(new Rect(16, 16, 390, 110), "DEMO — DUNGEON GREYBOX");
                GUI.Label(new Rect(28, 42, 365, 24), $"Health {_health.CurrentHealth:0}/{_health.MaximumHealth:0}   Ammo {_weapon.MagazineAmmo}/{_weapon.ReserveAmmo}");
                GUI.Label(new Rect(28, 66, 365, 24), $"Level {_experience.Level}   XP {_experience.CurrentExperience}/{_experience.RequiredExperience}");
                GUI.Label(new Rect(28, 90, 365, 24), _weapon.IsReloading ? "Reloading..." : "WASD move · Space jump · R reload · Q switch");
            }
            string objective = _rewardClaimed ? "Continue through the next room. Explore the overlook and both arena stairs." :
                Reward != null ? "Collect your reward in the room beyond the corridor (E)." :
                "Enter the corridor and defeat its four enemies for a guaranteed reward.";
            if (_health.transform.position.z > 53)
                objective = "Green side room: bonus chest. Right stairs and jumping route lead to the key.";
            if (_health.transform.position.x > 21 && _health.transform.position.z > 90)
                objective = "Cross the platforms, enter the room and take the key: E / RB-R1.";
            if (_keyAmbush != null && _keyAmbush.Session.HasKey)
                objective = _keyAmbush.CanEnterFinal ? "Key secured. Return to the arena and enter the final gate." :
                    "Clear both ambush groups to open the separate return exit.";
            GUI.Box(new Rect(16, Screen.height - 54, Mathf.Min(660, Screen.width - 32), 38), objective);
            string preparationError = GetRoomError();
            if (preparationError != null)
                GUI.Box(new Rect(16, 138, Mathf.Min(800, Screen.width - 32), 64), preparationError);
            if (!_health.IsDead)
            {
                if (!canvasHud) GUI.Label(new Rect(Screen.width / 2f - 5, Screen.height / 2f - 10, 20, 20), "+");
                return;
            }
            GUI.Box(new Rect(Screen.width / 2f - 170, Screen.height / 2f - 65, 340, 145), "You died — restart with a fresh run");
            if (GUI.Button(new Rect(Screen.width / 2f - 145, Screen.height / 2f - 25, 290, 35), "Retry (Enter)")) Restart();
            if (GUI.Button(new Rect(Screen.width / 2f - 145, Screen.height / 2f + 20, 290, 35), "Quit")) Application.Quit();
        }

        private void OnDestroy()
        {
            if (_encounter != null) _encounter.Victory -= HandleVictory;
            if (Reward != null) Reward.ChestOpened -= HandleRewardClaimed;
        }

        private string GetRoomError()
        {
            foreach (var room in _roomEncounters)
                if (room != null && room.LastError != null)
                    return $"Encounter preparation failed: {room.name}\n{room.LastError.Message}";
            if (_optionalReward != null && _optionalReward.LastError != null)
                return "Optional reward failed: " + _optionalReward.LastError;
            return null;
        }
    }
}
