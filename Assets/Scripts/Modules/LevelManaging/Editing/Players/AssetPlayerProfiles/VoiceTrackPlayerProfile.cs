using Extensions;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Modules.LevelManaging.Editing.Players.EventAssetsProviders;
using Modules.LevelManaging.Editing.Players.PlayersSetup;

namespace Modules.LevelManaging.Editing.Players.AssetPlayerProfiles
{
    internal sealed class VoiceTrackPlayerProfile: GenericAssetPlayerProfile<IVoiceTrackAsset ,VoiceTrackAssetPlayer, VoiceTrackPlayerSetup, VoiceTrackAssetsProvider>
    {
        public override DbModelType AssetType => DbModelType.VoiceTrack;
        private readonly AudioSourceManager _audioSourceManager;

        public VoiceTrackPlayerProfile(IAssetManager assetManager, AudioSourceManager audioSourceManager) : base(assetManager)
        {
            _audioSourceManager = audioSourceManager;
        }

        protected override VoiceTrackAssetPlayer CreatePlayer()
        {
            return new VoiceTrackAssetPlayer();
        }

        protected override VoiceTrackPlayerSetup CreateSetup()
        {
            return new VoiceTrackPlayerSetup(_audioSourceManager);
        }

        protected override VoiceTrackAssetsProvider CreateEventAssetsProvider()
        {
            return new VoiceTrackAssetsProvider(AssetManager);
        }
    }
}