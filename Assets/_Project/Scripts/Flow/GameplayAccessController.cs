using System;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Input;
using ProjectFirstRun.Player;
using ProjectFirstRun.Weapons;
using UnityEngine;

namespace ProjectFirstRun.Flow
{
    public sealed class GameplayAccessController : MonoBehaviour
    {
        [SerializeField] private HealthComponent _health;
        [SerializeField, Min(.01f)] private float _baselineTimeScale = 1;
        private static GameplayAccessController _globalOwner;
        private PlayerController _control;
        private PlayerInputReader _input;
        private PlayerMotor _motor;
        private PlayerWeaponController _weapon;
        private PlayerAbilityController _abilities;
        private bool _initialized, _retiring;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOwner() => _globalOwner = null;

        public void ValidateConfiguration()
        {
            if (_health == null || !float.IsFinite(_baselineTimeScale) || _baselineTimeScale <= 0)
                throw new InvalidOperationException("Assign access player and positive baseline time scale.");
            if (_health.GetComponent<PlayerController>() == null || _health.GetComponent<PlayerInputReader>() == null ||
                _health.GetComponent<PlayerMotor>() == null || _health.GetComponent<PlayerWeaponController>() == null ||
                _health.GetComponent<PlayerAbilityController>() == null)
                throw new InvalidOperationException("Access player requires control, input, motor, weapons and abilities.");
        }

        public void Initialize()
        {
            if (_initialized) return;
            ValidateConfiguration();
            _control = _health.GetComponent<PlayerController>();
            _input = _health.GetComponent<PlayerInputReader>();
            _motor = _health.GetComponent<PlayerMotor>();
            _weapon = _health.GetComponent<PlayerWeaponController>();
            _abilities = _health.GetComponent<PlayerAbilityController>();
            _control.SetCursorManaged(this, true);
            _globalOwner = this;
            _initialized = true;
            _retiring = false;
        }

        public void Apply(GameFlowAccess access)
        {
            if (!_initialized || _retiring || _globalOwner != this) return;
            _input.SetGameplayBlocked(this, access.BlockGameplay);
            _motor.SetSuspended(this, access.FreezeWorld);
            _weapon.SetWeaponBlocked(this, access.BlockGameplay);
            _abilities.SetAbilityBlocked(this, access.BlockGameplay);
            _health.SetDamageBlocked(this, access.ProtectPlayer);
            Time.timeScale = access.FreezeWorld ? 0 : _baselineTimeScale;
            AudioListener.pause = access.MuteAudio;
            Cursor.lockState = access.Cursor == GameFlowCursor.LockedHidden ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = access.Cursor == GameFlowCursor.FreeVisible;
        }

        public void RetireForReload() => _retiring = true;

        public void Shutdown()
        {
            if (!_initialized) return;
            _initialized = false;
            if (_input != null) _input.SetGameplayBlocked(this, false);
            if (_motor != null) _motor.SetSuspended(this, false);
            if (_weapon != null) _weapon.SetWeaponBlocked(this, false);
            if (_abilities != null) _abilities.SetAbilityBlocked(this, false);
            if (_health != null) _health.SetDamageBlocked(this, false);
            // An old scene must never overwrite the arriving scene's globals, even if unloaded later.
            if (_globalOwner != this || _retiring) return;
            _globalOwner = null;
            if (_control != null) _control.SetCursorManaged(this, false);
            Time.timeScale = _baselineTimeScale;
            AudioListener.pause = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDisable() => Shutdown();
    }
}
