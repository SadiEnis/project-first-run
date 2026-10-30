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
using ProjectFirstRun.Flow;

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
        [SerializeField] private DemoEntryController _entry;
        [SerializeField] private RunRestartController _restart;
        public bool IsEntryBlocking => _entry != null && _entry.IsBlocking;
        private bool IsRestarting => _restart != null && _restart.IsLoading;
        private readonly DemoMenuInput _deathMenu = new();
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
                _rewardPoint == null || _selection == null || _restart == null)
                throw new InvalidOperationException("Assign all demo opening scene references.");
            _loadout.ValidateConfiguredSettings();
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
            // The scene's shared settings own weapon and capacity, in Editor and player builds.
            if (!_loadout.IsInitialized)
                _loadout.InitializeConfigured();
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
            // Match the visible death screen; never compete with reward UI or the finale.
            bool visible = IsReady && !_selection.IsOpen && (_final == null || !_final.IsEnding) &&
                _health.IsDead && !IsEntryBlocking && !IsRestarting;
            int choice = _deathMenu.Poll(visible);
            if (choice == 0) Restart();
            else if (choice == 1) Application.Quit();
        }

        public void Restart()
        {
            _restart.Request(RunRestartSource.Death);
        }

        private void OnGUI()
        {
            if (!IsReady || IsEntryBlocking || _selection.IsOpen || (_final != null && _final.IsEnding)) return;
            bool canvasHud = _hud != null && _hud.ReplacesDebugHud;
            if (!canvasHud)
            {
                GUI.Box(new Rect(16, 16, 390, 110), "DEMO — DUNGEON GREYBOX");
                GUI.Label(new Rect(28, 42, 365, 24), $"Health {_health.CurrentHealth:0}/{_health.MaximumHealth:0}   Ammo {_weapon.MagazineAmmo}/{_weapon.ReserveAmmo}");
                GUI.Label(new Rect(28, 66, 365, 24), $"Level {_experience.Level}   XP {_experience.CurrentExperience}/{_experience.RequiredExperience}");
                ProjectFirstRun.UI.Hud.InputPrompts.Draw(new Rect(28, 90, 365, 24), _weapon.IsReloading ? "Reloading..." : "{Move} Move  {Jump} Jump  {Reload} Reload  {SwitchWeapon} Switch");
            }
            string objective = _rewardClaimed ? "Continue through the next room. Explore the overlook and both arena stairs." :
                Reward != null ? "Collect your reward beyond the corridor: {Interact}" :
                "Enter the corridor and defeat its four enemies for a guaranteed reward.";
            if (_health.transform.position.z > 53)
                objective = "Green side room: bonus chest. Right stairs and jumping route lead to the key.";
            if (_health.transform.position.x > 21 && _health.transform.position.z > 90)
                objective = "Cross the platforms, enter the room and take the key: {Interact}";
            if (_keyAmbush != null && _keyAmbush.Session.HasKey)
                objective = _keyAmbush.CanEnterFinal ? "Key secured. Return to the arena and enter the final gate." :
                    "Clear both ambush groups to open the separate return exit.";
            ProjectFirstRun.UI.Hud.InputPrompts.Draw(new Rect(16, Screen.height - 54, Mathf.Min(660, Screen.width - 32), 38), objective, true);
            string preparationError = GetRoomError();
            if (preparationError != null)
                GUI.Box(new Rect(16, 138, Mathf.Min(800, Screen.width - 32), 64), preparationError);
            if (!_health.IsDead)
            {
                if (!canvasHud) GUI.Label(new Rect(Screen.width / 2f - 5, Screen.height / 2f - 10, 20, 20), "+");
                return;
            }
            GUI.Box(new Rect(Screen.width / 2f - 280, Screen.height / 2f - 65, 560, 175), "You died — restart with a fresh run");
            bool wasEnabled = GUI.enabled;
            GUI.enabled = wasEnabled && !IsRestarting;
            if (_deathMenu.Button(new Rect(Screen.width / 2f - 145, Screen.height / 2f - 25, 290, 35), 0, "Retry")) Restart();
            if (_deathMenu.Button(new Rect(Screen.width / 2f - 145, Screen.height / 2f + 20, 290, 35), 1, "Quit")) Application.Quit();
            GUI.enabled = wasEnabled;
            if (_restart.LastError != null)
                GUI.Box(new Rect(Screen.width / 2f - 280, Screen.height / 2f + 115, 560, 40), "Restart failed. Please try again.");
            ProjectFirstRun.UI.Hud.InputPrompts.Draw(new Rect(Screen.width / 2f - 270, Screen.height / 2f + 70, 540, 30), DemoMenuInput.Hint);
        }

        private void OnDisable() => _deathMenu.Tick(false, 0, false);

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
