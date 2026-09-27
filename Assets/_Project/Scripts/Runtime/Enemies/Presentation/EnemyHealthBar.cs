using ProjectFirstRun.Combat;
using UnityEngine;
using UnityEngine.UI;
namespace ProjectFirstRun.Enemies.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyController), typeof(HealthComponent))]
    public sealed class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private Vector3 _offset = new Vector3(0, 2.75f, 0);
        [SerializeField] private Vector2 _size = new Vector2(1.5f, .14f);
        [SerializeField] private Color _backgroundColor = new Color(.08f, .08f, .08f, .9f);
        [SerializeField] private Color _fillColor = new Color(.9f, .12f, .08f, 1);
        [SerializeField] private Camera _cameraOverride;
        private EnemyController _enemy;
        private HealthComponent _health;
        private RectTransform _root, _fill;
        private float _fraction = -1;
        public float DisplayedFraction => _fraction;
        public bool IsVisible => _root != null && _root.gameObject.activeInHierarchy;
        private void Awake()
        {
            _enemy = GetComponent<EnemyController>();
            _health = GetComponent<HealthComponent>();
            Build();
        }
        private void Build()
        {
            if (_root != null) return;
            var go = new GameObject("Enemy health bar", typeof(RectTransform), typeof(Canvas));
            _root = go.GetComponent<RectTransform>();
            _root.SetParent(transform, false);
            _root.sizeDelta = _size * 100;
            _root.localScale = Vector3.one * .01f;
            go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            Image background = MakeImage("Background", _root, _backgroundColor);
            background.rectTransform.anchorMin = Vector2.zero;
            background.rectTransform.anchorMax = Vector2.one;
            Image fill = MakeImage("Health", _root, _fillColor);
            _fill = fill.rectTransform; _fill.pivot = new Vector2(0, .5f);
            _fill.anchorMin = Vector2.zero; _fill.anchorMax = Vector2.one;
            background.rectTransform.offsetMin = background.rectTransform.offsetMax = Vector2.zero;
            _fill.offsetMin = _fill.offsetMax = Vector2.zero;
            go.SetActive(false);
        }
        private static Image MakeImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>(); image.color = color; image.raycastTarget = false;
            return image;
        }
        private void OnEnable()
        {
            _health.HealthChanged += HealthChanged;
            Refresh();
        }
        private void OnDisable()
        {
            if (_health != null) _health.HealthChanged -= HealthChanged;
            if (_root != null) _root.gameObject.SetActive(false);
        }
        private void HealthChanged(float current, float maximum) => Refresh();
        private void LateUpdate() => Refresh();
        public void Refresh()
        {
            if (_root == null || _health == null) return;
            float fraction = Mathf.Clamp01(_health.CurrentHealth / _health.MaximumHealth);
            if (!Mathf.Approximately(_fraction, fraction))
            {
                _fraction = fraction;
                _fill.anchorMax = new Vector2(fraction, 1);
            }
            Camera camera = _cameraOverride != null ? _cameraOverride : Camera.main;
            bool visible = isActiveAndEnabled && _enemy.IsInitialized && _enemy.isActiveAndEnabled &&
                !_enemy.IsDead && !_health.IsDead && camera != null && camera.isActiveAndEnabled;
            if (visible)
            {
                _root.position = transform.position + _offset;
                _root.rotation = camera.transform.rotation;
            }
            if (_root.gameObject.activeSelf != visible) _root.gameObject.SetActive(visible);
        }
        private void OnDestroy()
        {
            if (_root != null) Destroy(_root.gameObject);
        }
    }
}
