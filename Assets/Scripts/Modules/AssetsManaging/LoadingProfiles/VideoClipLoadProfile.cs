using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.Unloaders;

namespace Modules.AssetsManaging.LoadingProfiles
{
    internal sealed class VideoClipLoadProfile: AssetLoadProfile<VideoClipFullInfo, VideoClipLoadArgs>
    {
        private readonly IBridge _bridge;

        public VideoClipLoadProfile(IBridge bridge)
        {
            _bridge = bridge;
        }

        public override AssetLoader<VideoClipFullInfo, VideoClipLoadArgs> GetAssetLoader()
        {
            return new VideoClipLoader(_bridge);
        }

        public override AssetUnloader GetUnloader()
        {
            return new VideoClipUnloader();
        }
    }
}