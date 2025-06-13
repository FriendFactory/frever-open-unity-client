using AssetBundleLoaders;
using Bridge.Models.Common;
using Bridge;
using Modules.LevelManaging.Assets;
using Object = UnityEngine.Object;

namespace Modules.AssetsManaging.Loaders
{
    internal abstract class InstantiatableBundleAssetLoader<TEntity, TArgs, TAsset> : BundleAssetLoader<TEntity, TArgs>
        where TEntity: IFilesAttachedEntity 
        where TArgs: LoadAssetArgs<TEntity>
        where TAsset: IAsset<TEntity>, ISceneObject
    {
        private readonly ISceneObjectHelper _sceneObjectHelper;
        protected Object InstantiatedObject;

        protected InstantiatableBundleAssetLoader(IBridge bridge, IAssetBundleLoader assetBundleLoader, ISceneObjectHelper sceneObjectHelper) 
            : base(bridge, assetBundleLoader)
        {
            _sceneObjectHelper = sceneObjectHelper;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnBundleUnpacked(Object unpackedObject)
        {
            UnpackedObject = unpackedObject;
            InstantiatedObject = Object.Instantiate(UnpackedObject);
            Target = InstantiatedObject;
            InitAsset();
            OnCompleted(GetSuccessResult());
        }

        protected void MoveAssetToPersistentScene(TAsset asset)
        {
            _sceneObjectHelper.MoveAssetToPersistantScene(asset);
        }

        protected override void ReleaseResources()
        {
            base.ReleaseResources();
            if (InstantiatedObject == null) return;
            Object.Destroy(InstantiatedObject);
            InstantiatedObject = null;
        }

    }
}