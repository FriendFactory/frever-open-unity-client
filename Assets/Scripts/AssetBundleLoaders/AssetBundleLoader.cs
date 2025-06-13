using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AssetBundleLoaders
{
    internal sealed class AssetBundleLoader : MonoBehaviour, IAssetBundleLoader
    {
        public IEnumerator LoadGameObjectFromBundle(AssetBundle bundle, Action<Object> onSuccess, Action<string> onFail)
        {
            var request = bundle.LoadAllAssetsAsync<GameObject>();
            yield return request;

            if (request == null || request.asset == null)
            {
                onFail?.Invoke("Loading asset from bundle failed.");
            }
            else
            {
                onSuccess?.Invoke(request.asset);
            }
            
        }

        public IEnumerator LoadSceneAsyncFromBundle(AssetBundle bundle, bool setActive, Action onSuccess, Action<string> onFail, Action onCancelled, CancellationToken cancellationToken = default)
        {
            var scenePath = bundle.GetAllScenePaths().First();
            //dont want to load same scene which is already loaded
            if (SceneManager.GetSceneByPath(scenePath).isLoaded)
            { 
                onSuccess?.Invoke();              
                yield break;
            }
            
            var sceneName = Path.GetFileNameWithoutExtension(scenePath);
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (cancellationToken.IsCancellationRequested)
            {
                SceneManager.UnloadSceneAsync(sceneName);
                onCancelled?.Invoke();
                UnloadSceneBundle(bundle);
                yield break;
            }

            if (setActive)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            }
            
            LightProbes.Tetrahedralize();
            onSuccess?.Invoke();
        }

        public IEnumerator LoadAnimationFromBundle(AssetBundle bundle, Action<Object> onSuccess, Action<string> onFail)
        {
            var names = bundle.GetAllAssetNames();
            var request = bundle.LoadAssetAsync<AnimationClip>(names[0]);
            yield return request;
            
            if (request == null || request.asset == null)
            {
                onFail?.Invoke("Loading asset from bundle failed.");
            }
            else
            {
                onSuccess?.Invoke(request.asset);
            }
        }
        
        private void UnloadSceneBundle(Object asset)
        {
            var bundle = asset as AssetBundle;
            if (bundle == null)
            {
                return;
            }

            if (bundle.isStreamedSceneAssetBundle)
            {
                bundle.Unload(true);
            }
        }

    }
}