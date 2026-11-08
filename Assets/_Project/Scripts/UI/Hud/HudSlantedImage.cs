using UnityEngine;
using UnityEngine.UI;

namespace ProjectFirstRun.UI.Hud
{
    /// <summary>Sprite-free bar with a fixed-size diagonal leading edge, including partial health.</summary>
    public sealed class HudSlantedImage : Image
    {
        [SerializeField, Min(0)] private float _slant = 18;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            if (r.width <= 0 || r.height <= 0) return;
            float cut = Mathf.Min(_slant, r.width);
            vh.AddVert(new Vector3(r.xMin, r.yMin), color, Vector2.zero);
            vh.AddVert(new Vector3(r.xMin, r.yMax), color, Vector2.zero);
            vh.AddVert(new Vector3(r.xMax - cut, r.yMax), color, Vector2.zero);
            vh.AddVert(new Vector3(r.xMax, r.yMin), color, Vector2.zero);
            vh.AddTriangle(0, 1, 2); vh.AddTriangle(0, 2, 3);
        }
    }
}
