using UnityEngine;
namespace ProjectFirstRun.Abilities.Lightning
{
    public sealed class LightningVisual : MonoBehaviour
    {
        private Material _material;
        private float _remaining = .2f;
        public static void Show(Vector3 from, Vector3 to, Material template, UnityEngine.SceneManagement.Scene scene)
        {
            var go = new GameObject("Lightning bolt");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
            var visual = go.AddComponent<LightningVisual>();
            visual._material = new Material(template);
            Color color = new Color(.35f, .75f, 1);
            if (visual._material.HasProperty("_BaseColor")) visual._material.SetColor("_BaseColor", color);
            if (visual._material.HasProperty("_EmissionColor")) visual._material.SetColor("_EmissionColor", color * 3);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = visual._material; line.useWorldSpace = true;
            line.startWidth = .12f; line.endWidth = .04f; line.positionCount = 7;
            Vector3 side = Vector3.Cross(to - from, Vector3.forward).normalized;
            if (side.sqrMagnitude < .01f) side = Vector3.right;
            for (int i = 0; i < 7; i++)
                line.SetPosition(i, Vector3.Lerp(from, to, i / 6f) +
                    (i == 0 || i == 6 ? Vector3.zero : side * (i % 2 == 0 ? .2f : -.2f)));
        }
        private void Update()
        {
            _remaining -= Time.deltaTime;
            if (_remaining <= 0) Destroy(gameObject);
        }
        private void OnDestroy() { if (_material != null) Destroy(_material); }
    }
}
