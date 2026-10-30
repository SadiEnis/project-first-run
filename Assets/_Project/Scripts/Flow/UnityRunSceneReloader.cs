using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Flow
{
    public sealed class UnityRunSceneReloader : IRunSceneReloader
    {
        public bool Begin(string scenePath)
        {
#if UNITY_EDITOR
            return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                scenePath, new LoadSceneParameters(LoadSceneMode.Single)) != null;
#else
            return SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single) != null;
#endif
        }
    }
}
