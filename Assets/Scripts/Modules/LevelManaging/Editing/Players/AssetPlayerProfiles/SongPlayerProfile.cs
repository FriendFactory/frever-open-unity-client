using Extensions;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Modules.LevelManaging.Editing.Players.EventAssetsProviders;
using Modules.LevelManaging.Editing.Players.PlayersSetup;

namespace Modules.LevelManaging.Editing.Players.AssetPlayerProfiles
{
    internal sealed class SongPlayerProfile: GenericAssetPlayerProfile<ISongAsset, SongAssetPlayer, SongPlayerSetup, SongAssetsProvider>
    {
        public override DbModelType AssetType => DbModelType.Song;
        
        public SongPlayerProfile(IAssetManager assetManager) : base(assetManager)
        {
        }
        
        protected override SongAssetPlayer CreatePlayer()
        {
            return new SongAssetPlayer();
        }

        protected override SongPlayerSetup CreateSetup()
        {
            return new SongPlayerSetup();
        }

        protected override SongAssetsProvider CreateEventAssetsProvider()
        {
            return new SongAssetsProvider(AssetManager);
        }
    }
}