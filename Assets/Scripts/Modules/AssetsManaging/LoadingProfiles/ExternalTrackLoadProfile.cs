using Bridge;
using Bridge.Services._7Digital.Models.TrackModels;
using Modules.AssetsManaging.LoadArgs;
using Modules.AssetsManaging.Loaders;
using Modules.AssetsManaging.Unloaders;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.MusicCacheManaging;

namespace Modules.AssetsManaging.LoadingProfiles
{
    internal sealed class ExternalTrackLoadProfile : AssetLoadProfile<ExternalTrackInfo, ExternalTrackLoadArgs>
    {
        private readonly IBridge _bridge;
        private readonly AudioSourceManager _audioSourceManager;
        private readonly ILicensedMusicProvider _licensedMusicProvider;

        public ExternalTrackLoadProfile(IBridge bridge, AudioSourceManager audioSourceManager, ILicensedMusicProvider licensedMusicProvider)
        {
            _bridge = bridge;
            _audioSourceManager = audioSourceManager;
            _licensedMusicProvider = licensedMusicProvider;
        }
        
        public override AssetLoader<ExternalTrackInfo, ExternalTrackLoadArgs> GetAssetLoader()
        {
            return new ExternalTrackLoader(_bridge, _audioSourceManager, _licensedMusicProvider);
        }

        public override AssetUnloader GetUnloader()
        {
            return new ExternalTrackUnloader(_licensedMusicProvider);
        }
    }
}