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
        public virtual bool CanLoad(string path) => Application.CanStreamedLevelBeLoaded(path);
        protected virtual AsyncOperation BeginLoad(string path) =>
            SceneManager.LoadSceneAsync(path, LoadSceneMode.Additive);

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
