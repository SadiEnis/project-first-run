using System;
using ProjectFirstRun.Combat;
using ProjectFirstRun.Input;
using ProjectFirstRun.Player;
using UnityEngine;

namespace ProjectFirstRun.Arenas
{
    /// <summary>Local traversal recovery, never a run checkpoint or resurrection.</summary>
    [DefaultExecutionOrder(200)]
    public sealed class ParkourRecovery : MonoBehaviour
    {
        [SerializeField] private BoxCollider _fallVolume;
        [SerializeField] private Transform _returnPoint;
        [SerializeField] private HealthComponent _health;
        [SerializeField, Range(.01f, 1f)] private float _damageFraction = .1f;
        [SerializeField, Min(.01f)] private float _fadeSeconds = .15f;
        private PlayerMotor _motor;
        private PlayerLook _look;
        private PlayerInputReader _input;
        private CharacterController _body;
        private bool _armed = true;
        private bool _returned;
        private float _elapsed;
        public bool IsRecovering { get; private set; }
        public float Fade { get; private set; }
        public string LastError { get; private set; }

        public void Configure(BoxCollider volume, Transform returnPoint, HealthComponent health)
        {
            if (IsRecovering) throw new InvalidOperationException("Cannot reconfigure during recovery.");
            _fallVolume = volume;
            _returnPoint = returnPoint;
            _health = health;
            ValidateConfiguration();
        }

        public void ValidateConfiguration()
        {
            if (_fallVolume == null || !_fallVolume.isTrigger || _returnPoint == null || _health == null ||
                !float.IsFinite(_damageFraction) || _damageFraction <= 0 || _damageFraction > 1 ||
                !float.IsFinite(_fadeSeconds) || _fadeSeconds <= 0)
                throw new InvalidOperationException("Assign a fall trigger, safe return point, player health and valid recovery settings.");
            _motor = _health.GetComponent<PlayerMotor>();
            _look = _health.GetComponent<PlayerLook>();
            _input = _health.GetComponent<PlayerInputReader>();
            _body = _health.GetComponent<CharacterController>();
            if (_motor == null || _look == null || _input == null || _body == null)
                throw new InvalidOperationException("Recovery requires the existing player motor, look, input and CharacterController.");
        }

        private void Start()
        {
            try { ValidateConfiguration(); ValidateReturnPoint(); }
            catch (Exception error) { LastError = error.Message; }
        }

        private bool IsInside()
        {
            return _fallVolume.enabled && _fallVolume.gameObject.activeInHierarchy && _body.enabled &&
                Physics.ComputePenetration(_fallVolume, _fallVolume.transform.position, _fallVolume.transform.rotation,
                    _body, _body.transform.position, _body.transform.rotation, out _, out _);
        }

        public void ValidateReturnPoint()
        {
            ValidateConfiguration();
            if ((_body.transform.lossyScale - Vector3.one).sqrMagnitude > .001f)
                throw new InvalidOperationException("Recovery currently requires unit player scale.");
            Vector3 position = _returnPoint.position;
            Quaternion yaw = Quaternion.Euler(0, _returnPoint.eulerAngles.y, 0);
            if (Physics.ComputePenetration(_fallVolume, _fallVolume.transform.position, _fallVolume.transform.rotation,
                _body, position, yaw, out _, out _))
                throw new InvalidOperationException("Return point must be outside the fall volume.");
            Vector3 center = position + yaw * _body.center;
            float radius = _body.radius;
            float halfSegment = Mathf.Max(0, _body.height * .5f - radius);
            foreach (var obstacle in Physics.OverlapCapsule(center + Vector3.up * halfSegment,
                center - Vector3.up * halfSegment, radius, ~0, QueryTriggerInteraction.Ignore))
                if (!obstacle.transform.IsChildOf(_body.transform))
                    throw new InvalidOperationException("Return point needs clear floor/headroom for the player capsule.");
            Vector3 feet = center - Vector3.up * (_body.height * .5f);
            bool supported = false;
            foreach (var hit in Physics.RaycastAll(feet + Vector3.up * .02f, Vector3.down, .35f, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(_body.transform) && hit.normal.y > .7f) supported = true;
            if (!supported) throw new InvalidOperationException("Return point must have a supporting floor within 0.35 m.");
        }

        private void Update() => Tick(Time.deltaTime);

        public void Tick(float deltaTime)
        {
            if (_health == null || LastError != null) return;
            if (_health.IsDead) { Release(); return; }
            if (deltaTime <= 0 || Time.timeScale <= 0) return;
            if (!IsRecovering)
            {
                if (!IsInside()) { _armed = true; return; }
                if (!_armed) return;
                try { ValidateReturnPoint(); }
                catch (Exception error) { LastError = error.Message; return; }
                _armed = false;
                IsRecovering = true;
                _elapsed = 0;
                _returned = false;
                _input.SetGameplayBlocked(this, true);
                _motor.SetSuspended(this, true);
                _health.ApplyDamage(new DamageInfo(_health.MaximumHealth * _damageFraction, null,
                    _health.transform.position, Vector3.down));
                if (_health.IsDead) Release();
                return;
            }
            _elapsed += deltaTime;
            if (!_returned)
            {
                Fade = Mathf.Clamp01(_elapsed / _fadeSeconds);
                if (_elapsed < _fadeSeconds) return;
                // Recheck because another actor may have occupied the anchor during the fade.
                try { ValidateReturnPoint(); }
                catch (Exception error) { LastError = error.Message; Release(); return; }
                _motor.Teleport(_returnPoint.position, Quaternion.Euler(0, _returnPoint.eulerAngles.y, 0));
                _look.ResetPitch();
                Physics.SyncTransforms();
                _returned = true;
                _elapsed = 0;
                return;
            }
            Fade = 1f - Mathf.Clamp01(_elapsed / _fadeSeconds);
            if (_elapsed >= _fadeSeconds) Release();
        }

        private void Release()
        {
            if (_health != null && _health.IsDead && _input != null)
                _input.SetGameplayInputEnabled(false);
            if (_input != null) _input.SetGameplayBlocked(this, false);
            if (_motor != null) _motor.SetSuspended(this, false);
            IsRecovering = false;
            Fade = 0;
        }

        private void OnDisable() => Release();

        private void OnGUI()
        {
            if (LastError != null)
                GUI.Box(new Rect(16, 210, Mathf.Min(800, Screen.width - 32), 70), "Parkour configuration error: " + LastError);
            if (Fade <= 0 || _health == null || _health.IsDead) return;
            int depth = GUI.depth;
            Color color = GUI.color;
            GUI.depth = -1000;
            GUI.color = new Color(0, 0, 0, Fade);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = color;
            GUI.depth = depth;
        }
    }
}
