using System;
using UnityEngine;

namespace ProjectFirstRun.Arenas
{
    [Serializable]
    public sealed class SceneTravelDestination
    {
        [SerializeField] private string _scenePath;
        [SerializeField] private string _entryId;
        public string ScenePath => _scenePath;
        public string EntryId => _entryId;

        public SceneTravelDestination(string scenePath, string entryId)
        {
            _scenePath = scenePath;
            _entryId = entryId;
            Validate();
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(_scenePath) ||
                !_scenePath.StartsWith("Assets/", StringComparison.Ordinal) ||
                !_scenePath.EndsWith(".unity", StringComparison.Ordinal) ||
                _scenePath.Contains("..") || _scenePath.Contains("\\"))
                throw new ArgumentException("Use a full Assets/.../*.unity scene path.");
            if (string.IsNullOrWhiteSpace(_entryId)) throw new ArgumentException("An entry ID is required.");
        }
    }
}
