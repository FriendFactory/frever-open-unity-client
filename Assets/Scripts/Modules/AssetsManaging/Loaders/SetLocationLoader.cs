using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssetBundleLoaders;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Modules.AssetsManaging.LoadArgs;
using Modules.AssetsManaging.UncompressedBundles;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies.AssetSceneMovers;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Task = System.Threading.Tasks.Task;

namespace Modules.AssetsManaging.Loaders
{
    internal sealed class SetLocationLoader : BundleAssetLoader<SetLocationFullInfo, SetLocationLoadArgs>
    {
        private readonly UncompressedBundlesManager _uncompressedBundlesManager;
        private readonly ISceneObjectHelper _sceneObjectHelper;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public SetLocationLoader(IBridge bridge, IAssetBundleLoader assetBundleLoader, UncompressedBundlesManager uncompressedBundlesManager, ISceneObjectHelper sceneObjectHelper) 
            : base(bridge, assetBundleLoader)
        {
            _uncompressedBundlesManager = uncompressedBundlesManager;
            _sceneObjectHelper = sceneObjectHelper;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override async Task Download(CancellationToken cancellationToken = default)
        {
            while (_uncompressedBundlesManager.IsDecompressingNow(Model.SetLocationBundle))
            {
                await Task.Delay(50, cancellationToken);
            }

            var assetBundleLoadResult = await GetAssetBundle(cancellationToken);
            Bundle = assetBundleLoadResult.AssetBundle;

            if (IsCancellationRequested)
            {
                OnCancelled();
                return;
            }
            
            if (assetBundleLoadResult.IsSuccess)
            {
                await UnpackBundle();
                if (IsCancellationRequested)
                {
                    OnCancelled();
                }
                return;
            }

            if (assetBundleLoadResult.IsFailed)
            {
                OnFailed(GetErrorResult(assetBundleLoadResult.FailReason));
            }
        }

        protected override IEnumerator UnpackBundleCoroutine(Action<Object> onUnpacked)
        {
            return AssetBundleLoader.LoadSceneAsyncFromBundle(Bundle, !LoadArgs.DeactivateOnLoad,()=> onUnpacked(null), x=> Debug.LogWarning(x));
        }

        protected override void InitAsset()
        {
            var view = new SetLocationAsset();
            var scene = SceneManager.GetSceneByPath(Bundle.GetAllScenePaths().First());
            var assetAttachingControl = new AssetAttachingControlProvider(scene, _sceneObjectHelper);
            
            view.Init(Model, scene, assetAttachingControl, LoadArgs.PictureInPictureLayerMask, SetLocationLoadArgs.PictureInPictureRenderScale);
            Bundle.Unload(false);
            Bundle = null;
            Asset = view;
        }

        private async Task<AssetBundleLoadingResult> GetAssetBundle(CancellationToken token)
        {
            var uncompressedBundle = await _uncompressedBundlesManager.GetUncompressedBundleIfExists(Model.SetLocationBundle, token);
            if (uncompressedBundle != null) return AssetBundleLoadingResult.Success(uncompressedBundle);

            var bundleModel = Model.SetLocationBundle;
            var fileInfo = bundleModel.GetCompatibleBundle();
            var result = await Bridge.GetAssetAsync(bundleModel, fileInfo, true, token);

            if (result.IsSuccess)
            {
                return AssetBundleLoadingResult.Success(result.Object as AssetBundle);
            }
            
            return result.IsError ? AssetBundleLoadingResult.Failed(result.ErrorMessage) : AssetBundleLoadingResult.Cancelled();
        }

        struct AssetBundleLoadingResult
        {
            public AssetBundle AssetBundle { get; private set; }
            public string FailReason { get; private set; }
            public bool IsCancelled { get; private set; }
            public bool IsSuccess => AssetBundle != null;
            public bool IsFailed => !IsCancelled && !IsSuccess;
            
            public static AssetBundleLoadingResult Success(AssetBundle assetBundle)
            {
                return new AssetBundleLoadingResult
                {
                    AssetBundle = assetBundle
                };
            }
            
            public static AssetBundleLoadingResult Failed(string error)
            {
                return new AssetBundleLoadingResult
                {
                    FailReason = error
                };
            }
            public static AssetBundleLoadingResult Cancelled()
            {
                return new AssetBundleLoadingResult
                {
                    IsCancelled = true
                };
            }
        }
    }
}

