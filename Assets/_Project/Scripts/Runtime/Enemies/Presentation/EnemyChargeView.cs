using UnityEngine;

namespace ProjectFirstRun.Enemies.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyAttackController))]
    public sealed class EnemyChargeView : MonoBehaviour
    {
        private EnemyController _enemy;
        private EnemyAttackController _attack;
        private LineRenderer _warning;
        private Material _lineMaterial;
        private TextMesh _label;
        private Renderer[] _body;
        private MaterialPropertyBlock _properties;

        private void Awake()
        {
            _enemy = GetComponent<EnemyController>();
            _attack = GetComponent<EnemyAttackController>();
            _body = GetComponentsInChildren<Renderer>();
            _properties = new MaterialPropertyBlock();
            GameObject warning = new GameObject("Charge Warning");
            warning.transform.SetParent(transform, false);
            _warning = warning.AddComponent<LineRenderer>();
            _warning.useWorldSpace = true;
            _warning.positionCount = 2;
            _warning.startWidth = _warning.endWidth = 0.12f;
            _warning.enabled = false;
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            if (shader != null)
            {
                _lineMaterial = new Material(shader);
                _lineMaterial.color = Color.yellow;
                _warning.sharedMaterial = _lineMaterial;
            }
            GameObject label = new GameObject("Enemy Identity");
            label.transform.SetParent(transform, false);
            label.transform.localPosition = Vector3.up * 2.4f;
            label.transform.localScale = Vector3.one * 0.06f;
            _label = label.AddComponent<TextMesh>();
            _label.anchor = TextAnchor.MiddleCenter;
            _label.fontSize = 48;
        }

        private void LateUpdate()
        {
            if (!_enemy.IsInitialized || _enemy.IsDead)
            {
                _warning.enabled = false;
                _label.text = "";
                return;
            }
            EnemyChargeState state = _attack.ChargeState;
            bool warning = state != null && state.Phase == EnemyChargePhase.Windup;
            Color color = _enemy.Definition.Rank == EnemyRank.Elite
                ? new Color(0.8f, 0.25f, 1f) : new Color(1f, 0.5f, 0.12f);
            if (warning) color = Color.yellow;
            _label.text = $"{_enemy.Definition.DisplayName} [{_enemy.Definition.Rank}]";
            _label.color = color;
            Camera viewingCamera = Camera.main;
            if (viewingCamera != null) _label.transform.rotation = viewingCamera.transform.rotation;
            if (state != null)
                foreach (Renderer body in _body)
                {
                    if (body == null) continue;
                    body.GetPropertyBlock(_properties);
                    _properties.SetColor("_BaseColor", color);
                    _properties.SetColor("_Color", color);
                    body.SetPropertyBlock(_properties);
                }
            _warning.enabled = warning && _attack.isActiveAndEnabled;
            if (!warning) return;
            Vector3 start = transform.position + Vector3.up * 0.08f;
            _warning.SetPosition(0, start);
            _warning.SetPosition(1, start + state.Direction * state.Config.Distance);
        }

        private void OnDisable()
        {
            if (_warning != null) _warning.enabled = false;
            if (_label != null) _label.text = "";
        }

        private void OnDestroy()
        {
            if (_lineMaterial != null) Destroy(_lineMaterial);
        }
    }
}
