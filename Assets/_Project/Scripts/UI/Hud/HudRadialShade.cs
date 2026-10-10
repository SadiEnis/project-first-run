using UnityEngine;
using UnityEngine.UI;

namespace ProjectFirstRun.UI.Hud
{
    /// <summary>Rectangular overlay that clears clockwise from twelve o'clock. No sprite dependency.</summary>
    public sealed class HudRadialShade : MaskableGraphic
    {
        [SerializeField, Range(0, 1)] private float _remaining;
        public float Remaining
        {
            get => _remaining;
            set { value = Mathf.Clamp01(value); if (_remaining == value) return; _remaining = value; SetVerticesDirty(); }
        }
        public static Vector2 Boundary(Rect r, float clockwiseDegrees)
        {
            float angle = clockwiseDegrees * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
            float x = Mathf.Abs(direction.x) > .000001f ? r.width * .5f / Mathf.Abs(direction.x) : float.MaxValue;
            float y = Mathf.Abs(direction.y) > .000001f ? r.height * .5f / Mathf.Abs(direction.y) : float.MaxValue;
            return r.center + direction * Mathf.Min(x, y);
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            if (_remaining <= 0 || r.width <= 0 || r.height <= 0) return;
            float start = (1 - _remaining) * 360;
            // Include exact rectangle corners so a full shade covers all pixels, even on non-square icons.
            float corner = Mathf.Atan2(r.width, r.height) * Mathf.Rad2Deg;
            vh.AddVert(r.center, color, Vector2.zero);
            vh.AddVert(Boundary(r, start), color, Vector2.zero);
            int previous = 1;
            for (int i = 0; i < 5; i++)
            {
                float end = i switch { 0 => corner, 1 => 180 - corner, 2 => 180 + corner, 3 => 360 - corner, _ => 360 };
                if (end <= start) continue;
                vh.AddVert(Boundary(r, end), color, Vector2.zero);
                vh.AddTriangle(0, previous, previous + 1);
                previous++;
            }
        }
    }
}
