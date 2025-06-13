using AssetBundleLoaders;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.Unloaders;

namespace Modules.AssetsManaging.LoadingProfiles
{
    internal sealed class BodyAnimationLoadProfile: AssetLoadProfile<BodyAnimationInfo, BodyAnimationLoadArgs>
    { 
        private readonly IBridge _bridge;
        private readonly IAssetBundleLoader _assetBundleLoader;

        public BodyAnimationLoadProfile(IBridge bridge, IAssetBundleLoader assetBundleLoader)
        {
            _bridge = bridge;
            _assetBundleLoader = assetBundleLoader;
        }

        public override AssetLoader<BodyAnimationInfo, BodyAnimationLoadArgs> GetAssetLoader()
        {
            return new BodyAnimationLoader(_bridge, _assetBundleLoader);
        }

        public override AssetUnloader GetUnloader()
        {
            return new BodyAnimationUnloader();
        }
    }
}