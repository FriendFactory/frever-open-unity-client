using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.Unloaders;

namespace Modules.AssetsManaging.LoadingProfiles
{
    internal sealed class PhotoLoadProfile: AssetLoadProfile<PhotoFullInfo, PhotoLoadArgs>
    {
        private readonly IBridge _bridge;

        public PhotoLoadProfile(IBridge bridge)
        {
            _bridge = bridge;
        }

        public override AssetLoader<PhotoFullInfo, PhotoLoadArgs> GetAssetLoader()
        {
            return new UserPhotoLoader(_bridge);
        }

        public override AssetUnloader GetUnloader()
        {
            return new TextureBaseAssetUnloader();
        }
    }
}