using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.Unloaders;

namespace Modules.AssetsManaging.LoadingProfiles
{
    internal sealed class VoiceTrackLoadProfile: AssetLoadProfile<VoiceTrackFullInfo, VoiceTrackLoadArgs>
    {
        private readonly IBridge _bridge;

        public VoiceTrackLoadProfile(IBridge bridge)
        {
            _bridge = bridge;
        }

        public override AssetLoader<VoiceTrackFullInfo, VoiceTrackLoadArgs> GetAssetLoader()
        {
            return new VoiceTrackLoader(_bridge);
        }

        public override AssetUnloader GetUnloader()
        {
            return new VoiceTrackUnloader();
        }
    }
}