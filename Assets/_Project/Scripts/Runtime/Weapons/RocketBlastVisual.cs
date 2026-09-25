using UnityEngine;

namespace ProjectFirstRun.Weapons
{
    public sealed class RocketBlastVisual : MonoBehaviour
    {
        private float _remaining = .18f;
        private float _diameter;
        public void Initialize(float radius) { _diameter = radius * 2; transform.localScale = Vector3.one * .1f; }
        private void Update()
        {
            _remaining -= Time.deltaTime;
            if (_remaining <= 0) { Destroy(gameObject); return; }
            transform.localScale = Vector3.one * Mathf.Lerp(.1f, _diameter, 1 - _remaining / .18f);
        }
    }
}
