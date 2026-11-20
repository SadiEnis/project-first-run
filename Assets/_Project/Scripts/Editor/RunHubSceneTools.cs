#if UNITY_EDITOR
using System;
using ProjectFirstRun.Flow;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
namespace ProjectFirstRun.Editor
{
    public static class RunHubSceneTools
    {
        public const string RoutesPath = "Assets/_Project/Data/Progression/SO_RunSceneRoutes.asset";
        public static string[] BuildScenePaths()
        {
            var routes = AssetDatabase.LoadAssetAtPath<RunSceneRoutes>(RoutesPath);
            if (routes == null) throw new InvalidOperationException("Missing run scene routes asset.");
            routes.ValidateConfiguration();
            var paths = new[] { routes.HubScenePath, routes.RunScenePath };
            foreach (var path in paths)
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                    throw new InvalidOperationException("Missing route scene: " + path);
            return paths;
        }
        [MenuItem("Project First Run/Run/Open Hub")]
        public static void OpenHub()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(BuildScenePaths()[0]);
        }
        [MenuItem("Project First Run/Run/Build Windows Run Loop")]
        public static void BuildWindows()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = BuildScenePaths(), target = BuildTarget.StandaloneWindows64,
                locationPathName = "Builds/RunLoop/ProjectFirstRun.exe", options = BuildOptions.Development });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Run loop build failed: " + report.summary.result);
        }
    }
}
#endif
