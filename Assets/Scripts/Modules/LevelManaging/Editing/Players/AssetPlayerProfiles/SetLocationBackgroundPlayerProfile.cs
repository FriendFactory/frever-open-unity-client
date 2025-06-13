using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Modules.LevelManaging.Editing.Players.EventAssetsProviders;
using Modules.LevelManaging.Editing.Players.PlayersSetup;

namespace Modules.LevelManaging.Editing.Players.AssetPlayerProfiles
{
    [UsedImplicitly]
    internal sealed class SetLocationBackgroundPlayerProfile : GenericAssetPlayerProfile<ISetLocationBackgroundAsset, SetLocationBackgroundAssetPlayer,
        SetLocationBackgroundPlayerSetup, SetLocationBackgroundsAssetProvider>
    {
        public SetLocationBackgroundPlayerProfile(IAssetManager assetManager) : base(assetManager)
        {
        }

        public override DbModelType AssetType => DbModelType.SetLocationBackground;
        
        protected override SetLocationBackgroundAssetPlayer CreatePlayer()
        {
            return new SetLocationBackgroundAssetPlayer();
        }

        protected override SetLocationBackgroundPlayerSetup CreateSetup()
        {
            return new SetLocationBackgroundPlayerSetup(AssetManager);
        }

        protected override SetLocationBackgroundsAssetProvider CreateEventAssetsProvider()
        {
            return new SetLocationBackgroundsAssetProvider(AssetManager);
        }
    }
}