using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectFirstRun.UI.Hud
{
    /// <summary>FIFO playback with session-scoped completion supplied by its owner.</summary>
    public sealed class HudTipPlayback
    {
        private readonly Queue<string> _pending = new();
        private readonly HashSet<string> _queued = new();
        private readonly ISet<string> _completed;
        public string Current { get; private set; }
        public float Elapsed { get; private set; }
        public HudTipPlayback(ISet<string> completed) =>
            _completed = completed ?? throw new ArgumentNullException(nameof(completed));
        public void Enqueue(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || _completed.Contains(id) || !_queued.Add(id)) return;
            _pending.Enqueue(id);
        }
        public void Tick(float delta, float enter, float hold, float exit)
        {
            if (!float.IsFinite(delta) || delta < 0 || !float.IsFinite(enter) || enter <= 0 ||
                !float.IsFinite(hold) || hold < 0 || !float.IsFinite(exit) || exit <= 0)
                throw new ArgumentOutOfRangeException(nameof(delta));
            if (Current == null && _pending.Count > 0) Current = _pending.Dequeue();
            if (Current == null) return;
            Elapsed += delta;
            // Do not skip unread queued tips after a hitch; each new card gets its own visible cycle.
            if (Elapsed >= enter + hold + exit)
            {
                _completed.Add(Current); _queued.Remove(Current);
                Current = null; Elapsed = 0;
            }
        }
        public float Amount(float enter, float hold, float exit)
        {
            if (Current == null) return 0;
            float linear = Elapsed < enter ? Elapsed / enter :
                Elapsed < enter + hold ? 1 : 1 - (Elapsed - enter - hold) / exit;
            float t = Mathf.Clamp01(linear);
            return t * t * (3 - 2 * t);
        }
        public void Clear()
        {
            _pending.Clear(); _queued.Clear(); Current = null; Elapsed = 0;
        }
    }

    public sealed class HudTipView : MonoBehaviour
    {
        [Serializable]
        public sealed class Message
        {
            public string Id;
            [TextArea] public string KeyboardText;
            [TextArea] public string GamepadText;
        }
        [SerializeField] private RectTransform _card;
        [SerializeField] private CanvasGroup _visibility;
        [SerializeField] private Text _body;
        [SerializeField, Min(.01f)] private float _enterSeconds = .25f;
        [SerializeField, Min(0)] private float _holdSeconds = 1.5f;
        [SerializeField, Min(.01f)] private float _exitSeconds = .25f;
        [SerializeField, Min(0)] private float _rightMargin = 24;
        [SerializeField] private Message[] _messages;
        private static readonly HashSet<string> SeenThisSession = new();
        private HudTipPlayback _playback;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() => SeenThisSession.Clear();
        private void Awake()
        {
            _playback = new HudTipPlayback(SeenThisSession);
            Hide();
        }
        public void ValidateConfiguration()
        {
            if (_card == null || _visibility == null || _body == null || _messages == null)
                throw new InvalidOperationException("Assign saved TIP card, CanvasGroup, text and messages.");
            if (!float.IsFinite(_enterSeconds) || _enterSeconds <= 0 || !float.IsFinite(_exitSeconds) || _exitSeconds <= 0 ||
                !float.IsFinite(_holdSeconds) || _holdSeconds < 0 || !float.IsFinite(_rightMargin) || _rightMargin < 0)
                throw new InvalidOperationException("TIP durations and margin must be finite and valid.");
            foreach (string id in new[] { "move", "weapon", "ability", "upgrade" })
                if (Find(id) == null) throw new InvalidOperationException("Missing TIP message: " + id);
        }
        private Message Find(string id)
        {
            if (_messages != null) foreach (var message in _messages)
                if (message != null && message.Id == id) return message;
            return null;
        }
        public void Enqueue(string id)
        {
            if (Find(id) == null) return;
            _playback ??= new HudTipPlayback(SeenThisSession);
            _playback.Enqueue(id);
        }
        public void Refresh(bool visible, bool terminal)
        {
            if (terminal) _playback?.Clear();
            if (!visible || Time.timeScale <= 0) { Hide(); return; }
            float enter = Mathf.Max(.01f, _enterSeconds), hold = Mathf.Max(0, _holdSeconds), exit = Mathf.Max(.01f, _exitSeconds);
            _playback ??= new HudTipPlayback(SeenThisSession);
            _playback.Tick(Time.deltaTime, enter, hold, exit);
            var message = Find(_playback.Current);
            if (message == null) { Hide(); return; }
            string text = InputPrompts.IsGamepad ? message.GamepadText : message.KeyboardText;
            if (_body.text != text) _body.text = text;
            _visibility.alpha = 1;
            _visibility.interactable = false; _visibility.blocksRaycasts = false;
            var position = _card.anchoredPosition;
            position.x = Mathf.Lerp(_card.rect.width + _rightMargin, -_rightMargin,
                _playback.Amount(enter, hold, exit));
            _card.anchoredPosition = position;
        }
        private void Hide()
        {
            if (_visibility != null) _visibility.alpha = 0;
        }
        private void OnDisable() { _playback?.Clear(); Hide(); }
    }
}
