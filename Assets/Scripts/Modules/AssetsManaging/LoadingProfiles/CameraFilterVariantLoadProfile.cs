using AssetBundleLoaders;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.Unloaders;
using Modules.LevelManaging.Assets;

namespace Modules.AssetsManaging.LoadingProfiles
{
    internal sealed class CameraFilterVariantLoadProfile: AssetLoadProfile<CameraFilterVariantInfo, CameraFilterVariantLoadArgs>
    {
        private readonly IBridge _bridge;
        private readonly IAssetBundleLoader _assetBundleLoader;
        private readonly ISceneObjectHelper _sceneObjectHelper;

        public CameraFilterVariantLoadProfile(IBridge bridge, IAssetBundleLoader assetBundleLoader, ISceneObjectHelper sceneObjectHelper)
        {
            _bridge = bridge;
            _assetBundleLoader = assetBundleLoader;
            _sceneObjectHelper = sceneObjectHelper;
        }

        public override AssetLoader<CameraFilterVariantInfo, CameraFilterVariantLoadArgs> GetAssetLoader()
        {
            return new CameraFilterVariantLoader(_bridge, _assetBundleLoader, _sceneObjectHelper);
        }

        public override AssetUnloader GetUnloader()
        {
            return new CameraFilterVariantUnloader();
        }
    }
}