using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectFirstRun.UI.Hud
{
    public sealed class HudGlyphGraphic : MaskableGraphic
    {
        private readonly List<Vector2[]> _strokes = new();
        private string _path;
        public void SetPath(string path)
        {
            if (_path == path) return;
            _path = path; _strokes.Clear();
            foreach (string stroke in (path ?? "0.2,0.2 0.8,0.2 0.8,0.8 0.2,0.8 0.2,0.2").Split(';'))
            {
                var points = new List<Vector2>();
                foreach (string token in stroke.Split(' '))
                {
                    var xy = token.Split(',');
                    if (xy.Length == 2 && float.TryParse(xy[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                        float.TryParse(xy[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                        points.Add(new Vector2(x, y));
                }
                _strokes.Add(points.ToArray());
            }
            SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            float width = Mathf.Max(1.5f, Mathf.Min(r.width, r.height) * .055f);
            foreach (var stroke in _strokes)
                for (int i = 1; i < stroke.Length; i++)
                {
                    Vector2 a = r.min + Vector2.Scale(stroke[i - 1], r.size);
                    Vector2 b = r.min + Vector2.Scale(stroke[i], r.size);
                    Vector2 d = b - a;
                    if (d.sqrMagnitude < .000001f) continue;
                    Vector2 normal = new Vector2(-d.y, d.x).normalized * width * .5f;
                    int first = vh.currentVertCount;
                    vh.AddVert(a - normal, color, Vector2.zero); vh.AddVert(a + normal, color, Vector2.zero);
                    vh.AddVert(b + normal, color, Vector2.zero); vh.AddVert(b - normal, color, Vector2.zero);
                    vh.AddTriangle(first, first + 1, first + 2); vh.AddTriangle(first, first + 2, first + 3);
                }
        }
    }
}
