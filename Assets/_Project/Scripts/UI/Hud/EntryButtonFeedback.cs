using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectFirstRun.UI.Hud
{
    public sealed class EntryButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, Range(1, 1.15f)] private float _selectedScale = 1.045f;
        private Vector3 _baseScale;
        private bool _captured;
        public bool Hovered { get; private set; }
        public void OnPointerEnter(PointerEventData data) => Hovered = true;
        public void OnPointerExit(PointerEventData data) => Hovered = false;
        public void Present(bool selected)
        {
            if (!_captured) { _baseScale = transform.localScale; _captured = true; }
            transform.localScale = Vector3.Lerp(transform.localScale, _baseScale * (selected ? _selectedScale : 1),
                1 - Mathf.Exp(-18 * Time.unscaledDeltaTime));
            var graphic = GetComponent<Image>();
            if (graphic != null) graphic.color = selected ? new Color(.15f, .48f, .55f, 1) : new Color(.08f, .13f, .19f, .95f);
        }
        private void OnDisable()
        {
            Hovered = false;
            if (_captured) transform.localScale = _baseScale;
        }
    }
}
