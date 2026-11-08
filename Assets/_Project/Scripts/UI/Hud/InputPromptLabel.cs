using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectFirstRun.UI.Hud
{
    /// <summary>Adapts an authored Text label to inline atlas icons without requiring a TMP font migration.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Text))]
    public sealed class InputPromptLabel : MonoBehaviour
    {
        private Text _source;
        private readonly List<GameObject> _runs = new();
        private string _last;
        private int _revision = -1, _style = -1;
        private Vector2 _size;
        private bool _sourceEnabled;
        private readonly TextGenerator _measure = new();

        private void OnEnable()
        {
            _source = GetComponent<Text>();
            _sourceEnabled = _source.enabled;
            _source.enabled = false;
            _last = null;
        }
        private void OnDisable()
        {
            foreach (var run in _runs) if (run != null) run.SetActive(false);
            if (_source != null) _source.enabled = _sourceEnabled;
        }

        private void LateUpdate()
        {
            int style = InputPrompts.Settings != null ? (int)InputPrompts.Settings.GamepadStyle : 0;
            var size = _source.rectTransform.rect.size;
            if (_last == _source.text && _revision == InputPrompts.Revision && _size == size && _style == style) return;
            _last = _source.text; _revision = InputPrompts.Revision; _size = size; _style = style;
            Render(size);
        }

        private void Render(Vector2 size)
        {
            var parts = InputPrompts.Parse(_source.text);
            float height = _source.fontSize * 1.25f, lineHeight = height * _source.lineSpacing;
            var positions = new List<Vector2>();
            var widths = new List<float>();
            var lineWidths = new Dictionary<float, float>();
            float x = 0, y = 0;
            var settings = _source.GetGenerationSettings(new Vector2(10000, 10000));
            settings.generateOutOfBounds = true;
            foreach (var part in parts)
            {
                float width = part.IsIcon ? InputPrompts.IconWidth(part, height) + 3 :
                    _measure.GetPreferredWidth(part.Text, settings) / _source.pixelsPerUnit;
                if (part.Text == "\n") { x = 0; y += lineHeight; width = 0; }
                else if (x > 0 && x + width > size.x) { x = 0; y += lineHeight; }
                positions.Add(new Vector2(x, y)); widths.Add(width); x += width;
                lineWidths[y] = x;
            }
            float vertical = ((int)_source.alignment / 3) * .5f * Mathf.Max(0, size.y - y - height);
            for (int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                if (i >= _runs.Count)
                {
                    var child = new GameObject("Prompt run", typeof(RectTransform));
                    child.layer = gameObject.layer;
                    child.transform.SetParent(transform, false);
                    AddGraphic<Text>(child.transform, "Text");
                    AddGraphic<RawImage>(child.transform, "Icon");
                    _runs.Add(child);
                }
                var run = _runs[i]; run.SetActive(part.Text != "\n");
                var rect = (RectTransform)run.transform;
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
                float horizontal = ((int)_source.alignment % 3) * .5f *
                    Mathf.Max(0, size.x - lineWidths[positions[i].y]);
                rect.anchoredPosition = new Vector2(positions[i].x + horizontal, -positions[i].y - vertical);
                rect.sizeDelta = new Vector2(widths[i], height);
                var image = run.GetComponentInChildren<RawImage>(true);
                var text = run.GetComponentInChildren<Text>(true);
                image.enabled = part.IsIcon; text.enabled = !part.IsIcon;
                image.raycastTarget = text.raycastTarget = false;
                if (part.IsIcon)
                {
                    image.texture = part.Texture; image.uvRect = part.UV; image.color = Color.white;
                    rect.sizeDelta = new Vector2(widths[i] - 3, height);
                }
                else
                {
                    text.font = _source.font; text.fontSize = _source.fontSize; text.fontStyle = _source.fontStyle;
                    text.color = _source.color; text.alignment = TextAnchor.MiddleLeft;
                    text.horizontalOverflow = HorizontalWrapMode.Overflow;
                    text.verticalOverflow = VerticalWrapMode.Overflow;
                    text.supportRichText = false; text.text = part.Text;
                }
            }
            for (int i = parts.Count; i < _runs.Count; i++) _runs[i].SetActive(false);
        }
        private static void AddGraphic<T>(Transform parent, string name) where T : Graphic
        {
            var child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(T));
            child.layer = parent.gameObject.layer;
            child.transform.SetParent(parent, false);
            var rect = (RectTransform)child.transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
