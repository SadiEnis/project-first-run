using System;
using ProjectFirstRun.Flow;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ProjectFirstRun.UI.Hud
{
    /// <summary>Authored merchant grid and Hub menu. Purchases remain owned by RunHubController.</summary>
    public sealed class RunHubView : MonoBehaviour
    {
        [SerializeField] private RunHubController _hub;
        [SerializeField] private RunHubInteraction _interaction;
        [SerializeField] private CanvasGroup _shop;
        [SerializeField] private CanvasGroup _menu;
        [SerializeField] private Button[] _cards;
        [SerializeField] private Text[] _cardLabels;
        [SerializeField] private HudGlyphGraphic[] _icons;
        [SerializeField] private Button _buy;
        [SerializeField] private Button _back;
        [SerializeField] private Button _resume;
        [SerializeField] private Button _quit;
        [SerializeField] private Text _balance;
        [SerializeField] private Text _description;
        [SerializeField] private Text _buyLabel;
        [SerializeField] private Text _status;
        [SerializeField] private Text _prompt;
        [SerializeField] private Text _crosshair;
        [SerializeField] private EventSystem _events;
        private int _selected, _quotedLevel;
        private bool _armed, _bound;
        private float _openedAt;

        public void ValidateConfiguration()
        {
            if (_hub == null || _interaction == null || _shop == null || _menu == null || _events == null ||
                _buy == null || _back == null || _resume == null || _quit == null || _balance == null ||
                _description == null || _buyLabel == null || _status == null || _prompt == null || _crosshair == null ||
                _cards == null || _cards.Length != 3 || _cardLabels == null || _cardLabels.Length != 3 ||
                _icons == null || _icons.Length != 3)
                throw new InvalidOperationException("Assign the saved Hub grid, detail panel, controls and prompts.");
            for (int i = 0; i < 3; i++)
                if (_cards[i] == null || _cardLabels[i] == null || _icons[i] == null)
                    throw new InvalidOperationException("Assign all Hub upgrade cards.");
        }
        private void Start()
        {
            ValidateConfiguration();
            for (int i = 0; i < _cards.Length; i++)
            {
                int index = i;
                _cards[i].onClick.AddListener(() => { if (_armed) Select(index); });
                BindPointer(_cards[i], () => Select(index));
            }
            BindPointer(_buy, null); BindPointer(_back, null); BindPointer(_resume, null); BindPointer(_quit, null);
            _buy.onClick.AddListener(Buy);
            _back.onClick.AddListener(Close);
            _resume.onClick.AddListener(Close);
            _quit.onClick.AddListener(Quit);
            _interaction.Changed += PresentMode;
            _bound = true;
            _icons[0].SetPath("0.2,0.15 0.2,0.7 0.3,0.9 0.4,0.7 0.4,0.15 0.2,0.15;0.6,0.15 0.6,0.7 0.7,0.9 0.8,0.7 0.8,0.15 0.6,0.15");
            _icons[1].SetPath("0.6,0.95 0.2,0.5 0.48,0.5 0.38,0.05 0.85,0.6 0.55,0.6 0.6,0.95");
            _icons[2].SetPath("0.5,0.95 0.85,0.75 0.75,0.3 0.5,0.05 0.25,0.3 0.15,0.75 0.5,0.95;0.5,0.7 0.5,0.3;0.3,0.5 0.7,0.5");
            PresentMode();
        }
        private void BindPointer(Button button, Action selected)
        {
            var trigger = button.GetComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entry.callback.AddListener(_ =>
            {
                if (!_armed) return;
                _events.SetSelectedGameObject(button.gameObject);
                selected?.Invoke();
            });
            trigger.triggers.Add(entry);
        }
        private void PresentMode()
        {
            _armed = false;
            _openedAt = Time.unscaledTime;
            SetGroup(_shop, _interaction.ShopOpen, false);
            SetGroup(_menu, _interaction.MenuOpen, false);
            _events.sendNavigationEvents = false;
            _events.SetSelectedGameObject(null);
            RefreshOffer();
        }
        private static void SetGroup(CanvasGroup group, bool visible, bool interactive)
        {
            group.alpha = visible ? 1 : 0;
            group.blocksRaycasts = visible;
            group.interactable = visible && interactive;
        }
        private void Update()
        {
            if (!_bound) return;
            bool modal = _interaction.HasModal;
            if (modal && !_armed && Time.unscaledTime - _openedAt >= .15f && ControlsReleased())
            {
                _armed = true;
                SetGroup(_shop, _interaction.ShopOpen, true);
                SetGroup(_menu, _interaction.MenuOpen, true);
                _events.sendNavigationEvents = true;
                _events.SetSelectedGameObject((_interaction.ShopOpen ? _cards[_selected] : _resume).gameObject);
            }
            if (_interaction.ShopOpen && _armed)
            {
                for (int i = 0; i < _cards.Length; i++)
                    if (_events.currentSelectedGameObject == _cards[i].gameObject && i != _selected) Select(i);
                if (_events.currentSelectedGameObject == null && NavigationRequested())
                    _events.SetSelectedGameObject(_cards[_selected].gameObject);
            }
            _crosshair.enabled = !modal && !_hub.IsLoading;
            _prompt.text = modal ? "" : _hub.IsLoading ? "Entering the dungeon..." :
                _interaction.Status ?? (_interaction.CanTrade ? "{Interact} Trade with the merchant" : "{Pause} Menu");
        }
        private static bool NavigationRequested()
        {
            var key = Keyboard.current;
            return InputPrompts.IsGamepad || (key != null && (key.upArrowKey.isPressed || key.downArrowKey.isPressed ||
                key.leftArrowKey.isPressed || key.rightArrowKey.isPressed || key.tabKey.isPressed));
        }
        private static bool ControlsReleased()
        {
            var pad = Gamepad.current;
            var key = Keyboard.current;
            return !(Mouse.current?.leftButton.isPressed ?? false) &&
                !(key != null && (key.enterKey.isPressed || key.spaceKey.isPressed || key.eKey.isPressed || key.escapeKey.isPressed)) &&
                !(pad != null && (pad.buttonSouth.isPressed || pad.buttonWest.isPressed || pad.buttonEast.isPressed ||
                    pad.startButton.isPressed || pad.rightShoulder.isPressed || pad.leftStick.ReadValue().sqrMagnitude > .1f || pad.dpad.ReadValue().sqrMagnitude > .1f));
        }
        private void Select(int index)
        {
            _selected = index;
            RefreshOffer();
            var nav = _buy.navigation;
            nav.selectOnLeft = _cards[index];
            _buy.navigation = nav;
        }
        private void RefreshOffer()
        {
            _balance.text = _hub.PermanentBalance.HasValue ? _hub.PermanentBalance.Value.ToString("N0") + " GOLD" : "GOLD unavailable";
            for (int i = 0; i < 3; i++)
            {
                string title = i == 0 ? _hub.AmmoUpgrade.DisplayName : i == 1 ? _hub.AbilitySlotUpgrade.DisplayName : _hub.UpgradeSlotUpgrade.DisplayName;
                int level = Level(i), max = Maximum(i);
                _cardLabels[i].text = title + "\n" + (_hub.IsReady ? level == max ? "MAX" : "LEVEL " + level + " / " + max : "Unavailable");
                _icons[i].color = i == _selected ? new Color(1, .76f, .35f) : new Color(.55f, .68f, .75f);
            }
            _quotedLevel = Level(_selected);
            _status.text = _hub.LastError ?? _hub.PurchaseMessage ?? "Permanent improvements. Applied to your next run.";
            if (!_hub.IsReady)
            {
                _description.text = "Your profile could not be confirmed. Retry reading before making a purchase.";
                _buyLabel.text = "Retry reading save";
                _buy.interactable = true;
                return;
            }
            bool maximum = _quotedLevel == Maximum(_selected);
            string effect, titleText;
            if (_selected == 0)
            {
                var def = _hub.AmmoUpgrade;
                titleText = def.DisplayName;
                effect = "Extra reserve magazines for your starting weapon.\n\nCurrent: +" + def.GetBonusMagazines(_quotedLevel) +
                    (maximum ? "" : "\nNext: +" + def.GetBonusMagazines(_quotedLevel + 1));
            }
            else
            {
                var def = _selected == 1 ? _hub.AbilitySlotUpgrade : _hub.UpgradeSlotUpgrade;
                titleText = def.DisplayName;
                effect = "Carry more " + (_selected == 1 ? "abilities" : "upgrades") + " during a run. Opens capacity; does not grant items.\n\nCurrent slots: " +
                    def.GetCapacity(_quotedLevel) + (maximum ? "" : "\nNext slots: " + def.GetCapacity(_quotedLevel + 1));
            }
            int cost = maximum ? 0 : Cost(_selected, _quotedLevel);
            _description.text = titleText + "\n\n" + effect + "\n\n" + (maximum ? "Fully developed." : "Cost: " + cost + " GOLD");
            // Keep the purchase button navigable, even at MAX or with insufficient funds.
            _buy.interactable = true;
            _buyLabel.text = maximum ? "Maximum level" : _hub.PermanentBalance < cost ? "Not enough GOLD" : "Purchase";
        }
        private int Level(int i) => i == 0 ? _hub.AmmoLevel : i == 1 ? _hub.AbilitySlotLevel : _hub.UpgradeSlotLevel;
        private int Maximum(int i) => i == 0 ? _hub.AmmoUpgrade.MaxLevel : i == 1 ? _hub.AbilitySlotUpgrade.MaxLevel : _hub.UpgradeSlotUpgrade.MaxLevel;
        private int Cost(int i, int level) => i == 0 ? _hub.AmmoUpgrade.GetNextCost(level) :
            i == 1 ? _hub.AbilitySlotUpgrade.GetNextCost(level) : _hub.UpgradeSlotUpgrade.GetNextCost(level);
        private void Buy()
        {
            if (!_armed || !_interaction.ShopOpen || _hub.IsLoading) return;
            if (!_hub.IsReady) _hub.RefreshBalance();
            else if (_selected == 0) _hub.PurchaseAmmo(_quotedLevel);
            else if (_selected == 1) _hub.PurchaseAbilitySlot(_quotedLevel);
            else _hub.PurchaseUpgradeSlot(_quotedLevel);
            RefreshOffer();
        }
        private void Close() { if (_armed) _interaction.Close(); }
        private void Quit() { if (_armed && _interaction.MenuOpen) _hub.Quit(); }
        private void OnDestroy()
        {
            if (_interaction != null) _interaction.Changed -= PresentMode;
        }
    }
}
