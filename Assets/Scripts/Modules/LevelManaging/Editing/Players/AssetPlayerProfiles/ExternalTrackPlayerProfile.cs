using Extensions;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Modules.LevelManaging.Editing.Players.EventAssetsProviders;
using Modules.LevelManaging.Editing.Players.PlayersSetup;

namespace Modules.LevelManaging.Editing.Players.AssetPlayerProfiles
{
    internal sealed class ExternalTrackPlayerProfile : GenericAssetPlayerProfile<IExternalTrackAsset, ExternalTrackAssetPlayer, ExternalTrackPlayerSetup, ExternalTrackAssetsProvider>
    {
        public ExternalTrackPlayerProfile(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override DbModelType AssetType => DbModelType.ExternalTrack;
        
        protected override ExternalTrackAssetPlayer CreatePlayer()
        {
            return new ExternalTrackAssetPlayer();
        }

        protected override ExternalTrackPlayerSetup CreateSetup()
        {
            return new ExternalTrackPlayerSetup();
        }

        protected override ExternalTrackAssetsProvider CreateEventAssetsProvider()
        {
            return new ExternalTrackAssetsProvider(AssetManager);
        }
    }
}