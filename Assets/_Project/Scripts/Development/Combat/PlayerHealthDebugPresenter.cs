#if UNITY_EDITOR

using ProjectFirstRun.Combat;
using UnityEngine;

namespace ProjectFirstRun.Development.Combat
{
    [DisallowMultipleComponent]
    public sealed class PlayerHealthDebugPresenter : MonoBehaviour
    {
        [SerializeField]
        private HealthComponent _playerHealth;

        private float _currentHealth;
        private float _maximumHealth;
        private string _lastEvent = "Waiting for damage...";

        private void Awake()
        {
            if (_playerHealth == null)
            {
                _playerHealth =
                    GetComponent<HealthComponent>();
            }

            if (_playerHealth == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerHealthDebugPresenter)} on '{name}' " +
                    $"requires a {nameof(HealthComponent)}.",
                    this);

                enabled = false;
                return;
            }

            CacheHealthValues();
        }

        private void OnEnable()
        {
            if (_playerHealth == null)
            {
                return;
            }

            _playerHealth.HealthChanged += HandleHealthChanged;
            _playerHealth.Damaged += HandleDamaged;
            _playerHealth.Died += HandleDied;
            _playerHealth.HealthReset += HandleHealthReset;
        }

        private void OnDisable()
        {
            if (_playerHealth == null)
            {
                return;
            }

            _playerHealth.HealthChanged -= HandleHealthChanged;
            _playerHealth.Damaged -= HandleDamaged;
            _playerHealth.Died -= HandleDied;
            _playerHealth.HealthReset -= HandleHealthReset;
        }

        private void OnGUI()
        {
            if (_playerHealth == null)
            {
                return;
            }

            float panelX = Screen.width - 300f;

            GUI.Box(
                new Rect(panelX, 20f, 280f, 115f),
                "Player Health Debug");

            GUI.Label(
                new Rect(panelX + 15f, 50f, 250f, 25f),
                $"Health: {_currentHealth} / {_maximumHealth}");

            GUI.Label(
                new Rect(panelX + 15f, 75f, 250f, 25f),
                $"Dead: {_playerHealth.IsDead}");

            GUI.Label(
                new Rect(panelX + 15f, 100f, 250f, 25f),
                $"Last Event: {_lastEvent}");
        }

        private void HandleHealthChanged(
            float currentHealth,
            float maximumHealth)
        {
            _currentHealth = currentHealth;
            _maximumHealth = maximumHealth;
        }

        private void HandleDamaged(
            DamageInfo damageInfo,
            DamageResult damageResult)
        {
            _lastEvent =
                $"Damage received: {damageResult.AppliedDamage}";
        }

        private void HandleDied(
            DamageInfo damageInfo,
            DamageResult damageResult)
        {
            _lastEvent = "Player died";
        }

        private void HandleHealthReset()
        {
            CacheHealthValues();
            _lastEvent = "Health reset";
        }

        private void CacheHealthValues()
        {
            _currentHealth = _playerHealth.CurrentHealth;
            _maximumHealth = _playerHealth.MaximumHealth;
        }
    }
}

#endif