using UnityEngine;
namespace ProjectFirstRun.Abilities.SniperBomb
{
    public sealed class SniperBlast : MonoBehaviour
    {
        private Material _material;
        private float _radius, _age;
        public static void Show(Vector3 point, float radius, Material material, UnityEngine.SceneManagement.Scene scene)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere); go.name = "Sniper explosion";
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
            var collider = go.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
            go.transform.position = point; go.transform.localScale = Vector3.one * .1f;
            var view = go.AddComponent<SniperBlast>(); view._radius = radius; view._material = new Material(material);
            go.GetComponent<Renderer>().sharedMaterial = view._material;
        }
        private void Update()
        {
            _age += Time.deltaTime;
            if (_age >= .2f) { Destroy(gameObject); return; }
            transform.localScale = Vector3.one * Mathf.Lerp(.1f, _radius * 2, _age / .2f);
        }
        private void OnDestroy() { if (_material != null) Destroy(_material); }
    }
}
