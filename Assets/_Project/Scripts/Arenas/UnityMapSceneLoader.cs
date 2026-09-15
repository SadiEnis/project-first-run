using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectFirstRun.Arenas
{
    public interface IMapSceneLoader
    {
        bool CanLoad(string path);
        Task<Scene> LoadAsync(string path);
        Task UnloadAsync(Scene scene);
    }

    public class UnityMapSceneLoader : IMapSceneLoader
    {
        public virtual bool CanLoad(string path)
        {
            if (Application.CanStreamedLevelBeLoaded(path)) return true;
#if UNITY_EDITOR
            // The playable fixture is intentionally usable directly from the
            // Editor before its scenes are added to a player build profile.
            return UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(path) != null;
#else
            return false;
#endif
        }
        protected virtual AsyncOperation BeginLoad(string path)
        {
#if UNITY_EDITOR
            if (!Application.CanStreamedLevelBeLoaded(path))
                return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                    path, new LoadSceneParameters(LoadSceneMode.Additive));
#endif
            return SceneManager.LoadSceneAsync(path, LoadSceneMode.Additive);
        }

        public async Task<Scene> LoadAsync(string path)
        {
            var operation = BeginLoad(path) ?? throw new InvalidOperationException("Scene loading did not start.");
            while (!operation.isDone) await Task.Yield();
            Scene result = SceneManager.GetSceneByPath(path);
            if (!result.IsValid() || !result.isLoaded) throw new InvalidOperationException("Scene did not load: " + path);
            return result;
        }

        public async Task UnloadAsync(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded) return;
            var operation = SceneManager.UnloadSceneAsync(scene) ??
                throw new InvalidOperationException("Scene unloading did not start.");
            while (!operation.isDone) await Task.Yield();
            if (scene.IsValid() && scene.isLoaded) throw new InvalidOperationException("Scene remains loaded.");
        }
    }
}
