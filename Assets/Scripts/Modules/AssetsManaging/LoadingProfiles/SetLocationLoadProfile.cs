using AssetBundleLoaders;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.UncompressedBundles;
using Modules.AssetsManaging.Unloaders;
using Modules.LevelManaging.Assets;

namespace Modules.AssetsManaging.LoadingProfiles
{
    internal sealed class SetLocationLoadProfile: AssetLoadProfile<SetLocationFullInfo, SetLocationLoadArgs>
    {
        private readonly IBridge _bridge;
        private readonly IAssetBundleLoader _assetBundleLoader;
        private readonly UncompressedBundlesManager _uncompressedBundlesManager;
        private readonly ISceneObjectHelper _sceneObjectHelper;

        public SetLocationLoadProfile(IBridge bridge, IAssetBundleLoader assetBundleLoader, UncompressedBundlesManager uncompressedBundlesManager, 
                                      ISceneObjectHelper sceneObjectHelper)
        {
            _bridge = bridge;
            _assetBundleLoader = assetBundleLoader;
            _uncompressedBundlesManager = uncompressedBundlesManager;
            _sceneObjectHelper = sceneObjectHelper;
        }

        public override AssetLoader<SetLocationFullInfo, SetLocationLoadArgs> GetAssetLoader()
        {
            return new SetLocationLoader(_bridge, _assetBundleLoader, _uncompressedBundlesManager, _sceneObjectHelper);
        }

        public override AssetUnloader GetUnloader()
        {
            return new SetLocationUnloader();
        }
    }
}