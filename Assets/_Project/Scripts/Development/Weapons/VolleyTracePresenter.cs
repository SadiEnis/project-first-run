using ProjectFirstRun.Weapons;
using UnityEngine;

namespace ProjectFirstRun.Development.Weapons
{
    /// <summary>Bounded reusable traces for content playtests.</summary>
    public sealed class VolleyTracePresenter : MonoBehaviour
    {
        [SerializeField] private PlayerWeaponController _weapon;
        [SerializeField] private Material _material;
        private readonly LineRenderer[] _lines = new LineRenderer[64];
        private float _remaining;
        private void OnEnable() { if (_weapon != null) _weapon.ShotFired += Show; }
        private void OnDisable()
        {
            if (_weapon != null) _weapon.ShotFired -= Show;
            Hide();
        }
        private void Show(HitscanVolleyResult volley)
        {
            Hide();
            if (_material == null) return;
            for (int i = 0; i < volley.Pellets.Count && i < _lines.Length; i++)
            {
                if (_lines[i] == null)
                {
                    var go = new GameObject("Pooled pellet trace " + i);
                    go.transform.SetParent(transform, false);
                    var line = go.AddComponent<LineRenderer>();
                    line.sharedMaterial = _material;
                    line.useWorldSpace = true;
                    line.positionCount = 2;
                    line.startWidth = .025f;
                    line.endWidth = .012f;
                    line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    line.receiveShadows = false;
                    _lines[i] = line;
                }
                var pellet = volley.Pellets[i];
                _lines[i].SetPosition(0, pellet.ShotOrigin);
                _lines[i].SetPosition(1, pellet.HitPoint);
                _lines[i].enabled = true;
            }
            _remaining = .12f;
        }
        private void Update()
        {
            if (_remaining <= 0) return;
            _remaining -= Time.deltaTime;
            if (_remaining <= 0) Hide();
        }
        private void Hide()
        {
            _remaining = 0;
            foreach (var line in _lines) if (line != null) line.enabled = false;
        }
    }
}
