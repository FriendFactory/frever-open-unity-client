using Extensions;
using Modules.AssetsManaging;
using System;
using System.Linq;
using System.Threading;
using Bridge.Models.ClientServer.Assets;
using JetBrains.Annotations;
using Modules.AssetsManaging.LoadArgs;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    [UsedImplicitly]
    internal sealed class CameraFilterVariantChanger : BaseChanger
    {
        private readonly IAssetManager _assetManager;
        private ICameraFilterVariantAsset _previousFilterVariant;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CameraFilterVariantChanger(IAssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Run(CameraFilterInfo cameraFilter, long variantId, Action<IAsset> onCompleted = null, Action onCancelled = null)
        {
            CancelAll();
            var variant = cameraFilter?.CameraFilterVariants.First(x => x.Id == variantId);
            var variantAssets = _assetManager.GetActiveAssets<ICameraFilterVariantAsset>();
            if (variantAssets.Any())
            {
                _previousFilterVariant = variantAssets.FirstOrDefault();
            }

            void OnLoaded(IAsset asset)
            {
                if(asset != null) CancellationSources.Remove(asset.Id);
                onCompleted(asset);
                OnCameraFilterVariantLoaded(asset);
            }

            void OnLoadFailed(string message)
            {
                if (!CancellationSources.ContainsKey(variant.Id)) return;
                InvokeAssetUpdated(DbModelType.CameraFilterVariant);
                OnFail(message, variant.Id, DbModelType.CameraFilterVariant);
                onCancelled?.Invoke();
            }

            void OnUserUnloadFilter()
            {
                OnLoaded(null);
            }
            
            if (variant != null)
            {
                var cancellationSource = new CancellationTokenSource();
                CancellationSources.Add(variant.Id, cancellationSource);
                var args = new CameraFilterVariantLoadArgs() { CancellationToken = cancellationSource.Token };
                
                InvokeAssetStartedUpdating(DbModelType.CameraFilterVariant, cameraFilter.Id);
                _assetManager.Load(variant, args, OnLoaded, OnLoadFailed);
            }
            else 
            {
                if(_previousFilterVariant != null)
                {
                    _previousFilterVariant.SetActive(false);
                    _assetManager.Unload(_previousFilterVariant, OnUserUnloadFilter);
                    _previousFilterVariant = null;
                }
                else
                {
                    OnUserUnloadFilter();
                }
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnCameraFilterVariantLoaded(IAsset asset)
        {
            var variantAssets = asset as ICameraFilterVariantAsset;
            variantAssets?.SetActive(true);
            
            if (asset != null & _previousFilterVariant != null && asset.Id != _previousFilterVariant.Id)
            {
                _previousFilterVariant.SetActive(false);
                _assetManager.Unload(_previousFilterVariant);
            }
            
            InvokeAssetUpdated(DbModelType.CameraFilterVariant);
        }
    }
}