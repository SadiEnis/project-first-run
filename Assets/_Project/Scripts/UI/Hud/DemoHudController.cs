using System;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Progression;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Weapons;
using UnityEngine;

namespace ProjectFirstRun.UI.Hud
{
    [DefaultExecutionOrder(500)]
    public sealed class DemoHudController : MonoBehaviour
    {
        [SerializeField] private DemoHudView _view;
        [SerializeField] private DemoOpeningController _demo;
        [SerializeField] private RewardSelectionController _selection;
        [SerializeField] private DemoFinalController _final;
        [SerializeField] private DemoKeyAmbushController _keyAmbush;
        private Abilities.PlayerAbilityController _abilities;
        private HealthComponent _health;
        private PlayerBuildController _build;
        private PlayerExperienceController _experience;
        private PlayerWeaponController _weapon;
        private bool _bound;
        public bool ReplacesDebugHud => isActiveAndEnabled && _bound && _view != null && _view.isActiveAndEnabled;

        public void ValidateConfiguration()
        {
            if (_view == null || _demo == null || _selection == null || _final == null || _keyAmbush == null || _demo.Health == null)
                throw new InvalidOperationException("Assign HUD view, demo, reward and final scene references.");
            _view.ValidateConfiguration();
            var player = _demo.Health.gameObject;
            if (player.GetComponent<PlayerBuildController>() == null ||
                player.GetComponent<PlayerExperienceController>() == null ||
                player.GetComponent<PlayerWeaponController>() == null)
                throw new InvalidOperationException("HUD player is missing build, experience or weapon data.");
        }

        private void Start()
        {
            try
            {
                ValidateConfiguration();
                _health = _demo.Health;
                _build = _health.GetComponent<PlayerBuildController>();
                _experience = _health.GetComponent<PlayerExperienceController>();
                _weapon = _health.GetComponent<PlayerWeaponController>();
                _abilities = _health.GetComponent<Abilities.PlayerAbilityController>();
                _bound = true;
            }
            catch (Exception error)
            {
                if (_view != null) _view.gameObject.SetActive(false);
                Debug.LogException(error, this);
                enabled = false; // Leave the legacy debug HUD available on invalid wiring.
            }
        }

        private void LateUpdate()
        {
            if (!_bound || _view == null) return;
            bool ready = _demo.IsReady && _build.IsInitialized && _experience.IsInitialized && _weapon.IsInitialized;
            bool visible = ShouldShow(ready, _health.IsDead, _selection.IsOpen, _final.IsEnding);
            _view.SetVisible(visible);
            if (!visible) return;
            var capacity = _build.Build.Capacity;
            _view.SetCapacity(capacity.AbilitySlots, capacity.UpgradeSlots);
            _view.SetHealth(_health.CurrentHealth, _health.MaximumHealth);
            _view.SetExperience(_experience.Level, _experience.CurrentExperience, _experience.RequiredExperience);
            _view.SetWeapon(_weapon.ActiveDefinition.DisplayName, _weapon.ActiveEntry?.Level ?? 1,
                _weapon.MagazineAmmo, _weapon.ReserveAmmo, _weapon.IsReloading);
            _view.SetItems(_build.Build, _abilities, _keyAmbush.Session.HasKey);
            _view.SetWeaponCard(_weapon.ActiveEntry);
        }

        public static bool ShouldShow(bool ready, bool dead, bool rewardOpen, bool ending) =>
            ready && !dead && !rewardOpen && !ending;

        private void OnDisable()
        {
            if (_view != null) _view.SetVisible(false);
        }
    }
}
