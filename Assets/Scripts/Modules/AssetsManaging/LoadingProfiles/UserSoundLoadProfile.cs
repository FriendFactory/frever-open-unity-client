using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.Unloaders;
using Modules.LevelManaging.Assets.AssetDependencies;

namespace Modules.AssetsManaging.LoadingProfiles
{
    internal sealed class UserSoundLoadProfile: AssetLoadProfile<UserSoundFullInfo, UserSoundLoadArgs>
    {
        private readonly IBridge _bridge;
        private readonly AudioSourceManager _audioSourceManager;

        public UserSoundLoadProfile(IBridge bridge, AudioSourceManager audioSourceManager)
        {
            _bridge = bridge;
            _audioSourceManager = audioSourceManager;
        }

        public override AssetLoader<UserSoundFullInfo, UserSoundLoadArgs> GetAssetLoader()
        {
            return new UserSoundAssetLoader(_bridge, _audioSourceManager);
        }

        public override AssetUnloader GetUnloader()
        {
            return new UserSoundUnloader();
        }
    }
}