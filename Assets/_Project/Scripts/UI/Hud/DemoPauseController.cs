using System;
using System.Collections.Generic;
using ProjectFirstRun.Abilities;
using ProjectFirstRun.Builds;
using ProjectFirstRun.Development.Arenas;
using ProjectFirstRun.Input;
using ProjectFirstRun.Items;
using ProjectFirstRun.Player;
using ProjectFirstRun.UI.Rewards;
using ProjectFirstRun.Weapons;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using ProjectFirstRun.Flow;
using UnityEngine.UI;

namespace ProjectFirstRun.UI.Hud
{
    [DefaultExecutionOrder(-400)]
    public sealed class DemoPauseController : MonoBehaviour
    {
        [SerializeField] private DemoOpeningController _demo;
        [SerializeField] private RunRestartController _restart;
        [SerializeField] private DemoFinalController _final;
        [SerializeField] private RewardSelectionController _selection;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _panel;
        [SerializeField] private RectTransform _buildRoot;
        [SerializeField] private Button[] _buttons;
        [SerializeField] private Text _status;
        [SerializeField] private HudIconCatalog _icons;
        [SerializeField] private HudItemCard _weaponCardTemplate;
        [SerializeField] private HudItemCard _itemCardTemplate;
        [SerializeField, Min(.05f)] private float _slideSeconds = .25f;
        private PlayerInputReader _input;
        private PlayerMotor _motor;
        private PlayerWeaponController _weapon;
        private PlayerAbilityController _abilities;
        private PlayerBuildController _build;
        private EventSystem _events;
        private bool _oldNavigation, _oldAudio, _oldCursor;
        private CursorLockMode _oldLock;
        private float _oldTime, _amount;
        private bool _closing, _armed, _axisHeld, _submitHeld, _pointer, _restarting;
        private int _selected;
        private Vector2 _shown;
        public bool IsPaused { get; private set; }
        public bool CanRestart => IsPaused && _armed && !_closing && !_restarting;

