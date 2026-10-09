using System;
using ProjectFirstRun.Arenas;
using UnityEngine;

namespace ProjectFirstRun.Development.Arenas
{
    /// <summary>Animates scene-authored greybox visuals without simulating destruction.</summary>
    public sealed class DemoEndingPresentation : MonoBehaviour
    {
        [SerializeField] private Transform _focus;
        [SerializeField] private Transform _strikingArm;
        [SerializeField] private Transform _raisedPose;
        [SerializeField] private Transform _impactPose;
        [SerializeField] private Transform[] _rubble;
        [SerializeField, Range(.1f, 60)] private float _revealSeconds = 1.8f;
        [SerializeField, Range(.1f, 60)] private float _windupSeconds = 1.2f;
        [SerializeField, Range(.1f, 60)] private float _strikeSeconds = .35f;
        [SerializeField, Range(.1f, 60)] private float _aftermathSeconds = 1.2f;
        [SerializeField, Range(.1f, 60)] private float _fadeSeconds = 1f;
        [SerializeField, Range(0, .15f)] private float _cameraShake = .05f;
        private Transform _view;
        private Vector3 _viewPosition, _viewLocalPosition, _armPosition;
        private Quaternion _viewRotation, _viewLocalRotation;
        private Vector3[] _rubblePositions;
        private Quaternion[] _rubbleRotations;
        private DemoEndingTimeline _timeline;
        public bool HasStarted => _timeline != null;
        public bool IsComplete => _timeline != null && _timeline.IsComplete;
        public float Fade => _timeline == null ? 0 : _timeline.Fade;
        public int ImpactCount { get; private set; }

        public void ValidateConfiguration()
        {
            if (_focus == null || _strikingArm == null || _raisedPose == null || _impactPose == null ||
                _rubble == null || _rubble.Length == 0 || !float.IsFinite(_cameraShake) || _cameraShake < 0 || _cameraShake > .15f)
                throw new InvalidOperationException("Assign the ending focus, striking arm, raised/impact poses, rubble and valid camera shake.");
            var unique = new System.Collections.Generic.HashSet<Transform>();
            foreach (var piece in _rubble)
                if (piece == null || !unique.Add(piece)) throw new InvalidOperationException("Assign distinct ending rubble transforms.");
            CreateTimeline();
        }

        private DemoEndingTimeline CreateTimeline() => new(_revealSeconds, _windupSeconds, _strikeSeconds, _aftermathSeconds, _fadeSeconds);

        public void Begin(Transform view)
        {
            if (HasStarted) return;
            ValidateConfiguration();
            if (view == null) throw new ArgumentNullException(nameof(view));
            _view = view;
            _viewPosition = view.position; _viewRotation = view.rotation;
            _viewLocalPosition = view.localPosition; _viewLocalRotation = view.localRotation;
            _armPosition = _strikingArm.position;
            _rubblePositions = new Vector3[_rubble.Length];
            _rubbleRotations = new Quaternion[_rubble.Length];
            for (int i = 0; i < _rubble.Length; i++)
            {
                _rubblePositions[i] = _rubble[i].position;
                _rubbleRotations[i] = _rubble[i].rotation;
            }
            _timeline = CreateTimeline();
        }

        public void Tick(float unscaledDeltaTime)
        {
            if (_timeline == null || IsComplete) return;
            if (_timeline.Tick(unscaledDeltaTime)) ImpactCount++;
            float t = _timeline.Elapsed;
            float windup = Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - _revealSeconds) / _windupSeconds));
            float strike = Mathf.Clamp01((t - _revealSeconds - _windupSeconds) / _strikeSeconds);
            _strikingArm.position = strike > 0
                ? Vector3.Lerp(_raisedPose.position, _impactPose.position, strike * strike)
                : Vector3.Lerp(_armPosition, _raisedPose.position, windup);

            float afterImpact = Mathf.Max(0, t - _timeline.ImpactAt);
            float reaction = _timeline.HasImpacted ? Mathf.Clamp01(1 - afterImpact / .8f) : 0;
            Vector3 lookDirection = _focus.position - _viewPosition;
            Quaternion target = lookDirection.sqrMagnitude > .001f ? Quaternion.LookRotation(lookDirection) : _viewRotation;
            _view.rotation = Quaternion.Slerp(_viewRotation, target, Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / _revealSeconds))) *
                Quaternion.Euler(Mathf.Sin(afterImpact * 25) * reaction * _cameraShake * 12, 0, 0);
            _view.position = _viewPosition + new Vector3(Mathf.Sin(afterImpact * 39), Mathf.Sin(afterImpact * 31), 0) * (_cameraShake * reaction);
            if (_timeline.HasImpacted)
                for (int i = 0; i < _rubble.Length; i++)
                {
                    float fall = Mathf.Clamp01(afterImpact / (_aftermathSeconds + .2f * i));
                    _rubble[i].position = _rubblePositions[i] + Vector3.down * (fall * fall * 5.5f);
                    _rubble[i].rotation = _rubbleRotations[i] * Quaternion.Euler(fall * 60, fall * 25 * (i % 2 == 0 ? 1 : -1), 0);
                }
        }

        public void RestoreView()
        {
            if (_view == null) return;
            _view.localPosition = _viewLocalPosition;
            _view.localRotation = _viewLocalRotation;
        }

        public void DrawOverlay()
        {
            if (_timeline == null) return;
            Color original = GUI.color;
            // Greybox dust wash; no imported VFX/audio assets or destructive physics.
            float dust = _timeline.HasImpacted ? .18f * Mathf.Clamp01(1 - (_timeline.Elapsed - _timeline.ImpactAt) / 2f) : 0;
            GUI.color = new Color(.5f, .43f, .34f, dust);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = new Color(0, 0, 0, Fade);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = original;
        }
    }
}
