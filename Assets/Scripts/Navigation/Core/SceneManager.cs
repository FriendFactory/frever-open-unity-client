using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Bridge.ExternalPackages.AsynAwaitUtility;
using Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Navigation.Core
{
    public static class SceneManager
    {
        private static string _currentScenePath;
        private static bool _isLoadingSceneNow;
        
        private static readonly HashSet<string> _scenesToUnload = new();
        private static readonly HashSet<string> _unloadingScenes = new();

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public static void LoadSceneIfNeeded(string nextScenePath, Action onLoaded, bool loadAsync)
        {
            if (_isLoadingSceneNow)
            {
                Debug.LogWarning($"Already loading another scene. Attempt to load {nextScenePath} ignored.");
                return;
            }

            var loadedScenes = GetLoadedScenes();
            var sceneName = Path.GetFileNameWithoutExtension(nextScenePath);
            var isSceneAlreadyLoaded = Array.Exists(loadedScenes, scene => scene.name.Equals(sceneName, StringComparison.InvariantCultureIgnoreCase));
            
            if (!isSceneAlreadyLoaded)
            {
                if (_currentScenePath != null && _currentScenePath != nextScenePath)
                {
                    _scenesToUnload.Add(_currentScenePath);
                }

                if (_scenesToUnload.Contains(nextScenePath))
                {
                    _scenesToUnload.Remove(nextScenePath);
                }
                
                onLoaded += () =>
                {
                    if (_currentScenePath != null && _currentScenePath != nextScenePath && _scenesToUnload.Contains(_currentScenePath))
                    {
                        CoroutineSource.Instance.StartCoroutine(UnloadSceneCoroutine(_currentScenePath));
                    }

                    _currentScenePath = nextScenePath;
                };

                if (loadAsync)
                {
                    LoadSceneAsync(nextScenePath, LoadSceneMode.Additive, onLoaded);
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(nextScenePath, LoadSceneMode.Additive);
                    CoroutineSource.Instance.ExecuteWithFrameDelay(() => onLoaded.Invoke());//to be sure all object are loaded and we can user scene GetRootGameObjects API
                }
               
            }
            else
            {
                onLoaded?.Invoke();
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static async void LoadSceneAsync(string sceneName, LoadSceneMode sceneMode, Action onLoaded = null)
        {
            await LoadSceneCoroutine(sceneName, sceneMode, onLoaded);
        }

        private static Scene[] GetLoadedScenes()
        {
            var scenes = new List<Scene>();

            for (var index = 0; index < UnityEngine.SceneManagement.SceneManager.sceneCount; ++index)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(index);
                var isUnloading = _unloadingScenes.Contains(scene.path);
                if (isUnloading) continue;
                scenes.Add(scene);
            }

            return scenes.ToArray();
        }

        //---------------------------------------------------------------------
        // Coroutines
        //---------------------------------------------------------------------

        private static IEnumerator LoadSceneCoroutine(string sceneName, LoadSceneMode sceneMode, Action onLoaded = null)
        {
            while (_unloadingScenes.Contains(sceneName))
            {
                yield return null;//wait until unloaded
            }
            
            var asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, sceneMode);
            _isLoadingSceneNow = true;

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            _isLoadingSceneNow = false;
            onLoaded?.Invoke();
        }

        private static IEnumerator UnloadSceneCoroutine(string sceneName, Action onUnloaded = null)
        {
            _unloadingScenes.Add(sceneName);
            _scenesToUnload.Remove(sceneName);
            var asyncUnload = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);

            while (!asyncUnload.isDone)
            {
                yield return null;
            }
            _unloadingScenes.Remove(sceneName);
            onUnloaded?.Invoke();
        }
    }
}