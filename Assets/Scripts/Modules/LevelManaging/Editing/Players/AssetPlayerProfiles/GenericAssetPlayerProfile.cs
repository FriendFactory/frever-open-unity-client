using Extensions;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Modules.LevelManaging.Editing.Players.EventAssetsProviders;
using Modules.LevelManaging.Editing.Players.PlayersSetup;

namespace Modules.LevelManaging.Editing.Players.AssetPlayerProfiles
{
    internal abstract class GenericAssetPlayerProfile<TAsset,TPlayer, TSetup, TProvider> : IAssetPlayerProfile<TAsset>
        where TAsset: IAsset
        where TPlayer : IAssetPlayer<TAsset> 
        where TSetup : IPlayerSetup<TPlayer> 
        where TProvider : IEventAssetsProvider
    {
        protected readonly IAssetManager AssetManager;

        protected GenericAssetPlayerProfile(IAssetManager assetManager)
        {
            AssetManager = assetManager;
        }

        public abstract DbModelType AssetType { get; }

        public IAssetPlayer GetPlayer()
        {
            return CreatePlayer();
        }
        
        public IPlayerSetup GetPlayerSetup()
        {
            return CreateSetup();
        }

        public IEventAssetsProvider GetAssetsProvider()
        {
            return CreateEventAssetsProvider();
        }

        protected abstract TPlayer CreatePlayer();

        protected abstract TSetup CreateSetup();

        protected abstract TProvider CreateEventAssetsProvider();
    }
}