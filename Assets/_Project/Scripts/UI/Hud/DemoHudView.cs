using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectFirstRun.UI.Hud
{
    /// <summary>Saved Canvas presentation only; never changes gameplay or advances timers.</summary>
    public sealed class DemoHudView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _visibility;
        [SerializeField] private Image _healthFill;
        [SerializeField] private Image _experienceFill;
        [SerializeField] private Text _healthText;
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _experienceText;
        [SerializeField] private Text _weaponText;
        [SerializeField] private Text _ammoText;
        [SerializeField] private Text _statusText;
        [SerializeField] private GameObject[] _abilitySlots;
        [SerializeField] private GameObject[] _upgradeSlots;
        [SerializeField] private HudIconCatalog _icons;
        [SerializeField] private HudItemCard[] _abilityCards;
        [SerializeField] private HudItemCard[] _upgradeCards;
        [SerializeField] private GameObject _key;
        [SerializeField] private HudWeaponCarousel _carousel;
        private float _health = float.NaN, _maximumHealth = float.NaN;
        private long _xp = -1, _required = -1;
        private int _level = -1, _magazine = -1, _reserve = -1, _weaponLevel = -1;
        private string _weapon;
        private bool _reloading;
        public bool IsVisible => _visibility != null && _visibility.alpha > 0;
        public int VisibleAbilitySlots => CountActive(_abilitySlots);
        public int VisibleUpgradeSlots => CountActive(_upgradeSlots);

        private void Awake()
        {
            if (_visibility != null) SetVisible(false);
        }

        public void ValidateConfiguration()
        {
            if (_visibility == null || _healthFill == null || _experienceFill == null ||
                _healthText == null || _levelText == null || _experienceText == null ||
                _weaponText == null || _ammoText == null || _statusText == null)
                throw new InvalidOperationException("Assign all basic HUD presentation references.");
            ValidateSlots(_abilitySlots, Builds.PlayerBuildCapacity.MaximumAbilitySlots);
            ValidateSlots(_upgradeSlots, Builds.PlayerBuildCapacity.MaximumUpgradeSlots);
            if (_icons == null || _key == null || _carousel == null ||
                _abilityCards == null || _abilityCards.Length != _abilitySlots.Length ||
                _upgradeCards == null || _upgradeCards.Length != _upgradeSlots.Length)
                throw new InvalidOperationException("Assign HUD icon, card, key and carousel references.");
            foreach (var card in _abilityCards) if (card == null) throw new InvalidOperationException("Missing ability card.");
            foreach (var card in _upgradeCards) if (card == null) throw new InvalidOperationException("Missing upgrade card.");
        }

        private static void ValidateSlots(GameObject[] slots, int maximum)
        {
            if (slots == null || slots.Length < maximum)
                throw new InvalidOperationException("HUD must author enough slots for supported capacity.");
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) throw new InvalidOperationException("Missing HUD slot.");
                for (int j = 0; j < i; j++)
                    if (slots[i] == slots[j]) throw new InvalidOperationException("Duplicate HUD slot reference.");
            }
        }

        public void SetVisible(bool visible)
        {
            if (_visibility == null) return;
            if (!visible && IsVisible && _carousel != null) _carousel.Snap();
            _visibility.alpha = visible ? 1 : 0;
            _visibility.interactable = false;
            _visibility.blocksRaycasts = false;
        }

        public void SetCapacity(int abilities, int upgrades)
        {
            if (abilities < 0 || abilities > Builds.PlayerBuildCapacity.MaximumAbilitySlots ||
                upgrades < 0 || upgrades > Builds.PlayerBuildCapacity.MaximumUpgradeSlots)
                throw new ArgumentOutOfRangeException(nameof(abilities), "Unsupported HUD capacity.");
            SetSlots(_abilitySlots, abilities);
            SetSlots(_upgradeSlots, upgrades);
        }

        private static void SetSlots(GameObject[] slots, int count)
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].activeSelf != (i < count)) slots[i].SetActive(i < count);
        }
        private static int CountActive(GameObject[] slots)
        {
            int count = 0;
            if (slots != null) foreach (var slot in slots) if (slot != null && slot.activeSelf) count++;
            return count;
        }

        public void SetHealth(float current, float maximum)
        {
            if (_health == current && _maximumHealth == maximum) return;
            _health = current; _maximumHealth = maximum;
            SetBar(_healthFill, maximum > 0 ? Mathf.Clamp01(current / maximum) : 0);
            _healthText.text = $"{current:0} / {maximum:0}";
        }

        public void SetExperience(int level, long current, long required)
        {
            if (_level == level && _xp == current && _required == required) return;
            _level = level; _xp = current; _required = required;
            SetBar(_experienceFill, required > 0 ? Mathf.Clamp01((float)((double)current / required)) : 0);
            _levelText.text = $"LEVEL {level}";
            _experienceText.text = $"{current} / {required} XP";
        }

        public void SetWeapon(string name, int level, int magazine, int reserve, bool reloading)
        {
            name = string.IsNullOrWhiteSpace(name) ? "WEAPON" : name;
            if (_weapon == name && _weaponLevel == level && _magazine == magazine &&
                _reserve == reserve && _reloading == reloading) return;
            _weapon = name; _weaponLevel = level; _magazine = magazine;
            _reserve = reserve; _reloading = reloading;
            _weaponText.text = $"{name}\nLV {level}";
            _ammoText.text = $"{magazine} / {reserve}";
            _statusText.text = reloading ? "RELOADING" :
                magazine == 0 && reserve == 0 ? "NO AMMO" : "R  RELOAD     Q  SWITCH";
        }

        private static void SetBar(Image image, float ratio)
        {
            image.fillAmount = ratio;
            // Anchor-width also supports the sprite-free greybox bars in the authored prefab.
            var maximum = image.rectTransform.anchorMax;
            maximum.x = ratio;
            image.rectTransform.anchorMax = maximum;
        }

        public void SetItems(Builds.PlayerBuild build, Abilities.PlayerAbilityController abilities, bool hasKey)
        {
            for (int i = 0; i < _abilityCards.Length; i++)
            {
                string id = i < build.Abilities.Count ? build.Abilities[i] : null;
                float remaining = 0;
                if (id != null && abilities != null)
                    foreach (var entry in abilities.Entries)
                        if (entry.Definition.StableId == id) { remaining = entry.CooldownFraction; break; }
                _abilityCards[i].Present(_icons, id, null,
                    id == null ? 0 : build.GetLevel(Items.ItemCategory.Ability, id), remaining);
            }
            for (int i = 0; i < _upgradeCards.Length; i++)
            {
                string id = i < build.Upgrades.Count ? build.Upgrades[i] : null;
                _upgradeCards[i].Present(_icons, id, null,
                    id == null ? 0 : build.GetLevel(Items.ItemCategory.Upgrade, id), 0);
            }
            if (_key.activeSelf != hasKey) _key.SetActive(hasKey);
        }

        public void SetWeaponCard(Weapons.PlayerWeaponRuntimeEntry entry) => _carousel.Present(entry, _icons);
    }
}
