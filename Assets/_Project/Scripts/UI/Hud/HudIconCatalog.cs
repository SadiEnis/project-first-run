using System;
using UnityEngine;

namespace ProjectFirstRun.UI.Hud
{
    [CreateAssetMenu(menuName = "Project First Run/UI/HUD icon catalog")]
    public sealed class HudIconCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string Id;
            public string Label;
            public Color Tint = Color.white;
            public Sprite Sprite;
            [Tooltip("Normalised polyline points; semicolon separates strokes. Sprite overrides this greybox glyph.")]
            public string Strokes;
        }
        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();
        public Entry Find(string id)
        {
            foreach (var entry in _entries) if (entry != null && entry.Id == id) return entry;
            return null;
        }
    }
}