        public void ValidateConfiguration()
        {
            if (_demo == null || _restart == null || _final == null || _selection == null || _group == null ||
                _panel == null || _buildRoot == null || _icons == null || _status == null ||
                _weaponCardTemplate == null || _itemCardTemplate == null ||
                _buttons == null || _buttons.Length != 3 || !float.IsFinite(_slideSeconds) || _slideSeconds <= 0)
                throw new InvalidOperationException("Assign pause menu, build area and demo references.");
            foreach (var button in _buttons)
                if (button == null || button.GetComponent<EntryButtonFeedback>() == null)
                    throw new InvalidOperationException("Pause buttons require entry-style feedback.");
        }
        private void Awake()
        {
            ValidateConfiguration();
            var player = _demo.Health;
            _input = player.GetComponent<PlayerInputReader>();
            _motor = player.GetComponent<PlayerMotor>();
            _weapon = player.GetComponent<PlayerWeaponController>();
            _abilities = player.GetComponent<PlayerAbilityController>();
            _build = player.GetComponent<PlayerBuildController>();
            _shown = _panel.anchoredPosition;
            _buttons[0].onClick.AddListener(Resume);
            _buttons[1].onClick.AddListener(Restart);
            _buttons[2].onClick.AddListener(Quit);
            Present();
        }
        public static bool CanOpen(bool ready, bool entry, bool dead, bool reward, bool ending, float timeScale) =>
            ready && !entry && !dead && !reward && !ending && timeScale > 0;
        public static int MoveSelection(int selected, int direction) => (selected + direction + 3) % 3;
        private static bool PausePressed() =>
            (Keyboard.current?.escapeKey.wasPressedThisFrame ?? false) ||
            (Gamepad.current?.startButton.wasPressedThisFrame ?? false);
        private static bool Held()
        {
            var k = Keyboard.current; var p = Gamepad.current;
            return (k != null && (k.escapeKey.isPressed || k.enterKey.isPressed || k.spaceKey.isPressed || k.numpadEnterKey.isPressed)) ||
                (p != null && (p.startButton.isPressed || p.buttonSouth.isPressed || p.buttonEast.isPressed || p.rightTrigger.isPressed)) ||
                (Mouse.current?.leftButton.isPressed ?? false);
        }
        private void Update()
        {
            if (PausePressed())
            {
                if (!IsPaused) Open();
                else if (_armed && !_closing) Resume();
            }
            if (!IsPaused) return;
            if (_armed && !_closing && (Gamepad.current?.buttonEast.wasPressedThisFrame ?? false)) Resume();
            if (_events == null && EventSystem.current != null)
            {
                _events = EventSystem.current;
                _oldNavigation = _events.sendNavigationEvents;
                _events.sendNavigationEvents = false;
            }
            float vertical = Gamepad.current?.leftStick.y.ReadValue() ?? 0;
            var p = Gamepad.current; var k = Keyboard.current; var m = Mouse.current;
            if (p != null && p.dpad.y.ReadValue() != 0) vertical = p.dpad.y.ReadValue();
            if (k != null && k.upArrowKey.isPressed) vertical = 1;
            if (k != null && k.downArrowKey.isPressed) vertical = -1;
            bool submit = (p?.buttonSouth.isPressed ?? false) ||
                (k != null && (k.enterKey.isPressed || k.spaceKey.isPressed || k.numpadEnterKey.isPressed));
            if (m != null && (m.delta.ReadValue().sqrMagnitude > .01f || m.leftButton.wasPressedThisFrame)) _pointer = true;
            if (Mathf.Abs(vertical) >= .6f || submit) _pointer = false;
            if (!_armed)
            {
                if (!Held() && Mathf.Abs(vertical) < .25f) _armed = true;
            }
            else if (!_closing && !_restarting)
            {
                if (Mathf.Abs(vertical) < .25f) _axisHeld = false;
                if (Mathf.Abs(vertical) >= .6f && !_axisHeld)
                {
                    _selected = MoveSelection(_selected, vertical > 0 ? -1 : 1);
                    _axisHeld = true;
                }
                if (submit && !_submitHeld) _buttons[_selected].onClick.Invoke();
            }
            _submitHeld = submit;
            _amount = Mathf.MoveTowards(_amount, _closing ? 0 : 1, Time.unscaledDeltaTime / _slideSeconds);
            Present();
            if (_closing && _amount == 0 && !Held()) Release();
        }
        private void LateUpdate()
        {
            if (IsPaused) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        }
        public void Open()
        {
            if (IsPaused || !CanOpen(_demo.IsReady, _demo.IsEntryBlocking, _demo.Health.IsDead,
                _selection.IsOpen, _final.IsEnding, Time.timeScale)) return;
            _oldTime = Time.timeScale; _oldAudio = AudioListener.pause;
            _oldLock = Cursor.lockState; _oldCursor = Cursor.visible;
            IsPaused = true; _closing = _armed = _axisHeld = _submitHeld = _restarting = false;
            _amount = 0;
            _selected = 0; _pointer = false; _status.text = "";
            SetLocks(true);
            Time.timeScale = 0; AudioListener.pause = true;
            RefreshBuild();
            Present();
        }
        public void Resume() { if (IsPaused && !_restarting && _armed) _closing = true; }
        public void Restart()
        {
            if (!CanRestart) return;
            _restarting = _restart.Request(RunRestartSource.Pause);
            _status.text = _restarting ? "Restarting..." : _restart.LastError != null ? "Restart failed. Please try again." : "";
            Present();
        }
        public void Quit()
        {
            if (!IsPaused || !_armed || _closing || _restarting) return;
#if UNITY_EDITOR
            Release();
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        private void SetLocks(bool value)
        {
            if (_input != null) _input.SetGameplayBlocked(this, value);
            if (_motor != null) _motor.SetSuspended(this, value);
            if (_weapon != null) _weapon.SetWeaponBlocked(this, value);
            if (_abilities != null) _abilities.SetAbilityBlocked(this, value);
            if (_demo != null && _demo.Health != null) _demo.Health.SetDamageBlocked(this, value);
        }
        private void Release()
        {
            if (!IsPaused) return;
            IsPaused = false; _amount = 0; _closing = false;
            SetLocks(false);
            if (!_restarting)
            {
                if (Time.timeScale == 0) Time.timeScale = _oldTime;
                AudioListener.pause = _oldAudio;
                Cursor.lockState = _oldLock; Cursor.visible = _oldCursor;
            }
            if (_events != null) { _events.sendNavigationEvents = _oldNavigation; _events = null; }
            Present();
        }
        private void Present()
        {
            if (_group == null || _panel == null) return;
            float t = Mathf.SmoothStep(0, 1, _amount);
            _panel.anchoredPosition = _shown + Vector2.left * ((_shown.x + _panel.rect.width / 2 + 30) * (1 - t));
            _group.alpha = IsPaused ? 1 : 0;
            _group.interactable = IsPaused && _armed && !_closing && !_restarting;
            _group.blocksRaycasts = IsPaused;
            if (_buttons == null) return;
            for (int i = 0; i < _buttons.Length; i++)
            {
                var feedback = _buttons[i].GetComponent<EntryButtonFeedback>();
                feedback.Present(_group.interactable && (_pointer ? feedback.Hovered : _selected == i));
            }
        }
        private void RefreshBuild()
        {
            foreach (Transform child in _buildRoot) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            var build = _build.Build;
            AddCards(build.Weapons, ItemCategory.Weapon, build, build.Capacity.WeaponSlots);
            AddCards(build.Abilities, ItemCategory.Ability, build, build.Capacity.AbilitySlots);
            AddCards(build.Upgrades, ItemCategory.Upgrade, build, build.Capacity.UpgradeSlots);
        }

        public static Vector2 BuildSlotPosition(ItemCategory category, int index, int slots)
        {
            if (slots <= 0) throw new ArgumentOutOfRangeException(nameof(slots));
            if (index < 0 || index >= slots) throw new ArgumentOutOfRangeException(nameof(index));
            if (category == ItemCategory.Weapon)
                return new Vector2((index - (slots - 1) * .5f) * 116, 115);
            if (category == ItemCategory.Ability)
                return new Vector2((index - (slots - 1) * .5f) * 60, 0);
            if (category != ItemCategory.Upgrade) throw new ArgumentOutOfRangeException(nameof(category));
            const int columns = 4;
            int row = index / columns;
            int rowCount = Math.Min(columns, slots - row * columns);
            return new Vector2((index % columns - (rowCount - 1) * .5f) * 60, -80 - row * 60);
        }

        private void AddCards(IReadOnlyList<string> ids, ItemCategory category, PlayerBuild build, int slots)
        {
            bool weapon = category == ItemCategory.Weapon;
            for (int i = 0; i < slots; i++)
            {
                // Clone the actual HUD card, including its frame, glyph/sprite and level label.
                var card = Instantiate(weapon ? _weaponCardTemplate : _itemCardTemplate, _buildRoot);
                card.name = category + " slot " + (i + 1);
                var rect = (RectTransform)card.transform;
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
                rect.anchoredPosition = BuildSlotPosition(category, i, slots);
                rect.sizeDelta = Vector2.one * (weapon ? 100 : 48);
                card.gameObject.SetActive(true);
                string id = i < ids.Count ? ids[i] : null;
                int level = 0;
                if (id != null && build.TryGetItem(category, id, out var item)) level = item.Level;
                card.Present(_icons, id, id == null ? null : _icons.Find(id)?.Label, level, 0);
                foreach (var graphic in card.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
            }
        }
        private void OnDisable() => Release();
    }
}

