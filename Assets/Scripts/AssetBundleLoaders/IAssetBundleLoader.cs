using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetBundleLoaders
{
    public interface IAssetBundleLoader
    {
        IEnumerator LoadGameObjectFromBundle(AssetBundle bundle, Action<Object> onSuccess, Action<string> onFail = null);
        IEnumerator LoadAnimationFromBundle(AssetBundle bundle, Action<Object> onSuccess, Action<string> onFail = null);
        IEnumerator LoadSceneAsyncFromBundle(AssetBundle bundle, bool setActive, Action onSuccess, Action<string> onFail = null, Action onCancelled = null, CancellationToken cancellationToken = default);
    }
}