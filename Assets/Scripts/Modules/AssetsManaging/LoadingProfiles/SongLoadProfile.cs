using Bridge;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.Unloaders;
using Modules.LevelManaging.Assets.AssetDependencies;

namespace Modules.AssetsManaging.LoadingProfiles
{
    internal sealed class SongLoadProfile: AssetLoadProfile<SongInfo, SongLoadArgs>
    {
        private readonly IBridge _bridge;
        private readonly AudioSourceManager _audioSourceManager;

        public SongLoadProfile(IBridge bridge, AudioSourceManager audioSourceManager)
        {
            _bridge = bridge;
            _audioSourceManager = audioSourceManager;
        }

        public override AssetLoader<SongInfo, SongLoadArgs> GetAssetLoader()
        {
            return new SongLoader(_bridge, _audioSourceManager);
        }

        public override AssetUnloader GetUnloader()
        {
            return new AudioUnloader();
        }
    }
}