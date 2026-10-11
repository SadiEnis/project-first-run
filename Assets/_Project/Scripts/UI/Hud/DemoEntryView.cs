using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectFirstRun.UI.Hud
{
    public sealed class DemoEntryView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Button _start;
        [SerializeField] private Button _settings;
        [SerializeField] private Text _status;
        [SerializeField] private EntryButtonFeedback _startFeedback;
        [SerializeField] private EntryButtonFeedback _settingsFeedback;
        public Button StartButton => _start;
        public void ValidateConfiguration()
        {
            if (_group == null || _start == null || _settings == null || _status == null ||
                _startFeedback == null || _settingsFeedback == null)
                throw new InvalidOperationException("Assign saved entry Canvas, buttons, status and hover feedback.");
        }
        public void Present(float alpha, bool interactive, bool pointer, int selected, string status)
        {
            _group.alpha = alpha;
            _group.interactable = interactive;
            _group.blocksRaycasts = alpha > 0;
            _status.text = status ?? "";
            _startFeedback.Present(interactive && (pointer ? _startFeedback.Hovered : selected == 0));
            _settingsFeedback.Present(interactive && (pointer ? _settingsFeedback.Hovered : selected == 1));
        }
        public void Hide()
        {
            if (_group == null) return;
            _group.alpha = 0;
            _group.interactable = _group.blocksRaycasts = false;
        }
    }
}
