#if UNITY_EDITOR

using ProjectFirstRun.Weapons;
using UnityEngine;

namespace ProjectFirstRun.Development.Weapons
{
    [DisallowMultipleComponent]
    public sealed class WeaponDebugPresenter : MonoBehaviour
    {
        [SerializeField]
        private PlayerWeaponController _weaponController;

        private string _lastEvent = "Waiting for weapon input...";

        private void Awake()
        {
            if (_weaponController == null)
            {
                _weaponController =
                    GetComponent<PlayerWeaponController>();
            }

            if (_weaponController == null)
            {
                Debug.LogError(
                    $"{nameof(WeaponDebugPresenter)} on '{name}' " +
                    $"requires a {nameof(PlayerWeaponController)}.",
                    this);

                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_weaponController == null)
            {
                return;
            }

            _weaponController.ShotFired += HandleShotFired;
            _weaponController.DryFired += HandleDryFired;
            _weaponController.AmmoChanged += HandleAmmoChanged;
            _weaponController.ReloadStarted += HandleReloadStarted;
            _weaponController.ReloadCompleted += HandleReloadCompleted;
            _weaponController.DamageApplied += HandleDamageApplied;
        }

        private void OnDisable()
        {
            if (_weaponController == null)
            {
                return;
            }

            _weaponController.ShotFired -= HandleShotFired;
            _weaponController.DryFired -= HandleDryFired;
            _weaponController.AmmoChanged -= HandleAmmoChanged;
            _weaponController.ReloadStarted -= HandleReloadStarted;
            _weaponController.ReloadCompleted -= HandleReloadCompleted;
            _weaponController.DamageApplied -= HandleDamageApplied;
        }

        private void OnGUI()
        {
            if (_weaponController == null ||
                !_weaponController.IsInitialized)
            {
                return;
            }

            GUI.Box(
                new Rect(20f, 20f, 390f, 145f),
                "Weapon Foundation Debug");

            GUI.Label(
                new Rect(35f, 50f, 360f, 25f),
                $"Weapon: {_weaponController.ActiveDefinition.DisplayName}");

            GUI.Label(
                new Rect(35f, 75f, 360f, 25f),
                $"Ammo: {_weaponController.MagazineAmmo} / " +
                $"{_weaponController.ReserveAmmo}");

            GUI.Label(
                new Rect(35f, 100f, 360f, 25f),
                $"Reloading: {_weaponController.IsReloading}");

            GUI.Label(
                new Rect(35f, 125f, 360f, 25f),
                $"Last Event: {_lastEvent}");
        }

        private void HandleShotFired(
            HitscanVolleyResult shotResult)
        {
            _lastEvent = $"Shot: {shotResult.Pellets.Count} pellets, {shotResult.Targets.Count} targets";
        }

        private void HandleDryFired()
        {
            _lastEvent = "Dry fire";
        }

        private void HandleAmmoChanged(
            int magazineAmmo,
            int reserveAmmo)
        {
            _lastEvent =
                $"Ammo changed: {magazineAmmo} / {reserveAmmo}";
        }

        private void HandleReloadStarted()
        {
            _lastEvent = "Reload started";
        }

        private void HandleReloadCompleted()
        {
            _lastEvent = "Reload completed";
        }

        private void HandleDamageApplied(
            HitscanShotResult shotResult)
        {
            _lastEvent =
                $"Damage applied: " +
                $"{shotResult.DamageResult.AppliedDamage}";
        }
    }
}

#endif
