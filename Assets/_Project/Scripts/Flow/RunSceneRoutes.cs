using System;
using UnityEngine;
namespace ProjectFirstRun.Flow
{
    [CreateAssetMenu(menuName = "Project First Run/Run Scene Routes")]
    public sealed class RunSceneRoutes : ScriptableObject
    {
        [SerializeField] private string _hubScenePath;
        [SerializeField] private string _runScenePath;
        public string HubScenePath => _hubScenePath;
        public string RunScenePath => _runScenePath;
        public void ValidateConfiguration()
        {
            if (!Valid(_hubScenePath) || !Valid(_runScenePath) || _hubScenePath == _runScenePath)
                throw new InvalidOperationException("Assign distinct saved hub and run scene paths.");
        }
        private static bool Valid(string path) => !string.IsNullOrWhiteSpace(path) &&
            path.StartsWith("Assets/", StringComparison.Ordinal) && path.EndsWith(".unity", StringComparison.Ordinal) &&
            !path.Contains("..") && !path.Contains("\\");
    }
}
