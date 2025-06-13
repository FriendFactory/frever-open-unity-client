using AssetBundleLoaders;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.Unloaders;
using Modules.LevelManaging.Assets;

namespace Modules.AssetsManaging.LoadingProfiles
{
    internal sealed class VfxLoadProfile: AssetLoadProfile<VfxInfo, VfxLoadArgs>
    {
        private readonly IBridge _bridge;
        private readonly IAssetBundleLoader _assetBundleLoader;
        private readonly ISceneObjectHelper _sceneObjectHelper;

        public VfxLoadProfile(IBridge bridge, IAssetBundleLoader assetBundleLoader, ISceneObjectHelper sceneObjectHelper)
        {
            _bridge = bridge;
            _assetBundleLoader = assetBundleLoader;
            _sceneObjectHelper = sceneObjectHelper;
        }

        public override AssetLoader<VfxInfo, VfxLoadArgs> GetAssetLoader()
        {
            return new VfxLoader(_bridge, _assetBundleLoader, _sceneObjectHelper);
        }

        public override AssetUnloader GetUnloader()
        {
            return new VfxUnloader();
        }
    }
}