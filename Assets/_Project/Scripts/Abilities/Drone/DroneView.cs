using ProjectFirstRun.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ProjectFirstRun.Abilities.Drone
{
    /// <summary>Temporary map-owned presentation. No collision or damage responsibility.</summary>
    public sealed class DroneView : MonoBehaviour
    {
        private GameObject _source;
        private LineRenderer _trace;
        private float _traceRemaining;
        public static DroneView Create(GameObject source, Material material, int index)
        {
            var root = new GameObject("Drone " + (index + 1));
            SceneManager.MoveGameObjectToScene(root, SceneManager.GetActiveScene());
            var view = root.AddComponent<DroneView>(); view._source = source;
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Temporary drone body"; body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(.35f, .15f, .4f);
            var collider = body.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
            body.GetComponent<Renderer>().sharedMaterial = material;
            view._trace = root.AddComponent<LineRenderer>();
            view._trace.sharedMaterial = material; view._trace.positionCount = 2;
            view._trace.widthMultiplier = .035f; view._trace.useWorldSpace = true; view._trace.enabled = false;
            return view;
        }
        public void ShowShot(Vector3 end)
        {
            _trace.SetPosition(0, transform.position); _trace.SetPosition(1, end);
            _traceRemaining = .07f; _trace.enabled = true;
        }
        private void Update()
        {
            var health = _source != null ? _source.GetComponent<HealthComponent>() : null;
            if (_source == null || !_source.activeInHierarchy || (health != null && health.IsDead))
            { gameObject.SetActive(false); Destroy(gameObject); return; }
            _traceRemaining -= Time.deltaTime;
            if (_traceRemaining <= 0) _trace.enabled = false;
        }
    }
}
