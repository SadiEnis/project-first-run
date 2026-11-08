using UnityEngine;
using UnityEngine.UI;

namespace ProjectFirstRun.UI.Hud
{
    public sealed class HudItemCard : MonoBehaviour
    {
        [SerializeField] private HudGlyphGraphic _glyph;
        [SerializeField] private Image _art;
        [SerializeField] private HudRadialShade _shade;
        [SerializeField] private Text _label;
        [SerializeField] private bool _showName;
        private string _id;
        private int _level = -1;
        public string ItemId => _id;
        public void Present(HudIconCatalog catalog, string id, string name, int level, float remaining)
        {
            if (_id != id || _level != level)
            {
                _id = id; _level = level;
                var icon = catalog != null ? catalog.Find(id) : null;
                bool occupied = !string.IsNullOrEmpty(id);
                _glyph.gameObject.SetActive(occupied && (icon == null || icon.Sprite == null));
                _art.gameObject.SetActive(occupied && icon != null && icon.Sprite != null);
                _art.sprite = icon?.Sprite;
                _glyph.color = icon != null ? icon.Tint : Color.white;
                _glyph.SetPath(icon?.Strokes);
                _label.text = !occupied ? "—" : _showName
                    ? (name ?? icon?.Label ?? id) + "\nLV " + level
                    : icon == null ? "? " + level : level.ToString();
            }
            _shade.Remaining = string.IsNullOrEmpty(id) ? 0 : remaining;
        }
    }
}
