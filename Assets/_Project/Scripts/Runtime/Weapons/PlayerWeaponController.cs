using System;
using ProjectFirstRun.Input;
using ProjectFirstRun.Stats;
using UnityEngine;

namespace ProjectFirstRun.Weapons
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerStatsController))]
    public sealed class PlayerWeaponController : MonoBehaviour
    {
        [Header("Shot References")]
        [SerializeField]
        private Camera _aimCamera;

        [SerializeField]
        private Transform _muzzle;

        [SerializeField]
        private GameObject _damageSource;

        private PlayerInputReader _inputReader;
        private PlayerStatsController _statsController;

        private WeaponDefinition _activeDefinition;
        private WeaponRuntimeState _runtimeState;
        private PlayerWeaponRuntimeEntry _activeEntry;
        private HitscanShotResolver _shotResolver;
        

        private bool _weaponControlEnabled = true;

        public event Action<HitscanShotResult> ShotFired;
        public event Action DryFired;
        public event Action<int, int> AmmoChanged;
        public event Action ReloadStarted;
        public event Action ReloadCompleted;
        public event Action<HitscanShotResult> DamageApplied;

        public bool IsInitialized =>
            _activeDefinition != null &&
            _runtimeState != null;

        public WeaponDefinition ActiveDefinition =>
            _activeDefinition;

        public int MagazineAmmo =>
            IsInitialized
                ? _runtimeState.MagazineAmmo
                : 0;

        public int ReserveAmmo =>
            IsInitialized
                ? _runtimeState.ReserveAmmo
                : 0;

        public bool IsReloading =>
            IsInitialized &&
            _runtimeState.IsReloading;

        public bool IsWeaponControlEnabled =>
            _weaponControlEnabled;

        public float CurrentDamage =>
            IsInitialized
                ? EvaluateCurrentDamage()
                : 0f;
        
        public PlayerWeaponRuntimeEntry ActiveEntry =>
            _activeEntry;

        private void Awake()
        {
            _inputReader =
                GetComponent<PlayerInputReader>();

            _statsController =
                GetComponent<PlayerStatsController>();

            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            if (_damageSource == null)
            {
                _damageSource = gameObject;
            }

            _shotResolver =
                new HitscanShotResolver(
                    _aimCamera,
                    _muzzle,
                    _damageSource);
        }

        private void Start()
        {
            if (IsInitialized)
            {
                PublishAmmoChanged();
            }
        }

        private void Update()
        {
            if (!IsInitialized ||
                !_weaponControlEnabled)
            {
                return;
            }

            UpdateRuntimeState(
                Time.deltaTime);

            HandleReloadInput();
            HandleFireInput();
        }

        public void Initialize(
            WeaponDefinition weaponDefinition)
        {
            if (weaponDefinition == null)
            {
                throw new ArgumentNullException(
                    nameof(weaponDefinition));
            }

            PlayerWeaponRuntimeEntry entry =
                new PlayerWeaponRuntimeEntry(
                    weaponDefinition);

            Equip(
                entry);
        }
        
        public void Equip(
            PlayerWeaponRuntimeEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(
                    nameof(entry));
            }

            _activeEntry =
                entry;

            _activeDefinition =
                entry.Definition;

            _runtimeState =
                entry.RuntimeState;

            PublishAmmoChanged();
        }

        public void ResetWeaponState()
        {
            if (!IsInitialized)
            {
                return;
            }

            _runtimeState.Reset();

            PublishAmmoChanged();
        }

        public void SetWeaponControlEnabled(
            bool isEnabled)
        {
            _weaponControlEnabled =
                isEnabled;
        }

        private void UpdateRuntimeState(
            float deltaTime)
        {
            bool reloadCompleted =
                _runtimeState.Tick(
                    deltaTime);

            if (!reloadCompleted)
            {
                return;
            }

            PublishAmmoChanged();

            ReloadCompleted?.Invoke();
        }

        private void HandleReloadInput()
        {
            if (!_inputReader
                    .WasReloadPressedThisFrame)
            {
                return;
            }

            WeaponReloadResult result =
                _runtimeState.TryStartReload();

            if (result ==
                WeaponReloadResult.Started)
            {
                ReloadStarted?.Invoke();
            }
        }

        private void HandleFireInput()
        {
            bool wasPressedThisFrame =
                _inputReader
                    .WasFirePressedThisFrame;

            bool fireRequested =
                _activeDefinition.TriggerMode
                switch
                {
                    WeaponTriggerMode.SemiAutomatic =>
                        wasPressedThisFrame,

                    WeaponTriggerMode.Automatic =>
                        _inputReader.IsFireHeld,

                    _ =>
                        false
                };

            if (!fireRequested)
            {
                return;
            }

            TryFire(
                wasPressedThisFrame);
        }

        private void TryFire(
            bool wasPressedThisFrame)
        {
            /*
             * Validate/evaluate damage before mutating weapon
             * runtime state. An invalid stat configuration
             * must not consume ammunition.
             */
            float damage =
                EvaluateCurrentDamage();

            WeaponFireResult fireResult =
                _runtimeState.TryFire();

            if (fireResult ==
                WeaponFireResult.EmptyMagazine)
            {
                // Prevents automatic weapons from producing
                // a dry-fire event every frame while held.
                if (wasPressedThisFrame)
                {
                    DryFired?.Invoke();
                }

                return;
            }

            if (fireResult !=
                WeaponFireResult.Fired)
            {
                return;
            }

            PublishAmmoChanged();

            HitscanShotResult shotResult =
                _shotResolver.Resolve(
                    damage,
                    _activeDefinition.Range,
                    _activeDefinition.DamageMask);

            ShotFired?.Invoke(
                shotResult);

            if (shotResult.DamageWasApplied)
            {
                DamageApplied?.Invoke(
                    shotResult);
            }
        }

        private float EvaluateCurrentDamage()
        {
            float damage =
                _statsController.Evaluate(
                    PlayerStatType.WeaponDamage,
                    _activeDefinition.BaseDamage);

            if (float.IsNaN(damage) ||
                float.IsInfinity(damage) ||
                damage <= 0f)
            {
                throw new InvalidOperationException(
                    $"Evaluated weapon damage for " +
                    $"'{_activeDefinition.name}' must be " +
                    $"finite and greater than zero. " +
                    $"Value: {damage}.");
            }

            return damage;
        }

        private void PublishAmmoChanged()
        {
            if (!IsInitialized)
            {
                return;
            }

            AmmoChanged?.Invoke(
                _runtimeState.MagazineAmmo,
                _runtimeState.ReserveAmmo);
        }

        private bool ValidateReferences()
        {
            bool isValid = true;

            if (_aimCamera == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerWeaponController)} " +
                    $"on '{name}' requires an aim camera.",
                    this);

                isValid = false;
            }

            if (_muzzle == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerWeaponController)} " +
                    $"on '{name}' requires a muzzle transform.",
                    this);

                isValid = false;
            }

            return isValid;
        }
    }
}
