using ProjectFirstRun.Abilities;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Input;
using ProjectFirstRun.Player;
using ProjectFirstRun.Progression;
using ProjectFirstRun.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectFirstRun.Development.Arenas
{
    /// <summary>Fixture-only IMGUI controls. Reserves reward UI while holding the modal state.</summary>
    public sealed class ContentArenaPanel : MonoBehaviour
    {
        [SerializeField] private ContentArenaController _arena;
        private PlayerController _control;
        private PlayerInputReader _input;
        private PlayerWeaponController _weapon;
        private PlayerAbilityController _abilities;
        private HealthComponent _health;
        private bool _controlWasEnabled, _inputWasEnabled, _weaponWasEnabled, _abilitiesWereEnabled, _selectionWasEnabled;
        private float _previousTimeScale;
        private CursorLockMode _cursorLock;
        private bool _cursorVisible;
        private Vector2 _scroll;
        public bool IsOpen { get; private set; }

        private void Awake()
        {
            _control = _arena.Player.GetComponent<PlayerController>();
            _input = _arena.Player.GetComponent<PlayerInputReader>();
            _weapon = _arena.Player.GetComponent<PlayerWeaponController>();
            _abilities = _arena.Player.GetComponent<PlayerAbilityController>();
            _health = _arena.Player.GetComponent<HealthComponent>();
        }

        private void Update()
        {
            if (IsOpen && _health.IsDead) Close();
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            {
                if (IsOpen) Close(); else TryOpen();
            }
        }

        public bool TryOpen()
        {
            if (IsOpen || !isActiveAndEnabled || !_arena.CanMutate || Time.timeScale <= 0 || !_arena.Selection.enabled) return false;
            _previousTimeScale = Time.timeScale;
            _controlWasEnabled = _control.IsControlEnabled;
            _inputWasEnabled = _input.IsGameplayInputEnabled;
            _weaponWasEnabled = _weapon.IsWeaponControlEnabled;
            _abilitiesWereEnabled = _abilities.IsAbilityControlEnabled;
            _selectionWasEnabled = _arena.Selection.enabled;
            _cursorLock = Cursor.lockState;
            _cursorVisible = Cursor.visible;
            // A chest opened by external code cannot capture our paused state and later restore it incorrectly.
            _arena.Selection.enabled = false;
            IsOpen = true;
            Time.timeScale = 0;
            _control.SetControlEnabled(false);
            _weapon.SetWeaponControlEnabled(false);
            _abilities.SetAbilityControlEnabled(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return true;
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            bool anotherSelection = _arena != null && _arena.Selection != null && _arena.Selection.IsOpen;
            if (!anotherSelection && Time.timeScale == 0) Time.timeScale = _previousTimeScale;
            if (_arena != null && _arena.Selection != null) _arena.Selection.enabled = _selectionWasEnabled;
            bool alive = _health != null && !_health.IsDead;
            if (!anotherSelection)
            {
                if (_control != null) _control.SetControlEnabled(alive && _controlWasEnabled);
                if (_input != null) _input.SetGameplayInputEnabled(alive && _inputWasEnabled);
                if (_weapon != null) _weapon.SetWeaponControlEnabled(alive && _weaponWasEnabled);
                if (_abilities != null) _abilities.SetAbilityControlEnabled(alive && _abilitiesWereEnabled);
                Cursor.lockState = alive ? _cursorLock : CursorLockMode.None;
                Cursor.visible = !alive || _cursorVisible;
            }
        }

        private void OnDisable() => Close();

        private void OnGUI()
        {
            if (_arena == null || !_arena.IsReady || _arena.Selection.IsOpen) return;
            var xp = _arena.Player.GetComponent<PlayerExperienceController>();
            GUILayout.BeginArea(new Rect(12, 12, 680, 150), GUI.skin.box);
            GUILayout.Label("CONTENT ARENA | F1: test controls | Q: weapon | E: chest");
            GUILayout.Label($"Health {_health.CurrentHealth:F0}/{_health.MaximumHealth:F0} | {_weapon.ActiveDefinition?.DisplayName} | Ammo {_weapon.MagazineAmmo}/{_weapon.ReserveAmmo}");
            GUILayout.Label($"Level {xp.Level} | XP {xp.CurrentExperience}/{xp.RequiredExperience} | Enemies {_arena.Registry.ActiveCount}");
            GUILayout.Label($"Damage/pellet {_weapon.CurrentDamage:F1} | Pellets {_weapon.ActiveEntry.Shot.PelletCount} | Push {_weapon.ActiveEntry.Shot.PushDistance:F1} m | R: reload");
            GUILayout.Label($"Prepare {_weapon.PreparationElapsed:F2}/{_weapon.ActiveEntry.Fire.PreparationDuration:F2}s | Rate {_weapon.ActiveEntry.ShotsPerSecond:F0}/s | Crit {_weapon.ActiveEntry.Fire.CriticalChance:P0} | Last crit: {_weapon.LastShotWasCritical}");
            GUILayout.Label(_health.IsDead ? "Dead: stop and restart Play for a fresh test." : _arena.LastResult);
            GUILayout.EndArea();
            if (!IsOpen) return;
            GUILayout.BeginArea(new Rect(12, 170, Mathf.Min(610, Screen.width - 24), Mathf.Max(100, Screen.height - 185)), GUI.skin.box);
            if (GUILayout.Button("Close panel / F1")) Close();
            _scroll = GUILayout.BeginScrollView(_scroll);
            GUILayout.Label("Replace enemies (no kill rewards; existing loot and build stay)");
            if (GUILayout.Button("Mixed group")) _arena.ReplaceGroup(-1);
            for (int i = 0; i < _arena.EnemyDefinitions.Count; i++)
                if (GUILayout.Button(_arena.EnemyDefinitions[i].name)) _arena.ReplaceGroup(i);
            GUILayout.Label("Items: acquire first, then level up; Q switches owned weapons");
            for (int i = 0; i < _arena.Items.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{_arena.Items[i].DisplayName} | Level {_arena.ItemLevel(i)}/{_arena.Items[i].MaximumLevel}", GUILayout.Width(330));
                if (GUILayout.Button("Acquire")) _arena.Acquire(i);
                if (GUILayout.Button("Level +1")) _arena.LevelUp(i);
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Grant 100 XP (normal level-up rewards)")) _arena.GrantExperience(100);
            GUILayout.Label("Spawn chest nearby; close panel to interact");
            for (int i = 0; i < _arena.Chests.Count; i++)
                if (GUILayout.Button(_arena.Chests[i].name)) _arena.SpawnChest(i);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
