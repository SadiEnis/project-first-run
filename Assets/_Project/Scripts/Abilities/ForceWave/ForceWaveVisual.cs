using ProjectFirstRun.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Abilities.ForceWave
{
    /// <summary>Presentation only: a map-owned arc with no colliders or damage callbacks.</summary>
    public sealed class ForceWaveVisual : MonoBehaviour
    {
        private LineRenderer _line;
        private Vector3 _forward;
        private float _angle, _reach, _elapsed;
        private GameObject _source;
        public static void Show(Vector3 origin, Vector3 forward, float angle, float reach, Material material, GameObject source)
        {
            var go = new GameObject("Force Wave feedback");
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
            go.transform.position = origin;
            var visual = go.AddComponent<ForceWaveVisual>();
            visual._source = source; visual._forward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
            visual._angle = angle; visual._reach = reach;
            visual._line = go.AddComponent<LineRenderer>(); visual._line.sharedMaterial = material;
            visual._line.useWorldSpace = false; visual._line.positionCount = 25;
            visual._line.widthMultiplier = .1f;
            visual.Draw(.1f);
        }
        private void Update()
        {
            var health = _source != null ? _source.GetComponent<HealthComponent>() : null;
            if (_source == null || (health != null && health.IsDead)) { Destroy(gameObject); return; }
            _elapsed += Time.deltaTime;
            if (_elapsed >= .25f) { Destroy(gameObject); return; }
            Draw(Mathf.Lerp(.1f, _reach, _elapsed / .25f));
        }
        private void Draw(float radius)
        {
            for (int i = 0; i < 25; i++)
                _line.SetPosition(i, Quaternion.AngleAxis(-_angle / 2 + _angle * i / 24, Vector3.up) * _forward * radius);
        }
    }
}
