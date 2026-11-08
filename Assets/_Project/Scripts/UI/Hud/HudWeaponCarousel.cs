using ProjectFirstRun.Weapons;
using UnityEngine;

namespace ProjectFirstRun.UI.Hud
{
    public sealed class HudWeaponCarousel : MonoBehaviour
    {
        [SerializeField] private HudItemCard[] _cards;
        [SerializeField, Min(.01f)] private float _duration = .3f;
        [SerializeField] private Vector2 _centre = new(0, 144);
        [SerializeField, Min(1)] private float _radius = 84;
        private readonly PlayerWeaponRuntimeEntry[] _entries = new PlayerWeaponRuntimeEntry[2];
        private readonly float[] _angles = { 0, 180 };
        private readonly float[] _from = new float[2], _to = new float[2];
        private string _selected;
        private float _elapsed;
        private bool _moving;
        private int _active;
        public static Vector2 Position(Vector2 centre, float radius, float angle) =>
            centre + new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
        public static float ClockwiseTarget(float angle, bool active)
        {
            float rest = active ? 0 : 180;
            return rest + Mathf.Floor((angle - rest + .0001f) / 360) * 360;
        }
        public void Present(PlayerWeaponRuntimeEntry entry, HudIconCatalog catalog)
        {
            if (entry == null) return;
            string id = entry.Definition.StableId;
            if (_selected != id)
            {
                int index = _entries[0]?.Definition.StableId == id ? 0 :
                    _entries[1]?.Definition.StableId == id ? 1 : _selected == null ? 0 : 1 - _active;
                _entries[index] = entry;
                bool first = _selected == null;
                _active = index; _selected = id;
                for (int i = 0; i < 2; i++)
                {
                    _from[i] = _angles[i];
                    _to[i] = ClockwiseTarget(_angles[i], i == _active);
                }
                _elapsed = 0; _moving = !first;
                if (first) Snap();
            }
            for (int i = 0; i < 2; i++)
            {
                _cards[i].gameObject.SetActive(_entries[i] != null);
                var item = _entries[i];
                if (item == null) continue;
                var state = item.RuntimeState;
                float shade = state.IsReloading && state.CommittedReloadDuration > 0
                    ? Mathf.Clamp01(state.ReloadTimeRemaining / state.CommittedReloadDuration)
                    : state.MagazineAmmo == 0 && state.ReserveAmmo == 0 ? 1 : 0;
                _cards[i].Present(catalog, item.Definition.StableId, item.Definition.DisplayName, item.Level, shade);
            }
            if (_moving)
            {
                _elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(_elapsed / Mathf.Max(.01f, _duration));
                float smooth = t * t * (3 - 2 * t);
                for (int i = 0; i < 2; i++) _angles[i] = Mathf.Lerp(_from[i], _to[i], smooth);
                if (t >= 1) _moving = false;
            }
            Place();
        }
        public void Snap()
        {
            _moving = false;
            for (int i = 0; i < 2; i++) _angles[i] = i == _active ? 0 : 180;
            Place();
        }
        private void Place()
        {
            if (_cards == null) return;
            for (int i = 0; i < _cards.Length; i++)
                if (_cards[i] != null)
                {
                    var rect = (RectTransform)_cards[i].transform;
                    rect.anchoredPosition = Position(_centre, _radius, _angles[i]);
                    rect.localRotation = Quaternion.identity;
                }
        }
        private void OnDisable() => Snap();
    }
}
