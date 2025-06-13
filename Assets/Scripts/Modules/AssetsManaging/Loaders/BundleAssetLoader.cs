using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using AssetBundleLoaders;
using Bridge.ExternalPackages.AsynAwaitUtility;
using Bridge.Models.Common;
using Bridge;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Modules.AssetsManaging.Loaders
{
    internal abstract class BundleAssetLoader<TEntity, TArgs> : FileAssetLoader<TEntity, TArgs> 
        where TEntity: IFilesAttachedEntity 
        where TArgs: LoadAssetArgs<TEntity>
    {
        protected readonly IAssetBundleLoader AssetBundleLoader;
        protected Object UnpackedObject;
        protected AssetBundle Bundle;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected BundleAssetLoader(IBridge bridge, IAssetBundleLoader assetBundleLoader) : base(bridge)
        {
            AssetBundleLoader = assetBundleLoader;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override async Task Download(CancellationToken cancellationToken = default)
        {
            var result = await Bridge.GetAssetAsync(Model, true, cancellationToken);
            
            Bundle = result.Object as AssetBundle;

            if (IsCancellationRequested)
            {
                OnCancelled();
                return;
            }
            
            if (result.IsSuccess)
            {
                await UnpackBundle();
                if (IsCancellationRequested)
                {
                    OnCancelled();
                }
            }
            else if (result.IsError)
            {
                OnFailed(GetErrorResult(result.ErrorMessage));
            }
        }

        protected async Task UnpackBundle()
        {
            await UnpackBundleCoroutine(asset =>
            {
                if (IsCancellationRequested)
                {
                    OnCancelled();
                    return;
                }
                
                OnBundleUnpacked(asset);
            });
        }

        protected virtual IEnumerator UnpackBundleCoroutine(Action<Object> onUnpacked)
        {
            return AssetBundleLoader.LoadGameObjectFromBundle(Bundle, onUnpacked, x=> UnpackingBundleFailed());
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        protected virtual void OnBundleUnpacked(Object unpackedObject)
        {
            UnpackedObject = unpackedObject;
            InitAsset();
            OnCompleted(GetSuccessResult());
        }

        private void UnpackingBundleFailed()
        {
            OnFailed(GetErrorResult("Unpacking bundle failed"));
        }

        protected override void ReleaseResources()
        {
            if (Bundle != null)
            {
                Bundle.Unload(true);
                Bundle = null;
            }
            
            base.ReleaseResources();
        }
    }
}
