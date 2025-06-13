using System;
using System.Collections;
using Common;
using Modules.LevelManaging.Assets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modules.AssetsManaging.Unloaders
{
    internal sealed class SetLocationUnloader : AssetUnloader
    {
        public override void Unload(IAsset asset, Action onSuccess = null)
        {
            var setLocation = asset as ISetLocationAsset;
            var unloadRoutine = UnloadSceneAndBundle(setLocation, onSuccess);
            CoroutineSource.Instance.StartCoroutine(unloadRoutine);
        }
        
        private IEnumerator UnloadSceneAndBundle(ISetLocationAsset setLocation, Action onSuccess)
        {
            yield return SceneManager.UnloadSceneAsync(setLocation.SceneName);
            Resources.UnloadUnusedAssets();
            onSuccess?.Invoke();
        }
    }
}