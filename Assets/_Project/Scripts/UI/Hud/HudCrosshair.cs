using ProjectFirstRun.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectFirstRun.UI.Hud
{
    public enum CrosshairStyle { Precision, Spread, Prepared, Projectile }

    public static class CrosshairGeometry
    {
        public static CrosshairStyle Style(WeaponDeliveryMode delivery, int pellets, float preparation)
        {
            if (delivery == WeaponDeliveryMode.Rocket) return CrosshairStyle.Projectile;
            if (pellets > 1) return CrosshairStyle.Spread;
            return preparation > 0 ? CrosshairStyle.Prepared : CrosshairStyle.Precision;
        }

        // Perspective cone projected into overlay Canvas units. m11 includes camera zoom/FOV.
        public static float SpreadRadius(float halfAngle, float projectionY, float pixelHeight, float canvasScale)
            => Mathf.Tan(halfAngle * Mathf.Deg2Rad) * Mathf.Abs(projectionY) * pixelHeight * .5f /
               Mathf.Max(.0001f, canvasScale);
    }

    /// <summary>Read-only weapon presentation. Never changes aim, recoil or spread.</summary>
    public sealed class HudCrosshair : MaskableGraphic
    {
        [SerializeField] private Color _readyColor = new Color(.92f, .96f, 1, .9f);
        [SerializeField] private Color _unavailableColor = new Color(1, .65f, .25f, .65f);
        [SerializeField] private Color _killColor = new Color(1, .32f, .22f, 1);
        [SerializeField, Min(.05f)] private float _hitDuration = .18f;
        [SerializeField, Range(1, 4)] private float _lineWidth = 1.5f;
        private PlayerWeaponController _weapon;
        private PlayerWeaponRuntimeEntry _entry;
        private bool _visible, _unavailable, _lethal;
        private float _radius, _preparation, _hitTime, _shotTime;
        private CrosshairStyle _style;

        public void Hide()
        {
            _visible = false;
            _hitTime = _shotTime = 0;
            _lethal = false;
            SetVerticesDirty();
        }

        public void Present(PlayerWeaponController weapon, bool visible)
        {
            if (_weapon != weapon)
            {
                Unbind();
                _weapon = weapon;
                if (_weapon != null)
                {
                    _weapon.ShotFired += OnShot;
                    _weapon.ProjectileLaunched += OnRocket;
                    _weapon.PlasmaLaunched += OnPlasma;
                    _weapon.DamageApplied += OnDamage;
                }
            }
            _visible = visible && _weapon != null && _weapon.IsInitialized && _weapon.AimCamera != null;
            if (!_visible)
            {
                Hide();
                return;
            }
            var entry = _weapon.ActiveEntry;
            if (!ReferenceEquals(_entry, entry))
            {
                _entry = entry;
                _shotTime = _hitTime = 0;
                _lethal = false;
            }
            var camera = _weapon.AimCamera;
            _radius = CrosshairGeometry.SpreadRadius(entry.Shot.HalfAngle,
                camera.projectionMatrix.m11, camera.pixelHeight, canvas != null ? canvas.scaleFactor : 1);
            // Place at this weapon camera's viewport centre, including non-fullscreen viewports.
            if (rectTransform.parent is RectTransform parent && canvas != null)
            {
                var uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, camera.pixelRect.center,
                    uiCamera, out var centre)) rectTransform.localPosition = new Vector3(centre.x, centre.y, 0);
            }
            _style = CrosshairGeometry.Style(entry.DeliveryMode, entry.Shot.PelletCount, entry.Fire.PreparationDuration);
            _preparation = entry.Fire.PreparationDuration > 0
                ? Mathf.Clamp01(_weapon.PreparationElapsed / entry.Fire.PreparationDuration) : 0;
            _unavailable = _weapon.IsReloading || _weapon.MagazineAmmo == 0;
            _hitTime = Mathf.Max(0, _hitTime - Time.deltaTime);
            _shotTime = Mathf.Max(0, _shotTime - Time.deltaTime);
            if (_hitTime == 0) _lethal = false;
            SetVerticesDirty();
        }

        private void OnShot(HitscanVolleyResult result) => FlashShot();
        private void OnRocket(RocketProjectile projectile) => FlashShot();
        private void OnPlasma(PlasmaProjectile projectile) => FlashShot();
        private void FlashShot() { if (_visible) _shotTime = .07f; }
        private void OnDamage(HitscanShotResult result)
        {
            if (!_visible || !result.DamageWasApplied) return;
            _lethal = (_hitTime > 0 && _lethal) || result.DamageResult.WasLethal;
            _hitTime = _hitDuration;
        }

        private void Unbind()
        {
            if (_weapon != null)
            {
                _weapon.ShotFired -= OnShot;
                _weapon.ProjectileLaunched -= OnRocket;
                _weapon.PlasmaLaunched -= OnPlasma;
                _weapon.DamageApplied -= OnDamage;
            }
            _weapon = null;
            _entry = null;
            _hitTime = _shotTime = 0;
            _visible = false;
        }
        protected override void OnDisable() { Unbind(); base.OnDisable(); }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (!_visible) return;
            var tint = _unavailable ? _unavailableColor : (_shotTime > 0 ? Color.white : _readyColor);
            Stroke(vh, new Vector2(-1, 0), new Vector2(1, 0), tint, 2);
            if (_style == CrosshairStyle.Spread)
            {
                // No decorative bloom: the ring is the actual cone boundary, not a blast radius.
                if (_radius > 0) for (int i = 0; i < 8; i++) Arc(vh, _radius, i * 45 + 5, 35, tint);
            }
            else if (_style == CrosshairStyle.Prepared)
            {
                for (int i = 0; i < 4; i++)
                {
                    var direction = Unit(i * 90);
                    Stroke(vh, direction * _radius, direction * (_radius + 6), tint, _lineWidth);
                }
                if (_preparation > 0) Arc(vh, Mathf.Max(14, _radius + 12), 90, -360 * _preparation,
                    new Color(.35f, .85f, 1, .8f));
            }
            else if (_style == CrosshairStyle.Projectile)
            {
                for (int i = 0; i < 4; i++)
                {
                    float x = (i % 2 == 0 ? -1 : 1), y = (i < 2 ? -1 : 1);
                    Stroke(vh, new Vector2(x * 7, y * 12), new Vector2(x * 12, y * 12), tint, _lineWidth);
                    Stroke(vh, new Vector2(x * 12, y * 12), new Vector2(x * 12, y * 7), tint, _lineWidth);
                }
            }
            else
            {
                Stroke(vh, new Vector2(-10, -3), new Vector2(-7, 0), tint, _lineWidth);
                Stroke(vh, new Vector2(7, 0), new Vector2(10, -3), tint, _lineWidth);
            }
            if (_hitTime > 0)
            {
                var hit = _lethal ? _killColor : Color.white;
                hit.a = Mathf.Clamp01(_hitTime / Mathf.Max(.01f, _hitDuration) * 2);
                for (int i = 0; i < 4; i++)
                {
                    var direction = Unit(45 + i * 90);
                    Stroke(vh, direction * 6, direction * 12, hit, 2);
                }
            }
        }

        private static Vector2 Unit(float degrees) => new Vector2(Mathf.Cos(degrees * Mathf.Deg2Rad), Mathf.Sin(degrees * Mathf.Deg2Rad));
        private void Arc(VertexHelper vh, float radius, float start, float sweep, Color tint)
        {
            int steps = Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(sweep) / 6));
            for (int i = 0; i < steps; i++) Stroke(vh, Unit(start + sweep * i / steps) * radius,
                Unit(start + sweep * (i + 1) / steps) * radius, tint, _lineWidth);
        }
        private static void Stroke(VertexHelper vh, Vector2 a, Vector2 b, Color tint, float width)
        {
            Quad(vh, a, b, new Color(0, 0, 0, tint.a * .8f), width + 2);
            Quad(vh, a, b, tint, width);
        }
        private static void Quad(VertexHelper vh, Vector2 a, Vector2 b, Color tint, float width)
        {
            var d = b - a;
            if (d.sqrMagnitude < .000001f) return;
            var n = new Vector2(-d.y, d.x).normalized * width * .5f;
            int first = vh.currentVertCount;
            vh.AddVert(a - n, tint, Vector2.zero); vh.AddVert(a + n, tint, Vector2.zero);
            vh.AddVert(b + n, tint, Vector2.zero); vh.AddVert(b - n, tint, Vector2.zero);
            vh.AddTriangle(first, first + 1, first + 2); vh.AddTriangle(first, first + 2, first + 3);
        }
    }
}
