using AssetBundleLoaders;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.AssetsManaging.Loaders
{
    internal sealed class CameraFilterVariantLoader : InstantiatableBundleAssetLoader<CameraFilterVariantInfo, CameraFilterVariantLoadArgs, CameraFilterVariantAsset>
    {
        public CameraFilterVariantLoader(IBridge bridge, IAssetBundleLoader assetBundleLoader, ISceneObjectHelper sceneObjectHelper) 
            : base(bridge, assetBundleLoader, sceneObjectHelper)
        {
        }
        
        protected override void InitAsset()
        {
            var view = new CameraFilterVariantAsset();
            var gameObject = Target as GameObject;
            view.Init(Model, gameObject, Bundle);
            MoveAssetToPersistentScene(view);
            Asset = view;
        }
    }
}